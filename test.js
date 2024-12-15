import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// Метрики
export let successfulRequests = new Counter('successful_requests'); // Успешные запросы (статус 2xx)
export let failedRequests = new Counter('failed_requests'); // Неуспешные запросы (все, кроме 2xx)
export let slowRequests = new Counter('slow_requests'); // Запросы, превышающие 500 мс
export let responseTime = new Trend('response_time', true); // Время выполнения запросов

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
const BASE_URL = 'http://localhost:5027';
const LOGIN_ENDPOINT = `${BASE_URL}/auth/login`;
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
        console.error('Token not received. Skipping the request to /users/search.');
        return;
    }

    // Настройка заголовков с токеном
    const params = {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    };

    // Параметры поиска
    const queryParams = `?firstName=Al&lastName=Jo`;

    // Отправка запроса на поиск пользователей
    const res = http.get(`${SEARCH_ENDPOINT}${queryParams}`, params);

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

    sleep(1); // Задержка между запросами
}

// Генерация HTML-отчета
export function handleSummary(data) {
    return {
        "summary.html": htmlReport(data), // Генерация файла summary.html
    };
}
