#!/bin/bash

# Configuration
PROJECT_PATH="./Vjezba.Web"  # UPDATE THIS PATH
SERVER_USER="leon"
SERVER_HOST="tockanetfotorulet.ddns.net"
PACKAGE_NAME="vjezba-app.tar.gz"

echo "=== Building and Deploying Vjezba Application ==="

# Navigate to project directory
echo "Navigating to project directory: $PROJECT_PATH"
cd "$PROJECT_PATH" || {
    echo "Error: Project directory not found: $PROJECT_PATH"
    echo "Please update PROJECT_PATH in this script"
    exit 1
}

# Clean previous build
echo "Cleaning previous build..."
rm -rf ./publish
rm -f "$PACKAGE_NAME"

# Build and publish
echo "Building and publishing application..."
dotnet publish -c Release -o ./publish

if [ $? -ne 0 ]; then
    echo "Error: Build failed"
    exit 1
fi

# Create deployment package
echo "Creating deployment package..."
tar -czf "$PACKAGE_NAME" -C ./publish .

if [ $? -ne 0 ]; then
    echo "Error: Failed to create package"
    exit 1
fi

# Transfer to server
echo "Transferring package to server..."
scp "$PACKAGE_NAME" "${SERVER_USER}@${SERVER_HOST}:/home/${SERVER_USER}/"

if [ $? -ne 0 ]; then
    echo "Error: Failed to transfer package to server"
    exit 1
fi

echo "✅ Package successfully transferred to server!"
echo "Now run the update script on the server: ssh ${SERVER_USER}@${SERVER_HOST} './update-app.sh'"

# Optional: Automatically run server script
read -p "Do you want to automatically run the server update script? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Running server update script..."
    ssh "${SERVER_USER}@${SERVER_HOST}" "./update-app.sh"
fi

echo "=== Deployment Complete ==="
