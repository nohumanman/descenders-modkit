import time
from PlayerDB import PlayerDB


class TimerNotStarted(Exception):
    pass

class AntiCheatMeasure(Exception):
    pass

class TrailTimer():
    def __init__(self, player):
        self.started = False
        self.player = player
        self.times = []
        self.give = 0.5
        self.time_started = 0

    def start_timer(self, checkpoint):
        self.started = True
        self.total_checkpoints = checkpoint.total_checkpoints
        self.time_started = time.time()
        self.times = []

    def cancel_timer(self):
        self.started = False
        self.times = []

    def end_timer(self):
        self.started = False
        PlayerDB.submit_time(
            self.player.steam_id,
            self.times,
            self.player.trail,
            self.player.monitored,
            self.player.world
        )

    def split(self, client_time : float, anti_cheat=True):
        if not self.started:
            raise TimerNotStarted("Timer has not started! Unable to split!")
        else:
            server_time = time.time()-self.time_started
            # If the client's submitted time is within 500 milliseconds
            # of the server's time, then accept the client's time,
            # otherwise, throw an error.
            import logging
            logging.info(f"server time: {server_time}, client time: {client_time}")
            if self.__within(
                float(server_time),
                float(client_time),
                float(self.give)
            ) and anti_cheat:
                self.times.append(client_time)
            else:
                raise AntiCheatMeasure("Client Time and Server Time are too far apart - cheating suspected!")

    def __within(self, first_val : float, second_val : float, seconds_give : float):
        max_second_val = second_val + seconds_give
        min_second_val = second_val - seconds_give
        if (first_val >= min_second_val and first_val <= max_second_val):
            return True
        else:
            return False

