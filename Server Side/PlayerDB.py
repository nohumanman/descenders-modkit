import sqlite3
import time
import os


script_path = os.path.dirname(os.path.realpath(__file__))

class PlayerDB():
    @staticmethod
    def execute_sql(statement : str, write=False):
        con = sqlite3.connect(script_path + "/TimeStats.db")
        execution = con.execute(statement)
        if write:
            con.commit()
        return execution.fetchall()

    @staticmethod
    def get_all_players():
        return PlayerDB.execute_sql('''SELECT * FROM Players''')

    @staticmethod
    def update_player(steam_id, steam_name, is_competitor, avatar_src):
        print("Submitting player to database - steam id", steam_id, "steam name:", steam_name)
        PlayerDB.execute_sql(
            f'''
            REPLACE INTO Players (steam_id, steam_name, is_competitor, ban_status, avatar_src)
            VALUES ("{steam_id}", "{steam_name}", "{is_competitor}", "unbanned", "{avatar_src}")
            ''', write=True
        )

    @staticmethod
    def update_trail(trail_name, world_name):
        PlayerDB.execute_sql(
            f'''
            REPLACE INTO Trail
            (trail_name, bikes_permitted_id, world_name, trail_src)
            VALUES (
                "{trail_name}",
                (select bikes_permitted_id from Trail where trail_name = "{trail_name}"),
                "{world_name}",
                (select trail_src from Trail where trail_name = "{trail_name}")
            )
            ''', write=True
        )

    @staticmethod
    def get_fastest_split_times(trail_name, competitors_only=False, min_timestamp=None, monitored_only=False):
        statement = '''
                SELECT "Split Times".time_id, "Split Times".checkpoint_num, "Split Times".checkpoint_time, Times.trail_name, Players.is_competitor, Players.steam_name, Times.timestamp from 
                "Split Times"
                INNER JOIN Times ON "Split Times".time_id = Times.time_id
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
        time_id = PlayerDB.execute_sql(statement)
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            print("No trail specified")
            return []
        times = PlayerDB.execute_sql(
            f'''
            SELECT * FROM "Split Times"
            WHERE time_id = "{fastest_time_on_trail_id}" ORDER BY checkpoint_num
            '''
        )
        split_times = [time[2] for time in times]
        return split_times

    @staticmethod
    def get_ban_status(steam_id):
        resp = PlayerDB.execute_sql(f'''SELECT ban_status FROM Players WHERE steam_id="{steam_id}"''')
        return resp[0][0]

    @staticmethod
    def get_leaderboard_descenders(trail_name, num=10):
        # to get the top 10 fastest times from Times.
        # this requires finding the highest checkpoint
        # number with the lowest checkpoint time, 10 times.
        # WHERE trail_name = "xyz"
        statement = f'''
            SELECT *, MIN(checkpoint_time) FROM Times
            INNER JOIN "Split Times" ON "Split Times".time_id=Times.time_id 
            INNER JOIN (SELECT max(checkpoint_num) AS max_checkpoint FROM "Split Times")  ON "Split Times".time_id=Times.time_id
            INNER JOIN Players ON Players.steam_id=Times.steam_id
            WHERE trail_name = "{trail_name}" AND checkpoint_num = max_checkpoint
            GROUP BY Players.steam_id
            ORDER BY checkpoint_time ASC
            LIMIT {num};
        '''
        result = PlayerDB.execute_sql(statement)
        return [
            {
                "steam_id" : time[0],
                "time_id" : time[1],
                "timestamp" : time[2],
                "trail_name": time[3],
                "was_monitored" : time[4],
                "total_time" : time[7],
                "steam_name": time[10],
                "is_competitor" : time[11]
            }
            for time in result
        ]

    @staticmethod
    def update_ban_status(steam_id, new_ban_status):
        PlayerDB.execute_sql(
            f'''
            UPDATE Players
            SET ban_status = "{new_ban_status}"
            WHERE steam_id="{steam_id}"
            ''',
            write=True
        )

    @staticmethod
    def get_time_on_world(steam_id, world="none"):
        statement = f'''
            SELECT sum(time_ended - time_started) AS total_time FROM Session
            WHERE steam_id = "{steam_id}"
        '''
        if world != "none":
            statement += f'''AND world_name = "{world}"'''
        result = PlayerDB.execute_sql(statement)
        if result[0][0] is None:
            return 0
        return result[0][0]

    @staticmethod
    def get_times_trail_ridden(trail_name):
        statement = f'''
            SELECT timestamp FROM Times
            WHERE trail_name = "{trail_name}"
        '''
        timestamps = PlayerDB.execute_sql(statement)
        return [stamp[0] for stamp in timestamps]

    @staticmethod
    def get_trail_data():
        times_req = PlayerDB.execute_sql(
            f'''
                SELECT * FROM trail_info
            '''
        )
        times = [
            {
                "trail_name" : time[0],
                "trail_src" : time[1],
                "world_name" : time[2],
                "downhill" : time[3],
                "enduro" : time[4],
                "hardtail" : time[5],
            }
            for time in times_req]
        return times

    @staticmethod
    def submit_time(steam_id, split_times, trail_name, being_monitored, current_world):
        PlayerDB.update_trail(trail_name, current_world)
        
        time_hash = hash(str(split_times[len(split_times)-1])+str(steam_id)+str(time.time()))
        PlayerDB.execute_sql(
            f'''
            INSERT INTO Times (steam_id, time_id, timestamp, trail_name, was_monitored)
            VALUES ("{steam_id}", "{time_hash}", {time.time()}, "{trail_name}", "{str(being_monitored)}")
            ''', write=True)
        for n, split_time in enumerate(split_times):
            PlayerDB.execute_sql(
            f'''
            INSERT INTO "Split Times" (
                time_id,
                checkpoint_num,
                checkpoint_time
                )
            VALUES (
                "{time_hash}",
                "{n}",
                {split_time}
                )
            ''', write=True)

    @staticmethod
    def end_session(steam_id, time_started, time_ended, world_name):
        PlayerDB.execute_sql(
            f'''
            INSERT INTO Session (steam_id, time_started, time_ended, world_name)
            VALUES ("{steam_id}", "{time_started}", "{time_ended}", "{world_name}")
            ''',
            write=True
        )
