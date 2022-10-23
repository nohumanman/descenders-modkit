from TrailTimer import TrailTimer
import discord
from discord.ext import commands
import threading
import asyncio
from DBMS import DBMS
import logging
import time

split_timer_logger = logging.getLogger('DescendersSplitTimer')


def posh_time(seconds):
    days = int(round(seconds / 86400))
    hours = int(round((seconds % 86400) / 3600))
    minutes = int(round((seconds % 3600) / 60))
    seconds = int(round(seconds % 60))
    return f"{days} days, {hours}h, {minutes}m, {seconds}s"


class DiscordBot(commands.Bot):
    def __init__(self, discord_token, command_prefix, socket_server):
        super().__init__(command_prefix, intents=discord.Intents().all())
        self.command_prefix = command_prefix
        self.queue = {}
        self.socket_server = socket_server
        self.loop = asyncio.get_event_loop()
        self.loop.create_task(self.start(discord_token))
        self.time_of_last_lux_request = 0
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
        split_timer_logger.info("DiscordBot.py - Discord bot started.")
        await self.wait_until_ready()

    async def on_message(self, message):
        split_timer_logger.info(f"DiscordBot.py - Message sent '{message}'")
        if message.author == self.user:
            return
        if (
            (
                (
                    "get" in message.content.lower()
                    or "how" in message.content.lower()
                )
                and (
                    "lux" in message.content.lower()
                    or "tron" in message.content.lower()
                )
            )
            and (time.time() - self.time_of_last_lux_request) > 60
        ):
            await message.channel.send(
                f"oi <@!{message.author.id}> look at "
                "https://youtu.be/NZ9qHsONS20"
            )
            self.time_of_last_lux_request = time.time()
        if message.content.startswith(self.command_prefix + "totaltime"):
            total_times = DBMS.get_total_times()
            text = ""
            i = 1
            for total_time in total_times:
                steam_name = total_time[1]
                act_time = float(round(total_time[2]))
                text += (
                    f"{i}. **{steam_name}** has "
                    f"spent {posh_time(act_time)} racing\n")
                i += 1
            try:
                await message.channel.send(text)
            except Exception:
                await message.channel.send(
                    "Sorry - too many players to display!"
                )
        if message.author.id in [id for id in self.queue]:
            leaderboard = DBMS.get_leaderboard(
                message.content,
                self.queue[message.author.id]
            )
            leaderboard_str = ""
            for i, player in enumerate(leaderboard):
                name = "**" + player["name"] + "**"
                time1 = TrailTimer.secs_to_str(float(player["time"]))
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
                penalty = player["penalty"]
                leaderboard_str += f"{num} - {name} with "
                leaderboard_str += f"{time1} on {bike} (v{version})"
                leaderboard_str += f" penalty of {penalty}"
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
        if message.content.startswith(self.command_prefix + "info"):
            try:
                player_name = message.content[
                    len(self.command_prefix + "info "):
                ]
            except Exception:
                player_name = "nohumanman"
            stats = DBMS.get_player_stats(
                DBMS.get_id_from_name(player_name)
            )[0]
            embed = discord.Embed()
            embed.add_field(name="Current rep", value=stats[2])
            embed.add_field(name="Times logged on", value=stats[4])
            embed.add_field(name="Trails Ridden", value=stats[5])
            embed.add_field(
                name="Total Time",
                value=posh_time(float(round(stats[6])))
            )
            embed.add_field(name="Total Top Places", value="0 lol")
            embed.set_footer(text="Stats for " + str(stats[0]))
            embed.set_author(
                name=f"Stats for {stats[1]}",
                url=f"https://steamcommunity.com/profiles/{stats[0]}/",
                icon_url=stats[7]
            )
            await message.channel.send(embed=embed)

        if message.content.startswith(self.command_prefix + 'top'):
            try:
                num = int(message.content.split(" ")[1])
            except (IndexError, KeyError):
                num = 3
            self.queue[message.author.id] = num
            await message.channel.send(
                'Please enter the name of the trail you want to check.'
            )
