""" Database Management System """
import time
import os
from datetime import datetime, timedelta
import logging
import aiosqlite

script_path = os.path.dirname(os.path.realpath(__file__))


def daterange(start_date: datetime, end_date: datetime):
    """ Generate a range of dates between the given start_date and end_date """
    for n in range(int((end_date - start_date).days)):
        yield start_date + timedelta(n)

class DBMS():
    """ A simple Database Management System (DBMS) class for managing data. """
    def __init__(self, db_file: str = "modkit.db"):
        self.db_file = script_path + f"/{db_file}"
        self.wait = False

    async def execute_sql(self, statement: str, write=False):
        """ Execute an SQL statement on a database """
        async with aiosqlite.connect(self.db_file) as db:
            async with db.execute(statement) as cur:
                if write:
                    await db.commit()
                rows = await cur.fetchall()
                return [list(row) for row in rows]

    async def get_id_from_name(self, steam_name):
        """ Get the Steam ID associated with a given Steam username. """
        return (await self.execute_sql(
            f'''
            SELECT Player.steam_id
            FROM Player
            WHERE Player.steam_name = "{steam_name}"'''
        ))[0][0]

    async def get_player_stats(self, steam_id):
        """ async def get_player_stats(self, steam_id): """
        statement = f'''
            SELECT
                Player.steam_id,
                Player.steam_name,
                Rep.rep,
                Max(Rep.timestamp) as last_login,
                trails_ridden,
                Player.avatar_src
            FROM
                Player,
                (
                    SELECT COUNT(*) as trails_ridden
                    FROM Time
                    WHERE Time.steam_id = {steam_id}
                )
            INNER JOIN Rep ON Rep.steam_id = Player.steam_id
            INNER JOIN Time ON Time.steam_id = Player.steam_id
            WHERE Player.steam_id = "{steam_id}"
            GROUP BY Rep.steam_id
        '''
        return self.execute_sql(statement)

    async def get_webhooks(self, trail_name):
        """ Get the webhooks associated with a given trail. """
        return self.execute_sql(
            "SELECT webhook_url FROM webhooks"
            f" WHERE trail_name = '{trail_name}'"
        )

    async def update_player(self, steam_id, steam_name, avatar_src):
        """ Update the player's name and avatar. """
        await self.execute_sql(
            f'''
            REPLACE INTO Player (
                steam_id,
                steam_name,
                avatar_src,
                ignore_times,
                ban_type
            )
            VALUES (
                "{steam_id}", "{steam_name}", "{avatar_src}",
                (
                    select ignore_times FROM Player
                    WHERE steam_id = "{steam_id}"
                ),
                (
                    select ban_type from Player
                    WHERE steam_id = "{steam_id}"
                )
            )
            ''', write=True
        )

    async def get_personal_fastest_split_times(self, trail_name: str, steam_id: int):
        """ Get the fastest split times for a given player on a given trail. """
        statement = f'''
            SELECT
                SplitTime.time_id,
                SplitTime.checkpoint_num,
                SplitTime.checkpoint_time,
                Time.trail_name,
                Player.steam_name,
                Time.timestamp
            from
                SplitTime
                INNER JOIN Time ON SplitTime.time_id = Time.time_id
                INNER JOIN Player ON Time.steam_id = Player.steam_id
            WHERE trail_name = "{trail_name}"
            AND (Time.ignore = "False" OR Time.ignore is NULL)
            AND Time.steam_id = {steam_id}
            ORDER BY checkpoint_num DESC, checkpoint_time ASC
            LIMIT 1
            '''
        time_id = await self.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            return []
        times = await self.execute_sql(
            f'''
            SELECT
                *
            FROM
                SplitTime
            WHERE
                time_id = "{fastest_time_on_trail_id}"
            ORDER BY
                checkpoint_num
            '''
        )
        split_times = [float(time[2]) for time in times]
        return split_times

    async def get_fastest_split_times(self,
        trail_name
    ):
        """ Get the fastest split times for a given trail. """
        statement = f'''
            SELECT
                SplitTime.time_id,
                SplitTime.checkpoint_num,
                SplitTime.checkpoint_time,
                Time.trail_name,
                Player.steam_name,
                Time.timestamp
            from
                SplitTime
                INNER JOIN Time ON SplitTime.time_id = Time.time_id
                INNER JOIN Player ON Time.steam_id = Player.steam_id
            WHERE trail_name = "{trail_name}"
            AND (Time.ignore = "False" OR Time.ignore is NULL)
            ORDER BY checkpoint_num DESC, checkpoint_time ASC
            LIMIT 1
            '''
        time_id = await self.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            return []
        times = await self.execute_sql(
            f'''
            SELECT
                *
            FROM
                SplitTime
            WHERE
                time_id = "{fastest_time_on_trail_id}"
            ORDER BY
                checkpoint_num
            '''
        )
        split_times = [time[2] for time in times]
        return split_times

    async def get_ban_status(self, steam_id: str):
        """ Get the ban status of a given player. """
        resp = await self.execute_sql(f'''
                SELECT ban_type FROM Player
                WHERE steam_id = "{steam_id}"
            ''')
        try:
            return resp[0][0]
        except IndexError:
            return ""

    async def get_valid_ids(self):
        """ Get the valid Discord IDs. """
        return await self.execute_sql('''
            SELECT
                discord_id
            FROM
                User
            WHERE
                valid = "TRUE"
            ''')

    async def get_steam_id(self, discord_id):
        """ Get the Steam ID associated with a given Discord ID. """
        statement = f'''
            SELECT steam_id FROM
            User where discord_id = "{discord_id}"
        '''
        try:
            return (await self.execute_sql(statement, write=True))[0][0]
        except IndexError:
            return None

    async def submit_steam_id(self, discord_id, steam_id):
        """ Submit a Steam ID to the database. """
        statement = f'''
            INSERT INTO PlayerAliases
            VALUES (
                {steam_id},
                (select valid FROM User WHERE discord_id = "{discord_id}"),
                "{steam_id}"
            )
        '''
        await self.execute_sql(statement, write=True)

    async def submit_alias(self, steam_id, alias):
        """ Submit an alias to the database. """
        statement = f'''
            SELECT alias
                FROM PlayerAliases
            WHERE steam_id = {steam_id}
        '''
        result = await self.execute_sql(statement)
        if alias not in [alias_result[0] for alias_result in result]:
            statement = f'''
                INSERT INTO PlayerAliases
                VALUES ({steam_id}, "{alias}")
            '''
            await self.execute_sql(statement, write=True)

    async def get_player_name(self, steam_id):
        """ Get the Steam name associated with a given Steam ID. """
        statement = f'''
            SELECT steam_name
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = await self.execute_sql(statement)
        try:
            return result[0][0]
        except IndexError:
            return ""

    async def get_time_details(self, time_id: str):
        """ Get the details of a given time. """
        statement = f'''
        SELECT
            *
        FROM
            all_times
        WHERE
            all_times.time_id = "{time_id}"
        '''
        result = await self.execute_sql(statement)
        return result[0]

    async def get_times_after_timestamp(self, timestamp: float, trail_name: str):
        """ Get the times after a given timestamp. """
        statement = f'''
            SELECT
                starting_speed,
                steam_name,
                bike_type,
                MAX(checkpoint_time)
            FROM
                Time
                INNER JOIN
                    SplitTime ON SplitTime.time_id = Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                LOWER(trail_name) = LOWER("{trail_name}")
                AND
                (Time.ignore = "False" OR Time.ignore is NULL)
                AND
                timestamp > {timestamp}
                AND
                was_monitored = "True"
            GROUP BY
                Time.time_id
            ORDER BY
                checkpoint_time ASC
        '''
        result = await self.execute_sql(statement)
        return result

    async def get_all_times(self, lim: int):
        """ Get all times. """
        statement = f'''
            SELECT * FROM all_times
            LIMIT {lim}
        '''
        result = await self.execute_sql(statement)
        return [
            {

                "steam_id": str(time[0]),
                "steam_name": time[1],
                "avatar_src": time[2],
                "ban_type": time[3],
                "ignore_times": time[4],
                "timestamp": time[5],
                "time_id": str(time[6]),
                "total_checkpoints": time[7],
                "total_time": time[8],
                "trail_name": time[9],
                "world_name": time[10],
                "was_monitored": time[11],
                "ignore": time[12],
                "bike_type": time[13],
                "starting_speed": time[14],
                "version": time[15],
                "verified": time[17]
            }
            for time in result
        ]

    async def get_all_players(self):
        """ Get all players. """
        statement = '''
            SELECT Player.steam_id, Player.steam_name, Player.avatar_src, Rep.rep, max(Rep.timestamp) as rep_timestamp FROM Player
            INNER JOIN Rep on Rep.steam_id = Player.steam_id
            GROUP BY Player.steam_id
            ORDER BY Player.steam_name ASC
        '''
        result = await self.execute_sql(statement)
        return result

    async def submit_locations(self, time_id, locations):
        """ Submit locations to the database."""
        for location in locations:
            timestamp = location[0]
            x = location[1][0]
            y = location[1][1]
            z = location[1][2]
            statement = f'''
                INSERT INTO Locations
                VALUES(
                    {time_id},
                    {timestamp},
                    {x}, {y}, {z}
                )
            '''
            await self.execute_sql(statement, write=True)

    async def get_start_bike(self, world_name: str):
        """ Get the starting bike for a given world. """
        statement = f'''
            SELECT start_bike
            FROM WorldInfo
            WHERE
                world_name="{world_name}"
        '''
        result = await self.execute_sql(statement)
        if len(result) < 1:
            return None
        return result[0][0]

    async def get_trails(self):
        """ Get all trails."""
        return [{
            "trail_name": trail[0],
            "world_name": trail[1],
            "times_ridden": trail[2],
            "leaderboard": await self.get_leaderboard(trail[0]),
            "average_start_speed": trail[3],
            "src": trail[4]
        } for trail in await self.execute_sql('''SELECT * FROM TrailInfo''')]

    async def get_worlds(self):
        """ Get all worlds. """
        return [world[0] for world in await self.execute_sql(
            '''SELECT world_name FROM Session GROUP BY world_name'''
        )]

    async def get_daily_plays(
        self,
        map_name: str,
        date_start: datetime,
        date_end: datetime
    ) -> list:
        """ Get the number of daily plays for a given map. """
        values = []
        for single_date in daterange(date_start, date_end):
            now = single_date
            then = datetime(1970, 1, 1)
            timestamp = (now - then).total_seconds()
            statement = f'''
                    SELECT time_started, time_ended, steam_id
                    FROM Session
                    WHERE (
                        time_started < {timestamp}
                        AND
                        time_started > {timestamp-86400}
                '''
            if map_name is not None:
                statement += f' AND world_name = "{map_name}"'
            statement += ") GROUP BY steam_id"
            sessions = await self.execute_sql(statement)
            values.append({
                "year": now.year,
                "month": now.month,
                "day": now.day,
                "users": len(sessions),
                "name": now.strftime("%m/%d/%Y")
            })
        return values

    async def get_to_verify(self, trail_name):
        """ Get runs that would be in the top 10 """
        statement = f'''
            SELECT * FROM (
                SELECT
                    starting_speed,
                    steam_name,
                    bike_type,
                    MIN(checkpoint_time),
                    Time.version,
                    Time.penalty,
                    Time.verified,
                    Time.time_id
                FROM
                    Time
                    INNER JOIN
                        SplitTime ON SplitTime.time_id = Time.time_id
                    INNER JOIN
                        (
                            SELECT
                                max(checkpoint_num) AS max_checkpoint
                            FROM
                                SplitTime
                                INNER JOIN
                                    Time ON Time.time_id = SplitTime.time_id
                                WHERE LOWER(Time.trail_name) = LOWER(
                                    "{trail_name}"
                                )
                        ) ON SplitTime.time_id=Time.time_id
                    INNER JOIN
                        Player ON Player.steam_id = Time.steam_id
                WHERE
                    LOWER(trail_name) = LOWER("{trail_name}")
                    AND
                    checkpoint_num = max_checkpoint
                    AND
                    (Time.ignore = "False")
                GROUP BY
                    trail_name,
                    Player.steam_id
                ORDER BY
                    checkpoint_time ASC
                LIMIT 10
            ) as dat
            WHERE dat.verified = 0
        '''
        result = await self.execute_sql(statement)
        return [
            {
                "place": i + 1,
                "time": time[3],
                "name": time[1],
                "bike": time[2],
                "starting_speed": time[0],
                "version": time[4],
                "penalty": time[5],
                "verified": str(time[6]),
                "time_id": str(time[7])
            }
            for i, time in enumerate(result)
        ]

    async def get_leaderboard(self, trail_name, num=10, steam_id="") -> list:
        """ Get the leaderboard for a given trail. """
        statement = f'''
            SELECT
                starting_speed,
                steam_name,
                bike_type,
                MIN(checkpoint_time),
                Time.version,
                Time.penalty,
                Time.verified,
                Time.time_id
            FROM
                Time
                INNER JOIN
                    SplitTime ON SplitTime.time_id = Time.time_id
                INNER JOIN
                    (
                        SELECT
                            max(checkpoint_num) AS max_checkpoint
                        FROM
                            SplitTime
                            INNER JOIN
                                Time ON Time.time_id = SplitTime.time_id
                            WHERE LOWER(Time.trail_name) = LOWER(
                                "{trail_name}"
                            )
                    ) ON SplitTime.time_id=Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                LOWER(trail_name) = LOWER("{trail_name}")
                AND
                checkpoint_num = max_checkpoint
                AND
                (Time.ignore = "False")
                AND (Time.verified = 1 OR Time.steam_id = "{steam_id}")
            GROUP BY
                trail_name,
                Player.steam_id
            ORDER BY
                checkpoint_time ASC
            LIMIT {num}
        '''
        result = await self.execute_sql(statement)
        return [
            {
                "place": i + 1,
                "time": time[3],
                "name": time[1],
                "bike": time[2],
                "starting_speed": time[0],
                "version": time[4],
                "penalty": time[5],
                "verified": str(time[6]),
                "time_id": str(time[7])
            }
            for i, time in enumerate(result)
        ]

    async def max_start_time(self, trail_time: str) -> float:
        """ Get the maximum starting time for a given trail. """
        statement = f'''
            SELECT max_starting_time FROM TrailStartTime
            WHERE trail_name = "{trail_time}"
        '''
        result = await self.execute_sql(statement)
        try:
            return float(result[0][0])
        except IndexError:
            return 50

    async def set_ignore_time(self, time_id, val):
        """ Set the ignore time for a given time. """
        statement = f'''
            UPDATE Time
            SET
                ignore = "{val}"
            WHERE
                time_id = {time_id}
        '''
        await self.execute_sql(statement, write=True)

    async def set_monitored(self, time_id, val):
        """ Set the monitored status for a given time. """
        statement = f'''
            UPDATE Time
            SET
                was_monitored = "{val}"
            WHERE
                time_id = {time_id}
        '''
        await self.execute_sql(statement, write=True)

    async def get_avatar(self, steam_id):
        """ Get the avatar for a given Steam ID. """
        statement = f'''
            SELECT avatar_src
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = await self.execute_sql(statement)
        try:
            return result[0][0]
        except IndexError:
            return ""

    async def submit_ip(self, steam_id, address, port):
        """ Submit an IP address to the database. """
        timestamp = time.time()
        statement = f'''
            INSERT INTO IP (steam_id, timestamp, address, port)
            VALUES ({steam_id}, {timestamp}, "{address}", {port})
        '''
        await self.execute_sql(statement, write=True)

    async def get_archive(self):
        """ Get the archive. """
        statement = '''
            SELECT
                sum(time_ended - time_started) AS total_time,
                Session.steam_id,
                Player.steam_name,
                Session.world_name,
                Player.avatar_src
            FROM
                Session
            INNER JOIN
                Player ON Session.steam_id = Player.steam_id
            GROUP BY
                Session.steam_id
            ORDER BY
                total_time DESC
        '''
        result = await self.execute_sql(statement)
        return result

    async def discord_login(self, discord_id, discord_name, email, steam_id):
        """ Log in to Discord. """
        statement = f'''
            INSERT OR IGNORE INTO User
            VALUES(
                {discord_id},
                "FALSE",
                "{steam_id}",
                "{discord_name}",
                "{email}"
            )
        '''
        await self.execute_sql(statement, write=True)

    async def get_penalty(self, time_id):
        """ Get the penalty for a given time. """
        statement = f'''
            SELECT penalty
            FROM Time
            WHERE time_id = "{time_id}"
        '''
        result = await self.execute_sql(statement)
        return result[0][0]

    async def get_version(self, time_id):
        """ Get the version for a given time. """
        statement = f'''
            SELECT version
            FROM Time
            WHERE time_id = "{time_id}"
        '''
        result = await self.execute_sql(statement)
        return result[0][0]

    async def get_total_times(self, limit=10):
        """ Get the total times for a given trail. """
        statement = f'''
            SELECT * FROM TotalTime
            LIMIT {limit}
        '''
        result = await self.execute_sql(statement)
        return result

    async def get_time_on_world(self, steam_id, world="none"):
        """ Get the time spent on a given world. """
        statement = f'''
            SELECT sum(time_ended - time_started) AS total_time FROM Session
            WHERE steam_id = "{steam_id}"
        '''
        if world != "none":
            statement += f'''AND world_name = "{world}"'''
        result = await self.execute_sql(statement)
        if result[0][0] is None:
            return 0
        return result[0][0]

    async def get_times_trail_ridden(self, trail_name):
        """ Get the number of times a given trail has been ridden. """
        statement = f'''
            SELECT timestamp FROM Time
            WHERE trail_name = "{trail_name}"
        '''
        timestamps = await self.execute_sql(statement)
        return [stamp[0] for stamp in timestamps]

    async def get_split_times(self, time_id):
        """ Get the split times for a given time. """
        statement = f'''
            SELECT checkpoint_time FROM SplitTime
            WHERE time_id = "{time_id}"
            ORDER BY checkpoint_num
        '''
        result = await self.execute_sql(statement)
        return [res[0] for res in result]

    async def verify_time(self, time_id: str):
        """ Verify a given time. """
        await self.execute_sql(
            f'''
                UPDATE Time
                SET verified = 1
                WHERE time_id = "{time_id}"
            ''',
            write=True
        )

    async def submit_time(
        self,
        steam_id: str,
        split_times,
        trail_name,
        being_monitored,
        current_world,
        bike_type,
        starting_speed,
        version,
        penalty: float,
        verified: str
    ):
        """ Submit a time to the database. """
        logging.info(
            "submit_time(%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)",
            steam_id, split_times, trail_name, being_monitored,
            current_world, bike_type, starting_speed, version, penalty, verified
        )
        time_id = hash(
            str(split_times[len(split_times)-1])
            + str(steam_id)
            + str(time.time())
        )
        # verified as "0" = unverified, "1" = verified
        await self.execute_sql(
            f'''
            INSERT INTO Time (
                steam_id, time_id, timestamp, world_name,
                trail_name, was_monitored, bike_type,
                ignore, starting_speed, version, penalty, verified
            )
            VALUES (
                "{steam_id}", "{time_id}", {time.time()},
                "{current_world}", "{trail_name}",
                "{str(being_monitored)}", "{bike_type}",
                "False", {starting_speed}, "{version}", {penalty}, {verified}
            )
            ''',
            write=True
        )
        for n, split_time in enumerate(split_times):
            await self.execute_sql(f'''
            INSERT INTO SplitTime (
                time_id,
                checkpoint_num,
                checkpoint_time
                )
            VALUES (
                {time_id},
                {n},
                {split_time}
                )
            ''', write=True)
        return time_id

    async def end_session(self, steam_id, time_started, time_ended, world_name):
        """ End a session. """
        await self.execute_sql(
            f'''
            INSERT INTO Session (
                steam_id,
                time_started,
                time_ended,
                world_name
            )
            VALUES (
                "{steam_id}",
                {time_started},
                {time_ended},
                "{world_name}"
            )
            ''',
            write=True
        )

    async def get_medals(self, steam_id, trail_name):
        """ Get the medals for a given trail. """
        x = f'''
            SELECT
                starting_speed,
                steam_name,
                bike_type,
                MIN(checkpoint_time),
                Time.version
            FROM
                Time
                INNER JOIN
                    SplitTime ON SplitTime.time_id = Time.time_id
                INNER JOIN
                    (
                        SELECT
                            max(checkpoint_num) AS max_checkpoint
                        FROM
                            SplitTime
                            INNER JOIN
                                Time ON Time.time_id = SplitTime.time_id
                            WHERE LOWER(Time.trail_name) = LOWER(
                                "{trail_name}"
                            )
                    ) ON SplitTime.time_id=Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                LOWER(trail_name) = LOWER("{trail_name}")
                AND
                checkpoint_num = max_checkpoint
                AND
                (Time.ignore = "False" OR Time.ignore is NULL)
                AND
                Player.steam_id = {steam_id}
            GROUP BY
                trail_name,
                Player.steam_id
            ORDER BY
                checkpoint_time ASC
        '''
        result = await self.execute_sql(x)
        if len(result) == 0:
            return [False, False, False, False]
        y = f'''
            SELECT medal_type, time
            FROM TrailMedal
            WHERE trail_name = "{trail_name}"
        '''
        rainbow_time = 0
        gold_time = 0
        silver_time = 0
        bronze_time = 0
        for medal in await self.execute_sql(y):
            if medal[0] == "rainbow":
                rainbow_time = float(medal[1])
            elif medal[0] == "gold":
                gold_time = float(medal[1])
            elif medal[0] == "silver":
                silver_time = float(medal[1])
            elif medal[0] == "bronze":
                bronze_time = float(medal[1])
        # rainbow, gold, silver, bronze
        to_return = [False, False, False, False]
        for player_time in result:
            to_return[0] = player_time[3] <= rainbow_time
            to_return[1] = player_time[3] <= gold_time
            to_return[2] = player_time[3] <= silver_time
            to_return[3] = player_time[3] <= bronze_time
        return to_return
