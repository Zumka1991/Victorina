#!/bin/bash

# Build and publish Docker images to Docker Hub
# Usage: ./build-and-publish.sh [version]
# Example: ./build-and-publish.sh v1.0.0
# If no version is provided, 'latest' will be used

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Docker Hub username
DOCKER_USERNAME="traxex864"

# Get version from argument or use 'latest'
VERSION="${1:-latest}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Building and Publishing Victorina${NC}"
echo -e "${GREEN}Version: ${VERSION}${NC}"
echo -e "${GREEN}========================================${NC}"

# Check if docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed${NC}"
    exit 1
fi

# Check if user is logged in to Docker Hub
if ! docker info | grep -q "Username: ${DOCKER_USERNAME}"; then
    echo -e "${YELLOW}Logging in to Docker Hub...${NC}"
    docker login
fi

echo ""
echo -e "${GREEN}Step 1/3: Building API image...${NC}"
docker build -t ${DOCKER_USERNAME}/victorina-api:${VERSION} \
    -f src/Victorina.Api/Dockerfile \
    .

echo ""
echo -e "${GREEN}Step 2/3: Building Bot image...${NC}"
docker build -t ${DOCKER_USERNAME}/victorina-bot:${VERSION} \
    -f src/Victorina.Bot/Dockerfile \
    .

echo ""
echo -e "${GREEN}Step 3/3: Building Admin image...${NC}"
docker build -t ${DOCKER_USERNAME}/victorina-admin:${VERSION} \
    -f admin/Dockerfile \
    admin/

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Pushing images to Docker Hub...${NC}"
echo -e "${GREEN}========================================${NC}"

echo ""
echo -e "${GREEN}Pushing API image...${NC}"
docker push ${DOCKER_USERNAME}/victorina-api:${VERSION}

echo ""
echo -e "${GREEN}Pushing Bot image...${NC}"
docker push ${DOCKER_USERNAME}/victorina-bot:${VERSION}

echo ""
echo -e "${GREEN}Pushing Admin image...${NC}"
docker push ${DOCKER_USERNAME}/victorina-admin:${VERSION}

# If version is not 'latest', also tag and push as 'latest'
if [ "$VERSION" != "latest" ]; then
    echo ""
    echo -e "${YELLOW}Also tagging and pushing as 'latest'...${NC}"

    docker tag ${DOCKER_USERNAME}/victorina-api:${VERSION} ${DOCKER_USERNAME}/victorina-api:latest
    docker tag ${DOCKER_USERNAME}/victorina-bot:${VERSION} ${DOCKER_USERNAME}/victorina-bot:latest
    docker tag ${DOCKER_USERNAME}/victorina-admin:${VERSION} ${DOCKER_USERNAME}/victorina-admin:latest

    docker push ${DOCKER_USERNAME}/victorina-api:latest
    docker push ${DOCKER_USERNAME}/victorina-bot:latest
    docker push ${DOCKER_USERNAME}/victorina-admin:latest
fi

echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}✓ All images successfully built and pushed!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo -e "Images published:"
echo -e "  • ${DOCKER_USERNAME}/victorina-api:${VERSION}"
echo -e "  • ${DOCKER_USERNAME}/victorina-bot:${VERSION}"
echo -e "  • ${DOCKER_USERNAME}/victorina-admin:${VERSION}"

if [ "$VERSION" != "latest" ]; then
    echo ""
    echo -e "Also available as:"
    echo -e "  • ${DOCKER_USERNAME}/victorina-api:latest"
    echo -e "  • ${DOCKER_USERNAME}/victorina-bot:latest"
    echo -e "  • ${DOCKER_USERNAME}/victorina-admin:latest"
fi

echo ""
echo -e "${YELLOW}To run the application:${NC}"
echo -e "  1. Create .env file with BOT_TOKEN=your_token"
echo -e "  2. Run: docker-compose up -d"
echo ""
