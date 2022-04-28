import time
import math

class TrailTimer():
    def __init__(self, trail_name):
        self.trail_name = trail_name
        self.started = False
        self.times = []
        self.total_checkpoints = None
        self.__boundaries = []
        self.time_started = 0

    def add_boundary(self, boundary_guid):
        if len(self.__boundaries) == 0:
            self.invalidate_timer()
        if boundary_guid not in self.__boundaries:
            self.__boundaries.append(boundary_guid)

    def remove_boundary(self, boundary_guid):
        if boundary_guid in self.__boundaries:
            self.__boundaries.remove(boundary_guid)
        if len(self.__boundaries) == 0:
            self.invalidate_timer()

    def start_timer(self, total_checkpoints : int):
        self.started = True
        self.total_checkpoints = total_checkpoints
        self.time_started = time.time()
        self.times = []

    def checkpoint(self):
        if self.started:
            self.times.append(time.time() - self.time_started)

    def invalidate_timer(self):
        print("TIME INVALIDATED!!")
        self.started = False
        self.times = []

    def end_timer(self):
        if self.total_checkpoints is None:
            self.invalidate_timer()
        self.times.append(time.time() - self.time_started)
        if (len(self.times) == self.total_checkpoints-1):
            print(f"Times submitted: {self.times}")
            #PlayerDB.submit_time(self.player.steam_id, self.times, self.player.trail, self.player.monitored, self.player.world)
        self.started = False
        self.times = []

    def secs_to_str(self, secs):
        d_mins = int(round(secs // 60))
        d_secs = int(round(secs - (d_mins * 60)))
        d_millis = int(round(secs-math.trunc(secs), 3) * 1000)
        if len(str(d_mins)) == 1:
            d_mins = "0" + str(d_mins)
        if len(str(d_secs)) == 1:
            d_secs = "0" + str(d_secs)
        while len(str(d_millis)) < 3:
            d_millis = str(d_millis) + "0"
        return f"{d_mins}:{d_secs}.{d_millis}"

