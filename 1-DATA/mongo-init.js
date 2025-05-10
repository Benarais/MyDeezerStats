
db = db.getSiblingDB('MyDeezerStats');

db.createCollection('listening');
db.createCollection('albumInfo');
db.createCollection('artistInfo');
db.createCollection('trackInfo');
db.createCollection('users');

// Création des index optimisés
// Pour les requêtes de statistiques
db.listening.createIndex({ Artist: 1 });
db.listening.createIndex({ Album: 1 });
db.listening.createIndex({ Track: 1 });
db.listening.createIndex({ Date: 1 });
db.listening.createIndex({ Artist: 1, Date: 1 }); // Pour les filtres combinés

// Index composés pour les jointures
//db.trackInfo.createIndex({ Artist: 1, Track: 1 }, { unique: true });
//db.albumInfo.createIndex({ Artist: 1, Album: 1 }, { unique: true });
//db.artistInfo.createIndex({ Artist: 1 }, { unique: true });


db.users.insertOne({
  "Email": "admin@admin.fr",
  "PasswordHash": "password123" 
});

db.createUser({
  user: 'api_user',
  pwd: 'api_password',
  roles: [{ role: 'readWrite', db: 'MyDeezerStats' }]
});