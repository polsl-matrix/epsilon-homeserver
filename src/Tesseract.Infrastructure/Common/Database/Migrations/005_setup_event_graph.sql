CREATE TABLE IF NOT EXISTS chat.room_events
(
    event_id  UUID      NOT NULL DEFAULT (uuid_generate_v4()) PRIMARY KEY,
    room_id   UUID      NOT NULL REFERENCES chat.rooms (room_id),
    timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    payload   TEXT      NOT NULL
);
