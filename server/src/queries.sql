-- name: get_pb_split_times
-- Get fastest split times for a given player on a given trail
SELECT
    SplitTime.checkpoint_num,
    MIN(SplitTime.checkpoint_time) AS min_checkpoint_time
FROM
    SplitTime
INNER JOIN Time ON SplitTime.time_id = Time.time_id
INNER JOIN Player ON Time.steam_id = Player.steam_id
WHERE
    Time.trail_name = :trail_name
    AND Time.ignored = 0
    AND Time.verified = 1
    AND SplitTime.time_id = (
        SELECT SplitTime.time_id
        FROM SplitTime
        INNER JOIN Time ON SplitTime.time_id = Time.time_id
        INNER JOIN Player ON Time.steam_id = Player.steam_id
        WHERE
            Time.trail_name = :trail_name
            AND Time.ignored = 0
            AND Time.verified = 1
            AND Time.steam_id = :steam_id
        ORDER BY
            SplitTime.checkpoint_num DESC, SplitTime.checkpoint_time ASC
        LIMIT 1
    )
GROUP BY
    SplitTime.checkpoint_num
ORDER BY
    SplitTime.checkpoint_num ASC;

-- name: get_wr_split_times
-- Get fastest split times for a given trail
SELECT
    SplitTime.checkpoint_num,
    MIN(SplitTime.checkpoint_time) AS min_checkpoint_time
FROM
    SplitTime
INNER JOIN Time ON SplitTime.time_id = Time.time_id
INNER JOIN Player ON Time.steam_id = Player.steam_id
WHERE
    Time.trail_name = :trail_name
    AND Time.ignored = 0
    AND Time.verified = 1
    AND SplitTime.time_id = (
        SELECT SplitTime.time_id
        FROM SplitTime
        INNER JOIN Time ON SplitTime.time_id = Time.time_id
        INNER JOIN Player ON Time.steam_id = Player.steam_id
        WHERE
            Time.trail_name = :trail_name
            AND Time.ignored = 0
            AND Time.verified = 1
        ORDER BY
            SplitTime.checkpoint_num DESC, SplitTime.checkpoint_time ASC
        LIMIT 1
    )
GROUP BY
    SplitTime.checkpoint_num
ORDER BY
    SplitTime.checkpoint_num ASC;

-- name: update_player!
-- Update the player's name and avatar
REPLACE INTO Player (
    steam_id,
    steam_name,
    avatar_src
)
VALUES (
    :steam_id, :steam_name, :avatar_src
);

-- name: get_replay_name^
-- Get the replay name associated with a given time.
SELECT (
    Time.time_id || '_' || Player.steam_name || '_' || Time.world_name
    || '_' || MAX(SplitTime.checkpoint_time)
) AS value
FROM Time
INNER JOIN Player ON Time.steam_id = Player.steam_id
INNER JOIN SplitTime ON Time.time_id = SplitTime.time_id
WHERE Time.time_id = :time_id;

-- name: player_id_from_name^
-- Get the player id from the player name.
SELECT Player.steam_id AS value
FROM Player
WHERE Player.steam_name = :steam_name;

-- name: player_name_from_id^
-- Get the 
SELECT steam_name AS value
FROM Player
WHERE steam_id = :steam_id;


-- name: get_authenticated_discord_ids
-- Get the valid Discord IDs.
SELECT discord_id
FROM
    User
WHERE
    valid = 1;

-- name: get_discord_steam_connetion^
-- get steam ID associated with given discord id
SELECT steam_id AS value
FROM User
WHERE discord_id = :discord_id;

-- name: get_time_details^
-- Get the time details for a given time id.
SELECT *
FROM
    all_times
WHERE
    time_id = :time_id;

-- name: get_all_times
-- Get all times for a given trail
SELECT *
FROM all_times
LIMIT :lim;

-- name: get_all_players
-- Get all players
SELECT
    Player.steam_id,
    Player.steam_name,
    Player.avatar_src
FROM Player
GROUP BY Player.steam_id
ORDER BY Player.steam_name ASC;

-- name: get_all_trails
-- Get all trails
SELECT * FROM TrailInfo;

-- name: get_all_worlds
-- Get all worlds
SELECT world_name
FROM Time
GROUP BY world_name;

-- name: get_leaderboard
-- Get the leaderboard for a given trail
SELECT
    Time.starting_speed,
    Player.steam_name,
    Time.bike_type,
    Time.version,
    Time.verified,
    Time.time_id,
    MIN(SplitTime.checkpoint_time) AS min_checkpoint_time
FROM
    Time
INNER JOIN
    SplitTime ON SplitTime.time_id = Time.time_id
INNER JOIN
    (
        SELECT MAX(SplitTime.checkpoint_num) AS max_checkpoint
        FROM
            SplitTime
        INNER JOIN
            Time ON SplitTime.time_id = Time.time_id
        WHERE LOWER(Time.trail_name) = LOWER(
            :trail_name
        )
        AND Time.verified = 1
        AND Time.ignored = 0
    ) AS max_checkpoint_table
    ON SplitTime.time_id = Time.time_id
INNER JOIN Player
    ON Player.steam_id = Time.steam_id
