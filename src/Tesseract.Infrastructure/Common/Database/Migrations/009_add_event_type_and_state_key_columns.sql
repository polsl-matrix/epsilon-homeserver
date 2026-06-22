ALTER TABLE chat.room_events
    ADD COLUMN IF NOT EXISTS event_type TEXT NOT NULL DEFAULT '';

ALTER TABLE chat.room_events
    ADD COLUMN IF NOT EXISTS state_key TEXT;

-- noinspection SqlWithoutWhere
UPDATE chat.room_events
SET event_type = payload::json ->> 'type',
    state_key  = payload::json ->> 'state_key';

ALTER TABLE chat.room_events
    ALTER COLUMN event_type DROP DEFAULT;

CREATE INDEX IF NOT EXISTS event_type_idx ON chat.room_events (event_type);
