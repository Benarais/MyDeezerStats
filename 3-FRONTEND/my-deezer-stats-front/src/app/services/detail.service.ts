import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, throwError, Observable } from 'rxjs';
import { AlbumItem, ArtistItem, TrackItem, DetailItem } from '../models/detail.models';

@Injectable({
  providedIn: 'root'
})
export class DetailService {
  private readonly apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('auth_token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  private handleError(error: HttpErrorResponse) {
    console.error('API Error:', error);
    return throwError(() => new Error(
      error.error?.message || 'Une erreur est survenue'
    ));
  }

  getDetails(type: 'album' | 'artist', identifier: string): Observable<DetailItem> {
  let endpoint = '';

  switch (type) {
    case 'album':
      endpoint = `/listening/album`;
      return this.http.get<AlbumItem>(`${this.apiUrl}${endpoint}`, {
        params: { identifier }, // Ajout de identifier comme paramètre de query string
        headers: this.getAuthHeaders()
      }).pipe(
        catchError(this.handleError)
      );
    case 'artist':
      endpoint = `/listening/artist`;
      return this.http.get<ArtistItem>(`${this.apiUrl}${endpoint}`, {
        params: { identifier }, // Ajout de identifier comme paramètre de query string
        headers: this.getAuthHeaders()
      }).pipe(
        catchError(this.handleError)
      );
    default:
      throw new Error('Type non supporté : ' + type);
  }
}


 /* getDetails(type: 'album' | 'artist', identifier: string): Observable<DetailItem> {
    let endpoint = '';

    switch (type) {
      case 'album':
        endpoint = `/listening/album/${identifier}`;
        return this.http.get<AlbumItem>(`${this.apiUrl}${endpoint}`, {
          headers: this.getAuthHeaders()
        }).pipe(
          catchError(this.handleError)
        );
      case 'artist':
        endpoint = `/listening/artist/${identifier}`;
        return this.http.get<ArtistItem>(`${this.apiUrl}${endpoint}`, {
          headers: this.getAuthHeaders()
        }).pipe(
          catchError(this.handleError)
        );
      default:
        throw new Error('Type non supporté : ' + type);
    }
  }*/
}
