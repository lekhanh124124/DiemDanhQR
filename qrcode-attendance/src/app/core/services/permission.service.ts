import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { CacheService } from './cache.service';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly BASE_URL = `${environment.apiBase}/permission`;

  constructor(private http: HttpClient, private auth: AuthService, private cache: CacheService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  getRoles(params?: any): Observable<any> {
    const user = this.auth.getUser?.() || this.auth.getUser?.();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `permissions:roles:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => {
      let p = new HttpParams();
      if (params) Object.entries(params).forEach(([k,v]) => { if (v!==undefined && v!==null && v!=='') p = p.set(k, String(v)); });
      return this.http.get(`${this.BASE_URL}/list`, { headers: this.headers(), params: p });
    }, 300000);
  }

  createRole(body: any): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-role`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  updateRole(body: any): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-role`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  deleteRole(maQuyen: number): Observable<any> {
    const params = new HttpParams().set('maQuyen', maQuyen);
    return this.http.delete(`${this.BASE_URL}/delete-role`, { headers: this.headers(), params }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  getFunctions(params?: any): Observable<any> {
    const user = this.auth.getUser?.() || this.auth.getUser?.();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `permissions:functions:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => {
      let p = new HttpParams();
      if (params) Object.entries(params).forEach(([k, v]) => {
        if (v !== undefined && v !== null) p = p.set(k, String(v));
      });
      return this.http.get(`${this.BASE_URL}/functions`, { headers: this.headers(), params: p });
    }, 300000);
  }

  createFunction(body: any): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-function`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  updateFunction(body: any): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-function`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  deleteFunction(maChucNang: number): Observable<any> {
    const params = new HttpParams().set('maChucNang', maChucNang);
    return this.http.delete(`${this.BASE_URL}/delete-function`, { headers: this.headers(), params }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  createRoleFunction(body: { CodeQuyen: string; CodeChucNang: string }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-role-function`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  updateRoleFunction(body: { FromCodeQuyen: string; FromCodeChucNang: string; ToCodeQuyen: string; ToCodeChucNang: string }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-role-function`, form, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  deleteRoleFunction(codeQuyen: string, codeChucNang: string): Observable<any> {
    const params = new HttpParams().set('codeQuyen', codeQuyen).set('codeChucNang', codeChucNang);
    return this.http.delete(`${this.BASE_URL}/delete-role-function`, { headers: this.headers(), params }).pipe(
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }

  getRoleFunctions(params?: any): Observable<any> {
    const user = this.auth.getUser?.() || this.auth.getUser?.();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `permissions:role-functions:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => {
      let p = new HttpParams();
      if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v)); });
      return this.http.get(`${this.BASE_URL}/role-functions`, { headers: this.headers(), params: p });
    }, 300000);
  }

  updateRoleFunctionGroup(body: any): Observable<any> {
    const onlyFrom = !!body?._onlyFrom;

    const maQuyen = body?.MaQuyen ?? body?.maQuyen ?? body?.FromMaQuyen ?? body?.FromCodeQuyen ?? '';
    const maChucNang = body?.MaChucNang ?? body?.maChucNang ?? body?.MaChucNang ?? body?.MaChucNang ?? '';
    const trangThai = (body && Object.prototype.hasOwnProperty.call(body, 'TrangThai')) ? body.TrangThai : (body?.trangThai ?? undefined);

    const form = new FormData();
    if (onlyFrom) {
      if (trangThai !== undefined && trangThai !== null) form.append('TrangThai', String(trangThai));
      if (body?.FromMaQuyen !== undefined && body?.FromMaQuyen !== null) form.append('FromMaQuyen', String(body.FromMaQuyen));
      if (body?.FromMaChucNang !== undefined && body?.FromMaChucNang !== null) form.append('FromMaChucNang', String(body.FromMaChucNang));
    } else {
      const entries: Record<string, any> = {
        MaQuyen: maQuyen,
        MaChucNang: maChucNang,
        TrangThai: trangThai,
        FromMaQuyen: maQuyen,
        FromMaChucNang: maChucNang,
        ToMaQuyen: maQuyen,
        ToMaChucNang: maChucNang
      };
      Object.entries(entries).forEach(([k, v]) => { if (v !== undefined && v !== null && String(v) !== '') form.append(k, String(v)); });
    }

    const attemptPrimary = () => this.http.put(`${this.BASE_URL}/update-role-function`, form, { headers: this.headers() });
    const attemptFallback = () => this.http.put(`${this.BASE_URL}/update-role-function-group`, form, { headers: this.headers() });

    return attemptPrimary().pipe(
      catchError((err: any) => {
        if (err && err.status === 404) {
          return attemptFallback();
        }
        throw err;
      }),
      tap(() => this.cache.clearPrefix('permissions:'))
    );
  }
}


