
db = db.getSiblingDB('MyDeezerStats');

db.createCollection('listening');
db.createCollection('albumInfo');
db.createCollection('artistInfo');
db.createCollection('trackInfo');
db.createCollection('users');

// db.songs.createIndex({ Artist: 1 });
// db.songs.createIndex({ Date: 1 });
db.listening.createIndex({ Artist: 1 });
db.listening.createIndex({ Date: 1 });
db.trackInfo.createIndex({ Artist: 1, Track: 1 })
db.users.insertOne({
  "Email": "admin@admin.fr",
  "PasswordHash": "password123" 
});

db.createUser({
  user: 'api_user',
  pwd: 'api_password',
  roles: [{ role: 'readWrite', db: 'MyDeezerStats' }]
});