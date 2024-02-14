# create database
import unittest
import sqlite3
import aiosql

# pylint: disable=missing-function-docstring, no-member

class TestTrailTimer(unittest.TestCase):
    """ Used to test the trail timer class """
    def test_setup(self):
        """ Sets up the test """
        # Mocking network_player
        self.conn = sqlite3.connect(':memory:')
        self.cur = self.conn.cursor()
        # Read the contents of the schema file
        with open('server/src/schema.sql', 'r') as schema_file:
            schema_sql = schema_file.read()
        # Execute the schema SQL commands
        self.cur.executescript(schema_sql)
        self.queries = aiosql.from_path("server/src/queries.sql", "sqlite3")

    def test_get_player_avatar(self):
        """ Tests the started function """
        self.test_setup()
        self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('76561198282799591', 'Player123', 'avatar')")
        avatar_src = self.queries.get_player_avatar(self.conn, steam_id="76561198282799591")
        print(avatar_src)
        self.assertEqual(avatar_src[0], 'avatar')

    def test_check_restraint(self):
        self.test_setup()
        # we want to test if the restraint is working
        with self.assertRaises(sqlite3.IntegrityError):
            self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('123', 'Player123', 'avatar')")

    def test_update_player(self):
        self.test_setup()
        self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('76561198282799591', 'Player123', 'avatar')")
        self.queries.update_player(self.conn, steam_id="76561198282799591", steam_name="Player123", avatar_src="avatar")
        self.cur.execute("SELECT * FROM Player WHERE steam_id = '76561198282799591'")
        self.assertEqual(self.cur.fetchone(), ('76561198282799591', 'Player123', 'avatar'))

    def test_get_authenticated_discord_ids(self):
        self.test_setup()
        self.cur.execute("INSERT INTO User (discord_id, valid, steam_id, discord_name, email) VALUES ('123', 'TRUE', '76561198282799591', 'Player123', 'email')")
        self.cur.execute("SELECT discord_id FROM User WHERE valid = 'TRUE';")
        discord_ids = self.queries.get_authenticated_discord_ids(self.conn)
        self.assertEqual([str(x[0]) for x in list(discord_ids)], ['123'])

    def test_get_all_players(self):
        self.test_setup()
        self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('76561198282799591', 'Player123', 'avatar')")
        self.cur.execute("SELECT steam_id FROM Player;")
        steam_ids = list(self.queries.get_all_players(self.conn))
        print([str(x[0]) for x in steam_ids])
        self.assertEqual([str(x[0]) for x in steam_ids], ['76561198282799591'])

    def test_update_player(self):
        self.test_setup()
        self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('76561198282799591', 'Player123', 'avatar')")
        self.queries.update_player(self.conn, steam_id="76561198282799591", steam_name="differentplayername", avatar_src="differnetavatar")
        self.cur.execute("SELECT * FROM Player WHERE steam_id = '76561198282799591'")
        self.assertEqual(self.cur.fetchone(), ('76561198282799591', 'differentplayername', 'differnetavatar'))

    def test_get_player_name_from_id(self):
        self.test_setup()
        self.cur.execute("INSERT INTO Player (steam_id, steam_name, avatar_src) VALUES ('76561198282799591', 'Player123', 'avatar')")
        steam_name = self.queries.player_name_from_id(self.conn, steam_id="76561198282799591")
        self.assertEqual(steam_name[0], 'Player123')

if __name__ == '__main__':
    unittest.main()
