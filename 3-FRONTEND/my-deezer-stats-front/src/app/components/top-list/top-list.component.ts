import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-top-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './top-list.component.html',
  styleUrl: './top-list.component.scss'
})
export class TopListComponent implements OnInit {

  @Input() type: 'album' | 'artist' | 'track' ='album';
  @Input() items: any[] = [];
  private route = inject(ActivatedRoute);

  periods = [
    { value: '4weeks', label: '4 dernières semaines' },
    { value: 'thisYear', label: 'Cette année' },
    { value: 'lastYear', label: 'Année dernière' },
    { value: 'allTime', label: 'Depuis le début' }
  ];

  isLoading = false;

  selectedPeriod = 'thisYear';

  constructor(
      private dashboardService: DashboardService
    ) {}

  ngOnInit(): void {
    console.log('ngOnInit called');
    this.route.params.subscribe(params => {
    const receivedType = params['type'];
    if (receivedType === 'album' || receivedType === 'artist' || receivedType === 'track') {
      this.type = receivedType;
    }
    this.loadData();
    });
  }

  onPeriodChange(): void {
    this.loadData();
  }

  loadData(): void {
  const nb = 50;
  switch (this.type) {
    case 'album':
      console.log("coucou");
      this.dashboardService.getTopAlbums(this.selectedPeriod, nb).subscribe(data => {
        this.items = data;
      });
      break;
    case 'artist':
      this.dashboardService.getTopArtists(this.selectedPeriod, nb).subscribe(data => {
        this.items = data;
      });
      break;
    case 'track':
      this.dashboardService.getTopTracks(this.selectedPeriod, nb).subscribe(data => {
        this.items = data;
      });
      break;
    default:
      console.error('Type non reconnu :', this.type);
      break;
  }
}

  getTitle(): string {
    console.log(this.type);
    switch (this.type) {
      case 'album': return 'Top 50 albums';
      case 'artist': return 'Top 50 artistes';
      case 'track': return 'Top 50 morceaux';
      default: return 'Top';
    }
  }

   formatTitle(item: any): string {
    if (this.type === 'track') {
      return `${item.track} - ${item.artist}${item.album ? ' (' + item.album + ')' : ''}`;
    } else if (this.type === 'album') {
      return `${item.title}${item.artist ? ' - ' + item.artist : ''}`;
    } else {
      return item.artist;
    }
  }

  getImage(item: any): string {
    if (this.type === 'track') return item.trackUrl || 'assets/default-cover.jpg';
    if (this.type === 'album') return item.coverUrl || 'assets/default-cover.jpg';
    return item.coverUrl || 'assets/default-cover.jpg';
  }

  getMainText(item: any): string {
    return this.type === 'track' ? item.track : this.type === 'album' ? item.title : item.artist;
  }

  getSubText(item: any): string {
    if (this.type === 'track') return `${item.artist} - ${item.album}`;
    if (this.type === 'album') return item.artist;
    return '';
  }

  navigateToDetail(item: any): void {
    // Navigation optionnelle si tu veux faire un zoom sur l'élément
    // this.router.navigate(['/detail', this.type, item.id]);
  }

}
