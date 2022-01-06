import sqlite3
import time
import datetime

class TombstoneHandler():
    def __init__(self):
        pass

    def get_tombstones_for_map(self, map, just_today, just_me, just_recent_deaths, steamId):
        db = sqlite3.connect("TombstoneDatabase.db")
        potential_1, potential_2 = "", ""
        potential_3, potential_4 = "", ""
        if just_today:
            smallestPossibleTimestamp = time.time() - 86400
            potential_2 = f"AND Timestamp > {smallestPossibleTimestamp}"
        if just_me:
            potential_3 = f"AND SteamId = \"{steamId}\""
        if just_recent_deaths:
            potential_1 = ", Max(Timestamp)"
            potential_4 = "GROUP BY SteamId"
        formula = f'''
        SELECT * {potential_1}
        FROM Tombstones
        WHERE map = "{map}"
        {potential_2}
        {potential_3}
        {potential_4}
        '''
        results = db.execute(formula).fetchall()
        steamNames = [result[1] for result in results]
        mapNames = [result[0] for result in results]
        dates = [result[2] for result in results]
        steamIds = [result[3] for result in results]
        posX = [result[4] for result in results]
        posY = [result[5] for result in results]
        posZ = [result[6] for result in results]
        timestamp = [result[7] for result in results]
        return {"steamNames" : steamNames, "dates" : dates, "steamIds" : steamIds, "xPoss" : posX, "yPoss" : posY, "zPoss" : posZ}

    def add_new_death_to_map(self, map, pos_x, pos_y, pos_z, steam_name, steam_id):
        db = sqlite3.connect("TombstoneDatabase.db")
        now = datetime.datetime.now()
        months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']
        month = months[now.month-1]
        date = now.day
        year = now.year
        date = "{date}th {month}, {year}".format(
            date=date,
            month=month,
            year=year
        )
        timestamp = time.time()
        db.execute('''INSERT INTO Tombstones VALUES("{map}", "{steam_name}","{date}", "{steam_id}", "{pos_x}", "{pos_y}", "{pos_z}", "{timestamp}")'''.format(
            map=map,
            steam_name= str(steam_name),
            date = str(date),
            steam_id = str(steam_id),
            pos_x = str(pos_x),
            pos_y = str(pos_y),
            pos_z = str(pos_z),
            timestamp = str(timestamp)
        ))
        db.commit()


# for testing:
#x = TombstoneHandler().add_new_death_to_map("Igloo Bike Park", "123", "152", "1", "nohumanman", "123091239012")
#x = TombstoneHandler().get_tombstones_for_map("Igloo Bike Park", mode="only_today")
# print(x)
