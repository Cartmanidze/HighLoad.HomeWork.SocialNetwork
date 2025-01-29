import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

// === Метрики ===
export let successfulRequests = new Counter('successful_requests');
export let failedRequests = new Counter('failed_requests');
export let slowRequests = new Counter('slow_requests');
export let responseTime = new Trend('response_time', true);

// === Настройки нагрузки ===
export let options = {
    stages: [
        { duration: '30s', target: 1 },    // 30s, разгон до 1 виртуального пользователя
        { duration: '30s', target: 10 },   // ещё 30s - до 10
        { duration: '30s', target: 100 },  // ещё 30s - до 100
        { duration: '30s', target: 1000 }, // ещё 30s - до 1000
    ],
    thresholds: {
        // 95-й перцентиль времени ответа < 500 мс
        http_req_duration: ['p(95)<500'],
    },
};

// === Константы для урлов ===
const BASE_URL = 'http://dialog-service:8080';
const LOGIN_URL = 'http://highload.homework.socialnetwork:8080/auth/login';
const DIALOG_SEND_URL = `${BASE_URL}/dialogs/send`;
const DIALOG_LIST_URL = `${BASE_URL}/dialogs/list`;

// Данные для логина (пример)
const LOGIN_CREDENTIALS = {
    email: "alice.johnson92@example.com",
    password: "SecurePass!2024"
};

// === Функция получения JWT-токена (если auth/login отсутствует - адаптируйте) ===
function getToken() {
    let res = http.post(
        LOGIN_URL,
        JSON.stringify(LOGIN_CREDENTIALS),
        { headers: { 'Content-Type': 'application/json' } }
    );

    check(res, {
        'login_ok': (r) => r.status === 200 && r.json('token') !== undefined
    });

    if (res.status === 200) {
        return res.json('token');
    } else {
        failedRequests.add(1);
        return null;
    }
}

// === Обработчик респонсов: записываем метрики ===
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

// === Основная функция, которая вызывается для каждого VU (вирт. пользователя) в течение stages ===
export default function () {
    // 1) Авторизация (JWT)
    let token = getToken();
    if (!token) {
        // Если логин не удался, то выходим
        return;
    }

    // Заголовки (JWT + JSON)
    let params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        }
    };

    // 2) Отправляем сообщение (POST /dialogs/send)
    // По логике контроллера: 
    //   request: { SenderId, Text }, 
    //   а реальный receiverId = userId из токена (или наоборот, см. ваш код).
    let sendBody = JSON.stringify({
        receiverId: "0042ec91-1f22-4e39-954c-6f13be92a423", // пример GUID
        text: "Hello from k6!"
    });
    let sendRes = http.post(DIALOG_SEND_URL, sendBody, params);
    handleResponse(sendRes);

    // 3) Получаем список сообщений (GET /dialogs/list?receiverId=xxx)
    //   request: Guid receiverId = ? 
    //   Указываем любой GUID, который "должен" существовать
    let listUrl = `${DIALOG_LIST_URL}?receiverId=0042ec91-1f22-4e39-954c-6f13be92a423`;
    let listRes = http.get(listUrl, params);
    handleResponse(listRes);

    // 4) Небольшая пауза
    sleep(0.4);
}
