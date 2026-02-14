#!/bin/bash

# Configuration
CONFIG="Release"
OUTPUT_DIR="./packages"

echo "Cleaning existing packages..."
rm -rf $OUTPUT_DIR
mkdir -p $OUTPUT_DIR

# Shared Projects
echo "Building and packing Shared projects..."
dotnet pack src/Shared/DiscountManager.Shared.SharedKernel/DiscountManager.Shared.SharedKernel.csproj --configuration $CONFIG --output $OUTPUT_DIR

# Modules
echo "Building and packing Modules..."
for dir in src/Modules/*/ ; do
    if [ -d "$dir" ]; then
        project_file=$(find "$dir" -maxdepth 2 -name "*.csproj")
        if [ -n "$project_file" ]; then
            echo "Processing $project_file..."
            dotnet pack "$project_file" --configuration $CONFIG --output $OUTPUT_DIR
        fi
    fi
done

echo "Done! Packages are available in $OUTPUT_DIR"
