CREATE EXTENSION IF NOT EXISTS pg_trgm;



CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;



-- Сотрудники, работающие с системой
CREATE TABLE users (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    username        TEXT            NOT NULL,
    password_hash   TEXT            NOT NULL,
    first_name      TEXT            NOT NULL,
    last_name       TEXT            NOT NULL,
    middle_name     TEXT            NULL,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT ck_users_username_not_blank CHECK (btrim(username) <> '') -- ??
);

CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMENT ON TABLE  users IS 'Сотрудники, оформляющие посещения';
COMMENT ON COLUMN users.password_hash IS 'BCrypt-хеш. Пароль в открытом виде не хранится';



-- Клиенты фитнес-центра
CREATE TABLE clients (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name      TEXT            NOT NULL,
    last_name       TEXT            NOT NULL,
    middle_name     TEXT            NULL,
    phone           VARCHAR(20)     NOT NULL,
    email           TEXT            NULL,
    status          TEXT            NOT NULL DEFAULT 'active',
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    CONSTRAINT ck_clients_status CHECK (status IN ('active', 'inactive', 'blocked')),
    CONSTRAINT ck_clients_names_not_blank CHECK (btrim(first_name) <> '' AND btrim(last_name) <> ''),
    CONSTRAINT ck_clients_phone_not_blank CHECK (btrim(phone) <> '')
);

CREATE TRIGGER trg_clients_updated_at
    BEFORE UPDATE ON clients
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX ix_clients_fullname_trgm
    ON clients USING gin (
        lower(last_name || ' ' || first_name || ' ' || coalesce(middle_name, ''))
        gin_trgm_ops
    );

CREATE INDEX ix_clients_phone_digits_trgm
    ON clients USING gin (
        regexp_replace(phone, '\D', '', 'g') gin_trgm_ops
    );

CREATE INDEX ix_clients_status ON clients (status);



-- Типы абонементов
CREATE TABLE ticket_types (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name            TEXT            NOT NULL,
    code            TEXT            NOT NULL,
    duration_days   INT             NULL,
    default_visits  INT             NULL,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    CONSTRAINT uq_ticket_types_code UNIQUE (code),
    CONSTRAINT ck_ticket_types_duration CHECK (duration_days IS NULL OR duration_days > 0),
    CONSTRAINT ck_ticket_types_visits   CHECK (default_visits IS NULL OR default_visits > 0)
);

CREATE TRIGGER trg_ticket_types_updated_at
    BEFORE UPDATE ON ticket_types
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

COMMENT ON COLUMN ticket_types.default_visits IS 'NULL = абонемент без ограничения по количеству посещений';



-- Абонементы
CREATE TABLE tickets (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id       UUID            NOT NULL,
    ticket_type_id  UUID            NOT NULL,
    date_start      DATE            NOT NULL,
    date_end        DATE            NOT NULL,
    visits_used     INT             NOT NULL DEFAULT 0,
    visits_limit    INT             NULL,
    is_cancelled    BOOLEAN         NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    CONSTRAINT fk_tickets_client 
        FOREIGN KEY (client_id) REFERENCES clients (id) ON DELETE RESTRICT,
    CONSTRAINT fk_tickets_type 
        FOREIGN KEY (ticket_type_id) REFERENCES ticket_types (id) ON DELETE RESTRICT,
    CONSTRAINT ck_tickets_period CHECK (date_end >= date_start),
    CONSTRAINT ck_tickets_limit  CHECK (visits_limit IS NULL OR visits_limit > 0),
    CONSTRAINT ck_tickets_used   CHECK (visits_used >= 0),
    CONSTRAINT ck_tickets_used_within_limit CHECK (visits_limit IS NULL OR visits_used <= visits_limit)
);

CREATE TRIGGER trg_tickets_updated_at
    BEFORE UPDATE ON tickets
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX ix_tickets_client_id  ON tickets (client_id);
CREATE INDEX ix_tickets_date_end   ON tickets (date_end);
CREATE INDEX ix_tickets_type_id    ON tickets (ticket_type_id);



-- Справочник тренеров
CREATE TABLE trainers (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name      TEXT            NOT NULL,
    last_name       TEXT            NOT NULL,
    middle_name     TEXT            NULL,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    CONSTRAINT ck_trainers_names_not_blank CHECK (btrim(first_name) <> '' AND btrim(last_name) <> '')
);

CREATE TRIGGER trg_trainers_updated_at
    BEFORE UPDATE ON trainers
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

CREATE INDEX ix_trainers_fullname_trgm
    ON trainers USING gin (
        lower(last_name || ' ' || first_name || ' ' || coalesce(middle_name, ''))
        gin_trgm_ops
    );



-- Посещения
CREATE TABLE visits (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    ticket_id       UUID            NOT NULL,
    visited_at      TIMESTAMPTZ     NOT NULL DEFAULT now(),
    trainer_id      UUID            NULL,
    user_id         UUID            NOT NULL,
    CONSTRAINT fk_visits_ticket
        FOREIGN KEY (ticket_id) REFERENCES tickets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_visits_trainer
        FOREIGN KEY (trainer_id) REFERENCES trainers (id) ON DELETE SET NULL,
    CONSTRAINT fk_visits_user
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
);

CREATE INDEX ix_visits_ticket_id  ON visits (ticket_id);
CREATE INDEX ix_visits_visited_at ON visits (visited_at DESC);
CREATE INDEX ix_visits_user_id    ON visits (user_id);



CREATE VIEW v_tickets AS
SELECT
    t.id,
    t.client_id,
    t.ticket_type_id,
    tt.code AS ticket_type_code,
    tt.name AS ticket_type_name,
    t.date_start,
    t.date_end,
    t.visits_limit,
    t.visits_used,
    CASE
        WHEN t.visits_limit IS NULL THEN NULL
        ELSE t.visits_limit - t.visits_used
    END AS visits_remaining,
    t.is_cancelled,
    CASE
        WHEN t.is_cancelled                         THEN 'cancelled'
        WHEN CURRENT_DATE < t.date_start            THEN 'pending'
        WHEN CURRENT_DATE > t.date_end              THEN 'expired'
        WHEN t.visits_limit IS NOT NULL
            AND t.visits_used >= t.visits_limit     THEN 'exhausted'
        ELSE 'active'
    END AS status,
    t.created_at
FROM tickets t
JOIN ticket_types tt ON tt.id = t.ticket_type_id;
COMMENT ON VIEW v_tickets IS 'Абонементы с вычисляемым статусом: cancelled | pending | expired | exhausted | active';




