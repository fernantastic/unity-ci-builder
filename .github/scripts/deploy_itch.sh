#!/bin/bash

# Check arguments
BuildDirectory="$1"
Target="$2"
Version="$3"
ButlerPath="$4"

if [ -z "$BuildDirectory" ] || [ -z "$Target" ] || [ -z "$Version" ]; then
    echo "Error: Missing arguments."
    echo "Usage: ./deploy_itch.sh <BuildDirectory> <Target> <Version> [ButlerPath]"
    exit 1
fi

# Determine Butler executable
if [ -n "$ButlerPath" ]; then
    BUTLER_EXEC="$ButlerPath"
else
    BUTLER_EXEC="butler"
fi

# Ensure Butler is accessible
if ! "$BUTLER_EXEC" -V &> /dev/null; then
    echo "Error: Butler command not found at '$BUTLER_EXEC'. Please ensure the path is correct or butler is in the system PATH."
    exit 1
fi

echo "--- Deploying to itch.io ---"
echo "Directory: $BuildDirectory"
echo "Target: $Target"
echo "Version: $Version"
echo "Butler Path: $BUTLER_EXEC"

# BUTLER_API_KEY must be set as an environment variable
"$BUTLER_EXEC" push "$BuildDirectory" "$Target" --userversion "$Version"
exit_code=$?

if [ $exit_code -ne 0 ]; then
    echo "Error: Butler deployment failed with exit code $exit_code"
    exit 1
else
    echo "itch.io deployment successful!"
fi

exit 0

