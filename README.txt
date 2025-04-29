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

ng generate service CacheService

docker run --rm --network mydeezerstats_mydeezer-network -v "%cd%\1-DATA:/data" curlimages/curl:8.6.0 -X POST http://deezerstats-api:5000/api/upload/import-excel -F "file=@/data/Book.xlsx"


docker run --rm --network mydeezerstats_mydeezer-network -v "%cd%\1-DATA:/data" curlimages/curl:8.6.0 -X POST http://deezer-api:8080/api/upload/import-excel -F "file=@/data/Book.xlsx"


docker exec -it mydeezerstats-api /bin/sh


 curl -X GET "http://deezer-api:8080/api/listening/top-albums?from=2025-01-01&to=2025-12-31" -H "accept: application/json"
 
 
 
 mydeezerstats_mydeezer-network

docker run --rm -it --network mydeezerstats_mydeezer-network curlimages/curl curl -X GET "http://deezer-api:8080/api/listening/top-albums" -H "accept: application/json"


curl -X GET "https://localhost:7124/api/listening/top-albums -H "accept: application/json"

curl -X POST http://localhost:7124/api/upload/import-excel -F "file=@/data/Book.xlsx"

curl -X POST http://localhost:5035/api/upload/import-excel -F "file=@1-DATA/Book.xlsx"


curl -X GET "http://localhost:5035/api/listening/top-artists