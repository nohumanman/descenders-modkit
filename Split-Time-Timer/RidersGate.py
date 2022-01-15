import random

class RidersGate():
    def __init__(self):
        self.should_start = "false"
        self.random_delay = random.randint(1, 3)
        self.refresh_random_delay()
    
    def refresh_random_delay(self):
        new_rand = random.randint(1, 3)
        while new_rand == self.random_delay:
            new_rand = random.randint(1, 3)
        self.random_delay = new_rand
