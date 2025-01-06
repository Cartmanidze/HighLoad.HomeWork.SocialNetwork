-- 1. Подключаем/создаём расширение Citus
CREATE EXTENSION IF NOT EXISTS citus;

-- 2. Создаём таблицу с учётом того,
--    что PRIMARY KEY должен включать колонку шардирования (sender_id).
CREATE TABLE IF NOT EXISTS messages (
    sender_id   UUID NOT NULL,
    id          UUID NOT NULL,
    receiver_id UUID NOT NULL,
    text        TEXT NOT NULL,
    created_at  TIMESTAMP NOT NULL,
    PRIMARY KEY (sender_id, id)
);

-- 3. Создаём неуникальный индекс для быстрых запросов
--    по сочетанию sender_id + receiver_id:
CREATE INDEX IF NOT EXISTS idx_messages_sender_receiver
    ON messages (sender_id, receiver_id);

-- 4. Подключаем worker-ноды
SELECT master_add_node('citus-worker1', 5432);
SELECT master_add_node('citus-worker2', 5432);

-- 5. Наконец, делаем таблицу распределённой:
SELECT create_distributed_table('messages', 'sender_id', 'hash');
