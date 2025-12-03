import { Component, OnInit } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AttendanceService } from '../../core/services/attendance.service';
import { AuthService } from '../../core/services/auth.service';
import { AttendanceUtilsService } from '../../core/services/attendance-utils.service';
import { CourseService } from '../../core/services/course.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  attendanceHistory: any[] = [];
  loading = false;
  hocKy?: number;
  namHoc?: number | string;
  hocKyOptions: number[] = [];
  namHocOptions: Array<number | string> = [];
  classSections: Array<{ maLopHocPhan: string; tenLopHocPhan: string }> = [];
  selectedClass?: string;

  constructor(
    private modal: NzModalService,
    private attendance: AttendanceService,
    private auth: AuthService,
    private attendanceUtils: AttendanceUtilsService,
    private courseService: CourseService,
    private scheduleService: ScheduleService
  ) {}

  ngOnInit(): void {

  }

  ngAfterViewInit(): void {
    this.loadSemesterOptions();
    setTimeout(() => this.loadClassSections(), 0);
  }

  private getPreferredStudentIdentifier(): string | undefined {
    const user = this.auth.getUser() || {};
    const candidates = [user?.tenDangNhap, user?.TenDangNhap, user?.maSinhVien, user?.MaSinhVien, user?.username, user?.maNguoiDung, user?.MaNguoiDung];
    for (const c of candidates) {
      if (c === undefined || c === null) continue;
      const s = String(c).trim();
      if (!s) continue;
      if (/[A-Za-z]/.test(s)) return s;
    }
    for (const c of candidates) {
      if (c === undefined || c === null) continue;
      const s = String(c).trim();
      if (s) return s;
    }
    return undefined;
  }

  private loadClassSections(): void {
    const maSV = this.getPreferredStudentIdentifier();
    if (!maSV) return;
    this.courseService.getCoursesCached({ MaSinhVien: maSV, exactKeys: true, Page: 1, PageSize: 500 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.classSections = (items || []).map((c: any) => {
        const lop = c?.lopHocPhan || c?.LopHocPhan || c || {};
        const ma = lop?.maLopHocPhan || lop?.MaLopHocPhan || c?.maLopHocPhan || c?.MaLopHocPhan || '';
        const ten = lop?.tenLopHocPhan || lop?.TenLopHocPhan || c?.tenLopHocPhan || c?.TenLopHocPhan || (c?.monHoc || c?.MonHoc || {}).tenMonHoc || '';
        return { maLopHocPhan: String(ma || '').trim(), tenLopHocPhan: String(ten || ma || '').trim() };
      }).filter((x: any) => x.maLopHocPhan);
    }, error: () => { this.classSections = []; } });
  }

  private loadSemesterOptions(): void {
    const token = this.auth.getToken();
    if (!token) {
      this.hocKyOptions = [1, 2, 3];
      const year = new Date().getFullYear();
      this.namHocOptions = [year, year - 1, year - 2];
      return;
    }

    this.courseService.getSemesters({ Page: 1, PageSize: 200 }).subscribe({
      next: (res: any) => {
        const items = res?.data?.items || res?.data || [];
        const kySet = new Set<number>();
        const namSet = new Set<number | string>();
        const semesters: Array<{ ky: number; nam: number | string; maHocKy?: string | number }> = [];

        (items || []).forEach((it: any) => {
          const hk = it?.hocKy ?? it;
          const kyRaw = hk?.ky ?? hk?.maHocKy ?? hk?.Ky ?? hk?.MaHocKy;
          const namRaw = hk?.namHoc ?? hk?.NamHoc ?? hk?.Nam;
          const ky = kyRaw !== undefined && kyRaw !== null ? Number(kyRaw) : null;
          const nam = namRaw !== undefined && namRaw !== null ? namRaw : null;
          if (ky !== null && !Number.isNaN(ky)) {
            kySet.add(ky);
            semesters.push({ ky, nam, maHocKy: hk?.maHocKy ?? hk?.MaHocKy });
          }
          if (nam !== null) namSet.add(nam);
        });

        this.hocKyOptions = Array.from(kySet).sort((a, b) => a - b);
        this.namHocOptions = Array.from(namSet).sort((a: any, b: any) => {
          const na = Number(a); const nb = Number(b); if (!isNaN(na) && !isNaN(nb)) return nb - na; return String(a).localeCompare(String(b));
        });

        if (semesters.length) {
          semesters.sort((x, y) => {
            const yna = Number(y.nam || 0); const xna = Number(x.nam || 0);
            if (yna !== xna) return yna - xna;
            return (y.ky || 0) - (x.ky || 0);
          });
          const recent = semesters[0];
          this.hocKy = recent?.ky ?? this.hocKy;
          this.namHoc = recent?.nam ?? this.namHoc;
        }
      },
      error: () => {
        this.hocKyOptions = [1,2,3];
        const year = new Date().getFullYear();
        this.namHocOptions = [year, year-1, year-2];
      }
    });
  }

  load(): void {
    const maSinhVien = this.getPreferredStudentIdentifier();
    if (!maSinhVien) return;
    const params: any = {};
    if (this.hocKy !== undefined && this.hocKy !== null) params.hocKy = Number(this.hocKy);
    if (this.namHoc !== undefined && this.namHoc !== null && this.namHoc !== '') params.namHoc = Number(this.namHoc);
    if (this.selectedClass) params.MaLopHocPhan = this.selectedClass;

    this.loading = true;
    this.attendance.getStudentHistory(String(maSinhVien), params)
      .pipe(finalize(() => { this.loading = false; }))
      .subscribe({
        next: (res: any) => {
          const raw = res?.data?.items || res?.data || [];
          this.attendanceHistory = (raw || []).map((it: any) => this.normalizeAttendanceRecord(it));
        },
        error: () => {
          this.attendanceHistory = this.attendanceHistory || [];
        }
      });
  }


  public clearFilters(): void {

    this.attendanceHistory = [];
  }

  private normalizeAttendanceRecord(it: any): any {
    const diem = it?.diemDanh || it?.DiemDanh || {};
    const trang = it?.trangThaiDiemDanh || it?.trangThaiDiemDanh || it?.TrangThaiDiemDanh || {};
    const buoi = it?.buoiHoc || it?.BuoiHoc || {};
    const lop = it?.lopHocPhan || it?.LopHocPhan || {};

    const ngay = buoi?.ngayHoc || buoi?.NgayHoc || buoi?.ngay || it?.ngay || it?.ngayHoc || '';
    const tenMon = lop?.tenLopHocPhan || lop?.TenLopHocPhan || (it?.monHoc && (it.monHoc.tenMonHoc || it.monHoc.TenMonHoc || it.monHoc)) || '';

    let trangThaiDisplay: any = trang?.tenTrangThai || trang?.TenTrangThai || trang?.codeTrangThai || trang?.CodeTrangThai;
    if (!trangThaiDisplay) {
      const dt = diem?.trangThai;
      if (dt === true || String(dt).toLowerCase() === 'true') trangThaiDisplay = 'Có mặt';
      else if (dt === false || String(dt).toLowerCase() === 'false') trangThaiDisplay = 'Vắng';
      else trangThaiDisplay = dt ?? '';
    }

    return {
      ...it,
      ngayHoc: String(ngay || '').trim(),
      tenMonHoc: String(tenMon || '').trim(),
      trangThai: trangThaiDisplay,
      _raw: it
    };
  }

   get totalSessions() {
    return this.attendanceHistory.length;
  }

  get presentCount() {
    return this.attendanceHistory.filter(r => this.attendanceUtils.isPresent(r)).length;
  }

  get presentPercent() {
    return this.totalSessions > 0
      ? (this.presentCount / this.totalSessions) * 100
      : 0;
  }

  get absentCount() {
    return this.attendanceHistory.filter(r => this.attendanceUtils.isAbsent(r)).length;
  }

  get absentPermittedCount() {
    return this.attendanceHistory.filter(r => this.attendanceUtils.isAbsent(r) && this.attendanceUtils.isPermitted(r)).length;
  }

  get absentUnpermittedCount() {
    return this.absentCount - this.absentPermittedCount;
  }
}
