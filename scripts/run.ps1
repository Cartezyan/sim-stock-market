echo "Rebuilding application..."
& $PSScriptRoot\rebuild.ps1

echo "Executing application"
docker-compose up --build