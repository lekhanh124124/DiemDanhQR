import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { CacheService } from './cache.service';

@Injectable({ providedIn: 'root' })
export class SubjectService {
  private baseUrl = `${environment.apiBase}/course/subjects`;

  constructor(private http: HttpClient, private authService: AuthService, private cache: CacheService) {}

  getSubjects(params?: any): Observable<any> {
    let httpParams = new HttpParams();
    const defaults = {
      Page: 1,
      PageSize: 20,
      SortBy: 'TenMonHoc',
      SortDir: 'DESC'
    };
    const final = { ...defaults, ...(params || {}) };
    Object.keys(final).forEach(key => {
      const val = final[key];
      if (val !== undefined && val !== null && val !== '') {
        httpParams = httpParams.set(key, String(val));
      }
    });

    const token = this.authService.getToken();
    const headers = new HttpHeaders({ Authorization: `Bearer ${token || ''}` });
    const user = this.authService.getUser?.();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `subjects:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.http.get(this.baseUrl, { params: httpParams, headers }), 300000);
  }
}
