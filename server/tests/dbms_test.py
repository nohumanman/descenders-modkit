import asyncio
import unittest
from unittest.mock import MagicMock
from src.dbms import DBMS

class TestDBMS(unittest.TestCase):
    def setUp(self):
        self.dbms = DBMS(":memory:", "server/src/queries.sql")

    def tearDown(self):
        pass

    def test_execute_sql(self):
        statement = "SELECT * FROM users"
        result = asyncio.run(self.dbms.execute_sql(statement))
        self.assertEqual(result, None)

    def test_get_id_from_name(self):
        steam_name = "john123"
        result = asyncio.run(self.dbms.get_id_from_name(steam_name))
        self.assertEqual(result, "12345")

    def test_get_replay_name_from_id(self):
        time_id = "67890"
        result = asyncio.run(self.dbms.get_replay_name_from_id(time_id))
        self.assertEqual(result, "replay1")

    def test_update_player(self):
        steam_id = "12345"
        steam_name = "john123"
        avatar_src = "avatar.jpg"
        result = asyncio.run(self.dbms.update_player(steam_id, steam_name, avatar_src))
        self.assertEqual(result, True)

    # Add more test cases for other methods...

if __name__ == '__main__':
    unittest.main()