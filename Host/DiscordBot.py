from TrailTimer import TrailTimer
import discord
from discord.ext import commands
import threading
import asyncio
from DBMS import DBMS
import logging


class DiscordBot(commands.Bot):
    def __init__(self, discord_token, command_prefix, socket_server):
        super().__init__(command_prefix, intents=discord.Intents().all())
        self.command_prefix = command_prefix
        self.queue = {}
        self.socket_server = socket_server
        self.loop = asyncio.get_event_loop()
        self.loop.create_task(self.start(discord_token))
        threading.Thread(target=self.loop.run_forever).start()

    async def new_fastest_time(self, time):
        channel_id = 929121402597015685
        channel = self.get_channel(channel_id)
        await channel.send(time)

    async def ban_note(self, message):
        channel_id = 997942390432206879
        channel = self.get_channel(channel_id)
        await channel.send(message)

    async def watch_user(self, user_name: str):
        await self.change_presence(
            status=discord.Status.online,
            activity=discord.Activity(
                type=discord.ActivityType.watching,
                name=user_name
            )
        )

    async def on_ready(self):
        logging.info("Discord bot started.")
        await self.wait_until_ready()
        await self.change_presence(
            status=discord.Status.online,
            activity=discord.Activity(
                type=discord.ActivityType.watching,
                name="your times."
            )
        )

    async def on_message(self, message):
        if message.author == self.user:
            return
        if message.author.id in [id for id in self.queue]:
            leaderboard = DBMS.get_leaderboard(
                message.content,
                self.queue[message.author.id]
            )
            leaderboard_str = ""
            for i, player in enumerate(leaderboard):
                name = player["name"]
                time = TrailTimer.secs_to_str(float(player["time"]))
                num = ""
                if i == 0:
                    num = "🥇"
                elif i == 1:
                    num = "🥈"
                elif i == 2:
                    num = "🥉"
                else:
                    num = TrailTimer.ord(i + 1)
                bike = player["bike"]
                version = player["version"]
                leaderboard_str += f"{num} - {name} with {time} on {bike} (v{version})"
                if player['starting_speed'] is not None:
                    leaderboard_str += " (starting speed of "
                    st_time = round(float(player['starting_speed']), 2)
                    leaderboard_str += f"{st_time}) \n"
                else:
                    leaderboard_str += "\n"
            try:
                await message.channel.send(
                    f"Top {self.queue[message.author.id]}"
                    f" on {message.content}\n\n{leaderboard_str}"
                )
            except Exception:
                await message.channel.send(
                    "Sorry - too many players to display!"
                )
            self.queue.pop(message.author.id)
        if message.content.startswith(self.command_prefix + 'top'):
            try:
                num = int(message.content.split(" ")[1])
            except (IndexError, KeyError):
                num = 3
            self.queue[message.author.id] = num
            await message.channel.send(
                'Please enter the name of the trail you want to check.'
            )
