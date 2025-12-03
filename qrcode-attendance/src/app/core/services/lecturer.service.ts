import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, of, throwError } from 'rxjs';
import { map, switchMap, catchError, tap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { CacheService } from './cache.service';

@Injectable({ providedIn: 'root' })
export class LecturerService {
  private readonly BASE_URL = `${environment.apiBase}/lecturer`;

  constructor(private http: HttpClient, private auth: AuthService, private cache: CacheService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${this.auth.getToken()}`,
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache'
    });
  }

  getLecturers(params?: {
    Page?: number; PageSize?: number; SortBy?: string; SortDir?: 'asc'|'desc';
    Khoa?: string; HocHam?: string|number; HocVi?: string; NgayTuyenDungFrom?: string; NgayTuyenDungTo?: string; TrangThaiUser?: boolean;
  }): Observable<any> {
    let p = new HttpParams();
    if (params) {
      const hasStatus = Object.prototype.hasOwnProperty.call(params, 'TrangThaiUser') || Object.prototype.hasOwnProperty.call(params as any, 'TrangThai');
      const statusVal: any = (params as any).TrangThaiUser ?? (params as any).TrangThai;
      Object.entries(params).forEach(([k,v]) => {
        if (v!==undefined && v!==null && v!=='') p = p.set(k, String(v));
      });
      if (hasStatus && statusVal !== undefined && statusVal !== '') {
        const s = String(statusVal);
        p = p.set('TrangThaiUser', s);
        p = p.set('TrangThai', s);
        p = p.set('trangThaiUser', s);
        p = p.set('trangThai', s);
      }
    }

    p = p.set('_ts', String(Date.now()));
    return this.http.get(`${this.BASE_URL}/list`, { headers: this.headers(), params: p }).pipe(
      map((res: any) => {
        const raw = res?.data?.items || res?.data || [];
        const normalized = (raw || []).map((g: any) => ({
          ...g,
          maGiangVien: g.maGiangVien || g.MaGiangVien,
          hoTen: g.hoTen || g.HoTen || g.tenGiangVien || g.TenGiangVien || g.hoTen,
          khoa: g.khoa || g.Khoa,
          hocHam: g.hocHam || g.HocHam,
          hocVi: g.hocVi || g.HocVi,
          ngayTuyenDung: g.ngayTuyenDung || g.NgayTuyenDung,
          trangThai: g.trangThai,
          TrangThai: g.TrangThai,
          trangThaiUser: g.trangThaiUser
        }));
        return { ...res, data: { ...(res.data || {}), items: normalized } };
      })
    );
  }

  getLecturersCached(params?: any, ttlMs = 60000): Observable<any> {
    const user = this.auth.getUser?.() || this.auth.getUser?.();
    const userKey = (user && (user.tenDangNhap || user.TenDangNhap || user.username || user.maNguoiDung)) || 'anon';
    const cacheKey = `lecturers:${userKey}:${JSON.stringify(params || {})}`;
    return this.cache.getOrFetch(cacheKey, () => this.getLecturers(params), ttlMs);
  }

  createLecturer(formData: FormData): Observable<any> {
    return this.http.post(`${this.BASE_URL}/create`, formData, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('lecturers:'))
    );
  }

  updateProfile(formData: FormData): Observable<any> {
    return this.http.put(`${this.BASE_URL}/profile`, formData, { headers: this.headers() }).pipe(
      tap(() => this.cache.clearPrefix('lecturers:'))
    );
  }

  deleteLecturer(maGiangVien: string): Observable<any> {
    const form = new FormData();
    form.append('MaGiangVien', maGiangVien);
    form.append('TrangThai', 'false');
    form.append('TrangThaiUser', 'false');
    return this.updateProfile(form).pipe(
      tap(() => this.cache.clearPrefix('lecturers:'))
    );
  }

  getProfile(maGiangVien: string): Observable<any> {
    return this.getLecturers({ Keyword: maGiangVien, Page: 1, PageSize: 200 } as any).pipe(
      map((listRes: any) => {
        const items = listRes?.data?.items || [];
        const g = items.find(
          (x: any) => (x.giangVien?.maGiangVien || x.giangVien?.MaGiangVien) === maGiangVien
        );
        if (!g) return null;

        const st = g.nguoiDung?.trangThai === true || g.nguoiDung?.trangThai === 'true';
        const normalized = {
          maGiangVien: g.giangVien?.maGiangVien || g.giangVien?.MaGiangVien || '-',
          hoTen: g.nguoiDung?.hoTen || '-',
          khoa: g.khoa?.tenKhoa && g.khoa.tenKhoa !== 'null' ? g.khoa.tenKhoa : '-',
          hocHam: g.giangVien?.hocHam && g.giangVien.hocHam !== 'null' ? g.giangVien.hocHam : '-',
          hocVi: g.giangVien?.hocVi && g.giangVien.hocVi !== 'null' ? g.giangVien.hocVi : '-',
          ngayTuyenDung: g.giangVien?.ngayTuyenDung && g.giangVien.ngayTuyenDung !== 'null'
          ? g.giangVien.ngayTuyenDung
          : '-',
          email: g.nguoiDung?.email && g.nguoiDung.email !== 'null' ? g.nguoiDung.email : '-',
          soDienThoai: g.nguoiDung?.soDienThoai && g.nguoiDung.soDienThoai !== 'null'
          ? g.nguoiDung.soDienThoai
          : '-',
          ngaySinh: g.nguoiDung?.ngaySinh && g.nguoiDung.ngaySinh !== 'null' ? g.nguoiDung.ngaySinh : '-',
          diaChi: g.nguoiDung?.diaChi && g.nguoiDung.diaChi !== 'null' ? g.nguoiDung.diaChi : '-',
          trangThaiUser: st,
          anhDaiDien: g.nguoiDung?.anhDaiDien || null
        };

        return { data: normalized };
      }),
      switchMap((found: any) => of(found)),
      catchError((err) => {
        if (err?.status === 404)
          return throwError(() => ({ status: 404, message: `Giảng viên ${maGiangVien} không tìm thấy` }));
        return throwError(() => err);
      })
    );
  }

  setStatus(maGiangVien: string, status: boolean): Observable<any> {
    const form = new FormData();
    form.append('MaGiangVien', String(maGiangVien));
    form.append('TrangThai', String(status));
    form.append('TrangThaiUser', String(status));
    return this.updateProfile(form).pipe(
      tap(() => this.cache.clearPrefix('lecturers:'))
    );
  }
}
