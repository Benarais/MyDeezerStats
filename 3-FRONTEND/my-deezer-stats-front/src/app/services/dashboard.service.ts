import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { Album, Artist, Track, Recent } from '../models/dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  
  private readonly apiUrl = 'http://localhost:5000/api';
  last4Weeks: Date = new Date();
  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  uploadExcelFile(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/upload/import-excel`, formData, {
      headers: new HttpHeaders({
        'Accept': 'application/json'
      }),
      reportProgress: true,
      observe: 'response'
    }).pipe(
      catchError(error => {
        return throwError(() => ({
          status: error.status,
          error: error.error,
          message: error.error?.title || error.message
        }));
      })
    );
  }

  getTopAlbums(period: string): Observable<Album[]> {
    const { from, to } = this.getDateRange(period);
    
    const params = new HttpParams()
      .set('from', from.toISOString())  
      .set('to', to.toISOString());
  
    return this.http.get<Album[]>(`${this.apiUrl}/listening/top-albums`, {
      headers: this.getAuthHeaders(),
      params: params
    });
  }

  getTopArtists(period : string): Observable<Artist[]> {
    const { from, to } = this.getDateRange(period);
    
    const params = new HttpParams()
      .set('from', from.toISOString())
      .set('to', to.toISOString());
    return this.http.get<Artist[]>(`${this.apiUrl}/listening/top-artists`, {
      headers: this.getAuthHeaders(),
      params: params
    });
  }

  getTopTracks(period : string): Observable<Track[]> {
    const { from, to } = this.getDateRange(period);
    
    const params = new HttpParams()
      .set('from', from.toISOString()) 
      .set('to', to.toISOString());
    return this.http.get<Track[]>(`${this.apiUrl}/listening/top-tracks`, {
      headers: this.getAuthHeaders(),
      params: params
    });
  }

  getRecentListens(period: string): Observable<Recent[]> {
    const { from, to } = this.getDateRange(period);
    
    const params = new HttpParams()
      .set('from', from.toISOString())  
      .set('to', to.toISOString());
    return this.http.get<Recent[]>(`${this.apiUrl}/listening/recent`, {
      headers: this.getAuthHeaders(),
      params: params
    });
  }

  private getDateRange(period: string): { from: Date, to: Date } {
    console.log("Period received:", period);
    const currentDate = new Date();
    const year = currentDate.getFullYear();
    const previousYear = year - 1;
  
    switch (period) {
      case "4weeks":
        const fourWeeksAgo = this.last4Weeks;
        fourWeeksAgo.setDate(currentDate.getDate() - 28); 
        return { from: fourWeeksAgo, to: currentDate };
  
      case "thisYear":
        return { from: new Date(`${year}-01-01`), to: new Date(`${year}-12-31`) };
  
      case "lastYear":
        return { from: new Date(`${previousYear}-01-01`), to: new Date(`${previousYear}-12-31`) };
  
      case "allTime":
        return { from: new Date('2000-01-01'), to: currentDate };  
  
      default:
        return { from: new Date('2000-01-01'), to: currentDate };
    }
  }
}
