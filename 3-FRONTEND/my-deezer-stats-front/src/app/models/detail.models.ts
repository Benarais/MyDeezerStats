import { Track } from "./dashboard.models";

export interface BaseItem {
  coverUrl?: string;  // Propriété partagée
}

export interface AlbumItem extends BaseItem {
  type: 'album';
  title?: string;
  artist?: string;
  playCount?: number;
  totalDuration?: number;
  releaseDate?: string;
  tracks: Track[];
}

export interface ArtistItem extends BaseItem {
  type: 'artist';
  artist: string;
  albumCount?: number;
  trackCount?: number;
  popularity?: number;
  topAlbums?: Array<{
    title: string;
    coverUrl?: string;
    releaseDate?: string;
  }>;
}

export interface TrackItem extends BaseItem {
  type: 'track';
  album?: string;
  artist?: string;
  name?: string;
  duration?: number;
  firstPlayed?: string;
  lastPlayed?: string;
  playHistory?: PlayHistory[];
}

export interface PlayHistory {
  date: string;
  count: number;
}

export type DetailItem = AlbumItem | ArtistItem | TrackItem;
