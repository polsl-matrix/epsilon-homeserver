CREATE TABLE IF NOT EXISTS auth.devices
(
    user_id      UUID         NOT NULL,
    device_id    VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NULL,

    PRIMARY KEY (user_id, device_id),
    FOREIGN KEY (user_id) REFERENCES identity.users (user_id) ON DELETE CASCADE
);

ALTER TABLE auth.sessions
    ADD COLUMN IF NOT EXISTS device_id VARCHAR(255) NULL;

ALTER TABLE auth.sessions
    ADD CONSTRAINT fk_sessions_devices
        FOREIGN KEY (user_id, device_id)
            REFERENCES auth.devices (user_id, device_id)
            ON DELETE CASCADE;
