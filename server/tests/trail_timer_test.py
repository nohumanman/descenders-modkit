import unittest
from unittest.mock import AsyncMock
from src.trail_timer import TrailTimer
import asyncio

class TestTrailTimer(unittest.IsolatedAsyncioTestCase):
    def setUp(self):
        self.network_player = AsyncMock()
        self.timer = TrailTimer("Trail 1", self.network_player)

    async def test_add_boundary(self):
        self.timer.timer_info.started = True
        self.timer.network_player.send = AsyncMock()
        await self.timer.add_boundary("boundary1")
        self.assertEqual(len(self.timer._TrailTimer__boundaries), 1)
        self.assertFalse(self.timer.timer_info.auto_verify)
        self.timer.network_player.send.assert_called_with("SPLIT_TIME|Time requires review")

    async def test_remove_boundary(self):
        self.timer._TrailTimer__boundaries = ["boundary1", "boundary2"]
        self.timer.timer_info.started = True
        self.timer.network_player.send = AsyncMock()
        await self.timer.remove_boundary("boundary1")
        self.assertEqual(len(self.timer._TrailTimer__boundaries), 1)
        self.assertTrue(self.timer.timer_info.auto_verify)

    async def test_remove_boundary_invalidates(self):
        self.timer._TrailTimer__boundaries = ["boundary1"]
        self.timer.timer_info.started = True
        self.timer.network_player.send = AsyncMock()
        await self.timer.remove_boundary("boundary1")
        self.assertEqual(len(self.timer._TrailTimer__boundaries), 0)
        self.assertFalse(self.timer.timer_info.auto_verify)
        self.timer.network_player.send.assert_called_with("SPLIT_TIME|Time requires review")


    async def test_reset_boundaries(self):
        self.timer._TrailTimer__boundaries = ["boundary1", "boundary2"]
        self.timer.reset_boundaries()
        self.assertEqual(len(self.timer._TrailTimer__boundaries), 0)

    async def test_start_timer_with_no_bounds(self):
        self.timer.timer_info.started = False
        self.timer.timer_info.times = [1.0, 2.0, 3.0]
        self.timer.invalidate_timer = AsyncMock()
        await self.timer.start_timer(5)
        self.assertEqual(self.timer.timer_info.total_checkpoints, 0)
        self.assertEqual(len(self.timer._TrailTimer__checkpoints), 0)
        self.assertEqual(self.timer.timer_info.times, [1.0, 2.0, 3.0])
        self.timer.invalidate_timer.assert_called_with("OUT OF BOUNDS!", always=True)

    async def test_start_timer(self):
        self.timer.add_boundary("boundary1")
        self.timer.timer_info.started = False
        self.timer.invalidate_timer = AsyncMock()
        await self.timer.add_boundary("boundary1")
        await self.timer.start_timer(5)
        self.timer.timer_info.times = [1.0, 2.0, 3.0]
        self.assertEqual(self.timer.timer_info.auto_verify, True)
        self.assertEqual(self.timer.timer_info.total_checkpoints, 5)
        self.assertEqual(len(self.timer._TrailTimer__checkpoints), 0)
        self.assertEqual(self.timer.timer_info.times, [1.0, 2.0, 3.0])

    async def test_can_end(self):
        self.timer._TrailTimer__boundaries = []
        self.timer.timer_info.times = [1.0, 2.0, 3.0]
        self.timer.timer_info.started = True
        result = await self.timer.can_end()
        self.assertFalse(result[0])
        self.assertEqual(result[1], "ERR001: Out of Bounds")

if __name__ == '__main__':
    unittest.main()