import discord
from discord.ext import commands
import threading
import asyncio
from DBMS import DBMS
import logging


class DiscordBot(commands.Bot):
    def __init__(self, discord_token, command_prefix, socket_server):
        super().__init__(command_prefix, intents=discord.Intents().all())
        self.queue = []
        self.socket_server = socket_server
        self.loop = asyncio.get_event_loop()
        self.loop.create_task(self.start(discord_token))
        threading.Thread(target=self.loop.run_forever).start()

    async def on_message(self, message):
        if message.author == self.user:
            return
        if message.author.id in self.queue:
            logging.info("Getting leaderboard")
            logging.info(message.content)
            leaderboard = DBMS.get_leaderboard(message.content, 5)
            logging.info("dmbs request successfull.")
            leaderboard_str = ""
            for i, player in enumerate(leaderboard):
                name = player["steam_name"]
                time = player["total_time"]
                leaderboard_str += f"{str(i+1)} - {name} with {time}\n"
            logging.info(leaderboard_str)
            await message.channel.send(
                f"Top five on {message.content}\n\n{leaderboard_str}"
            )
            self.queue.remove(message.author.id)
        if message.content.startswith('!top'):
            self.queue.append(message.author.id)
            await message.channel.send(
                'Please enter the name of the trail you want to check.'
            )
