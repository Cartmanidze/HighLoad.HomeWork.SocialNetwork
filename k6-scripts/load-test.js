import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

export let successfulRequests = new Counter('successful_requests');
export let failedRequests = new Counter('failed_requests');
export let slowRequests = new Counter('slow_requests');
export let responseTime = new Trend('response_time', true);

export let options = {
    stages: [
        { duration: '30s', target: 1 },
        { duration: '30s', target: 10 },
        { duration: '30s', target: 100 },
        { duration: '30s', target: 1000 },
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],
    },
};

const BASE_URL = 'http://nginx:80';
const LOGIN_URL = `${BASE_URL}/auth/login`;
const USER_URL = `${BASE_URL}/users/000074ac-8a22-450f-a836-d730cd8c2a00`;
const SEARCH_URL = `${BASE_URL}/users/search?firstName=Al&lastName=Jo`;
const LOGIN_CREDENTIALS = { email: "alice.johnson92@example.com", password: "SecurePass!2024" };

function getToken() {
    let res = http.post(LOGIN_URL, JSON.stringify(LOGIN_CREDENTIALS), { headers: { 'Content-Type': 'application/json' } });
    check(res, { 'login_ok': (r) => r.status === 200 && r.json('token') !== undefined });
    if (res.status === 200) {
        return res.json('token');
    } else {
        failedRequests.add(1);
        return null;
    }
}

function handleResponse(res) {
    responseTime.add(res.timings.duration);
    if (res.status >= 200 && res.status < 300) {
        successfulRequests.add(1);
    } else {
        failedRequests.add(1);
    }
    if (res.timings.duration > 500) {
        slowRequests.add(1);
    }
}

export default function () {
    let token = getToken();
    if (!token) {
        return;
    }

    let params = {
        headers: {
            Authorization: `Bearer ${token}`
        }
    };

    let responses = http.batch([
        ['GET', USER_URL, null, params],
        ['GET', SEARCH_URL, null, params]
    ]);

    responses.forEach(r => handleResponse(r));

    sleep(0.4);
}
