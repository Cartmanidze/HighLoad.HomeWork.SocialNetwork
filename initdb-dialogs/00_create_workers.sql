CREATE EXTENSION IF NOT EXISTS Citus;

-- Подключаем worker1
SELECT master_add_node('citus-worker1', 5432);
-- Подключаем worker2
SELECT master_add_node('citus-worker2', 5432);