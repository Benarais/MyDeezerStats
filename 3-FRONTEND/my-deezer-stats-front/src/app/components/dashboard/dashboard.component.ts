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
    }
  }

  onPeriodChange(): void {
    this.loadDashboardData();
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';
  
    // Appels parallèles avec gestion individuelle
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
  }


}