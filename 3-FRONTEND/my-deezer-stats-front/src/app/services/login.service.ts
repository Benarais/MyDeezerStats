import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AuthResponse {
  token?: string;
}

export interface SignInResponse {
  isSuccess?: boolean;
}

@Injectable({ providedIn: 'root' })
export class LoginService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly TOKEN_KEY = 'auth_token';

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password });
  }

   // Méthode pour créer un compte
  signUp(email: string, password: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/signup`, { email, password });
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  // Vérifie si l'utilisateur est authentifié
  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null && !this.isTokenExpired(token);   
  }

  // Méthode pour vérifier si le token est expiré
  private isTokenExpired(token: string): boolean {
    try {
      const decoded = this.decodeToken(token); // Décode le token
      const expiry = decoded.exp; // Récupère la date d'expiration du token
      if (!expiry) return false; // Si pas de date d'expiration, on suppose que le token n'est pas expiré
  
      const now = Math.floor(Date.now() / 1000); // Heure actuelle en secondes (UTC)
      return now >= expiry; // Compare la date d'expiration avec l'heure actuelle
    } catch (e) {
      console.error('Erreur de décodage du token', e);
      return true; // Si le décodage échoue, on considère le token comme expiré
    }
  }

   // Méthode pour décoder le token JWT (en supposant qu'il est au format JWT)
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