CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL,
    "DateOfBirth" DATE NOT NULL,
    "Gender" TEXT NOT NULL,
    "Interests" TEXT,
    "City" TEXT,
    "Email" TEXT NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL
);

-- Убедимся, что расширение для трёхграммных индексов включено
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Создаём GIN-индекс для ускорения поиска по подстроке в имени и фамилии
CREATE INDEX IF NOT EXISTS "users_name_trgm_idx" ON "Users"
USING GIN (( "FirstName" || ' ' || "LastName" ) gin_trgm_ops);