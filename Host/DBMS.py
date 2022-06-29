from re import T
import sqlite3
import time
import os
import json

script_path = os.path.dirname(os.path.realpath(__file__))


class DBMS():
    @staticmethod
    def execute_sql(statement: str, write=False):
        con = sqlite3.connect(script_path + "/SplitTimer.db")
        execution = con.execute(statement)
        if write:
            con.commit()
        result = execution.fetchall()
        con.close()
        return result

    @staticmethod
    def get_webhooks(trail_name):
        return DBMS.execute_sql(
            "SELECT webhook_url FROM webhooks"
            f" WHERE trail_name = '{trail_name}'"
        )

    @staticmethod
    def get_all_players():
        return DBMS.execute_sql('''SELECT * FROM Players''')

    @staticmethod
    def update_player(steam_id, steam_name, avatar_src):
        DBMS.execute_sql(
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

    @staticmethod
    def get_fastest_split_times(
        trail_name,
        competitors_only=False,
        min_timestamp=None,
        monitored_only=False
    ):
        latest_version = ""
        with open(script_path + "/current_version.json") as json_file:
            data = json.load(json_file)
            latest_version = data["latest_version"]

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
            WHERE trail_name = "{trail_name}" AND Time.version = "{latest_version}"
            AND (Time.ignore = "FALSE" OR Time.ignore is NULL)
            ORDER BY checkpoint_num DESC, checkpoint_time ASC
            LIMIT 1
            '''
        time_id = DBMS.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            print("No trail specified")
            return []
        times = DBMS.execute_sql(
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

    @staticmethod
    def log_rep(steam_id, rep):
        statement = f'''
            INSERT INTO Rep (
                steam_id,
                timestamp,
                rep
            )
            VALUES (
                "{steam_id}",
                {time.time()},
                "{rep}"
            )
            '''
        DBMS.execute_sql(statement, write=True)

    @staticmethod
    def get_ban_status(steam_id: str):
        resp = DBMS.execute_sql(f'''
                SELECT ban_type FROM Player
                WHERE steam_id = "{steam_id}"
            ''')
        return resp[0][0]

    @staticmethod
    def get_valid_ids():
        return DBMS.execute_sql('''
            SELECT
                discord_id
            FROM
                User
            WHERE
                valid = "TRUE"
            ''')

    @staticmethod
    def get_steam_id(discord_id):
        statement = f'''
            SELECT steam_id FROM
            User where discord_id = "{discord_id}"
        '''
        try:
            return DBMS().execute_sql(statement, write=True)[0][0]
        except Exception:
            return None

    @staticmethod
    def submit_steam_id(discord_id, steam_id):
        statement = f'''
            INSERT INTO PlayerAliases
            VALUES (
                {steam_id},
                (select valid FROM User WHERE discord_id = "{discord_id}"),
                "{steam_id}"
            )
        '''
        DBMS.execute_sql(statement, write=True)

    @staticmethod
    def submit_alias(steam_id, alias):
        statement = f'''
            SELECT alias
                FROM PlayerAliases
            WHERE steam_id = {steam_id}
        '''
        result = DBMS.execute_sql(statement)
        if alias not in [alias_result[0] for alias_result in result]:
            statement = f'''
                INSERT INTO PlayerAliases
                VALUES ({steam_id}, "{alias}")
            '''
            DBMS.execute_sql(statement, write=True)

    @staticmethod
    def get_player_name(steam_id):
        statement = f'''
            SELECT steam_name
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = DBMS.execute_sql(statement)
        try:
            return result[0][0]
        except Exception:
            return ""
    

    @staticmethod
    def get_all_times():
        statement = f'''
            SELECT
                *,
                MIN(checkpoint_time)
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
                    ) ON SplitTime.time_id=Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                checkpoint_num = max_checkpoint
                AND
                (Time.ignore = "FALSE" OR Time.ignore is NULL)
            GROUP BY
                trail_name
            ORDER BY
                checkpoint_time ASC
        '''

    @staticmethod
    def get_leaderboard(trail_name, num=10) -> list:
        latest_version = ""
        with open(script_path + "/current_version.json") as json_file:
            data = json.load(json_file)
            latest_version = data["latest_version"]

        statement = f'''
            SELECT
                starting_speed,
                steam_name,
                bike_type,
                MIN(checkpoint_time)
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
                            WHERE LOWER(Time.trail_name) = LOWER("{trail_name}")
                    ) ON SplitTime.time_id=Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                LOWER(trail_name) = LOWER("{trail_name}")
                AND
                checkpoint_num = max_checkpoint
                AND
                (Time.ignore = "FALSE" OR Time.ignore is NULL)
                AND
                (Time.version = "{latest_version}")
            GROUP BY
                trail_name,
                Player.steam_id
            ORDER BY
                checkpoint_time ASC
            LIMIT {num}
        '''
        result = DBMS.execute_sql(statement)
        return [
            {
                "place": i + 1,
                "time": time[3],
                "name": time[1],
                "bike": time[2],
                "starting_speed": time[0]
            }
            for i, time in enumerate(result)
        ]

    @staticmethod
    def get_avatar(steam_id):
        statement = f'''
            SELECT avatar_src
                FROM Player
            WHERE steam_id = {steam_id}
        '''
        result = DBMS.execute_sql(statement)
        try:
            return result[0][0]
        except Exception:
            return ""

    @staticmethod
    def submit_ip(steam_id, address, port):
        timestamp = time.time()
        statement = f'''
            INSERT INTO IP (steam_id, timestamp, address, port)
            VALUES ({steam_id}, {timestamp}, "{address}", {port})
        '''
        DBMS.execute_sql(statement, write=True)

    @staticmethod
    def get_archive():
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
        result = DBMS.execute_sql(statement)
        return result

    @staticmethod
    def get_time_on_world(steam_id, world="none"):
        statement = f'''
            SELECT sum(time_ended - time_started) AS total_time FROM Session
            WHERE steam_id = "{steam_id}"
        '''
        if world != "none":
            statement += f'''AND world_name = "{world}"'''
        result = DBMS.execute_sql(statement)
        if result[0][0] is None:
            return 0
        return result[0][0]

    @staticmethod
    def get_times_trail_ridden(trail_name):
        statement = f'''
            SELECT timestamp FROM Time
            WHERE trail_name = "{trail_name}"
        '''
        timestamps = DBMS.execute_sql(statement)
        return [stamp[0] for stamp in timestamps]

    @staticmethod
    def submit_time(
        steam_id,
        split_times,
        trail_name,
        being_monitored,
        current_world,
        bike_type,
        starting_speed,
        version
    ):
        time_id = hash(
            str(split_times[len(split_times)-1])
            + str(steam_id)
            + str(time.time())
        )
        DBMS.execute_sql(
            f'''
            INSERT INTO Time (
                steam_id,
                time_id,
                timestamp,
                world_name,
                trail_name,
                was_monitored,
                bike_type,
                starting_speed,
                version
            )
            VALUES (
                "{steam_id}",
                "{time_id}",
                {time.time()},
                "{current_world}",
                "{trail_name}",
                "{str(being_monitored)}",
                "{bike_type}",
                "{starting_speed}",
                "{version}"
            )
            ''', write=True)
        for n, split_time in enumerate(split_times):
            DBMS.execute_sql(f'''
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

    @staticmethod
    def end_session(steam_id, time_started, time_ended, world_name):
        DBMS.execute_sql(
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
