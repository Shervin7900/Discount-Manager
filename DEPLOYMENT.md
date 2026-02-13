# Discount Manager - Production Deployment Guide

<!-- ## Prerequisites

- Docker & Docker Compose installed
- Kubernetes cluster (for K8s deployment)
- GitHub account (for CI/CD)
- Domain name and SSL certificates (for production) -->

## Deployment Options

### Option 1: Docker Compose (Recommended for small-medium deployments)

#### 1. Prepare Environment
```bash
cd infrastructure
cp .env.production.template .env.production
# Edit .env.production with your values
nano .env.production
```

#### 2. Deploy
```bash
chmod +x scripts/deploy.sh
./scripts/deploy.sh production
```

#### 3. Verify
```bash
docker-compose ps
docker-compose logs -f
```

### Option 2: Kubernetes (Recommended for large-scale deployments)

#### 1. Create Namespace
```bash
kubectl create namespace discount-manager
```

#### 2. Create Secrets
```bash
# JWT Secret
kubectl create secret generic jwt-secret \
  --from-literal=key='YourSuperSecretKeyForJwtTokenGenerationMinimum32Characters!' \
  -n discount-manager

# Database Connection Strings
kubectl create secret generic db-connection-strings \
  --from-literal=catalog-db='Server=...' \
  --from-literal=shops-db='Server=...' \
  --from-literal=discount-db='Server=...' \
  --from-literal=inventory-db='Server=...' \
  --from-literal=ordering-db='Server=...' \
  --from-literal=payment-db='Server=...' \
  --from-literal=identity-db='Server=...' \
  --from-literal=customer-db='Server=...' \
  -n discount-manager
```

#### 3. Deploy Applications
```bash
kubectl apply -f infrastructure/k8s/gateway-deployment.yaml
kubectl apply -f infrastructure/k8s/bootstrapper-deployment.yaml
```

#### 4. Verify
```bash
kubectl get pods -n discount-manager
kubectl get services -n discount-manager
kubectl logs -f deployment/discount-manager-gateway -n discount-manager
```

## CI/CD Setup

### GitHub Actions

1. **Enable GitHub Actions** in your repository settings

2. **Add Secrets** to your repository:
   - `DOCKER_USERNAME` - Docker Hub username
   - `DOCKER_PASSWORD` - Docker Hub password
   - `KUBE_CONFIG` - Kubernetes config (base64 encoded)

3. **Push to trigger pipeline**:
   ```bash
   git push origin develop  # Deploys to staging
   git push origin main     # Deploys to production
   ```

### Pipeline Stages

1. **Build & Test** - Compiles code and runs tests
2. **Security Scan** - Scans for vulnerabilities with Trivy
3. **Build Docker Images** - Creates and pushes images
4. **Deploy Staging** - Auto-deploys to staging (develop branch)
5. **Deploy Production** - Auto-deploys to production (main branch)

## SSL/HTTPS Setup

### Using Let's Encrypt with Certbot

```bash
# Install Certbot
sudo apt-get install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal (already configured)
sudo certbot renew --dry-run
```

### Update Nginx Configuration

Uncomment the SSL section in `infrastructure/nginx/nginx.conf`:
```nginx
server {
    listen 443 ssl http2;
    server_name api.discountmanager.local;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    # ... rest of config
}
```

## Monitoring

### Access Dashboards

- **Prometheus**: http://your-domain:9090
- **Grafana**: http://your-domain:3000 (admin/password from .env)
- **Vector API**: http://your-domain:8686

### Import Grafana Dashboards

1. Login to Grafana
2. Add Prometheus data source (http://prometheus:9090)
3. Import dashboards:
   - ASP.NET Core Dashboard (ID: 10915)
   - Nginx Dashboard (ID: 12708)
   - Redis Dashboard (ID: 11835)

## Scaling

### Docker Compose
```bash
docker-compose up -d --scale gateway=3 --scale bootstrapper=2
```

### Kubernetes
```bash
# Manual scaling
kubectl scale deployment discount-manager-gateway --replicas=5 -n discount-manager

# Auto-scaling is configured via HPA (already in deployment files)
```

## Backup & Recovery

### Database Backup
```bash
# Backup all databases
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $SQL_SA_PASSWORD \
  -Q "BACKUP DATABASE [DiscountManager_Catalog] TO DISK = '/var/opt/mssql/backup/catalog.bak'"

# Repeat for other databases
```

### Restore Database
```bash
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P $SQL_SA_PASSWORD \
  -Q "RESTORE DATABASE [DiscountManager_Catalog] FROM DISK = '/var/opt/mssql/backup/catalog.bak'"
```

## Troubleshooting

### View Logs
```bash
# Docker Compose
docker-compose logs -f gateway
docker-compose logs -f bootstrapper

# Kubernetes
kubectl logs -f deployment/discount-manager-gateway -n discount-manager
kubectl logs -f deployment/discount-manager-bootstrapper -n discount-manager
```

### Health Checks
```bash
# Gateway
curl http://localhost:5000/health

# Bootstrapper
curl http://localhost:5169/health
```

### Database Connection Issues
```bash
# Test SQL Server connection
docker-compose exec bootstrapper dotnet ef database update --verbose
```

## Performance Tuning

### Nginx
- Enable HTTP/2
- Configure caching headers
- Tune worker processes

### .NET Application
- Enable response compression
- Configure connection pooling
- Optimize EF Core queries

### Database
- Create indexes on frequently queried columns
- Configure connection pool size
- Enable query caching

## Security Checklist

- [ ] Change default passwords
- [ ] Enable HTTPS/SSL
- [ ] Configure firewall rules
- [ ] Set up rate limiting
- [ ] Enable security headers
- [ ] Configure CORS properly
- [ ] Use secrets management
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Backup encryption

## Support

For issues and questions:
- GitHub Issues: https://github.com/your-org/discount-manager/issues
- Documentation: https://docs.discountmanager.com
- Email: support@discountmanager.com
