""" Used to test the trail timer class """
import unittest
from unittest.mock import AsyncMock
from src.dbms import DBMS

class TestDBMS(unittest.IsolatedAsyncioTestCase):
    """ Used to test the trail timer class """
    def setUp(self):
        """ Sets up the test """
        self.dbms: DBMS = DBMS("modkit_test.db")

    async def test_get_id(self):
        """ Tests the started function """
        steam_id = await self.dbms.get_id_from_name("nohumanman")
        self.assertEqual(steam_id, "76561198282799591")

    async def test_update_player(self):
        await self.dbms.update_player("76561198282799591", "nohumanman", "before-change")
        self.assertEqual(
            await self.dbms.get_avatar("76561198282799591"),
            "before-change"
        )
        await self.dbms.update_player("76561198282799591", "nohumanman", "new-avatar-src")
        self.assertEqual(
            await self.dbms.get_avatar("76561198282799591"),
            "new-avatar-src"
        )

if __name__ == '__main__':
    unittest.main()
