-- Включаем расширение для генерации UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL,
    "DateOfBirth" DATE NOT NULL,
    "Gender" TEXT NOT NULL,
    "Interests" TEXT,
    "City" TEXT,
    "Email" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL
);

-- Включаем расширение для трёхграммных индексов (если не было подключено ранее)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Создаём GIN-индекс для ускорения поиска по подстроке в имени и фамилии
CREATE INDEX IF NOT EXISTS "users_name_trgm_idx" ON "Users"
USING GIN (( "FirstName" || ' ' || "LastName" ) gin_trgm_ops);
