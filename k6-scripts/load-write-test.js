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

const BASE_URL = 'http://highload.homework.socialnetwork:8080';
const GENERATE_USERS_ENDPOINT = `${BASE_URL}/auth/generate-users`;
const COUNT = 10;

export default function () {
    const url = `${GENERATE_USERS_ENDPOINT}?count=${COUNT}`;
    const res = http.post(url, null);
    handleResponse(res);
    sleep(1);
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
    check(res, { 'status is ok': (r) => r.status >= 200 && r.status < 300 });
}
