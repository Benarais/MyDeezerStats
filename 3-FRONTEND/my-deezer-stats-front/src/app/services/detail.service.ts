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

  getDetails(type: 'album' | 'artist' | 'track', identifier: string): Observable<DetailItem> {
    let endpoint = '';

    switch (type) {
      case 'album':
        endpoint = `/detail/album/${identifier}`;
        return this.http.get<AlbumItem>(`${this.apiUrl}${endpoint}`, {
          headers: this.getAuthHeaders()
        }).pipe(
          catchError(this.handleError)
        );
      case 'artist':
        endpoint = `/detail/artist/${identifier}`;
        return this.http.get<ArtistItem>(`${this.apiUrl}${endpoint}`, {
          headers: this.getAuthHeaders()
        }).pipe(
          catchError(this.handleError)
        );
      case 'track':
        endpoint = `/detail/track/${identifier}`;
        return this.http.get<TrackItem>(`${this.apiUrl}${endpoint}`, {
          headers: this.getAuthHeaders()
        }).pipe(
          catchError(this.handleError)
        );
      default:
        throw new Error('Type non supporté : ' + type);
    }
  }
}
