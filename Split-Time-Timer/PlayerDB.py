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
    def become_competitor(steam_id, is_competitor):
        con = sqlite3.connect("TimeStats.db")
        if is_competitor:
            con.execute(
                f'''
                REPLACE INTO Players (is_competitor)
                VALUES ("true") WHERE steam_id = "{steam_id}"
                '''
            )
        else:
            con.execute(
                f'''
                REPLACE INTO Players (is_competitor)
                VALUES ("false") WHERE steam_id = "{steam_id}"
                '''
            )
        print("Function not complete - become_competitor")

    @staticmethod
    def get_fastest_split_times(trail_name, competitors_only=False):
        con = sqlite3.connect("TimeStats.db")
        temp = -5555490718959231365
        print("Getting fastest split times...")
        times = con.execute(
            f'''
            SELECT * FROM "Split Times"
            WHERE time_id = "{temp}" ORDER BY checkpoint_num
            '''
        ).fetchall()
        print(times)
        split_times = [time[2] for time in times]
        print(times)
        con.close()
        return split_times

    @staticmethod
    def submit_time(steam_id, split_times, trail_name):
        con = sqlite3.connect("TimeStats.db")
        time_hash = hash(str(split_times[len(split_times)-1])+str(steam_id)+str(time.time()))
        con.execute(
            f'''
            INSERT INTO Times (steam_id, time_id, timestamp, trail_name)
            VALUES ("{steam_id}", "{time_hash}", {time.time()}, "{trail_name}")
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
