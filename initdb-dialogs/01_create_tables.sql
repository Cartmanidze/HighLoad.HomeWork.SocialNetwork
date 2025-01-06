-- Создаём таблицу messages (обычную, пока не распределённую)
CREATE TABLE IF NOT EXISTS messages (
  id uuid PRIMARY KEY,
  sender_id uuid NOT NULL,
  receiver_id uuid NOT NULL,
  text text NOT NULL,
  created_at timestamptz NOT NULL
);

-- Превращаем её в распределённую (Citus):
SELECT create_distributed_table('messages', 'sender_id', 'hash');