import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

export let successfulRequests = new Counter('successful_requests'); // Успешные запросы (статус 2xx)
export let failedRequests = new Counter('failed_requests');         // Неуспешные запросы (все, кроме 2xx)
export let slowRequests = new Counter('slow_requests');             // Запросы, превышающие 500 мс
export let responseTime = new Trend('response_time', true);         // Время выполнения запросов

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
const GENERATE_USERS_ENDPOINT = `${BASE_URL}/auth/generate-users`;

// Допустим мы хотим создать 10 пользователей за один запрос
const COUNT = 10;

export default function () {
    // Отправляем запрос на генерацию пользователей
    const url = `${GENERATE_USERS_ENDPOINT}?count=${COUNT}`;
    const res = http.post(url, null); // POST запрос без тела, только query параметр

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

    check(res, {
        'status is 200-299': (r) => r.status >= 200 && r.status < 300,
    });
}
