import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  const token = localStorage.getItem('auth_token'); // À harmoniser avec votre login
  
  if (token) {
    // Vérification supplémentaire pour les environnements de production
    if (isTokenValid(token)) { // Implémentez cette fonction selon votre JWT
      return true;
    } else {
      // Token invalide ou expiré
      localStorage.removeItem('auth_token');
    }
  }
  
  // Redirection avec préservation de l'URL
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};

// Fonction helper pour la validation du token (exemple basique)
function isTokenValid(token: string): boolean {
  try {
    // Exemple de décodage basique du JWT
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp > Date.now() / 1000;
  } catch {
    return false;
  }
}