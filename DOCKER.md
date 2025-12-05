# Docker Deployment Guide

## Quick Start

### Prerequisites
- Docker and Docker Compose installed
- Telegram Bot Token from [@BotFather](https://t.me/botfather)

### Running with Docker Compose

1. Clone the repository:
```bash
git clone https://github.com/Zumka1991/Victorina.git
cd Victorina
```

2. Create `.env` file:
```bash
cp .env.example .env
```

3. Edit `.env` and add your Telegram Bot Token:
```env
BOT_TOKEN=your_telegram_bot_token_here
```

4. Start all services:
```bash
docker-compose up -d
```

5. Access the services:
- **Admin Panel**: http://localhost:3002
- **API**: http://localhost:5001
- **PostgreSQL**: localhost:5433

### Stopping the Services

```bash
docker-compose down
```

To also remove volumes (database data):
```bash
docker-compose down -v
```

## Building and Publishing Images

### Prerequisites
- Docker Hub account
- Logged in to Docker Hub: `docker login`

### Build and Publish

To build and publish all images to Docker Hub:

```bash
# Build and push with 'latest' tag
./build-and-publish.sh

# Build and push with specific version tag
./build-and-publish.sh v1.0.0
```

This script will:
1. Build API image: `traxex864/victorina-api`
2. Build Bot image: `traxex864/victorina-bot`
3. Build Admin image: `traxex864/victorina-admin`
4. Push all images to Docker Hub
5. If version tag is provided, also tag as 'latest'

## Services

### API Service
- **Image**: `traxex864/victorina-api:latest`
- **Port**: 5000
- **Purpose**: REST API for the application
- **Dependencies**: PostgreSQL

### Bot Service
- **Image**: `traxex864/victorina-bot:latest`
- **Purpose**: Telegram bot
- **Dependencies**: PostgreSQL, API

### Admin Panel Service
- **Image**: `traxex864/victorina-admin:latest`
- **Port**: 3000
- **Purpose**: Web administration interface
- **Dependencies**: API

### PostgreSQL Service
- **Image**: `postgres:16-alpine`
- **Port**: 5432
- **Database**: victorina
- **User**: postgres
- **Password**: postgres (change in production!)

## Environment Variables

### Bot Service
- `BOT_TOKEN`: Telegram Bot API token (required)
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `Api__BaseUrl`: Internal API URL

### API Service
- `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
- `ASPNETCORE_URLS`: ASP.NET Core listening URL

### Admin Service
- `VITE_API_URL`: External API URL for browser access

## Database Migrations

Database migrations are applied automatically on API startup.

To manually run migrations:

```bash
docker-compose exec api dotnet ef database update
```

## Logs

View logs for all services:
```bash
docker-compose logs -f
```

View logs for specific service:
```bash
docker-compose logs -f api
docker-compose logs -f bot
docker-compose logs -f admin
```

## Troubleshooting

### Port Already in Use

If ports 3000 or 5000 are already in use, modify `docker-compose.yml`:

```yaml
api:
  ports:
    - "5001:80"  # Change 5000 to 5001

admin:
  ports:
    - "3001:80"  # Change 3000 to 3001
```

Don't forget to update `VITE_API_URL` in admin service if you change the API port.

### Database Connection Issues

1. Check if PostgreSQL is healthy:
```bash
docker-compose ps
```

2. Check PostgreSQL logs:
```bash
docker-compose logs postgres
```

3. Restart PostgreSQL:
```bash
docker-compose restart postgres
```

### Bot Not Responding

1. Verify your bot token is correct in `.env`
2. Check bot logs:
```bash
docker-compose logs bot
```
3. Restart bot service:
```bash
docker-compose restart bot
```

## Production Considerations

For production deployment:

1. **Change Database Credentials**:
   - Update PostgreSQL password in `docker-compose.yml`
   - Update connection strings in all services

2. **Use Secrets Management**:
   - Consider using Docker secrets or external secret managers
   - Don't commit `.env` file with real credentials

3. **Enable HTTPS**:
   - Add reverse proxy (nginx/traefik) with SSL certificates
   - Use Let's Encrypt for free SSL certificates

4. **Resource Limits**:
   - Add memory and CPU limits to services
   - Monitor resource usage

5. **Backup Database**:
   - Set up regular database backups
   - Store backups in secure location

6. **Monitoring**:
   - Add monitoring solutions (Prometheus, Grafana)
   - Set up alerting for service failures

## Architecture

```
┌─────────────┐
│   Admin     │ (Port 3000)
│   (React)   │
└──────┬──────┘
       │
       ▼
┌─────────────┐     ┌─────────────┐
│     API     │────▶│  PostgreSQL │
│  (ASP.NET)  │     │  (Database) │
└──────┬──────┘     └─────────────┘
       │
       ▼
┌─────────────┐
│  Telegram   │
│     Bot     │
└─────────────┘
```

## License

[Your License Here]
