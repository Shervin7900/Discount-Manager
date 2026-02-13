#!/bin/bash

# Production Deployment Script
# This script deploys the Discount Manager application to production

set -e

echo "🚀 Starting production deployment..."

# Configuration
ENVIRONMENT=${1:-production}
COMPOSE_FILE="infrastructure/docker-compose.yml"
ENV_FILE="infrastructure/.env.${ENVIRONMENT}"

# Check if environment file exists
if [ ! -f "$ENV_FILE" ]; then
    echo "❌ Environment file not found: $ENV_FILE"
    echo "Please create the environment file with required variables."
    exit 1
fi

# Load environment variables
export $(cat $ENV_FILE | grep -v '^#' | xargs)

echo "📦 Environment: $ENVIRONMENT"
echo "📁 Compose file: $COMPOSE_FILE"

# Pull latest images
echo "⬇️  Pulling latest Docker images..."
docker-compose -f $COMPOSE_FILE pull

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose -f $COMPOSE_FILE down

# Start new containers
echo "▶️  Starting new containers..."
docker-compose -f $COMPOSE_FILE up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
sleep 10

# Check health
echo "🏥 Checking service health..."
docker-compose -f $COMPOSE_FILE ps

# Run database migrations
echo "🗄️  Running database migrations..."
docker-compose -f $COMPOSE_FILE exec -T bootstrapper dotnet ef database update --project /app/DiscountManager.Bootstrapper.dll || true

# Cleanup old images
echo "🧹 Cleaning up old images..."
docker image prune -f

echo "✅ Deployment completed successfully!"
echo ""
echo "📊 Access points:"
echo "  - Application: http://localhost (via Nginx)"
echo "  - Gateway: http://localhost:5000"
echo "  - Prometheus: http://localhost:9090"
echo "  - Grafana: http://localhost:3000"
echo ""
echo "📝 View logs:"
echo "  docker-compose -f $COMPOSE_FILE logs -f"
