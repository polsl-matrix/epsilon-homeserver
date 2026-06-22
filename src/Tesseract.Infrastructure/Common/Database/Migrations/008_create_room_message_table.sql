CREATE TABLE IF NOT EXISTS chat.room_messages
(
    message_id UUID DEFAULT (uuid_generate_v4()) PRIMARY KEY,

    room_id    UUID NOT NULL REFERENCES chat.rooms (room_id),
    user_id    UUID NOT NULL REFERENCES identity.users (user_id),

    body       TEXT NOT NULL
);
