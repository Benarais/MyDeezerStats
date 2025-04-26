@echo off
echo 🚀 Build des conteneurs...
docker compose build

echo 📦 Lancement des conteneurs...
docker compose up -d

echo ✅ Tout est bon !
pause
