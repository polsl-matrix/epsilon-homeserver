CREATE TABLE IF NOT EXISTS chat.room_memberships
(
    room_id UUID NOT NULL REFERENCES chat.rooms (room_id),
    user_id UUID NOT NULL REFERENCES identity.users (user_id),

    PRIMARY KEY (room_id, user_id)
);
