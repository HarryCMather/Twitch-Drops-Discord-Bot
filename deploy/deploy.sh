#!/bin/bash

# I'm making a number of assumptions with this code:
# 1 - The current user who is deploying the Bot will have the following folder and file structure:
#     /home/user/TwitchDropsDiscordBot
#        TwitchDropsDiscordBot
#        Deploy
#        Settings.json
# 2 - The Settings.json contains all substituted variables, so will overwrite the extracted artifact's Settings.json file.
# 3 - 7-zip is installed, though this can be switched to other extraction tools based on preferences...
# 4 - The twitchdropsdiscordbot service has already been set-up as per the instructions in /deploy/twitchdropsdiscordbot.service in this repository.
# 5 - When this script is executed, the built artifact (7z files) from the build pipeline should have already been copied to "~/TwitchDropsDiscordBot/Deploy".
# 6 - The final "rm -r ./*" in theory isn't required, but I've added this to reduce disk space usage in the event of a failure.

echo "Switching directory to ~/TwitchDropsDiscordBot/Deploy...";
cd ~/TwitchDropsDiscordBot/Deploy;

echo "Extracting the Bot and removing the archive...";
7z x ./TwitchDropsDiscordBot.zip;
rm ./TwitchDropsDiscordBot.zip;

echo "Backing up the previous Alert History file...";
cp -f ../TwitchDropsDiscordBot/AlertHistory.txt ../AlertHistory.txt;

echo "Copying the Alert History file into deploy folder";
cp -f ../AlertHistory.txt ./AlertHistory.txt;

echo "Overwriting the Settings file with local copy...";
cp -f ../Settings.json ./Settings.json;

echo "Stopping the Bot...";
sudo service twitchdropsdiscordbot stop;

echo "Overwriting existing Bot files";
rm -r ../TwitchDropsDiscordBot/*;
mv ./* ../TwitchDropsDiscordBot/;

echo "Granting execute permission to the Bot...";
chmod +x ../TwitchDropsDiscordBot/TwitchDropsDiscordBot;

echo "Starting the Bot...";
sudo service twitchdropsdiscordbot start;

echo "Ensuring Deploy folder is empty...";
rm -r ./*;
