-- Создание таблицы счетчиков, если её ещё нет
CREATE TABLE IF NOT EXISTS counters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(50) NOT NULL,
    count INT NOT NULL DEFAULT 0,
    last_updated TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (user_id, type)
);

-- Создание индекса для быстрого поиска по user_id
CREATE INDEX IF NOT EXISTS idx_counters_user_id ON counters(user_id);

-- Создание индекса для быстрого поиска по типу счетчика
CREATE INDEX IF NOT EXISTS idx_counters_type ON counters(type); 