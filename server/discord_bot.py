""" A discord bot used to interact to get times and such """
import logging
import time
import threading
import asyncio
import discord
from discord.ext import commands
from discord import HTTPException
from trail_timer import TrailTimer
from dbms import DBMS


split_timer_logger = logging.getLogger('DescendersSplitTimer')


def posh_time(seconds):
    """ Used to convert seconds to days, hours, minutes, and seconds """
    days = int(round(seconds / 86400))
    hours = int(round((seconds % 86400) / 3600))
    minutes = int(round((seconds % 3600) / 60))
    seconds = int(round(seconds % 60))
    return f"{days} days, {hours}h, {minutes}m, {seconds}s"


class DiscordBot(commands.Bot):
    """ DiscordBot class used to create discord bot instance """
    def __init__(self, discord_token, command_prefix, socket_server, dbms : DBMS):
        super().__init__(command_prefix, intents=discord.Intents().all())
        self.dbms = dbms
        self.command_prefix = command_prefix
        self.queue = {}
        self.socket_server = socket_server
        self.loop = asyncio.get_event_loop()
        self.loop.create_task(self.start(discord_token))
        self.changing_presence = False
        self.time_of_last_lux_request = 0
        self.command_outputs = {
            "!help": "this message",
            "!inspect <time_id>": "shows details of a particular run",
            "!top <number>" : "shows the top <number> of runs on a trail",
            "!totaltime" : "shows the total time spent on a trail entered",
            "!info <player_name>" : "shows info about a player"
        }
        threading.Thread(target=self.loop.run_forever).start()

    async def new_fastest_time(self, time_of_run):
        """ Used to send a message to the split time server giving the new fastest time """
        channel_id = 929121402597015685
        channel = self.get_channel(channel_id)
        await channel.send(time_of_run)

    async def ban_note(self, message):
        """ Used to send a message to the split time server dev channel of a ban record """
        channel_id = 997942390432206879
        channel = self.get_channel(channel_id)
        await channel.send(message)

    async def new_time(self, message):
        """ Used to send a message to the split time server dev channel of a ban record """
        channel_id = 1166082973385375744
        channel = self.get_channel(channel_id)
        await channel.send(message)

    async def set_presence(self, user_name: str):
        """ Used to set the presence of the discord bot """
        if not self.changing_presence:
            self.changing_presence = True
            try:
                await self.change_presence(
                    status=discord.Status.online,
                    activity=discord.Activity(
                        type=discord.ActivityType.watching,
                        name=user_name
                    )
                )
            except AttributeError:
                split_timer_logger.info("Failed to change presence")
            self.changing_presence = False

    async def on_message(self, message):
        split_timer_logger.info("Message sent '%s'", message)
        if message.author == self.user:
            return
        if message.content.startswith(self.command_prefix + "help"):
            hp = ""
            for command, description in self.command_outputs.items():
                hp += "`!" + command + "`\r - " + description + "\n"
            await message.channel.send(hp)
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
        if message.content.startswith(self.command_prefix + "inspect"):
            time_id = message.content.split(" ")[1]
            split_times = self.dbms.get_split_times(time_id)
            out = "__Inspection of " + time_id + "__\n\n"
            for i, split_time in enumerate(split_times):
                out += f"Checkpoint {i+1}: {str(split_time)}\n"
            await message.channel.send(out)
            out = f"{time_id} had a penalty of {self.dbms.get_penalty(time_id)}\n"
            await message.channel.send(out)
            out = f"{time_id} was on version {self.dbms.get_version(time_id)}\n"
            await message.channel.send(out)
            out = (
                "Replay should be found at "
                f"https://split-timer.nohumanman.com/static/replays/{time_id}.replay\n"
                )
            await message.channel.send(out)
        if message.content.startswith(self.command_prefix + "totaltime"):
            total_times = self.dbms.get_total_times()
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
            except HTTPException:
                await message.channel.send(
                    "Sorry - too many players to display!"
                )
        if message.author.id in [id for id in self.queue]:
            leaderboard = self.dbms.get_leaderboard(
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
                verified = player["verified"] == "1"
                if not verified: leaderboard_str += "||"
                leaderboard_str += f"{num} - {time1} - {name}"
                time_id = player['time_id']
                if not verified: leaderboard_str += f"  ----⚠️ [UNVERIFIED](https://split-timer.nohumanman.com/static/replays/{time_id}.replay) ⚠️ ----||"
                leaderboard_str += "\n"
            try:
                await message.channel.send(
                    f"Top {self.queue[message.author.id]}"
                    f" on {message.content}\n\n{leaderboard_str}"
                )
            except HTTPException:
                await message.channel.send(
                    "Sorry - too many players to display!"
                )
            self.queue.pop(message.author.id)
        if message.content.startswith(self.command_prefix + "info"):
            try:
                player_name = message.content[
                    len(self.command_prefix + "info "):
                ]
            except IndexError:
                player_name = "nohumanman"
            stats = self.dbms.get_player_stats(
                self.dbms.get_id_from_name(player_name)
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
