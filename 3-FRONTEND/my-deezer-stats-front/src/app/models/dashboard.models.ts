export interface Album {
  title: string;
  artist: string;
  count: number;
  coverUrl: string;
  totalDuration: number;
  totalListening: number;
}

export interface Artist {
  artist: string;
  count: number;
  coverUrl: string;
  //totalListening: number;
}

export interface Track{
  track: string;
  album: string;
  artist: string;
  trackUrl: string;
  count: number;
  duration: number;
  totalListening: number;
  lastListen: string;
}

export interface Recent {
  track: string;
  artist: string;
  album: string;
  date: string; 
  imageUrl?: string;
}

