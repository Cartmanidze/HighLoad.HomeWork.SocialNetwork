-- Создаём таблицу messages (обычную, пока не распределённую)
CREATE TABLE IF NOT EXISTS messages (
  id UUID PRIMARY KEY,
  sender_id UUID NOT NULL,
  receiver_id UUID NOT NULL,
  text TEXT NOT NULL,
  created_at TIMESTAMP NOT NULL

);

CREATE EXTENSION IF NOT EXISTS Citus;

-- Подключаем worker1
SELECT master_add_node('citus-worker1', 5432);
-- Подключаем worker2
SELECT master_add_node('citus-worker2', 5432);


-- Превращаем её в распределённую (Citus):
SELECT create_distributed_table('messages', 'sender_id', 'hash');