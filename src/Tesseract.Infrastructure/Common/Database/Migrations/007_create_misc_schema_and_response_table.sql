CREATE SCHEMA IF NOT EXISTS misc AUTHORIZATION pg_database_owner;

CREATE TABLE IF NOT EXISTS misc.responses
(
    user_id      UUID  NOT NULL REFERENCES identity.users,
    path_hash    BYTEA NOT NULL,

    status_code  INT   NOT NULL,
    content_type VARCHAR(64),
    content      TEXT  NOT NULL,

    PRIMARY KEY (user_id, path_hash)
);
