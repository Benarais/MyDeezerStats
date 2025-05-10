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

curl -X POST http://localhost:5035/api/upload/import-excel -F "file=@1-DATA/deezer.xlsx"


curl -X GET "http://localhost:5035/api/listening/top-albums

docker exec -it deezer-api curl https://api.deezer.com

curl -X GET "http://localhost:5035/api/listening/top-albums?from=2025-01-01T00:00:00.000Z&to=2025-12-31T00:00:00.000Z" 

docker exec -it deezer-mongodb mongosh

pour aller en base de données: docker exec -it deezer-mongodb mongosh -u admin -p admin --authenticationDatabase admin
db.trackInfo.find({Duration: { $ne: null, $ne: 0 }})$$db.listening.find().limit(5).pretty()

db.listening.find().limit(5).pretty()

curl -X POST http://localhost:5035/api/upload/import-excel -F "file=@1-DATA/Book.xlsx"


GET /api/listening/top-albums?from=2025-01-01T00:00:00.000Z&to=2025-12-31T00:00:00.000Z HTTP/1.1
Accept: application/json, text/plain, */*
Accept-Encoding: gzip, deflate, br, zstd
Accept-Language: en-GB,en-US;q=0.9,en;q=0.8,fr;q=0.7,zh-CN;q=0.6,zh;q=0.5
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY4MWI2N2UwZjY3ZDU5YjEwMWQ4NjFlMCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbkBhZG1pbi5mciIsImV4cCI6MTc0Njc1MzI5NywiaXNzIjoiTXlEZWV6ZXJTdGF0c0FQSSIsImF1ZCI6Ik15RGVlemVyU3RhdHNDbGllbnQifQ.7lwZyQalzynG8yg9kXmOzf33XcJS30Q3mKvxzD2sOxE
Connection: keep-alive
Host: localhost:5000
Origin: http://localhost:4200
Referer: http://localhost:4200/
Sec-Fetch-Dest: empty
Sec-Fetch-Mode: cors
Sec-Fetch-Site: same-site
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36
sec-ch-ua: "Chromium";v="136", "Google Chrome";v="136", "Not.A/Brand";v="99"
sec-ch-ua-mobile: ?0
sec-ch-ua-platform: "Windows"


/api/listening/top-albums?from=2025-01-01T00:00:00.000Z&to=2025-12-31T00:00:00.000Z