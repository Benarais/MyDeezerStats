import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthResponse, SignInResponse, SignUpResponse } from '../models/login.model';


@Injectable({ providedIn: 'root' })
export class LoginService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly TOKEN_KEY = 'auth_token';

  constructor(private http: HttpClient) {}

  // Login
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        catchError(error => {
          console.error('Erreur lors de la connexion', error);
          return throwError(() => new Error('Échec de la connexion'));
        })
      );
  }

  // Inscription
  signUp(email: string, password: string): Observable<SignUpResponse> {
    return this.http.post<SignUpResponse>(`${this.apiUrl}/signup`, { email, password }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Erreur lors de l\'inscription :', error);
        let errorMessage = 'Échec de l\'inscription';
  
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.status === 409) {
          errorMessage = 'Ce compte existe déjà. Essayez de vous connecter.';
        } else if (error.status === 500) {
          errorMessage = 'Erreur serveur. Veuillez réessayer plus tard.';
        }
  
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  // Déconnexion
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // Vérifie si l'utilisateur est authentifié
  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired(token);   
  }

  // Vérifie si le token est expiré
  private isTokenExpired(token: string): boolean {
    try {
      const decoded = this.decodeToken(token); // Décode le token
      const expiry = decoded.exp; // Récupère la date d'expiration
      if (!expiry) return false; // Si pas de date d'expiration, on suppose qu'il n'est pas expiré
      const now = Math.floor(Date.now() / 1000); // Heure actuelle en secondes
      return now >= expiry; // Compare la date d'expiration avec l'heure actuelle
    } catch (e) {
      console.error('Erreur de décodage du token', e);
      return true; // Si le décodage échoue, on considère le token comme expiré
    }
  }

  // Décode le token JWT
  private decodeToken(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const decoded = decodeURIComponent(
      atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
    );
    return JSON.parse(decoded);
  }

  // Récupère le token du localStorage
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }
}
