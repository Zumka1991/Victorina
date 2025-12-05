#!/bin/bash
set -e

echo "Stopping all containers..."
docker-compose down

echo "Removing all volumes (including database)..."
docker-compose down -v

echo "Removing images..."
docker rmi traxex864/victorina-api:latest traxex864/victorina-bot:latest traxex864/victorina-admin:latest || true

echo "Pulling fresh images..."
docker-compose pull

echo "Starting services..."
docker-compose up -d

echo "Done! Services are starting up."
echo "Check logs with: docker-compose logs -f"
