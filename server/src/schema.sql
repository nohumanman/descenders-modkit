CREATE TABLE IF NOT EXISTS "User" (
        "discord_id"    INTEGER UNIQUE,
        "valid" TEXT,
        "steam_id"      TEXT,
        "discord_name"  TEXT,
        "email" TEXT,
        PRIMARY KEY("discord_id")
);
CREATE TABLE IF NOT EXISTS "TrailMedal" (
        "trail_name"    INTEGER,
        "medal_type"    TEXT,
        "time"  REAL
);
CREATE TABLE IF NOT EXISTS "TrailIcon" (
        "trail_name"    TEXT UNIQUE,
        "trail_icon"    TEXT
);
CREATE TABLE IF NOT EXISTS "WorldInfo" (
        "world_name"    TEXT,
        "start_bike"    TEXT,
        "whitelist_id"  INTEGER,
        "blacklist_id"  INTEGER
);
CREATE TABLE IF NOT EXISTS "TrailStartTime" (
        "trail_name"    TEXT UNIQUE,
        "max_starting_time"     REAL
);
CREATE TABLE Player
(
        "steam_id"      TEXT UNIQUE,
        "steam_name"    TEXT,
        "avatar_src"    TEXT,
        PRIMARY KEY("steam_id"),
        CHECK(length(steam_id) == length("76561198282799591"))
);
CREATE VIEW "TotalTime" AS SELECT Session.steam_id, Player.steam_name, sum(time_ended - time_started) AS total_time FROM Session
INNER JOIN Player ON Player.steam_id = Session.steam_id
GROUP BY Session.steam_id
ORDER BY total_time DESC;
CREATE TABLE IF NOT EXISTS "Time" (
        "steam_id"      TEXT,
        "time_id"       INTEGER NOT NULL UNIQUE,
        "timestamp"     REAL,
        "world_name"    TEXT,
        "trail_name"    TEXT,
        --"was_monitored"       TEXT,
        "bike_type"     TEXT,

        "starting_speed"        REAL,
        "version"       TEXT,
        --"penalty"     REAL,
        "verified"      INT NOT NULL,
        "ignored"       INT NOT NULL,
        --"verified_tmp"        int,
        PRIMARY KEY("time_id" AUTOINCREMENT),
        CONSTRAINT ignored CHECK(ignored == 0 or ignored == 1),
        CONSTRAINT verified CHECK(verified == 0 or verified == 1)
);
CREATE TABLE IF NOT EXISTS "SplitTime" (
        "time_id"       INTEGER NOT NULL REFERENCES Time(time_id) ON DELETE CASCADE,
        "checkpoint_num"        INTEGER,
        "checkpoint_time"       INTEGER
);
CREATE INDEX idx_time_filter ON Time(trail_name, ignored, verified, steam_id);
CREATE INDEX idx_splittime_id_num ON SplitTime(time_id, checkpoint_num);
CREATE INDEX idx_player_steam_id ON Player(steam_id);
CREATE VIEW all_times AS
SELECT
        Time.steam_id,
        Player.steam_name,
        Player.avatar_src,
        Time.timestamp,
        Time.time_id,
        SplitTime.checkpoint_num as total_checkpoints,
        SplitTime.checkpoint_time as total_time,
        Time.trail_name,
        Time.world_name,
        Time.ignored,
        Time.bike_type,
        Time.starting_speed,
        Time.version,
        max(SplitTime.checkpoint_num),
        Time.verified
FROM Time
LEFT JOIN Player
        ON Player.steam_id = Time.steam_id
INNER JOIN SplitTime
        ON SplitTime.time_id = Time.time_id

GROUP BY Time.time_id
ORDER BY Time.timestamp DESC
/* all_times(steam_id,steam_name,avatar_src,timestamp,time_id,total_checkpoints,total_time,trail_name,world_name,ignored,bike_type,starting_speed,version,"max(SplitTime.checkpoint_num)",verified) */;
CREATE VIEW "TrailInfo" AS SELECT
        Time.trail_name,
        Time.world_name,
        COUNT(CASE WHEN Time.ignored = 0 AND Time.verified = 1 THEN Time.trail_name END) as times_ridden,
        AVG(Time.starting_speed) as average_start_speed,
        TrailIcon.trail_icon
FROM Time
LEFT JOIN TrailIcon ON TrailIcon.trail_name = Time.trail_name
WHERE Time.ignored = 0 AND Time.verified = 1
GROUP BY Time.trail_name
ORDER BY Time.trail_name DESC;
/* TrailInfo(trail_name,world_name,times_ridden,average_start_speed,trail_icon) */;