WHERE
    LOWER(Time.trail_name) = LOWER(:trail_name)
    AND
    SplitTime.checkpoint_num = max_checkpoint_table.max_checkpoint
    AND
    ((Time.ignored = 0 AND Time.verified = 1) OR :require_spectated)
    AND (
        Time.spectated_by = :spectated_by
        OR NOT :require_spectated
    )
    AND Time.timestamp >= :timestamp
GROUP BY
    Time.trail_name,
    Player.steam_id
ORDER BY
    SplitTime.checkpoint_time ASC
LIMIT :lim;

-- name: get_player_avatar^
-- Get the player's avatar
SELECT avatar_src
FROM Player
WHERE steam_id = :steam_id;

-- name: submit_discord_details!
-- Submit the discord details
INSERT OR REPLACE INTO User (discord_id, valid, steam_id, discord_name, email)
VALUES (
    :discord_id,
    COALESCE((SELECT valid FROM User WHERE discord_id = :discord_id), FALSE),
    :steam_id,
    :discord_name,
    :email
);

-- name: verify_time!
-- Verify a time
UPDATE Time
SET verified = NOT verified
WHERE time_id = :time_id;

-- name: submit_time!
-- Submit a time
INSERT INTO Time (
    steam_id, -- TEXT
    time_id, -- INTEGER NOT NULL UNIQUE
    timestamp, -- REAL
    world_name, -- TEXT
    trail_name, -- TEXT
    bike_type, -- TEXT
    starting_speed, -- REAL
    version, -- TEXT
    verified, -- INT NOT NULL
    ignored, -- INT NOT NULL
    spectated_by -- TEXT
)
VALUES (
    :steam_id, -- TEXT
    :time_id, -- INTEGER NOT NULL UNIQUE
    :timestamp, -- REAL
    :world_name, -- TEXT
    :trail_name, -- TEXT
    :bike_type, -- TEXT
    :starting_speed, -- REAL
    :version, -- TEXT
    :verified, -- INT NOT NULL
    :ignored, -- INT NOT NULL
    :spectated_by -- TEXT
);

-- name: submit_split!
-- Submit a split time
INSERT INTO
SplitTime (
    time_id,
    checkpoint_num,
    checkpoint_time
)
VALUES (
    :time_id,
    :checkpoint_num,
    :checkpoint_time
);

-- name: get_player_medals
-- Get the medals for a given trail
SELECT
    CASE WHEN MIN(SplitTime.checkpoint_time) <= (
        SELECT time FROM TrailMedal
        WHERE trail_name = LOWER(:trail_name) AND medal_type = 'rainbow'
    ) THEN TRUE ELSE FALSE END AS rainbow,
    CASE WHEN MIN(SplitTime.checkpoint_time) <= (
        SELECT time FROM TrailMedal
        WHERE trail_name = LOWER(:trail_name) AND medal_type = 'gold'
    ) THEN TRUE ELSE FALSE END AS gold,
    CASE WHEN MIN(SplitTime.checkpoint_time) <= (
        SELECT time FROM TrailMedal
        WHERE trail_name = LOWER(:trail_name) AND medal_type = 'silver'
    ) THEN TRUE ELSE FALSE END AS silver,
    CASE WHEN MIN(SplitTime.checkpoint_time) <= (
        SELECT time FROM TrailMedal
        WHERE trail_name = LOWER(:trail_name) AND medal_type = 'bronze'
    ) THEN TRUE ELSE FALSE END AS bronze
FROM
    Time
INNER JOIN SplitTime ON SplitTime.time_id = Time.time_id
INNER JOIN (
    SELECT MAX(SplitTime.checkpoint_num) AS max_checkpoint
    FROM SplitTime
    INNER JOIN Time ON SplitTime.time_id = Time.time_id
    WHERE LOWER(Time.trail_name) = LOWER(:trail_name)
) AS max_checkpoints ON SplitTime.time_id = Time.time_id
INNER JOIN Player ON Player.steam_id = Time.steam_id
WHERE
    LOWER(Time.trail_name) = LOWER(:trail_name)
    AND SplitTime.checkpoint_num = max_checkpoints.max_checkpoint
    AND (Time.ignored = 0 AND Time.verified = 1)
    AND Player.steam_id = :steam_id
GROUP BY
    Time.trail_name,
    Player.steam_id
ORDER BY
    MIN(SplitTime.checkpoint_time) ASC;




-- name: get_average_start_time^
-- Get the average start time for a given trail
SELECT AVG(starting_speed) AS value
FROM Time
WHERE
    trail_name = :trail_name
    AND ignored = 0
    AND verified = 1;

-- name: add_pending_item!
-- Add a pending item to gift to a user

INSERT INTO PendingItem
(steam_id, item_id)
VALUES (:steam_id, :item_id);

-- name: redeem_pending_item!
-- Sets redeem to true for an item

UPDATE PendingItem
SET time_redeemed = :current_timestamp
WHERE (
    steam_id = :steam_id
    AND item_id = :item_id
);

-- name: get_pending_items
-- Gets pending items for a given steam_id
SELECT item_id FROM PendingItem
WHERE (
    steam_id = :steam_id
    AND time_redeemed IS NULL
);
