import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, finalize, shareReplay } from 'rxjs/operators';

interface LoginResponse {
  status: number;
  message?: string;
  data: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private BASE_URL = `${environment.apiBase}/auth`;
  private user$ = new BehaviorSubject<any>(this.getUser());
  private refreshTimeout: any = null;
  private refreshCall$: Observable<any> | null = null;
  private periodicRefreshTimer: any = null;

  constructor(private http: HttpClient, private router: Router) {}

  // API spec: POST /api/auth/login
  login(tenDangNhap: string, matKhau: string): Observable<LoginResponse> {
    const form = new FormData();
    form.append('tendangnhap', tenDangNhap);
    form.append('matkhau', matKhau);
    return this.http.post<LoginResponse>(`${this.BASE_URL}/login`, form).pipe(
      tap(res => {
        if (res?.data) {
          this.saveTokens(res.data.accessToken, res.data.refreshToken);
          const user = res.data.nguoiDung || res.data.user || res.data;
          this.saveUser(user);
          this.user$.next(user);
          this.scheduleRefresh();
        }
      })
    );
  }

  logout(): void {
    const token = this.getAccessToken();
    if (token) {
      const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
      this.http.post(`${this.BASE_URL}/logout`, {}, { headers }).subscribe({ next: () => {}, error: () => {} });
    }
    this.clear();
    this.router.navigate(['/auth/login']);
  }

  // API spec: POST /api/auth/refreshtoken
  refreshToken(): Observable<any> {
    const refresh = this.getRefreshToken();
    const user = this.getUser();
    if (!refresh || !user) return throwError(() => new Error('No refresh token'));

    if (this.refreshCall$) return this.refreshCall$;

    const form = new FormData();
    const ten = user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung || '';
    form.append('TenDangNhap', String(ten));
    form.append('RefreshToken', String(refresh));

    this.refreshCall$ = this.http.post(`${this.BASE_URL}/refreshtoken`, form).pipe(
      tap((res: any) => {
        if (res?.data?.accessToken) {
          this.saveTokens(res.data.accessToken, res.data.refreshToken);
          this.scheduleRefresh();
        }
      }),
      finalize(() => { this.refreshCall$ = null; }),
      shareReplay(1)
    );

    return this.refreshCall$;
  }

  // API spec: POST /api/auth/changepassword
  changePassword(body: { matkhaucu: string; matkhaumoi: string }): Observable<any> {
    const form = new FormData();
    form.append('matkhaucu', body.matkhaucu);
    form.append('matkhaumoi', body.matkhaumoi);
    return this.http.post(`${this.BASE_URL}/changepassword`, form);
  }

  // API spec: POST /api/auth/refreshpassword
  refreshPassword(body: { TenDangNhap: string }): Observable<any> {
    const form = new FormData();
    form.append('TenDangNhap', body.TenDangNhap);
    return this.http.post(`${this.BASE_URL}/refreshpassword`, form);
  }

  saveTokens(accessToken: string | null, refreshToken: string | null) {
    if (accessToken) localStorage.setItem('access_token', accessToken);
    if (refreshToken) localStorage.setItem('refresh_token', refreshToken);
    this.scheduleRefresh();
  }

  getAccessToken(): string | null { return localStorage.getItem('access_token'); }
  getRefreshToken(): string | null { return localStorage.getItem('refresh_token'); }

  getToken(): string | null { return this.getAccessToken(); }

  saveUser(user: any): void { if (user) localStorage.setItem('user', JSON.stringify(user)); }
  getUser(): any { try { return JSON.parse(localStorage.getItem('user') || 'null'); } catch { return null; } }

  clear(): void {
    if (this.refreshTimeout) { clearTimeout(this.refreshTimeout); this.refreshTimeout = null; }
    localStorage.removeItem('access_token'); localStorage.removeItem('refresh_token'); localStorage.removeItem('user'); this.user$.next(null);
  }

  private scheduleRefresh() {
    try {
      if (this.refreshTimeout) {
        clearTimeout(this.refreshTimeout);
        this.refreshTimeout = null;
      }
      const token = this.getAccessToken();
      if (!token) return;
      const payload = this.parseJwt(token);
      const exp = payload?.exp;
      if (!exp) return;
      const expiresAt = exp * 1000;
      const now = Date.now();
      const msBefore = Math.max(1000, expiresAt - now - 60_000);
      if (expiresAt <= now + 2000) {
        this.refreshToken().subscribe({ next: () => {}, error: () => { this.clear(); } });
        return;
      }
      this.refreshTimeout = setTimeout(() => {
        this.refreshToken().subscribe({ next: () => {}, error: () => { this.clear(); } });
      }, msBefore);
    } catch (e) {
    }
  }

  private parseJwt(token: string | null): any {
    if (!token) return null;
    try {
      const parts = token.split('.');
      if (parts.length < 2) return null;
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch (e) {
      return null;
    }
  }

}
