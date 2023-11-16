""" Used to test the trail timer class """
import unittest
from trail_timer import TrailTimer

class TestTrailTimer(unittest.TestCase):

    def test_posh_time(self):
        """ Tests the posh_time function """
        tests = [
            [0, "00:00.000"],
            [0.9999, "00:01.000"],
            [1, "00:01.000"],
            [1.5, "00:01.500"],
            [2, "00:02.000"],
            [10, "00:10.000"],
            [60, "01:00.000"],
            [61, "01:01.000"],
            [61.5, "01:01.500"],
            [61.9999, "01:02.000"],
            [61.9999999, "01:02.000"],
            
            # Testing edge cases
            [0.000001, "00:00.000"],
            [0.0000001, "00:00.000"],
        ]
        for test in tests:
            self.assertEqual(
                TrailTimer.secs_to_str(test[0]),
                test[1]
            )

if __name__ == '__main__':
    unittest.main()
