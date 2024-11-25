import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

// Метрика для подсчета ошибок
export let errorCount = new Counter('errors');

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
const BASE_URL = 'http://your-api-url.com/users/search';
const TEST_FIRST_NAME = 'John';
const TEST_LAST_NAME = 'Doe';

export default function () {
    let params = {
        headers: {
            Authorization: 'Bearer your-access-token', // Замените на действительный токен
        },
    };

    // Отправка запроса
    let res = http.get(`${BASE_URL}?firstName=${TEST_FIRST_NAME}&lastName=${TEST_LAST_NAME}`, params);

    // Проверка успешности ответа
    let success = check(res, {
        'status is 200': (r) => r.status === 200,
        'response time is less than 500ms': (r) => r.timings.duration < 500,
    });

    // Увеличение счётчика ошибок при неудаче
    if (!success) {
        errorCount.add(1);
    }

    sleep(1); // Задержка между запросами
}
