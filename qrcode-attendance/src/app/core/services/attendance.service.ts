import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, throwError, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly BASE_URL = `${environment.apiBase}/attendance`;

  constructor(private http: HttpClient, private auth: AuthService) { }

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  // GET /api/attendance/records
  getRecords(params?: {
    Page?: number; PageSize?: number; SortBy?: string; SortDir?: string;
    MaDiemDanh?: number; ThoiGianQuet?: string; CodeTrangThai?: string; LyDo?: string; TrangThai?: boolean;
    MaBuoi?: number; MaSinhVien?: string;
  }): Observable<any> {
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v)); });
    return this.http.get(`${this.BASE_URL}/records`, { headers: this.headers(), params: p });
  }

  // POST /api/attendance/create-record
  createRecord(body: { MaBuoi: number; MaSinhVien: string; CodeTrangThai: string; LyDo?: string; TrangThai?: boolean }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-record`, form, { headers: this.headers() });
  }

  // PUT /api/attendance/update-record
  updateRecord(body: { MaDiemDanh: number; CodeTrangThai?: string; LyDo?: string; TrangThai?: boolean; MaTrangThai?: number }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-record`, form, { headers: this.headers() }).pipe(
      catchError(err => {
        const status = err && (err.status || err.statusCode || 0);
        if (status === 415 || status === 500) {
          const jsonBody: any = {};
          Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) jsonBody[k] = v; });
          const headers = this.headers();
          return this.http.put(`${this.BASE_URL}/update-record`, jsonBody, { headers });
        }
        return throwError(() => err);
      })
    );
  }

  // GET /api/attendance/statuses
  getStatuses(params?: any): Observable<any> {
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v)); });
    return this.http.get(`${this.BASE_URL}/statuses`, { headers: this.headers(), params: p });
  }

  // POST /api/attendance/create-status
  createStatus(body: { CodeTrangThai: string; TenTrangThai: string }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.post(`${this.BASE_URL}/create-status`, form, { headers: this.headers() });
  }

  // PUT /api/attendance/update-status
  updateStatus(body: { MaTrangThai: number; CodeTrangThai?: string; TenTrangThai?: string }): Observable<any> {
    const form = new FormData();
    Object.entries(body || {}).forEach(([k, v]) => { if (v !== undefined && v !== null) form.append(k, String(v)); });
    return this.http.put(`${this.BASE_URL}/update-status`, form, { headers: this.headers() });
  }

  // DELETE /api/attendance/delete-status?MaTrangThai=6
  deleteStatus(maTrangThai: number): Observable<any> {
    const params = new HttpParams().set('MaTrangThai', String(maTrangThai));
    return this.http.delete(`${this.BASE_URL}/delete-status`, { headers: this.headers(), params });
  }

  generateQrByBuoi(params: { maBuoi: number; ttlSeconds?: number; pixelsPerModule?: number }): Observable<Blob> {
    let p = new HttpParams().set('maBuoi', String(params.maBuoi));
    if (params.ttlSeconds !== undefined) p = p.set('ttlSeconds', String(params.ttlSeconds));
    if (params.pixelsPerModule !== undefined) p = p.set('pixelsPerModule', String(params.pixelsPerModule));
    return this.http.post(`${this.BASE_URL}/qr`, null, {
      headers: this.headers(),
      params: p,
      responseType: 'blob'
    });
  }

  generateQrByBuoiJson(params: { maBuoi: number; ttlSeconds?: number; pixelsPerModule?: number }): Observable<any> {
    let p = new HttpParams().set('maBuoi', String(params.maBuoi));
    if (params.ttlSeconds !== undefined) p = p.set('ttlSeconds', String(params.ttlSeconds));
    if (params.pixelsPerModule !== undefined) p = p.set('pixelsPerModule', String(params.pixelsPerModule));
    return this.http.post(`${this.BASE_URL}/qr`, null, { headers: this.headers(), params: p });
  }

  /**
   * POST /api/attendance/checkin?token=...&latitude=...&longitude=...
   * token: token from scanned QR
   * latitude/longitude: optional GPS coordinates
   */
  checkin(token: string, latitude?: number | null, longitude?: number | null): Observable<any> {
    let p = new HttpParams().set('token', token);
    if (latitude !== undefined && latitude !== null) p = p.set('latitude', String(latitude));
    if (longitude !== undefined && longitude !== null) p = p.set('longitude', String(longitude));
    return this.http.post(`${this.BASE_URL}/checkin`, null, { headers: this.headers(), params: p });
  }

  getStudentHistory(maSinhVien: string, params?: { hocKy?: number; namHoc?: number; maMonHoc?: string }): Observable<any> {
    if (!maSinhVien) return throwError(() => new Error('Missing maSinhVien'));
    const p: any = { MaSinhVien: String(maSinhVien), PageSize: 500 };
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') p[k] = v; });
    return this.getRecords(p);
  }

  // GET /api/attendance/ratio-by-khoa?maHocKy=1
  getRatioByKhoa(maHocKy?: number): Observable<any> {
    let params = new HttpParams();
    if (maHocKy !== undefined && maHocKy !== null) params = params.set('maHocKy', String(maHocKy));
    return this.http.get(`${this.BASE_URL}/ratio-by-khoa`, { headers: this.headers(), params });
  }

  // GET /api/attendance/ratio-by-lophocphan/gv?maHocKy
  // Tỉ lệ vắng/có mặt theo LHP mà GIẢNG VIÊN đang dạy
  getRatioByLopHocPhanForGV(maHocKy?: number): Observable<any> {
    let params = new HttpParams();
    if (maHocKy !== undefined && maHocKy !== null) params = params.set('maHocKy', String(maHocKy));
    return this.http.get(`${this.BASE_URL}/ratio-by-lophocphan/gv`, { headers: this.headers(), params });
  }

  // GET /api/attendance/ratio-by-lophocphan/sv?maHocKy
  // Tỉ lệ vắng/có mặt theo LHP mà SINH VIÊN đang học
  getRatioByLopHocPhanForSV(maHocKy?: number): Observable<any> {
    let params = new HttpParams();
    if (maHocKy !== undefined && maHocKy !== null) params = params.set('maHocKy', String(maHocKy));
    return this.http.get(`${this.BASE_URL}/ratio-by-lophocphan/sv`, { headers: this.headers(), params });
  }

  scanQr(body: { maSinhVien?: string; qrToken?: string; maBuoi?: number }): Observable<any> {
    if (body?.qrToken) return this.checkin(body.qrToken);
    if (body?.maBuoi && body?.maSinhVien) {
      return this.createRecord({ MaBuoi: body.maBuoi, MaSinhVien: String(body.maSinhVien), CodeTrangThai: 'P' });
    }
    return throwError(() => new Error('Invalid scan payload: missing qrToken or maBuoi+maSinhVien'));
  }
}
