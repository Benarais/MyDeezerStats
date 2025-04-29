// src/app/interceptors/jwt.interceptor.ts
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { LoginService } from '../services/login.service'; // Ajustez le chemin

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private loginService: LoginService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Récupère le token depuis LoginService
    const token = this.loginService.getToken();

    // Clone la requête pour ajouter le header Authorization si token existe
    if (token && !this.isAuthRequest(request.url)) {
      request = request.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          console.error('Token expiré ou invalide - Déconnexion...');
          this.loginService.logout();
          // Redirigez vers /login si nécessaire (via un Router injecté)
        }
        return throwError(() => error);
      })
    );
  }

  // Exclut les URLs d'authentification pour éviter une boucle
  private isAuthRequest(url: string): boolean {
    return url.includes('/api/auth/login') || url.includes('/api/auth/signup');
  }
}