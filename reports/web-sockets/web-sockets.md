# Инструкция по работе с WebSocket в PostService

Ниже описан краткий пошаговый сценарий работы с сервисом, где при создании поста подписчики мгновенно получают уведомление через WebSocket.

## 1. Подготовка

1. Убедитесь, что сервис запущен и доступен по адресу `http://localhost:8090` (или другой, в зависимости от настроек).
2. У вас должен быть **Bearer JWT-токен**, содержащий `sub` (или другой claim) с `UserId`.

## 2. Подключение по WebSocket

1. **Эндпоинт** для вебсокета:  
   ```
ws://localhost:8090/post/feed/posted
   ```
   (Если вы используете HTTPS, замените на `wss://`.)
2. **Авторизация**:
   - В хендшейке WebSocket передаётся заголовок `Authorization: Bearer <ваш_токен>`.
   - Например, в Postman (v10+):
     1. Создайте «New WebSocket Request».
     2. Введите URL `ws://localhost:8090/post/feed/posted`.
     3. Перейдите во вкладку «Headers» (или «Connection Configuration»).
     4. Добавьте заголовок:
        - **Key**: `Authorization`
        - **Value**: `Bearer <ваш_JWT_токен>`
     5. Нажмите «Connect».
   - При успешном подключении сервер сопоставит `UserId` из токена и будет удерживать WebSocket-соединение для последующих уведомлений.

## 3. Создание поста (REST API)

1. Откройте любой HTTP-клиент (Postman, curl).
2. Отправьте POST-запрос:
   ```
POST http://localhost:8090/posts
Authorization: Bearer <тот_же_или_другой_JWT>
Content-Type: application/json

{
"authorId": "22222222-2222-2222-2222-222222222222",
"content": "Hello from user 2222"
}
   ```
3. При успехе API вернёт 200/OK с информацией о созданном посте.

## 4. Получение уведомления по WebSocket

- Если у **друзей** автора (`authorId`) есть активное WebSocket-подключение, они **сразу** получат JSON-уведомление вида:
  ```json
  {
    "type": "PostCreated",
    "postId": "<GUID>",
    "authorId": "22222222-2222-2222-2222-222222222222",
    "content": "Hello from user 2222",
    "createdAt": "2025-01-29T12:00:00Z"
  }
  ```
- Уведомление приходит автоматически в WebSocket-клиент, без дополнительных запросов.