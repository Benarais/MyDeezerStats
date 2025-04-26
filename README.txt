##Commande pour inserer les données dans MongoDB via l'API contenerisé

docker compose build
docker compose up -d

docker run --rm ^
    --network mydeezerstats_mydeezer-network ^
    -v "%cd%\1-DATA:/data" ^
    curlimages/curl:8.6.0 ^
    -X POST http://deezer-api:8080/api/upload/import-excel ^
    -F "file=@/data/Book.xlsx"

###Requête pour récupérer le top 20 albums
curl -X GET "http://localhost:5035/api/listening/top-albums?from=2025-01-01&to=2025-12-31" -H "accept: application/json"

curl -X GET http://localhost:5035/api/listening/top-albums