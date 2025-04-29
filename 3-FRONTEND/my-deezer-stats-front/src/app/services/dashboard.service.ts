import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CacheService } from './cache.service';
import { Observable, map } from 'rxjs';
import { Album, Artist, Track, Recent } from '../models/dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = 'http://localhost:5000/api/listening';

  constructor(private http: HttpClient, private cacheService: CacheService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  getTopAlbums(): Observable<Album[]> {
    return this.http.get<Album[]>(`${this.apiUrl}/top-albums`, {
      headers: this.getAuthHeaders()
    });
  }

  getTopArtists(): Observable<Artist[]> {
    return this.http.get<Artist[]>(`${this.apiUrl}/top-artists`, {
      headers: this.getAuthHeaders()
    });
  }

  getTopTracks(): Observable<Track[]> {
    return this.http.get<Track[]>(`${this.apiUrl}/top-tracks`, {
      headers: this.getAuthHeaders()
    });
  }

  getRecentListens(): Observable<Recent[]> {
    return this.http.get<Recent[]>(`${this.apiUrl}/recent`, {
      headers: this.getAuthHeaders()
    });
  }
}
