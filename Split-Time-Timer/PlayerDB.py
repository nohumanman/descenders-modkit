import sqlite3
import time

class PlayerDB():
    @staticmethod
    def add_player(steam_id, steam_name, is_competitor):
        print("Submitting player to database - steam id", steam_id, "steam name:", steam_name)
        con = sqlite3.connect("TimeStats.db")
        con.execute(
            f'''
            REPLACE INTO Players (steam_id, steam_name, is_competitor)
            VALUES ("{steam_id}", "{steam_name}", "{is_competitor}")
            '''
        )
        con.commit()

    @staticmethod
    def become_competitor(steam_id, is_competitor, steam_name):
        con = sqlite3.connect("TimeStats.db")
        if is_competitor:
            con.execute(
                f'''
                REPLACE INTO Players (steam_id, steam_name, is_competitor)
                VALUES ("{steam_id}", "{steam_name}", "true")
                '''
            )
        else:
            con.execute(
                f'''
                REPLACE INTO Players (steam_id, steam_name, is_competitor)
                VALUES ("{steam_id}", "{steam_name}", "false")
                '''
            )
        con.commit()
        print("Function not complete - become_competitor")

    @staticmethod
    def get_fastest_split_times(trail_name, competitors_only=False, min_timestamp=None, monitored_only=False):
        con = sqlite3.connect("TimeStats.db")
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
        #print(statement)
        time_id = con.execute(statement).fetchall()
        try:
            fastest_time_on_trail_id = time_id[0][0]
        except IndexError:
            print("No trail specified")
            return []

        times = con.execute(
            f'''
            SELECT * FROM "Split Times"
            WHERE time_id = "{fastest_time_on_trail_id}" ORDER BY checkpoint_num
            '''
        ).fetchall()
        #print(times)
        split_times = [time[2] for time in times]
        #print(times)
        con.close()
        return split_times

    @staticmethod
    def execute_sql(statement : str, write=False):
        with sqlite3.connect("TimeStats.db") as con:
            execution = con.execute(statement)
            if write:
                con.commit()
            return execution.fetchall()      

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
        print(result)
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
    def get_leaderboard_data():
        con = sqlite3.connect("TimeStats.db")
        times_req = con.execute(
            f'''
                SELECT * FROM Players
                LEFT JOIN Times ON Players.steam_id=Times.steam_id
                LEFT JOIN "Split Times" ON Times.time_id = "Split Times".time_id
                ORDER BY checkpoint_num
            '''
        ).fetchall()
        times = [
            {
                "steam_id" : time[0],
                "steam_name" : time[1],
                "is_competitor" : time[2],
                #"steam_id" : time[3],
                "time_id" : time[4],
                "timestamp" : time[5],
                "trail_name" : time[6],
                "was_monitored" : time[7],
                "checkpoint_num" : time[9],
                "checkpoint_time" : time[10]
            }
            for time in times_req]
        return times

    @staticmethod
    def submit_time(steam_id, split_times, trail_name, being_monitored):
        con = sqlite3.connect("TimeStats.db")
        time_hash = hash(str(split_times[len(split_times)-1])+str(steam_id)+str(time.time()))
        con.execute(
            f'''
            INSERT INTO Times (steam_id, time_id, timestamp, trail_name, was_monitored)
            VALUES ("{steam_id}", "{time_hash}", {time.time()}, "{trail_name}", "{str(being_monitored)}")
            ''')
        for n, split_time in enumerate(split_times):
            con.execute(
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
            ''')
        con.commit()

    @staticmethod
    def end_session(steam_id, time_started, time_ended):
        con = sqlite3.connect("TimeStats.db")
        con.execute(
            f'''
            INSERT INTO Session (steam_id, time_started, time_ended)
            VALUES ("{steam_id}", "{time_started}", "{time_ended}")
            ''')
