# Как работает решардинг

- DialogService всегда подключается к **координатору**.
- Если вы добавите новые worker-ноды в кластер, вы можете выполнить команду:

  ```sql
  SELECT master_add_node('citus-worker3', 5432);
  ```

- Чтобы **перераспределить** части таблицы `messages` на новые воркеры, можно сделать:

  ```sql
  SELECT rebalance_table_shards('messages');
  ```

- Это запускает **онлайн-миграцию** шардов внутри Citus без изменения вашего приложения. Пользователи продолжают работать: ваши `INSERT`/`SELECT` запросы просто приходят на координатор, а Citus выполняет их на «старых» или «новых» шардах, постепенно перенося данные.