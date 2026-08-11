#!/bin/bash

# Database Backup Service - Docker Quick Start Script

echo "=================================="
echo "Database Backup Service - Docker"
echo "=================================="
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo "Creating .env file from template..."
    cp .env.template .env
    echo ""
    echo "⚠️  Please edit .env file with your configuration before running!"
    echo "   Run: nano .env"
    echo ""
    exit 1
fi

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker and try again."
    exit 1
fi

echo "✅ Docker is running"
echo ""

# Ask what to do
echo "What would you like to do?"
echo "1) Build and start the container"
echo "2) Start existing container"
echo "3) Stop the container"
echo "4) View logs"
echo "5) Rebuild container"
echo "6) Remove container and volumes"
echo ""
read -p "Enter your choice (1-6): " choice

case $choice in
    1)
        echo ""
        echo "Building and starting container..."
        docker-compose up -d --build
        echo ""
        echo "✅ Container started!"
        echo "   View logs with: docker-compose logs -f"
        ;;
    2)
        echo ""
        echo "Starting container..."
        docker-compose up -d
        echo ""
        echo "✅ Container started!"
        ;;
    3)
        echo ""
        echo "Stopping container..."
        docker-compose down
        echo ""
        echo "✅ Container stopped!"
        ;;
    4)
        echo ""
        echo "Viewing logs (Ctrl+C to exit)..."
        docker-compose logs -f
        ;;
    5)
        echo ""
        echo "Rebuilding container..."
        docker-compose down
        docker-compose up -d --build
        echo ""
        echo "✅ Container rebuilt and started!"
        ;;
    6)
        echo ""
        read -p "⚠️  This will remove the container and backup volumes. Continue? (y/N): " confirm
        if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
            docker-compose down -v
            echo "✅ Container and volumes removed!"
        else
            echo "Cancelled."
        fi
        ;;
    *)
        echo "Invalid choice"
        exit 1
        ;;
esac
