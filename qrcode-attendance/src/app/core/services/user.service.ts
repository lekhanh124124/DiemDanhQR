import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = `${environment.apiBase}/user`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken?.() || (this.authService as any).getAccessToken?.();
    if (!token) throw new Error('Token không hợp lệ');
    return new HttpHeaders({ 'Authorization': `Bearer ${token}`, 'Accept': 'application/json' });
  }

  // NGƯỜI DÙNG
  getThongTinCaNhan(): Observable<any> {
    const headers = this.getAuthHeaders();

    return this.http.get(`${this.baseUrl}/info`, { headers }).pipe(
      catchError((err: any) => {
        try {
          const msg = err?.error?.Message || err?.message || '';
          if (err?.status === 404 && /không có hồ sơ/i.test(msg)) {
            const currentUser = this.authService.getUser();
            const fallback = { data: currentUser || this.getCurrentUser(), fallback: true };
            return of(fallback);
          }
        } catch (e) {
        }
        return throwError(() => err);
      })
    );
  }

  getUserInfoByUsername(tenDangNhap: string): Observable<any> {
    const headers = this.getAuthHeaders();
    const params = new HttpParams().set('TenDangNhap', tenDangNhap);
    return this.http.get(`${this.baseUrl}/info`, { headers, params }).pipe(
      catchError((err: any) => {
        return throwError(() => err);
      })
    );
  }

  updateUserProfile(data: { [k: string]: any }): Observable<any> {
    const headers = this.getAuthHeaders();
    const formData = new FormData();
    Object.entries(data || {}).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        formData.append(key, value instanceof File ? value : String(value));
      }
    });
    return this.http.put(`${this.baseUrl}/update`, formData, { headers });
  }

  createUser(data: { [k: string]: any }): Observable<any> {
    const headers = this.getAuthHeaders();
    const form = new FormData();
    Object.entries(data || {}).forEach(([k, v]) => {
      if (v !== undefined && v !== null) {
        form.append(k, v instanceof File ? v : String(v));
      }
    });
    return this.http.post(`${this.baseUrl}/create`, form, { headers });
  }

  listUsers(options?: {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDir?: string;
    tenDangNhap?: string;
    hoTen?: string;
    maQuyen?: string | number;
    codeQuyen?: string;
    trangThai?: any;
  }): Observable<any> {
    const headers = this.getAuthHeaders();

    let params = new HttpParams();
    if (options?.page !== undefined && options?.page !== null) params = params.set('page', String(options.page));
    if (options?.pageSize !== undefined && options?.pageSize !== null) params = params.set('pageSize', String(options.pageSize));
    if (options?.sortBy) params = params.set('sortBy', options.sortBy);
    if (options?.sortDir) params = params.set('sortDir', options.sortDir);
    if (options?.tenDangNhap) params = params.set('tenDangNhap', options.tenDangNhap);
    if (options?.hoTen) params = params.set('hoTen', options.hoTen);
    if (options?.maQuyen !== undefined && options?.maQuyen !== null && String(options.maQuyen).trim() !== '') params = params.set('maQuyen', String(options.maQuyen));
    if (options?.codeQuyen !== undefined && options?.codeQuyen !== null && String(options.codeQuyen).trim() !== '') params = params.set('codequyen', String(options.codeQuyen));
    if (options?.trangThai !== undefined && options?.trangThai !== null && String(options.trangThai).trim() !== '') params = params.set('trangThai', String(options.trangThai));
    return this.http.get(`${this.baseUrl}/list`, { headers, params });
  }

  getCurrentUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  getUserActivity(options?: {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDir?: 'asc' | 'desc';
    tenDangNhap?: string;
    dateFrom?: string; // yyyy-MM-dd
    dateTo?: string;   // yyyy-MM-dd
  }): Observable<any> {
    const headers = this.getAuthHeaders();
    let params = new HttpParams();
    if (options?.page) params = params.set('Page', options.page);
    if (options?.pageSize) params = params.set('PageSize', options.pageSize);
    if (options?.sortBy) params = params.set('SortBy', options.sortBy);
    if (options?.sortDir) params = params.set('SortDir', options.sortDir);
    if (options?.tenDangNhap) params = params.set('tendangnhap', options.tenDangNhap);
    if (options?.dateFrom) params = params.set('DateFrom', options.dateFrom);
    if (options?.dateTo) params = params.set('DateTo', options.dateTo);
    return this.http.get(`${this.baseUrl}/activity`, { headers, params });
  }
}
