import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { CacheService } from './cache.service';

@Injectable({ providedIn: 'root' })
export class ScheduleService {
  private baseUrl = `${environment.apiBase}/schedule`;

  constructor(private http: HttpClient, private authService: AuthService, private cache: CacheService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  // Lấy danh sách buổi học
  getSchedules(params: any = {}): Observable<any> {
    let httpParams = new HttpParams();
    for (const key in params) {
      if (Object.prototype.hasOwnProperty.call(params, key)) {
        const v = params[key];
        if (v === undefined || v === null) continue;
        const s = String(v);
        if (s.trim() === '') continue;
        httpParams = httpParams.set(key, s);
      }
    }

    const providedMs = params?.maSinhVien || params?.MaSinhVien || params?.masinhvien;
    if (providedMs) {
      const ms = String(providedMs);
      if (ms.trim() !== '') {
        httpParams = httpParams.set('maSinhVien', ms);
        httpParams = httpParams.set('MaSinhVien', ms);
        httpParams = httpParams.set('masinhvien', ms);
      }
    }


    return this.http.get(`${this.baseUrl}/list`, {
      headers: this.getAuthHeaders(),
      params: httpParams
    });
  }

  getSchedulesCached(params: any = {}, ttlMs = 60_000): Observable<any> {
    const token = this.authService.getToken();
    if (!token) return throwError(() => new Error('Token không hợp lệ'));
    const user = this.authService.getUser();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `schedules:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.getSchedules(params), ttlMs);
  }

  // Lấy danh sách phòng học
  getRooms(params: any = {}): Observable<any> {
    let httpParams = new HttpParams();
    for (const key in params) {
      if (params[key] !== undefined && params[key] !== null) {
        httpParams = httpParams.set(key, params[key]);
      }
    }

    return this.http.get(`${this.baseUrl}/rooms`, {
      headers: this.getAuthHeaders(),
      params: httpParams
    });
  }

  // Tạo phòng học
  createRoom(body: { TenPhong: string; ToaNha?: string; Tang?: number; SucChua?: number; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.baseUrl}/create-room`, form, { headers: this.getAuthHeaders() }).pipe(
      tap(() => this.cache.clearPrefix('schedules:'))
    );
  }

  // Cập nhật phòng học
  updateRoom(body: { MaPhong: number; TenPhong?: string; ToaNha?: string; Tang?: number; SucChua?: number; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.baseUrl}/update-room`, form, { headers: this.getAuthHeaders() }).pipe(
      tap(() => this.cache.clearPrefix('schedules:'))
    );
  }

  // Xóa phòng học
  deleteRoom(maPhong: number): Observable<any> {
    const headers = this.getAuthHeaders();
    const params = new HttpParams().set('maPhong', String(maPhong));
    return new Observable(observer => {
      this.http.delete(`${this.baseUrl}/delete-room`, { headers, params }).subscribe({
        next: (res) => { observer.next(res); observer.complete(); },
        error: () => {
          this.updateRoom({ MaPhong: maPhong, TrangThai: false }).subscribe({
            next: (res2) => { observer.next(res2); observer.complete(); },
            error: (e2) => { observer.error(e2); }
          });
        }
      });
    });
  }

  // Tạo buổi học
  createSchedule(body: { MaLopHocPhan: string; MaPhong: number; NgayHoc: string; TietBatDau: number; SoTiet: number; GhiChu?: string; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.baseUrl}/create-schedule`, form, { headers: this.getAuthHeaders() }).pipe(
      tap(() => this.cache.clearPrefix('schedules:'))
    );
  }

  // Cập nhật buổi học
  updateSchedule(body: { MaBuoi: number; MaPhong?: number; NgayHoc?: string; TietBatDau?: number; SoTiet?: number; GhiChu?: string; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.baseUrl}/update-schedule`, form, { headers: this.getAuthHeaders() }).pipe(
      tap(() => this.cache.clearPrefix('schedules:'))
    );
  }

  // Xóa buổi học (fallback: tắt trạng thái nếu không có API xóa)
  deleteSchedule(maBuoi: number): Observable<any> {
    const headers = this.getAuthHeaders();
    const params = new HttpParams().set('maBuoi', String(maBuoi));
    return new Observable(observer => {
      this.http.delete(`${this.baseUrl}/delete-schedule`, { headers, params }).subscribe({
        next: (res) => { this.cache.clearPrefix('schedules:'); observer.next(res); observer.complete(); },
        error: () => {
          this.updateSchedule({ MaBuoi: maBuoi, TrangThai: false }).subscribe({
            next: (res2) => { observer.next(res2); observer.complete(); },
            error: (e2) => { observer.error(e2); }
          });
        }
      });
    });
  }

  // Sinh buổi học tự động cho một Lớp Học Phần
  autoGenerate(body: { MaLopHocPhan: string } ): Observable<any> {
    const form = new FormData();
    if (body && body.MaLopHocPhan !== undefined && body.MaLopHocPhan !== null) {
      form.append('MaLopHocPhan', String(body.MaLopHocPhan));
    }
    return this.http.post(`${this.baseUrl}/auto-generate`, form, { headers: this.getAuthHeaders() }).pipe(
      tap(() => this.cache.clearPrefix('schedules:'))
    );
  }
}
