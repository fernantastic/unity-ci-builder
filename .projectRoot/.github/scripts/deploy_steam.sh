#!/bin/bash

# Check arguments
SteamAppID="$1"
BuildDirectory="$2"

if [ -z "$SteamAppID" ]; then
    echo "Error: Missing Steam App ID."
    echo "Usage: ./deploy_steam.sh <SteamAppID> [BuildDirectory]"
    exit 1
fi

# Ensure SteamCMD is accessible
if ! command -v steamcmd &> /dev/null; then
    echo "Error: SteamCMD command not found. Please ensure steamcmd is in the system PATH."
    exit 1
fi

VDFPath="$VDF_PATH"

if [ ! -f "$VDFPath" ]; then
    echo "Error: Steam build VDF file not found at $VDFPath. Aborting deployment."
    exit 1
fi

echo "--- Deploying to Steam ---"
echo "App ID: $SteamAppID"
echo "VDF Config: $VDFPath"

# STEAM_USER and STEAM_PASSWORD must be set as environment variables
steamcmd +login "$STEAM_USER" "$STEAM_PASSWORD" +run_app_build "$VDFPath" +quit
exit_code=$?

if [ $exit_code -ne 0 ]; then
    echo "Error: SteamCMD deployment failed with exit code $exit_code"
    exit 1
else
    echo "Steam deployment successful!"
fi

exit 0


