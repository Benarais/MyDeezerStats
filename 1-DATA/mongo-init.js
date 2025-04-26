// Crée la base + collection + indexes + user dédié
db = db.getSiblingDB('MyDeezerStats');

db.createCollection('listening');
db.songs.createIndex({ Artist: 1 });
db.songs.createIndex({ Date: 1 });
db.listening.createIndex({ Artist: 1 });
db.listening.createIndex({ Date: 1 });


db.createCollection('users');

// Insère un utilisateur avec un mot de passe haché (ici, en utilisant un exemple de mot de passe 'password123').
db.users.insertOne({
  "Email": "admin@admin.fr",
  "PasswordHash": "password123" 
});


db.createUser({
  user: 'api_user',
  pwd: 'api_password',
  roles: [{ role: 'readWrite', db: 'MyDeezerStats' }]
});