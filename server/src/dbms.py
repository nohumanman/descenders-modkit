""" Database Management System """
import time
import os
from datetime import datetime, timedelta
import logging
import aiosqlite
import aiosql

script_path = os.path.dirname(os.path.realpath(__file__))


def daterange(start_date: datetime, end_date: datetime):
    """ Generate a range of dates between the given start_date and end_date """
    for n in range(int((end_date - start_date).days)):
        yield start_date + timedelta(n)

class DBMS():
    """ A simple Database Management System (DBMS) class for managing data. """
    def __init__(self, db_file: str = "modkit.db", queries_file: str = "queries.sql"):
        self.db_file = script_path + f"/{db_file}"
        self.wait = False
        self.queries = aiosql.from_path(queries_file, "aiosqlite")

    async def execute_sql(self, statement: str, write=False):
        """ Execute an SQL statement on a database """
        return

    async def get_id_from_name(self, steam_name):
        """ Get the Steam ID associated with a given Steam username. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_replay_name(
                db,
                steam_name=steam_name
            )

    async def get_replay_name_from_id(self, time_id):
        """ Get the replay name associated with a given time. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_replay_name(
                db,
                time_id=time_id
            )

    async def update_player(self, steam_id, steam_name, avatar_src):
        """ Update the player's name and avatar """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.update_player(
                db,
                steam_id=steam_id,
                steam_name=steam_name,
                avatar_src=avatar_src
            )

    async def get_personal_fastest_split_times(self, trail_name: str, steam_id: int):
        """ Get the fastest split times for a given player on a given trail. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            splits = await self.queries.get_pb_split_times(
                db,
                trail_name=trail_name,
                steam_id=steam_id
            )
        return [float(time[0]) for time in splits]

    async def get_fastest_split_times(self,
        trail_name
    ):
        """ Get the fastest split times for a given trail. """
        async with aiosqlite.connect(self.db_file) as conn:
            # pylint: disable=no-member
            splits = await self.queries.get_wr_split_times(
                conn,
                trail_name=trail_name
            )
        return [float(time[0]) for time in splits]

    async def get_valid_ids(self):
        """ Get the valid Discord IDs. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_authenticated_discord_ids(db)

    async def get_steam_id(self, discord_id):
        """ Get the Steam ID associated with a given Discord ID. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_discord_steam_connetion(
                db,
                discord_id=discord_id
            )

    async def get_player_name(self, steam_id):
        """ Get the Steam name associated with a given Steam ID. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.player_name_from_id(
                db,
                steam_id=steam_id
            )

    async def get_time_details(self, time_id: str):
        """ Get the details of a given time. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_time_details(
                db,
                time_id=time_id
            )

    async def get_all_times(self, limit: int):
        """ Get all times. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            result = await self.queries.get_all_times(
                db,
                lim=limit
            )
        return [
            {

                "steam_id": str(time[0]),
                "steam_name": time[1],
                "avatar_src": time[2],
                "timestamp": time[3],
                "time_id": str(time[4]),
                "total_checkpoints": time[5],
                "total_time": time[6],
                "trail_name": time[7],
                "world_name": time[8],
                "ignore": time[9],
                "bike_type": time[10],
                "starting_speed": time[11],
                "version": time[12],
                "verified": time[14]
            }
            for time in result
        ]

    async def get_all_players(self):
        """ Get all players. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_all_players(db)

    async def get_trails(self):
        """ Get all trails."""
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return [{
                "trail_name": trail[0],
                "world_name": trail[1],
                "times_ridden": trail[2],
                "leaderboard": await self.get_leaderboard(trail[0]),
                "average_start_speed": trail[3],
                "src": trail[4]
            } for trail in await self.queries.get_all_trails(db)]

    async def get_worlds(self):
        """ Get all worlds. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return [world[0] for world in await self.queries.get_all_worlds(db)]

    async def get_leaderboard(self, trail_name, num=10) -> list:
        """ Get the leaderboard for a given trail. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return [
                {
                    "place": i + 1,
                    "time": time[3],
                    "name": time[1],
                    "bike": time[2],
                    "starting_speed": time[0],
                    "version": time[4],
                    "verified": str(time[5]),
                    "time_id": str(time[6])
                } for i, time in enumerate(
                    await self.queries.get_leaderboard(
                        db, trail_name=trail_name, lim=num
                    )
                )
            ]

    async def max_start_time(self, trail_time: str) -> float:
        """ Get the maximum starting time for a given trail. """
        return 10

    async def get_avatar(self, steam_id):
        """ Get the avatar for a given Steam ID. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.get_player_avatar(db, steam_id=steam_id)

    async def discord_login(self, discord_id: str, discord_name: str, email: str, steam_id: int):
        """ Log in to Discord. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            await self.queries.submit_discord_details(
                db,
                discord_id=discord_id,
                discord_name=discord_name,
                email=email,
                steam_id=steam_id
            )

    async def verify_time(self, time_id: str):
        """ Verify a given time. """
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            return await self.queries.verify_time(db, time_id=time_id)

    async def submit_time(
        self,
        steam_id: str,
        split_times,
        trail_name: str,
        current_world: str,
        bike_type: str,
        starting_speed: float,
        version: str,
        verified: bool,
        ignored: bool
    ):
        """ Submit a time to the database. """
        time_id = hash(
            str(split_times[len(split_times)-1])
            + str(steam_id)
            + str(time.time())
        )
        logging.info(
            '''
            self.queries.submit_time(
                db,
                steam_id=str(%s),
                time_id=int(%s),
                timestamp=float(%s),
                world_name=str(%s),
                trail_name=str(%s),
                bike_type=str(%s),
                starting_speed=float(starting_speed),
                version=str(version),
                verified=lambda: 1 if %s else 0,
                ignored=%s,
            ''', steam_id, time_id, time.time(), current_world,
            trail_name, bike_type, verified, ignored
        )
        async with aiosqlite.connect(self.db_file) as db:
            # pylint: disable=no-member
            await self.queries.submit_time(
                db,
                steam_id=str(steam_id),
                time_id=int(time_id),
                timestamp=float(time.time()),
                world_name=str(current_world),
                trail_name=str(trail_name),
                bike_type=str(bike_type),
                starting_speed=float(starting_speed),
                version=str(version),
                verified= 1 if verified else 0,
                ignored= 1 if ignored else 0
            )
            for n, split_time in enumerate(split_times):
                # pylint: disable=no-member
                await self.queries.submit_split(
                    db,
                    time_id=int(time_id),
                    checkpoint_num=int(n),
                    checkpoint_time=float(split_time)
                )
            await db.commit()
        return time_id

    async def get_medals(self, steam_id, trail_name):
        """ Get the medals for a given trail. """
        x = f'''
            SELECT
                starting_speed,
                steam_name,
                bike_type,
                MIN(checkpoint_time),
                Time.version
            FROM
                Time
                INNER JOIN
                    SplitTime ON SplitTime.time_id = Time.time_id
                INNER JOIN
                    (
                        SELECT
                            max(checkpoint_num) AS max_checkpoint
                        FROM
                            SplitTime
                            INNER JOIN
                                Time ON Time.time_id = SplitTime.time_id
                            WHERE LOWER(Time.trail_name) = LOWER(
                                "{trail_name}"
                            )
                    ) ON SplitTime.time_id=Time.time_id
                INNER JOIN
                    Player ON Player.steam_id = Time.steam_id
            WHERE
                LOWER(trail_name) = LOWER("{trail_name}")
                AND
                checkpoint_num = max_checkpoint
                AND
                (Time.ignored = 0 AND Time.verified = 1)
                AND
                Player.steam_id = {steam_id}
            GROUP BY
                trail_name,
                Player.steam_id
            ORDER BY
                checkpoint_time ASC
        '''
        result = await self.execute_sql(x)
        if len(result) == 0:
            return [False, False, False, False]
        y = f'''
            SELECT medal_type, time
            FROM TrailMedal
            WHERE trail_name = "{trail_name}"
        '''
        rainbow_time = 0
        gold_time = 0
        silver_time = 0
        bronze_time = 0
        for medal in await self.execute_sql(y):
            if medal[0] == "rainbow":
                rainbow_time = float(medal[1])
            elif medal[0] == "gold":
                gold_time = float(medal[1])
            elif medal[0] == "silver":
                silver_time = float(medal[1])
            elif medal[0] == "bronze":
                bronze_time = float(medal[1])
        # rainbow, gold, silver, bronze
        to_return = [False, False, False, False]
        for player_time in result:
            to_return[0] = player_time[3] <= rainbow_time
            to_return[1] = player_time[3] <= gold_time
            to_return[2] = player_time[3] <= silver_time
            to_return[3] = player_time[3] <= bronze_time
        return to_return
