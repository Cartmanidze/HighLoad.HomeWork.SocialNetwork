import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

// Метрики
export let successfulRequests = new Counter('successful_requests'); // Успешные запросы (статус 2xx)
export let failedRequests = new Counter('failed_requests');         // Неуспешные запросы (все, кроме 2xx)
export let slowRequests = new Counter('slow_requests');             // Запросы, превышающие 500 мс
export let responseTime = new Trend('response_time', true);         // Время выполнения запросов

// Настройка нагрузки
export let options = {
    stages: [
        { duration: '30s', target: 1 },    // 1 пользователь
        { duration: '30s', target: 10 },   // 10 пользователей
        { duration: '30s', target: 100 },  // 100 пользователей
        { duration: '30s', target: 1000 }, // 1000 пользователей
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% запросов должны завершиться быстрее 500мс
    },
};

// Тестовые данные
const BASE_URL = 'http://highload.homework.socialnetwork:8080';
const LOGIN_ENDPOINT = `${BASE_URL}/auth/login`;
const USER_GET_ENDPOINT = `${BASE_URL}/users/000074ac-8a22-450f-a836-d730cd8c2a00`;
const SEARCH_ENDPOINT = `${BASE_URL}/users/search`;

const LOGIN_CREDENTIALS = {
    email: "alice.johnson92@example.com",
    password: "SecurePass!2024"
};

// Авторизация пользователя и получение токена
function authenticateUser() {
    const payload = JSON.stringify(LOGIN_CREDENTIALS);
    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(LOGIN_ENDPOINT, payload, params);

    check(res, {
        'login successful': (r) => r.status === 200 && r.json('token') !== undefined,
    });

    if (res.status === 200) {
        return res.json('token'); // Возвращает токен
    } else {
        failedRequests.add(1); // Отмечаем неуспешный запрос
        return null;
    }
}

export default function () {
    // Получаем токен для текущего пользователя
    const token = authenticateUser();

    if (!token) {
        console.error('Token not received. Skipping requests.');
        return;
    }

    // Настройка заголовков с токеном
    const params = {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    };

    // Сначала запрос к /users/{id}
    let res = http.get(USER_GET_ENDPOINT, params);
    handleResponse(res);

    // Затем запрос к /users/search
    const queryParams = `?firstName=Al&lastName=Jo`;
    res = http.get(`${SEARCH_ENDPOINT}${queryParams}`, params);
    handleResponse(res);

    sleep(1); // Задержка между итерациями
}

// Функция для обработки ответа и метрик
function handleResponse(res) {
    // Сохраняем время выполнения запроса
    responseTime.add(res.timings.duration);

    // Проверка успешности ответа
    if (res.status >= 200 && res.status < 300) {
        successfulRequests.add(1); // Запросы с кодом 2xx
    } else {
        failedRequests.add(1); // Все остальные запросы
    }

    // Проверка, если время ответа больше 500 мс
    if (res.timings.duration > 500) {
        slowRequests.add(1);
    }
}
