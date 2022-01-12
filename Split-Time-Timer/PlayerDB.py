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
    def become_competitor(steam_id):
        print("Function not complete - become_competitor")

    @staticmethod
    def submit_time(steam_id, split_times, trail_name):
        con = sqlite3.connect("TimeStats.db")
        time_hash = hash(str(split_times[len(split_times)-1])+str(steam_id)+str(time.time()))
        con.execute(
            f'''
            INSERT INTO Times (steam_id, time_id, total_time, timestamp, trail_name)
            VALUES ("{steam_id}", "{time_hash}", "{split_times[len(split_times)-1]}", {time.time()}, "{trail_name}")
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
