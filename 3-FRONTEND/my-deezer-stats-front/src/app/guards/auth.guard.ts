import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  // 1. Vérifier la présence du token JWT
  const token = localStorage.getItem('token');
  
  // 2. Si token présent et valide (ajoutez une vérification réelle en production)
  if (token) {
    return true;
  }
  
  // 3. Rediriger vers login si non authentifié
  router.navigate(['/login'], {
    queryParams: { returnUrl: state.url } // Préserve l'URL demandée
  });
  return false;
};
