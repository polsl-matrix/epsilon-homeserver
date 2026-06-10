CREATE TABLE users
(
    localpart     TEXT PRIMARY KEY,
    password_hash TEXT,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE devices
(
    user_localpart TEXT        NOT NULL REFERENCES users (localpart) ON DELETE CASCADE,
    device_id      TEXT        NOT NULL,
    display_name   TEXT,
    access_token   TEXT        NOT NULL UNIQUE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_localpart, device_id)
);
