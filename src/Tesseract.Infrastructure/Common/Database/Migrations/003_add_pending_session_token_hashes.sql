ALTER TABLE auth.sessions
    ADD COLUMN IF NOT EXISTS pending_access_token_hash BYTEA NULL,
    ADD COLUMN IF NOT EXISTS pending_refresh_token_hash BYTEA NULL;
