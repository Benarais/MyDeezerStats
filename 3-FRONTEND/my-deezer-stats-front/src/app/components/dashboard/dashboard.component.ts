import { Component, OnInit } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { DashboardService} from '../../services/dashboard.service';
import {Album, Artist, Track, Recent } from "../../models/dashboard.models"

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  imports: [CommonModule]
})
export class DashboardComponent implements OnInit {
  topAlbums: Album[] = [];
  topArtists: Artist[] = [];
  topTracks: Track[] = [];
  recentListens: Recent[] = [];

  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(
    private loginService: LoginService,
    private router: Router,
    private dashboardService: DashboardService
  ) {}



  ngOnInit(): void {
    if (!this.loginService.isAuthenticated()) {
      this.router.navigate(['/login']);
    } else {
      this.loadDashboardData();  // Charger les données du dashboard
    }
  }

  private loadDashboardData(): void {
    // Appeler le service pour récupérer les données
    this.dashboardService.getTopAlbums().subscribe(
      (albums) => {
        this.topAlbums = albums;
        this.topAlbums.forEach(album => {
        });
      },
      (error) => {
        this.errorMessage = 'Erreur lors du chargement des albums';
        console.error(error);
      }
    );

    this.dashboardService.getTopArtists().subscribe(
      (artists) => {
        this.topArtists = artists;
      },
      (error) => {
        this.errorMessage = 'Erreur lors du chargement des artistes';
        console.error(error);
      }
    );

    this.dashboardService.getTopTracks().subscribe(
      (tracks) => {
        this.topTracks = tracks;
      },
      (error) => {
        this.errorMessage = 'Erreur lors du chargement des morceaux';
        console.error(error);
      }
    );

    this.dashboardService.getRecentListens().subscribe(
      (tracks) => {
        this.recentListens = tracks;
      },
      (error) => {
        this.errorMessage = 'Erreur lors du chargement des derniers morceaux';
        console.error(error);
      }
    );
    
    this.isLoading = false;  // Une fois les données chargées, on désactive le chargement
  }
}