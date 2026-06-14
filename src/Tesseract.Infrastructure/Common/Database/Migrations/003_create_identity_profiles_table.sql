CREATE TABLE IF NOT EXISTS identity.profiles
(
    user_id      UUID PRIMARY KEY,
    display_name VARCHAR(255) NULL,
    avatar_url   TEXT         NULL,

    FOREIGN KEY (user_id) REFERENCES identity.users (user_id) ON DELETE CASCADE
);
