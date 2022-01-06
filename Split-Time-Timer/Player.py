import time
from threading import Thread


class Player():
    def __init__(self, name, id):
        self.name = name
        self.id = id
        self.checkpoint_times = []
        self.current_trail = None
        self.time_started = None
        self.total_checkpoints = None
        self.has_entered_checkpoint = False

    def entered_checkpoint(self, time, trail_name):
        if self.time_started is not None:
            self.checkpoint_times.append(time)
            self.has_entered_checkpoint = True
            self.current_trail = trail_name
            Thread(target=self.disable_entered_checkpoint, args=(5,)).start()

    def finished_trail(self, time, trail_name):
        pass

    def started_trail(self, time, trail_name, total_checkpoints):
        self.current_trail = trail_name
        self.total_checkpoints = 0
        self.time_started = time

    def disable_entered_checkpoint(self, delay):
        time.sleep(delay)
        self.has_entered_checkpoint = False
