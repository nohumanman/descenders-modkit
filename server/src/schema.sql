CREATE TABLE IF NOT EXISTS User (
    discord_id INTEGER UNIQUE,
    valid INTEGER,
    steam_id TEXT,
    discord_name TEXT,
    email TEXT,
    PRIMARY KEY (discord_id)
);
CREATE TABLE IF NOT EXISTS TrailMedal (
    trail_name INTEGER,
    medal_type TEXT,
    time REAL
);
CREATE TABLE IF NOT EXISTS TrailIcon (
    trail_name TEXT UNIQUE,
    trail_icon TEXT
);
CREATE TABLE IF NOT EXISTS WorldInfo (
    world_name TEXT,
    start_bike TEXT,
    whitelist_id INTEGER,
    blacklist_id INTEGER
);
CREATE TABLE IF NOT EXISTS TrailStartTime (
    trail_name TEXT UNIQUE,
    max_starting_time REAL
);
CREATE TABLE IF NOT EXISTS Player (
    steam_id TEXT UNIQUE CHECK (LENGTH(steam_id = 17)),
    steam_name TEXT,
    avatar_src TEXT,
    PRIMARY KEY (steam_id)
);
CREATE TABLE IF NOT EXISTS SplitTime (
    time_id INTEGER NOT NULL REFERENCES Time (time_id) ON DELETE CASCADE,
    checkpoint_num INTEGER,
    checkpoint_time INTEGER
);
CREATE INDEX idx_time_filter ON Time (trail_name, ignored, verified, steam_id);
CREATE INDEX idx_splittime_id_num ON SplitTime (time_id, checkpoint_num);
CREATE INDEX idx_player_steam_id ON Player (steam_id);

CREATE VIEW all_times AS
SELECT
    Time.steam_id,
    Player.steam_name,
    Player.avatar_src,
    Time.timestamp,
    Time.time_id,
    SplitTime.checkpoint_num AS total_checkpoints,
    SplitTime.checkpoint_time AS total_time,
    Time.trail_name,
    Time.world_name,
    Time.ignored,
    Time.bike_type,
    Time.starting_speed,
    Time.version,
    Time.verified,
    MAX(SplitTime.checkpoint_num) AS MaxCheckpointNumber
FROM Time
LEFT JOIN Player
    ON Time.steam_id = Player.steam_id
INNER JOIN SplitTime
    ON Time.time_id = SplitTime.time_id
GROUP BY Time.time_id
ORDER BY Time.timestamp DESC;

CREATE VIEW TrailInfo AS SELECT
    Time.trail_name,
    Time.world_name,
    TrailIcon.trail_icon,
    COUNT(
        CASE
            WHEN Time.ignored = 0 AND Time.verified = 1 THEN Time.trail_name
        END
    ) AS times_ridden,
    AVG(Time.starting_speed) AS average_start_speed
FROM Time
LEFT JOIN TrailIcon ON Time.trail_name = TrailIcon.trail_name
WHERE Time.ignored = 0 AND Time.verified = 1
GROUP BY Time.trail_name
ORDER BY Time.trail_name DESC;

CREATE TABLE IF NOT EXISTS PendingItem (
    steam_id TEXT,
    item_id TEXT,
    time_redeemed INTEGER -- NULL when not redeemed, timestamp otherwise
);
