CREATE SCHEMA IF NOT EXISTS auth AUTHORIZATION pg_database_owner;

CREATE TABLE IF NOT EXISTS auth.sessions
(
    session_id                 UUID DEFAULT (uuid_generate_v4()),

    user_id                    UUID  NOT NULL,

    current_auth_token_hash    BYTEA NOT NULL,
    current_refresh_token_hash BYTEA NOT NULL,

    PRIMARY KEY (session_id),
    FOREIGN KEY (user_id) REFERENCES identity.users (user_id) ON DELETE CASCADE
);
