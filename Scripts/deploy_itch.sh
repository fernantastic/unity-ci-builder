#!/bin/bash

# Check arguments
BuildDirectory="$1"
Target="$2"
Version="$3"

if [ -z "$BuildDirectory" ] || [ -z "$Target" ] || [ -z "$Version" ]; then
    echo "Error: Missing arguments."
    echo "Usage: ./deploy_itch.sh <BuildDirectory> <Target> <Version>"
    exit 1
fi

# Ensure Butler is accessible
if ! command -v butler &> /dev/null; then
    echo "Error: Butler command not found. Please ensure butler is in the system PATH."
    exit 1
fi

echo "--- Deploying to itch.io ---"
echo "Directory: $BuildDirectory"
echo "Target: $Target"
echo "Version: $Version"

# BUTLER_API_KEY must be set as an environment variable
butler push "$BuildDirectory" "$Target" --userversion "$Version"
exit_code=$?

if [ $exit_code -ne 0 ]; then
    echo "Error: Butler deployment failed with exit code $exit_code"
    exit 1
else
    echo "itch.io deployment successful!"
fi

exit 0

