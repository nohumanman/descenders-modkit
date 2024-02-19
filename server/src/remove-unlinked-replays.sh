#!/bin/bash

sqlite3 modkit.db 'select time_id from Time where ignored = 0' > static/replays/all_time_ids

# cd to replays directory
cd static/replays

# Read file names from all_time_ids and iterate through them
while IFS= read -r file_name; do
    if [ ! -e "${file_name}.replay" ]; then
        echo "Replay $file_name not found. Deleting now."
        sqlite3 "../../modkit.db" "UPDATE Time SET ignored = 1 WHERE time_id='$file_name';";
    fi
done < all_time_ids

rm all_time_ids;

echo "ignored all times that have no replay file"

mkdir -p archived-replays/

# delete all replays that are not in the database
for file in *.replay; do
    valid=$(sqlite3 "../../modkit.db" "SELECT * FROM Time WHERE time_id = ${file%.*};");
    if [ -z "$valid" ]; then
        echo "Replay $file not found in database. Deleting now."
        mv -- "$file" archived-replays/
    fi
done

echo "archived all replays that are not in the database"
