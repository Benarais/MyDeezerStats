import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CacheService {
  private readonly STORAGE_KEY = 'album-image-cache';

  constructor() {
    // Si le cache local existe déjà dans le localStorage, on le charge
    const storedCache = localStorage.getItem(this.STORAGE_KEY);
    if (storedCache) {
      this.cache = new Map(JSON.parse(storedCache));
    }
  }

  private cache: Map<string, string> = new Map();

  // Vérifie si l'image est dans le cache
  getCache(key: string): string | null {
    return this.cache.get(key) || null;
  }

  // Ajoute l'image dans le cache
  setCache(key: string, value: string): void {
    this.cache.set(key, value);
    this.saveCacheToStorage();
  }

  // Sauvegarde le cache dans le localStorage
  private saveCacheToStorage(): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify([...this.cache]));
  }
}
