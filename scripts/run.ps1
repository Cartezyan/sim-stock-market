echo "Rebuilding application..."
& $PSScriptRoot\rebuild.ps1

echo "Executing application"
docker-compose up --build -d
docker-compose scale market=5
docker-compose up 