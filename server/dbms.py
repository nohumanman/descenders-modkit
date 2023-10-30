""" Database Management System """
import sqlite3
import time
import os
from datetime import datetime, timedelta

script_path = os.path.dirname(os.path.realpath(__file__))


def daterange(start_date: datetime, end_date: datetime):
    for n in range(int((end_date - start_date).days)):
        yield start_date + timedelta(n)

class DBMS():
    def __init__(self):
        self.con = None

    def execute_sql(self, statement: str, write=False):
        if self.con is None:
            self.con = sqlite3.connect(script_path + "/modkit.db", check_same_thread=False)
        cur = self.con.cursor()
        try:
            execution = cur.execute(statement)
        except sqlite3.OperationalError:
            return []
        if write:
            self.con.commit()
        result = execution.fetchall()
        return result

    def get_id_from_name(self, steam_name):
        return self.execute_sql(
            f'''
            SELECT Player.steam_id
            FROM Player
            WHERE Player.steam_name = "{steam_name}"'''
        )[0][0]

    def get_player_stats(self, steam_id):
        statement = f'''
            SELECT
                Player.steam_id,
                Player.steam_name,
                Rep.rep,
                Max(Rep.timestamp) as last_login,
                times_logged_on,
                trails_ridden,
                total_time,
                Player.avatar_src
            FROM
                Player,
                (
                    SELECT COUNT(*) as times_logged_on
                    FROM Session
                    WHERE Session.steam_id = {steam_id}
                ),
                (
                    SELECT COUNT(*) as trails_ridden
                    FROM Time
                    WHERE Time.steam_id = {steam_id}
                ),
                (
                    SELECT
                        sum(Session.time_ended - Session.time_started)
                        AS total_time
                    FROM Session WHERE steam_id = {steam_id}
                )
            INNER JOIN Rep ON Rep.steam_id = Player.steam_id
            INNER JOIN Time ON Time.steam_id = Player.steam_id
            WHERE Player.steam_id = "{steam_id}"
            GROUP BY Rep.steam_id
        '''
        return self.execute_sql(statement)

    def get_webhooks(self, trail_name):
        return self.execute_sql(
            "SELECT webhook_url FROM webhooks"
            f" WHERE trail_name = '{trail_name}'"
        )

    def update_player(self, steam_id, steam_name, avatar_src):
        self.execute_sql(
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

    def get_personal_fastest_split_times(self, trail_name, steam_id):
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
        time_id = self.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            return []
        times = self.execute_sql(
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

    def get_fastest_split_times(self,
        trail_name
    ):
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
        time_id = self.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            return []
        times = self.execute_sql(
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

    def get_ban_status(self, steam_id: str):
        resp = self.execute_sql(f'''
                SELECT ban_type FROM Player
                WHERE steam_id = "{steam_id}"
            ''')
        try:
            return resp[0][0]
        except IndexError:
            return ""

    def get_valid_ids(self):
        return self.execute_sql('''
            SELECT
                discord_id
            FROM
                User
            WHERE
                valid = "TRUE"
            ''')

    def get_steam_id(self, discord_id):
        statement = f'''
            SELECT steam_id FROM
            User where discord_id = "{discord_id}"
        '''
        try:
            return self.execute_sql(statement, write=True)[0][0]
        except IndexError:
            return None

    def submit_steam_id(self, discord_id, steam_id):
        statement = f'''
            INSERT INTO PlayerAliases
            VALUES (
                {steam_id},
                (select valid FROM User WHERE discord_id = "{discord_id}"),
                "{steam_id}"
            )
        '''
        self.execute_sql(statement, write=True)

    def submit_alias(self, steam_id, alias):
        statement = f'''
            SELECT alias
                FROM PlayerAliases
            WHERE steam_id = {steam_id}
        '''
        result = self.execute_sql(statement)
        if alias not in [alias_result[0] for alias_result in result]:
            statement = f'''
                INSERT INTO PlayerAliases
                VALUES ({steam_id}, "{alias}")
            '''
            self.execute_sql(statement, write=True)

    def get_player_name(self, steam_id):
        statement = f'''
            SELECT steam_name
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = self.execute_sql(statement)
        try:
            return result[0][0]
        except IndexError:
            return ""

    def get_time_details(self, time_id: str):
        statement = f'''
        SELECT
            *
        FROM
            all_times
        WHERE
            all_times.time_id = "{time_id}"
        '''
        result = self.execute_sql(statement)
        return result[0]

    def get_times_after_timestamp(self, timestamp: float, trail_name: str):
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
        result = self.execute_sql(statement)
        return result

    def get_all_times(self, lim: int):
        statement = f'''
            SELECT * FROM all_times
            LIMIT {lim}
        '''
        result = self.execute_sql(statement)
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
                "version": time[15]
            }
            for time in result
        ]

    def get_all_players(self):
        statement = '''
            SELECT Player.steam_id, Player.steam_name, Player.avatar_src, Rep.rep, max(Rep.timestamp) as rep_timestamp FROM Player
            INNER JOIN Rep on Rep.steam_id = Player.steam_id
            GROUP BY Player.steam_id
            ORDER BY Player.steam_name ASC
        '''
        result = self.execute_sql(statement)
        return result

    def submit_locations(self, time_id, locations):
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
            self.execute_sql(statement, write=True)

    def get_start_bike(self, world_name: str):
        statement = f'''
            SELECT start_bike
            FROM WorldInfo
            WHERE
                world_name="{world_name}"
        '''
        result = self.execute_sql(statement)
        if len(result) < 1:
            return None
        return result[0][0]

    def get_trails(self):
        return [{
            "trail_name": trail[0],
            "world_name": trail[1],
            "times_ridden": trail[2],
            "leaderboard": self.get_leaderboard(trail[0]),
            "average_start_speed": trail[3],
            "src": trail[4]
        } for trail in self.execute_sql('''SELECT * FROM TrailInfo''')]

    def get_worlds(self):
        return [world[0] for world in self.execute_sql(
            '''SELECT world_name FROM Session GROUP BY world_name'''
        )]

    def get_daily_plays(
        self,
        map_name: str,
        date_start: datetime,
        date_end: datetime
    ) -> list:
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
            sessions = self.execute_sql(statement)
            values.append({
                "year": now.year,
                "month": now.month,
                "day": now.day,
                "users": len(sessions),
                "name": now.strftime("%m/%d/%Y")
            })
        return values

    def get_leaderboard(self, trail_name, num=10, steam_id="") -> list:
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
                (Time.ignore = "False" OR Time.ignore is NULL)
                AND (Time.verified = "1" OR Time.steam_id = "{steam_id}")
            GROUP BY
                trail_name,
                Player.steam_id
            ORDER BY
                checkpoint_time ASC
            LIMIT {num}
        '''
        result = self.execute_sql(statement)
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

    def max_start_time(self, trail_time: str) -> float:
        statement = f'''
            SELECT max_starting_time FROM TrailStartTime
            WHERE trail_name = "{trail_time}"
        '''
        result = self.execute_sql(statement)
        try:
            return float(result[0][0])
        except IndexError:
            return 50

    def set_ignore_time(self, time_id, val):
        statement = f'''
            UPDATE Time
            SET
                ignore = "{val}"
            WHERE
                time_id = {time_id}
        '''
        self.execute_sql(statement, write=True)

    def set_monitored(self, time_id, val):
        statement = f'''
            UPDATE Time
            SET
                was_monitored = "{val}"
            WHERE
                time_id = {time_id}
        '''
        self.execute_sql(statement, write=True)

    def get_avatar(self, steam_id):
        statement = f'''
            SELECT avatar_src
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = self.execute_sql(statement)
        try:
            return result[0][0]
        except IndexError:
            return ""

    def submit_ip(self, steam_id, address, port):
        timestamp = time.time()
        statement = f'''
            INSERT INTO IP (steam_id, timestamp, address, port)
            VALUES ({steam_id}, {timestamp}, "{address}", {port})
        '''
        self.execute_sql(statement, write=True)

    def get_archive(self):
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
        result = self.execute_sql(statement)
        return result

    def discord_login(self, discord_id, discord_name, email, steam_id):
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
        self.execute_sql(statement, write=True)

    def get_penalty(self, time_id):
        statement = f'''
            SELECT penalty
            FROM Time
            WHERE time_id = "{time_id}"
        '''
        result = self.execute_sql(statement)
        return result[0][0]

    def get_version(self, time_id):
        statement = f'''
            SELECT version
            FROM Time
            WHERE time_id = "{time_id}"
        '''
        result = self.execute_sql(statement)
        return result[0][0]

    def get_total_times(self, limit=10):
        statement = f'''
            SELECT * FROM TotalTime
            LIMIT {limit}
        '''
        result = self.execute_sql(statement)
        return result

    def get_time_on_world(self, steam_id, world="none"):
        statement = f'''
            SELECT sum(time_ended - time_started) AS total_time FROM Session
            WHERE steam_id = "{steam_id}"
        '''
        if world != "none":
            statement += f'''AND world_name = "{world}"'''
        result = self.execute_sql(statement)
        if result[0][0] is None:
            return 0
        return result[0][0]

    def get_times_trail_ridden(self, trail_name):
        statement = f'''
            SELECT timestamp FROM Time
            WHERE trail_name = "{trail_name}"
        '''
        timestamps = self.execute_sql(statement)
        return [stamp[0] for stamp in timestamps]

    def get_split_times(self, time_id):
        statement = f'''
            SELECT checkpoint_time FROM SplitTime
            WHERE time_id = "{time_id}"
            ORDER BY checkpoint_num
        '''
        result = self.execute_sql(statement)
        return [res[0] for res in result]

    def verify_time(self, time_id: str):
        self.execute_sql(
            f'''
                UPDATE Time
                SET verified = 1
                WHERE time_id = "{time_id}"
            ''',
            write=True
        )

    def submit_time(
        self,
        steam_id: str,
        split_times,
        trail_name,
        being_monitored,
        current_world,
        bike_type,
        starting_speed,
        version,
        penalty: float
    ):
        time_id = hash(
            str(split_times[len(split_times)-1])
            + str(steam_id)
            + str(time.time())
        )
        self.execute_sql(
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
                "False", "{starting_speed}", "{version}", {penalty}, "0"
            )
            ''',
            write=True
        )
        for n, split_time in enumerate(split_times):
            self.execute_sql(f'''
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

    def end_session(self, steam_id, time_started, time_ended, world_name):
        self.execute_sql(
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

    def get_medals(self, steam_id, trail_name):
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
        result = self.execute_sql(x)
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
        for medal in self.execute_sql(y):
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
