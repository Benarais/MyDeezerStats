import { Component, OnInit } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService} from '../../services/dashboard.service';
import {Album, Artist, Track, Recent } from "../../models/dashboard.models"
import { finalize, forkJoin } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule, FormsModule]
})
export class DashboardComponent implements OnInit {
  topAlbums: Album[] = [];
  topArtists: Artist[] = [];
  topTracks: Track[] = [];
  recentListens: Recent[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  

  periods = [
    { value: '4weeks', label: '4 dernières semaines' },
    { value: 'thisYear', label: 'Cette année' },
    { value: 'lastYear', label: 'Année dernière' },
    { value: 'allTime', label: 'Depuis le début' }
  ];

  selectedPeriod = 'thisYear'; 

  constructor(
    private loginService: LoginService,
    private router: Router,
    private dashboardService: DashboardService
  ) {}

  ngOnInit(): void {
    if (!this.loginService.isAuthenticated()) {
      this.router.navigate(['/login']);
    } else {
      this.loadDashboardData();
      //console.log(this.recentListens.length);  
    }
  }

  onPeriodChange(): void {
    this.isLoading = true;
    this.dashboardService.last4Weeks = new Date(this.recentListens[0].date);
    this.loadDashboardData();
    this.isLoading  = false;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Vérification du type de fichier
      if (!this.isExcelFile(file)) {
        alert('Veuillez sélectionner un fichier Excel (.xlsx, .xls)');
        return;
      }

      this.uploadExcelFile(file);
    }
  }

  private isExcelFile(file: File): boolean {
    const allowedExtensions = ['.xlsx', '.xls'];
    const fileName = file.name.toLowerCase();
    return allowedExtensions.some(ext => fileName.endsWith(ext));
  }

  private uploadExcelFile(file: File) {
    this.isLoading = true;
    console.log(this.isLoading);
    const formData = new FormData();
    formData.append('file', file, file.name);
    this.dashboardService.uploadExcelFile(formData).subscribe({
      next: (response) => {
        console.log('Réponse du serveur:', response);
        alert('Fichier importé avec succès !');
        this.loadDashboardData(); 
      },
      error: (error) => {
        console.error('Erreur détaillée:', {
          status: error.status,
          message: error.error?.title || error.message,
          details: error.error?.errors // Affiche les détails de validation
        });
        alert(`Erreur ${error.status}: ${error.error?.title || 'Échec de l\'import'}`);
        this.isLoading = false;
      }
    });
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';
  
    forkJoin([
      this.dashboardService.getTopAlbums(this.selectedPeriod),
      this.dashboardService.getTopArtists(this.selectedPeriod),
      this.dashboardService.getTopTracks(this.selectedPeriod),
      this.dashboardService.getRecentListens(this.selectedPeriod)
    ]).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe({
      next: ([albums, artists, tracks, recentListens]) => {
        this.topAlbums = albums;
        this.topArtists = artists;
        this.topTracks = tracks;
        this.recentListens = recentListens;
      },
      error: (err) => {
        this.errorMessage = 'Erreur lors du chargement';
        console.error('Détail :', err);
      }
    });
    this.isLoading = false;
  }

  navigateToDetail(type: 'album' | 'artist', item: any): void {
  let identifier = '';
    console.log(`Item received for ${type}:`, item);
  switch (type) {
    case 'album':
      const albumTitle = item.title ?? '';
      const albumArtist = item.artist ?? '';
      identifier = albumTitle && albumArtist ? `${albumTitle}|${albumArtist}` : '';
      break;

    case 'artist':
      identifier = item.artist ?? '';
      console.log(identifier);
      break;
  }

  if (identifier) {
    this.router.navigate(['/detail', type], { 
      queryParams: { identifier } 
    });
  } else {
    console.warn(`Incomplete data for type: ${type}`);
  }
}

}