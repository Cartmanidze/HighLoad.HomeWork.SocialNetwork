-- Создадим таблицу для постов
CREATE TABLE IF NOT EXISTS Posts (
  Id UUID PRIMARY KEY,
  AuthorId UUID NOT NULL,
  Content TEXT NOT NULL,
  CreatedAt TIMESTAMP NOT NULL,
  UpdatedAt TIMESTAMP NOT NULL
);

-- Создадим таблицу для дружбы
CREATE TABLE IF NOT EXISTS Friendships (
  Id UUID PRIMARY KEY,
  UserId UUID NOT NULL,
  FriendId UUID NOT NULL,
  CreatedAt TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX ux_friendship_userid_friendid ON Friendships(UserId, FriendId);

