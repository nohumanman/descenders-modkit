""" Used to test the trail timer class """
import unittest
from unittest.mock import AsyncMock
from src.trail_timer import TrailTimer

class TestTrailTimer(unittest.IsolatedAsyncioTestCase):
    """ Used to test the trail timer class """
    def setUp(self):
        """ Sets up the test """
        # Mocking network_player
        self.network_player = AsyncMock()
        self.network_player.info.steam_id = "123"
        self.network_player.info.steam_name = "Player123"
        self.network_player.info.world_name = "World1"
        self.network_player.info.bike_type = "Mountain"
        self.network_player.info.version = "1.0"

        # Creating TrailTimer instance
        self.trail_timer : TrailTimer = TrailTimer("Trail1", self.network_player)

    async def test_started(self):
        """ Tests the started function """
        self.assertFalse(self.trail_timer.timer_info.started)
        await self.trail_timer.add_boundary(0)
        await self.trail_timer.start_timer(5)
        self.assertTrue(self.trail_timer.timer_info.started)

    async def test_start_timer(self):
        """ Tests the start_timer function """
        await self.trail_timer.add_boundary(0)
        await self.trail_timer.start_timer(5)
        self.assertTrue(self.trail_timer.timer_info.started)

    async def test_end_timer(self):
        """ Tests the stop_timer function """
        await self.trail_timer.add_boundary(0)
        await self.trail_timer.start_timer(5)
        self.assertTrue(self.trail_timer.timer_info.started)
        await self.trail_timer.end_timer(20)
        self.assertFalse(self.trail_timer.timer_info.started)

    async def test_add_boundary(self):
        """ Tests the stop_timer function """
        await self.trail_timer.start_timer(5)
        self.assertFalse(self.trail_timer.timer_info.started)

    async def test_boundary_error(self):
        """ Tests the stop_timer function """
        await self.trail_timer.add_boundary(0)
        await self.trail_timer.start_timer(5)
        self.assertTrue(self.trail_timer.timer_info.started)

    async def test_submit_time(self):
        """ Tests the submit_time function """
        await self.trail_timer.add_boundary(0)
        await self.trail_timer.start_timer(5)
        self.assertTrue(self.trail_timer.timer_info.started)
        await self.trail_timer.end_timer(20)
        self.assertFalse(self.trail_timer.timer_info.started)

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
