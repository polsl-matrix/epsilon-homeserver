CREATE SCHEMA IF NOT EXISTS chat AUTHORIZATION pg_database_owner;

CREATE TABLE IF NOT EXISTS chat.rooms
(
    room_id   UUID DEFAULT (uuid_generate_v4()),

    -- @formatter:off
    localpart VARCHAR(255) NOT NULL CHECK (localpart ~ '^[0-9a-z\-.=_/+]{1,255}$'),
    domain    VARCHAR(255) NOT NULL CHECK (domain ~ '^(?:\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}|\[[0-9A-Fa-f:.]{2,45}\]|[0-9A-Za-z\-.]{1,255})(?::\d{1,5})?$'),
    -- @formatter:on

    PRIMARY KEY (room_id),
    UNIQUE (localpart, domain)
);

