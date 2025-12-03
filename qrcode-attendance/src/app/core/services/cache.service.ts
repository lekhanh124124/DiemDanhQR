import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

interface CacheEntry {
  value: any;
  expiry: number;
}

@Injectable({ providedIn: 'root' })
export class CacheService {
  private cache = new Map<string, CacheEntry>();

  get<T>(key: string): T | null {
    const e = this.cache.get(key);
    if (!e) return null;
    if (Date.now() > e.expiry) {
      this.cache.delete(key);
      return null;
    }
    return e.value as T;
  }

  set<T>(key: string, value: T, ttlMs = 60_000): void {
    const expiry = Date.now() + ttlMs;
    this.cache.set(key, { value, expiry });
  }

  delete(key: string): void {
    this.cache.delete(key);
  }

  clearPrefix(prefix: string): void {
    for (const key of Array.from(this.cache.keys())) {
      if (key.startsWith(prefix)) this.cache.delete(key);
    }
  }

  getOrFetch<T>(key: string, fetchFn: () => Observable<T>, ttlMs = 60_000): Observable<T> {
    const cached = this.get<T>(key);
    if (cached !== null) return of(cached);
    return fetchFn().pipe(
      tap(v => this.set(key, v, ttlMs))
    );
  }
}
