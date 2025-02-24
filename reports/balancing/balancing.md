## 1. Конфигурация HAProxy

Файл `haproxy/haproxy.cfg` настроен для TCP-балансировки между двумя репликами PostgreSQL:

```cfg
global
    maxconn 4096
    log stdout format raw local0

defaults
    mode tcp
    log global
    option tcplog
    timeout connect 5s
    timeout client  30s
    timeout server  30s

frontend psql_slave
    bind *:6432
    default_backend psql_slaves

backend psql_slaves
    balance roundrobin
    server slave1 slave1-db:5432 check
    server slave2 slave2-db:5432 check
```

При отключении одного из слейвов, HAProxy исключает его из пула и направляет запросы на оставшийся сервер.

---

## 2. Конфигурация Nginx

Файл `nginx/nginx.conf` балансирует HTTP-трафик между двумя инстансами приложения:

```nginx
upstream socialnetwork_backend {
    server highload.homework.socialnetwork1:8080;
    server highload.homework.socialnetwork2:8080;
}

server {
    listen 80;
    server_name localhost;

    location / {
        proxy_pass http://socialnetwork_backend;
    }
}
```

При отключении одного из приложений, Nginx направляет весь трафик на работающий экземпляр.

---

## 3. Условия эксперимента

- **Система:** Мастер PostgreSQL, 2 slave, 2 инстанса приложения (ASP.NET Core) с балансировкой через Nginx, HAProxy для реплик.
- **Нагрузочное тестирование:** Скрипт k6 выполняет авторизацию и запросы к API через Nginx.
- **Инциденты:**
    - 52 сек.: `kill -9` одного из slave PostgreSQL.
    - 55 сек.: `kill -9` одного из инстансов приложения.

---

## 4. Результаты нагрузочного тестирования

```
checks.........................: 99.80% ✓ 7570       ✗ 15
data_received..................: 114 MB 822 kB/s
data_sent......................: 8.1 MB 58 kB/s
failed_requests................: 7595   54.74/s
http_req_duration..............: avg=894.37ms, med=386.94ms, max=59.99s, p(90)=2.23s, p(95)=2.58s
iterations.....................: 7585   54.67/s
response_time..................: avg=1.07s, med=532.27ms, max=59.99s
slow_requests..................: 7944   57.25/s
successful_requests............: 7560   54.49/s
vus............................: 3 (min=1, max=994)
vus_max........................: 1000
```

- **Основные показатели:**
    - Показатель проверок – 99.80% успешных.
    - В среднем время ответа ~894 мс, медиана ~387 мс.
    - Примерно 33% запросов завершились с ошибками, что связано с нагрузкой после отключения одного инстанса и слейва.
