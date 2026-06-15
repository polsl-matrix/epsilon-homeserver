ALTER TABLE auth.sessions
    ADD COLUMN IF NOT EXISTS pending_access_token_hash BYTEA NULL,
    ADD COLUMN IF NOT EXISTS pending_refresh_token_hash BYTEA NULL;

CREATE TABLE IF NOT EXISTS auth.session_pending_tokens
(
    pending_token_id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    session_id                 UUID        NOT NULL REFERENCES auth.sessions (session_id) ON DELETE CASCADE,
    pending_access_token_hash  BYTEA       NOT NULL UNIQUE,
    pending_refresh_token_hash BYTEA       NOT NULL UNIQUE,
    created_at                 TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_session_pending_tokens_session_id
    ON auth.session_pending_tokens (session_id);
