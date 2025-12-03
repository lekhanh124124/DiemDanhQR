import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { CacheService } from './cache.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private readonly BASE_URL = `${environment.apiBase}/course`;

  constructor(private http: HttpClient, private authService: AuthService, private cache: CacheService) {}

  getCourses(options?: {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDir?: 'ASC' | 'DESC';
    keyword?: string;
    maLopHocPhan?: string;
    tenLopHocPhan?: string;
    trangThai?: string;
    maMonHoc?: string;
    tenMonHoc?: string;
    soTinChi?: number;
    soTiet?: number;
    maGiangVien?: string;
    tenGiangVien?: string;
    hocKy?: number;
    maSinhVien?: string;
      namHoc?: string;
    exactKeys?: boolean;
    }): Observable<any> {
    const token = this.authService.getToken();
    if (!token) return throwError(() => new Error('Token không hợp lệ'));

    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    const exact = !!(options && (options as any).exactKeys);
    let params = new HttpParams();
    params = params.set('Page', options?.page?.toString() || '1');
    params = params.set('PageSize', options?.pageSize?.toString() || '20');
    params = params.set('SortBy', options?.sortBy || 'TenMonHoc');
    params = params.set('SortDir', options?.sortDir || 'DESC');

    const o: any = options || {};
    if (exact) {
      Object.entries(o).forEach(([k, v]) => {
        if (k === 'exactKeys') return;
        if (v === undefined || v === null) return;
        const s = String(v);
        if (s.trim() === '') return;
        params = params.set(k, s);
      });
    } else {
      const safeSet = (key: string, value: any) => {
        if (value === undefined || value === null) return;
        const s = String(value);
        if (s.trim() === '') return;
        params = params.set(key, s);
      };

      safeSet('Keyword', o.keyword);
      safeSet('maLopHocPhan', o.maLopHocPhan ?? o.MaLopHocPhan);
      safeSet('MaLopHocPhan', o.MaLopHocPhan ?? o.maLopHocPhan);
      safeSet('tenLopHocPhan', o.tenLopHocPhan ?? o.TenLopHocPhan);
      safeSet('trangThai', o.trangThai ?? o.TrangThai);
      safeSet('MaMonHoc', o.maMonHoc ?? o.MaMonHoc);
      safeSet('tenMonHoc', o.tenMonHoc ?? o.TenMonHoc);
      safeSet('TenMonHoc', o.tenMonHoc ?? o.TenMonHoc);
      safeSet('soTinChi', o.soTinChi?.toString());
      safeSet('soTiet', o.soTiet?.toString());
      safeSet('maGiangVien', o.maGiangVien ?? o.MaGiangVien);
      safeSet('tenGiangVien', o.tenGiangVien ?? o.TenGiangVien);
      if (o.MaHocKy !== undefined && o.MaHocKy !== null) {
        safeSet('MaHocKy', String(o.MaHocKy));
      }
      if (o.hocKy !== undefined && o.hocKy !== null) {
        safeSet('Ky', String(o.hocKy));
      }
      safeSet('NamHoc', o.namHoc ? String(o.namHoc) : undefined);

      if (options?.maSinhVien) {
        const ms = String(options.maSinhVien);
        if (ms.trim() !== '') {
          params = params.set('maSinhVien', ms);
          params = params.set('MaSinhVien', ms);
          params = params.set('masinhvien', ms);
        }
      }
    }

    return this.http.get(`${this.BASE_URL}/list`, { headers, params });
  }

  getCoursesCached(options?: any, ttlMs = 60_000): Observable<any> {
    const user = this.authService.getUser();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const userMg = (user && (user.maGiangVien || user.MaGiangVien || user?.nguoiDung?.maGiangVien || user?.nguoiDung?.MaGiangVien)) || '';
    const cacheKey = `courses:${userKey}:${userMg}:${JSON.stringify(options || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.getCourses(options), ttlMs);
  }

  // Subjects
  getSubjects(params?: any): Observable<any> {
    const token = this.authService.getToken();
    if (!token) return throwError(() => new Error('Token không hợp lệ'));
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v)); });
    const user = this.authService.getUser();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `subjects:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.http.get(`${this.BASE_URL}/subjects`, { headers, params: p }));
  }

  createSubject(body: { MaMonHoc: string; TenMonHoc: string; SoTinChi: number; SoTiet: number; MoTa?: string; TrangThai?: boolean }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-subject`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('subjects:'))
    );
  }

  updateSubject(body: { MaMonHoc: string; TenMonHoc?: string; SoTinChi?: number; SoTiet?: number; MoTa?: string; TrangThai?: boolean }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-subject`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('subjects:'))
    );
  }

  // Courses
  createCourse(body: { MaLopHocPhan: string; TenLopHocPhan: string; MaMonHoc: string; MaGiangVien: string; MaHocKy?: number; TrangThai?: boolean }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-course`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('courses:'))
    );
  }

  addStudentToCourse(body: { MaLopHocPhan: string; MaSinhVien: string; NgayThamGia?: string; TrangThai?: boolean }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/add-student`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('courses:'))
    );
  }

  updateCourse(body: { MaLopHocPhan: string; TenLopHocPhan?: string; TrangThai?: boolean; MaMonHoc?: string; MaGiangVien?: string; MaHocKy?: number }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-course`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('courses:'))
    );
  }

  // Semesters
  getSemesters(params?: { Page?: number; PageSize?: number; SortBy?: string; SortDir?: string; NamHoc?: number; Ky?: number }): Observable<any> {
    const token = this.authService.getToken();
    if (!token) return throwError(() => new Error('Token không hợp lệ'));
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v)); });
    const user = this.authService.getUser();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `semesters:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.http.get(`${this.BASE_URL}/semesters`, { headers, params: p }));
  }

  createSemester(body: { NamHoc: number; Ky: number }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-semester`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('semesters:'))
    );
  }

  updateSemester(body: { MaHocKy: number; NamHoc?: number; Ky?: number }): Observable<any> {
    const headers = new HttpHeaders({ Authorization: `Bearer ${this.authService.getToken()}` });
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-semester`, form, { headers }).pipe(
      tap(() => this.cache.clearPrefix('semesters:'))
    );
  }
}
