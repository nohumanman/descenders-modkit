from random import randint
from time import sleep


class Human():
    def __init__(self):
        self.alive = True
        self.stupid = True
        self.risk_level = 0

    def live(self):
        print("Living...")
        while True:
            self.risk_level = randint(0, 5)
            self.wait_for_next_day()

    def wait_for_next_day(self):
        sleep(86400)


you = Human()
you.live()
