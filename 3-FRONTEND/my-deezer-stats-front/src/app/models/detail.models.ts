import { Track } from "./dashboard.models";

export interface BaseItem {
  coverUrl?: string;  // Propriété partagée
}

export interface AlbumItem extends BaseItem {
  type: 'album';
  title?: string;
  artist?: string;
  playCount?: number;
  totalDuration: number;
  totalListening: number;
  releaseDate?: string;
  trackInfos: Track[];
}

export interface ArtistItem extends BaseItem {
  type: 'artist';
  artist: string;
  playCount?: number;
  totalListening: number;
  nbFans? : number;
  trackInfos: Track[];
}

export type DetailItem = AlbumItem | ArtistItem ;
