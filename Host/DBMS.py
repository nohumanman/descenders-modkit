import sqlite3
import time
import os

script_path = os.path.dirname(os.path.realpath(__file__))

class DBMS():
    @staticmethod
    def execute_sql(statement : str, write=False):
        con = sqlite3.connect(script_path + "/SplitTimer.db")
        execution = con.execute(statement)
        if write:
            con.commit()
        result = execution.fetchall()
        con.close()
        return result

    @staticmethod
    def get_all_players():
        return DBMS.execute_sql('''SELECT * FROM Players''')

    @staticmethod
    def update_player(steam_id, steam_name, avatar_src):
        print("Submitting player to database - steam id", steam_id, "steam name:", steam_name)
        DBMS.execute_sql(
            f'''
            REPLACE INTO Player (steam_id, steam_name, avatar_src)
            VALUES ("{steam_id}", "{steam_name}", "{avatar_src}")
            ''', write=True
        )

    @staticmethod
    def get_fastest_split_times(trail_name, competitors_only=False, min_timestamp=None, monitored_only=False):
        statement = '''
                SELECT SplitTime.time_id, SplitTime.checkpoint_num, SplitTime.checkpoint_time, Times.trail_name, Players.is_competitor, Players.steam_name, Times.timestamp from 
                SplitTime
                INNER JOIN Times ON SplitTime.time_id = Times.time_id
                INNER JOIN Players ON Times.steam_id = Players.steam_id\n
            '''
        statement += f'''WHERE trail_name = "{trail_name}"'''''
        if min_timestamp is not None:
            statement += f''' AND timestamp>{float(min_timestamp)}'''
        elif competitors_only:
            statement += ''' AND is_competitor != "false"'''
        elif monitored_only:
            statement += ''' AND was_monitored != "True"'''
        statement += '''
            order by checkpoint_num DESC, checkpoint_time asc
            limit 1;
            '''
        time_id = DBMS.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            print("No trail specified")
            return []
        times = DBMS.execute_sql(
            f'''
            SELECT * FROM SplitTime
            WHERE time_id = "{fastest_time_on_trail_id}" ORDER BY checkpoint_num
            '''
        )
        split_times = [time[2] for time in times]
        return split_times

    @staticmethod
    def get_ban_status(steam_id):
        resp = DBMS.execute_sql(f'''SELECT ban_status FROM Players WHERE steam_id="{steam_id}"''')
        return resp[0][0]

    @staticmethod
    def get_leaderboard(trail_name, num=10) -> list:
        statement = f'''
            SELECT *, MIN(checkpoint_time) FROM Time
            INNER JOIN SplitTime ON SplitTime.time_id=Time.time_id 
            INNER JOIN (SELECT max(checkpoint_num) AS max_checkpoint FROM SplitTime)  ON SplitTime.time_id=Time.time_id
            INNER JOIN Player ON Player.steam_id=Time.steam_id
            WHERE trail_name = "{trail_name}" AND checkpoint_num = max_checkpoint
            GROUP BY Player.steam_id
            ORDER BY checkpoint_time ASC
            LIMIT {num};
        '''
        result = DBMS.execute_sql(statement)
        return [
            {
                "steam_id" : time[0],
                "time_id" : time[1],
                "timestamp" : time[2],
                "world_name" : time[3],
                "trail_name": time[4],
                "was_monitored" : time[5],
                "total_time" : time[9],
                "bike" : time[6],
                "steam_name": time[12],
                "avatar_src" : time[13],
            }
            for time in result
        ]

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
    def submit_time(steam_id, split_times, trail_name, being_monitored, current_world, bike_type):
        time_id = hash(str(split_times[len(split_times)-1])+str(steam_id)+str(time.time()))
        DBMS.execute_sql(
            f'''
            INSERT INTO Time (steam_id, time_id, timestamp, world_name, trail_name, was_monitored, bike_type)
            VALUES ("{steam_id}", "{time_id}", {time.time()}, "{current_world}", "{trail_name}", "{str(being_monitored)}", "{bike_type}")
            ''', write=True)
        for n, split_time in enumerate(split_times):
            DBMS.execute_sql(
            f'''
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
            INSERT INTO Session (steam_id, time_started, time_ended, world_name)
            VALUES ("{steam_id}", {time_started}, {time_ended}, "{world_name}")
            ''',
            write=True
        )
