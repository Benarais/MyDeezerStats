import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
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

  getTopAlbums(period: string): Observable<Album[]> {
    const { from, to } = this.getDateRange(period);
    
    const params = new HttpParams()
      .set('from', from.toISOString())  // Convertir la date en ISO string
      .set('to', to.toISOString());
  
    return this.http.get<Album[]>(`${this.apiUrl}/top-albums`, {
      headers: this.getAuthHeaders(),
      params: params
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

  loadPeriods(): string[] {
    const currentDate = new Date();
    const year = currentDate.getFullYear();
    const previousYear = year-1;
    return ["4 dernière semaines", year.toString(), previousYear.toString(), "depuis le début"]
  }

  private getDateRange(period: string): { from: Date, to: Date } {
    const currentDate = new Date();
    const year = currentDate.getFullYear();
    const previousYear = year - 1;
  
    switch (period) {
      case "4 dernière semaines":
        // 4 dernières semaines
        const fourWeeksAgo = new Date(currentDate);
        fourWeeksAgo.setDate(currentDate.getDate() - 28);  // 28 jours en arrière
        return { from: fourWeeksAgo, to: currentDate };
  
      case year.toString():
        // L'année en cours
        return { from: new Date(`${year}-01-01`), to: new Date(`${year}-12-31`) };
  
      case previousYear.toString():
        // L'année précédente
        return { from: new Date(`${previousYear}-01-01`), to: new Date(`${previousYear}-12-31`) };
  
      case "depuis le début":
        // Depuis le début (date la plus ancienne)
        return { from: new Date('2000-01-01'), to: currentDate };  // Adapté selon ta donnée
  
      default:
        // Période par défaut (par exemple, toute la période)
        return { from: new Date('2000-01-01'), to: currentDate };
    }
  }
}
