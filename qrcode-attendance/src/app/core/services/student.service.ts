import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, of, throwError } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

export interface Student {
  maSinhVien: string;
  hoTen: string;
  gioiTinh?: number;
  email?: string;
  soDienThoai?: string;
  ngaySinh?: string;
  danToc?: string;
  tonGiao?: string;
  diaChi?: string;
  lopHanhChinh?: string;
  namNhapHoc?: number;
  khoa?: string;
  nganh?: string;
  anhDaiDien?: string;
  trangThai?: boolean;
}


@Injectable({ providedIn: 'root' })
export class StudentService {
  private baseUrl = `${environment.apiBase}/student`;

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  setStatus(maSinhVien: string, status: boolean): Observable<any> {
    const form = new FormData();
    form.append('MaSinhVien', String(maSinhVien));
    form.append('TrangThai', String(status));
    form.append('TrangThaiUser', String(status));
    return this.updateStudent(form);
  }

  getStudents(params?: any): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined && params[key] !== '')
          httpParams = httpParams.set(key, params[key]);
      });
    }
    return this.http.get(`${this.baseUrl}/list`, {
      headers: this.getAuthHeaders(),
      params: httpParams,
    });
  }

  createStudent(formData: FormData): Observable<any> {
    return this.http.post(`${this.baseUrl}/create`, formData, {
      headers: this.getAuthHeaders(),
    });
  }

  //PUT /api/student/update (form-data)
  updateStudent(formData: FormData): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.put(`${this.baseUrl}/update`, formData, { headers });
  }

  // POST /api/student/bulk-import - form-data: file (excel)
  bulkImport(formData: FormData): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(`${this.baseUrl}/bulk-import`, formData, { headers });
  }

  // cập nhật TrangThai = false theo spec (PUT /api/student/update)
  deleteStudent(maSinhVien: string): Observable<any> {
    const formData = new FormData();
    formData.append('MaSinhVien', maSinhVien);
    formData.append('TrangThai', 'false');
    const headers = this.getAuthHeaders();
    return this.http.put(`${this.baseUrl}/update`, formData, { headers });
  }

  // GET profile nếu backend hỗ trợ dạng /api/student/profile/{maSV}
  getProfile(identifier: string): Observable<Student | null> {
    const fetchBy = (key: string) =>
      this.getStudents({ [key]: identifier, Page: 1, PageSize: 1 }).pipe(
        map((listRes: any) => {
          const items = listRes?.data?.items || listRes?.data || [];
          return (items && items.length) ? this.normalizeStudent(items[0]) : null;
        }),
        catchError(() => of(null))
      );

    return fetchBy('MaNguoiDung').pipe(
      switchMap((res) => (res ? of(res) : fetchBy('MaSinhVien')))
    );
  }

  private normalizeStudent(s: any): Student {
    const coerceBool = (v: any): boolean | undefined => {
      if (v === undefined || v === null) return undefined;
      if (typeof v === 'boolean') return v;
      if (typeof v === 'number') return v === 1;
      if (typeof v === 'string') {
        const low = v.toLowerCase();
        return low === 'true' || low === '1';
      }
      return undefined;
    };
    const nguoiDung = s?.nguoiDung || s?.NguoiDung || null;
    const sinhVien = s?.sinhVien || s?.SinhVien || null;
    const nganhObj = s?.nganh || s?.Nganh || null;
    const khoaObj = s?.khoa || s?.Khoa || null;

    const maSinhVien = (sinhVien?.maSinhVien || sinhVien?.MaSinhVien) || s?.maSinhVien || s?.MaSinhVien || '';
    const hoTen = (nguoiDung?.hoTen || nguoiDung?.HoTen) || s?.hoTen || s?.HoTen || '';
    const namNhapHoc = (sinhVien?.namNhapHoc || sinhVien?.NamNhapHoc) || s?.namNhapHoc || s?.NamNhapHoc;
    const nganh = (nganhObj?.tenNganh || nganhObj?.TenNganh) || (nganhObj?.codeNganh) || s?.nganh || s?.Nganh || null;
    const khoa = (khoaObj?.tenKhoa || khoaObj?.TenKhoa) || (khoaObj?.codeKhoa) || s?.khoa || s?.Khoa || null;

    return {
      maSinhVien: maSinhVien,
      hoTen: hoTen,
      gioiTinh: s?.gioiTinh ?? s?.GioiTinh,
      email: s?.email || s?.Email,
      soDienThoai: s?.soDienThoai || s?.SoDienThoai,
      ngaySinh: s?.ngaySinh || s?.NgaySinh,
      danToc: s?.danToc || s?.DanToc,
      tonGiao: s?.tonGiao || s?.TonGiao,
      diaChi: s?.diaChi || s?.DiaChi,
      lopHanhChinh: s?.lopHanhChinh || s?.LopHanhChinh,
      namNhapHoc: namNhapHoc,
      khoa: typeof khoa === 'string' ? khoa : undefined,
      nganh: typeof nganh === 'string' ? nganh : undefined,
      anhDaiDien: s?.anhDaiDien || s?.AnhDaiDien || (nguoiDung?.anhDaiDien || nguoiDung?.AnhDaiDien) || null,
      trangThai: coerceBool(s?.trangThai) ?? coerceBool(s?.TrangThai) ?? true,
    };
  }

  // POST /api/student/add-to-course
  addToCourse(body: { MaLopHocPhan: string; MaSinhVien: string; NgayThamGia?: string; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    if (body?.MaLopHocPhan) form.append('MaLopHocPhan', String(body.MaLopHocPhan));
    if (body?.MaSinhVien) form.append('MaSinhVien', String(body.MaSinhVien));
    if (body?.NgayThamGia) form.append('NgayThamGia', String(body.NgayThamGia));
    if (typeof body?.TrangThai === 'boolean') form.append('TrangThai', String(body.TrangThai));
    return this.http.post(`${this.baseUrl}/add-to-course`, form, { headers: this.getAuthHeaders() });
  }

  // POST /api/student/add-to-course-bulk
  addToCourseBulk(formData: FormData): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.post(`${this.baseUrl}/add-to-course-bulk`, formData, { headers });
  }

  // PUT /api/student/remove-from-course
  removeFromCourse(body: { MaLopHocPhan: string; MaSinhVien: string }): Observable<any> {
    const form = new FormData();
    if (body?.MaLopHocPhan) form.append('MaLopHocPhan', String(body.MaLopHocPhan));
    if (body?.MaSinhVien) form.append('MaSinhVien', String(body.MaSinhVien));
    const headers = this.getAuthHeaders();
    return this.http.put(`${this.baseUrl}/remove-from-course`, form, { headers });
  }
}
