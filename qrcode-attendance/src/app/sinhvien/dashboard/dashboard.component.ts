import { Component, AfterViewInit, OnInit, OnDestroy } from '@angular/core';
import Chart from 'chart.js/auto'
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { StudentService } from '../../core/services/student.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { CourseService } from '../../core/services/course.service';
import { AttendanceService } from '../../core/services/attendance.service';
import { AttendanceUtilsService } from '../../core/services/attendance-utils.service';
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, AfterViewInit, OnDestroy {
  public authUser: any = null;
  userProfile: any = null;
  profileLoaded = false;
  scheduleLoaded = false;
  attendanceLoaded = false;
  // classSectionsLoaded = false;
  todaySchedule: any[] = [];
  classSections: any[] = [];
  semesters: Array<{ label: string; hocKy?: number; namHoc?: string; key: string }> = [];
  selectedSemesterKey?: string;
  selectedSemesterForStats?: string;
  semesterClassSections: any[] = [];
  attendanceRates: Array<{ subject: string; percent: number; present: number; total: number }> = [];
  attendanceRatesLoading = false;
  semestersLoading = false;
  semestersRetryCount = 0;
  maxSemesterRetries = 3;

  private chartRef: any = null;
  private scheduleScrollTimer: any = null;
  private scheduleContainer?: HTMLElement | null = null;
  private statsSemesterSubj: Subject<string | undefined> = new Subject();
  private statsSubscription?: Subscription;

  constructor(
    private auth: AuthService,
    private studentService: StudentService,
    private scheduleService: ScheduleService,
    private attendanceService: AttendanceService,
    private courseService: CourseService,
    private attendanceUtils: AttendanceUtilsService
  ) {}

  ngOnInit(): void {
    const user = this.auth.getUser();
    this.authUser = user;
    this.userProfile = this.normalizeStudent(user) || null;
    const maSV = this.resolveStudentCode();

    if (maSV) {
      // this.authUser = user;
      this.userProfile = this.normalizeStudent(user) || null;
    }
    try {
      if (maSV) {
        this.loadProfile();
        this.loadTodaySchedule();
        try { this.loadSemestersFromApi(); } catch (e) {}
      }
    } catch (e) {}

    this.statsSubscription = this.statsSemesterSubj.pipe(debounceTime(300)).subscribe((key) => {
      if (!key || key === 'choose') { this.loadAttendanceRates(undefined); return; }
      const found = (this.semesters || []).find(s => s.key === key) as any;
      const numericMaHocKy = Number(found?.MaHocKy ?? found?.hocKy ?? found?.HocKy);
      this.loadAttendanceRates(!isNaN(numericMaHocKy) ? numericMaHocKy : undefined);
    });
  }

  ngAfterViewInit(): void {
    this.renderChart();
  }

  loadProfile(): void {
    if (this.profileLoaded) return;
    const maSV = this.resolveStudentCode();
    if (!maSV) return;
    this.loadProfileFlexible(String(maSV));
    this.profileLoaded = true;
  }

  loadProfileFlexible(identifier: string): void {
    if (!identifier) return;
    try {
      this.studentService.getProfile(identifier).subscribe({
        next: (profile: any) => {
          if (profile) {
            this.userProfile = profile;
          }
        },
        error: () => {}
      });
    } catch (e) {}
  }

  loadTodaySchedule(): void {
    if (this.scheduleLoaded) return;
    const maSV = this.resolveStudentCode();
    if (!maSV) return;
    this.scheduleService.getSchedulesCached({ MaSinhVien: maSV, PageSize: 500 }).subscribe((res: any) => {
      const items = res?.data?.items || res?.data || [];
      const todayKey = this.dateKey(new Date());
      this.todaySchedule = (items || []).filter((it: any) => {
        const d = it?.NgayHoc || it?.ngayHoc || it?.ngay || it?.ngayHocStr;
        return d ? this.dateKey(new Date(d)) === todayKey : false;
      });
      const map = new Map<string, any>();
      (items || []).forEach((it: any) => {
        const id = it?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLop || it?.maLop;
        if (id && !map.has(id)) map.set(id, { id, ten: it?.TenMonHoc || it?.tenMonHoc || it?.TenLopHocPhan || it?.tenLopHocPhan });
      });
      this.classSections = Array.from(map.values());
      this.initSemesters(items || []);
      setTimeout(() => { try { this.scheduleContainer = document.getElementById('todayScheduleContainer'); this.startScheduleAutoScroll(); } catch(e){} }, 200);
      this.scheduleLoaded = true;
    }, () => {});
  }

  loadAttendanceRates(maHocKy?: number): void {
    const maSV = this.resolveStudentCode();
    if (!maSV) return;
    if (maHocKy === undefined || maHocKy === null) {
      this.attendanceRates = [];
      this.attendanceRatesLoading = false;
      this.renderChart();
      return;
    }
    this.attendanceRatesLoading = true;
    this.attendanceService.getRatioByLopHocPhanForSV(maHocKy).subscribe({
      next: (res: any) => {
        this.attendanceRatesLoading = false;
        const items = res?.data?.items || res?.data || [];
        if (items && items.length) {
          this.attendanceRates = (items || []).map((it: any) => {
            let subject = 'Không rõ';
            if (it?.monHoc) subject = it.monHoc?.tenMonHoc || it.monHoc?.TenMonHoc || it.monHoc?.maMonHoc || String(it.monHoc);
            else if (it?.monhoc) subject = it.monhoc?.tenMonHoc || it.monhoc?.TenMonHoc || it.monhoc?.maMonHoc || String(it.monhoc);
            else if (it?.tenMonHoc) subject = it.tenMonHoc || it.TenMonHoc;
            else if (it?.lopHocPhan) subject = it.lopHocPhan?.tenLopHocPhan || it.lopHocPhan?.TenLopHocPhan || it.lopHocPhan?.maLopHocPhan || String(it.lopHocPhan);
            else if (it?.LopHocPhan) subject = it.LopHocPhan?.tenLopHocPhan || it.LopHocPhan?.TenLopHocPhan || it.LopHocPhan?.maLopHocPhan || String(it.LopHocPhan);

            const present = Number(it?.tongCoMat || it?.tongCoMatSinhVien || it?.present || it?.soCoMat || it?.coMat || 0) || 0;
            const total = Number(it?.tongBuoi || it?.soLuong || it?.tong || it?.totalCount || it?.soLuongSinhVien || 0) || 0;
            const percent = (it?.tyLeCoMat !== undefined && it?.tyLeCoMat !== null) ? Number(it.tyLeCoMat) : (typeof it?.percent === 'number' ? it.percent : (total ? Math.round((present / total) * 100) : 0));
            return { subject, present, total, percent };
          });
        } else {
          const histItems = res?.data || [];
          const grouped: Record<string, { present: number; total: number }> = {};
          (histItems || []).forEach((rec: any) => {
            const subject = rec?.tenMonHoc || rec?.TenMonHoc || rec?.monHoc || 'Không rõ';
            if (!grouped[subject]) grouped[subject] = { present: 0, total: 0 };
            grouped[subject].total += 1;
            if (this.attendanceUtils.isPresent(rec)) grouped[subject].present += 1;
          });
          this.attendanceRates = Object.keys(grouped).map(k => ({ subject: k, present: grouped[k].present, total: grouped[k].total, percent: grouped[k].total ? Math.round((grouped[k].present / grouped[k].total) * 100) : 0 }));
        }
        this.renderChart();
        this.attendanceLoaded = true;
      },
      error: (err: any) => {
        this.attendanceRatesLoading = false;
        const ma = String(maSV);
        this.attendanceService.getStudentHistory(ma, { hocKy: maHocKy }).subscribe({
          next: (r: any) => {
            const items = r?.data?.items || r?.data || [];
            const grouped: Record<string, { present: number; total: number }> = {};
            (items || []).forEach((rec: any) => {
              const subject = rec?.tenMonHoc || rec?.TenMonHoc || rec?.monHoc || 'Không rõ';
              if (!grouped[subject]) grouped[subject] = { present: 0, total: 0 };
              grouped[subject].total += 1;
              if (this.attendanceUtils.isPresent(rec)) grouped[subject].present += 1;
            });
            this.attendanceRates = Object.keys(grouped).map(k => ({ subject: k, present: grouped[k].present, total: grouped[k].total, percent: grouped[k].total ? Math.round((grouped[k].present / grouped[k].total) * 100) : 0 }));
            this.renderChart();
            this.attendanceLoaded = true;
          },
          error: () => { this.attendanceRates = []; this.renderChart(); }
        });
      }
    });
  }


  onSelectSemesterForStats(key: string | undefined): void {
    this.selectedSemesterForStats = key;
    this.statsSemesterSubj.next(key);
  }

  ngOnDestroy(): void {
    if (this.chartRef && this.chartRef.destroy) this.chartRef.destroy();
    this.stopScheduleAutoScroll();
    try { if (this.statsSubscription) this.statsSubscription.unsubscribe(); } catch (e) {}
  }

  private dateKey(d: Date): string {
    return `${d.getFullYear()}-${d.getMonth()+1}-${d.getDate()}`;
  }

  private renderChart(): void {
    const ctx = document.getElementById('studentAttendanceChart') as HTMLCanvasElement | null;
    if (!ctx) return;
    const labels = this.attendanceRates.map(r => r.subject);
    const data = this.attendanceRates.map(r => r.percent);
    if (this.chartRef && this.chartRef.destroy) this.chartRef.destroy();
    this.chartRef = new Chart(ctx, {
      type: 'bar',
      data: { labels, datasets: [{ label: 'Tỉ lệ điểm danh (%)', data, backgroundColor: labels.map((_, i) => ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B'][i % 4]) }] },
      options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, max: 100 } } }
    });
  }

  private initSemesters(items: any[]): void {
    const set = new Map<string, { hocKy?: number; namHoc?: string; MaHocKy?: any }>();
    (items || []).forEach(it => {
      const hocKyObj = it?.HocKy ?? it?.hocKy ?? it?.hocky ?? null;
      let hocKy: any = undefined;
      let maHocKy: any = undefined;
      if (hocKyObj && typeof hocKyObj === 'object') {
        maHocKy = hocKyObj?.maHocKy ?? hocKyObj?.MaHocKy ?? hocKyObj?.MaHocKy;
        hocKy = hocKyObj?.ky ?? hocKyObj?.Ky ?? hocKyObj?.hocKy ?? hocKyObj?.HocKy;
      } else {
        hocKy = hocKyObj;
        maHocKy = it?.MaHocKy ?? it?.maHocKy ?? undefined;
      }
      const namHoc = it?.NamHoc ?? it?.namHoc ?? it?.namHocStr;
      const key = `${hocKy ?? 'x'}|${namHoc ?? 'y'}`;
      if (!set.has(key)) set.set(key, { hocKy, namHoc, MaHocKy: maHocKy });
    });
    this.semesters = Array.from(set.entries()).map(([k, v]) => ({ key: k, hocKy: v.hocKy, namHoc: v.namHoc, MaHocKy: v.MaHocKy, label: `${v.hocKy ? 'HK' + v.hocKy : ''} ${v.namHoc ? '(' + v.namHoc + ')' : ''}`.trim() }));
    if (this.semesters.length) this.semesters.unshift({ key: 'choose', label: 'Chọn học kỳ' });
    const first = this.semesters.find(s => s.key !== 'choose');
    if (first) {
      this.onSelectSemester(first.key);
    }
  }

  private loadSemestersFromApi(): void {
    try {
      this.semestersLoading = true;
      this.courseService.getSemesters({ Page: 1, PageSize: 200 }).subscribe({
        next: (res: any) => {
          this.semestersLoading = false;
          this.semestersRetryCount = 0;
          const items = res?.data?.items || res?.data || [];
          const map = new Map<string, { key: string; hocKy?: any; namHoc?: any; MaHocKy?: any; label: string }>();
          (items || []).forEach((it: any) => {
            const hkObj = it?.hocKy || it?.HocKy || null;
            let hocKy: any = undefined;
            let namHoc: any = undefined;
            let maHocKy: any = undefined;
            if (hkObj && typeof hkObj === 'object') {
              maHocKy = hkObj?.maHocKy ?? hkObj?.MaHocKy ?? hkObj?.maHocKy;
              hocKy = hkObj?.ky ?? hkObj?.Ky ?? hkObj?.hocKy ?? hkObj?.HocKy;
              namHoc = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam ?? hkObj?.Nam;
            } else {
              maHocKy = it?.MaHocKy ?? it?.maHocKy;
              hocKy = it?.HocKy ?? it?.hocKy ?? it?.Ky;
              namHoc = it?.NamHoc ?? it?.namHoc ?? it?.nam;
            }
            const key = `${hocKy ?? 'x'}|${namHoc ?? 'y'}`;
            const label = `${hocKy ? 'HK' + hocKy : ''} ${namHoc ? '(' + namHoc + ')' : ''}`.trim() || (it?.Label || it?.Ten || 'Khác');
            if (!map.has(key)) map.set(key, { key, hocKy, namHoc, MaHocKy: maHocKy, label });
          });
          this.semesters = Array.from(map.values());
          if (this.semesters.length) this.semesters.unshift({ key: 'choose', label: 'Chọn học kỳ' } as any);
          const first = this.semesters.find((s: any) => s.key !== 'choose');
          if (first) this.onSelectSemester(first.key);
        },
        error: () => {
          this.semestersLoading = false;
          try {
            this.semestersRetryCount = (this.semestersRetryCount || 0) + 1;
            if (this.semestersRetryCount <= this.maxSemesterRetries) {
              const delay = 500 * Math.pow(2, this.semestersRetryCount - 1);
              setTimeout(() => {
                this.loadSemestersFromApi();
              }, delay);
            }
          } catch (e) {}
        }
      });
    } catch (e) {}
  }

  onSelectSemester(key: string): void {
    this.selectedSemesterKey = key;
    if (!key || key === 'choose') { this.semesterClassSections = []; return; }
    const [hk, nh] = key.split('|');
    const pasthk = hk === 'x' ? undefined : Number(hk);
    const pastnh = nh === 'y' ? undefined : nh;
    const maSV = this.resolveStudentCode();
    if (!maSV) return;
    let maHocKy: any = undefined;
    const found = (this.semesters || []).find(s => s.key === this.selectedSemesterKey);
    if (found) maHocKy = (found as any).MaHocKy ?? (found as any).hocKy ?? (found as any).HocKy ?? undefined;

    if (maHocKy !== undefined && maHocKy !== null && String(maHocKy).trim() !== '') {
      const hocKyVal = Number((found as any).hocKy ?? (found as any).MaHocKy ?? maHocKy);
      const courseOpts: any = { page: 1, pageSize: 200, hocKy: hocKyVal };
      if (maHocKy !== undefined && maHocKy !== null && String(maHocKy).trim() !== '') {
        courseOpts.MaHocKy = maHocKy;
      }
      const forcedMaSinhVien = this.resolveStudentCode();
      if (forcedMaSinhVien) courseOpts.maSinhVien = String(forcedMaSinhVien);
      if (courseOpts.maNguoiDung) delete courseOpts.maNguoiDung;
      this.courseService.getCourses(courseOpts).subscribe({
        next: (res: any) => {
          const items = res?.data?.items || res?.data || [];
          const map = new Map<string, any>();
          (items || []).forEach((it: any) => {
            const id = it?.lopHocPhan?.maLopHocPhan || it?.lopHocPhan?.MaLopHocPhan || it?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLop || it?.maLop;
            const ten = it?.lopHocPhan?.tenLopHocPhan || it?.lopHocPhan?.TenLopHocPhan || it?.monHoc?.tenMonHoc || it?.monHoc?.TenMonHoc || it?.TenMonHoc || it?.tenMonHoc || it?.Ten || it?.ten;
            if (id && !map.has(id)) map.set(id, { id, ten });
          });
          const arr = Array.from(map.values());
          if (arr.length) {
            this.semesterClassSections = arr;
            return;
          }
          this.loadClassSectionsFromSchedules(maHocKy, maSV, pasthk, pastnh);
        },
        error: () => {
          this.loadClassSectionsFromSchedules(maHocKy, maSV, pasthk, pastnh);
        }
      });
    } else {
      this.loadClassSectionsFromSchedules(maHocKy, maSV, pasthk, pastnh);
    }
  }

  private loadClassSectionsFromSchedules(maHocKy: any, maSV: any, pasthk: any, pastnh: any) {
    const params: any = { MaSinhVien: maSV, PageSize: 500 };
    if (maHocKy !== undefined && maHocKy !== null && String(maHocKy).trim() !== '') params.MaHocKy = maHocKy;
    this.scheduleService.getSchedulesCached(params).subscribe((res: any) => {
      const items = res?.data?.items || res?.data || [];
      const map = new Map<string, any>();
      (items || []).filter((it: any) => {
        const hocKyVal = it?.HocKy ?? it?.hocKy ?? it?.hocky ?? (it?.buoiHoc?.hocKy);
        const namHocVal = it?.NamHoc ?? it?.namHoc ?? it?.namHocStr ?? (it?.buoiHoc?.namHoc);
        const matchHocKy = pasthk === undefined || String(hocKyVal) === String(pasthk);
        const matchNamHoc = pastnh === undefined || String(namHocVal) === String(pastnh);
        return matchHocKy && matchNamHoc;
      }).forEach((it: any) => {
        const id = it?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLop || it?.maLop || it?.lopHocPhan?.maLopHocPhan;
        if (id && !map.has(id)) map.set(id, { id, ten: it?.TenMonHoc || it?.tenMonHoc || it?.lopHocPhan?.tenLopHocPhan || it?.TenLopHocPhan || it?.ten });
      });
      this.semesterClassSections = Array.from(map.values());
    }, () => { this.semesterClassSections = []; });
  }

  private startScheduleAutoScroll(): void {
    this.stopScheduleAutoScroll();
    if (!this.scheduleContainer) return;
    const el = this.scheduleContainer;
    if (el.scrollHeight <= el.clientHeight) return;
    this.scheduleScrollTimer = setInterval(() => {
      try {
        if (!el) return;
        el.scrollTop += 1;
        if (el.scrollTop + el.clientHeight >= el.scrollHeight) {
          el.scrollTop = 0;
        }
      } catch (e) {}
    }, 60);
  }

  private stopScheduleAutoScroll(): void {
    if (this.scheduleScrollTimer) { clearInterval(this.scheduleScrollTimer); this.scheduleScrollTimer = null; }
  }

  private normalizeStudent(s: any) {
    if (!s) return null;
    const nguoiDung = s?.nguoiDung || s?.NguoiDung || null;
    const sinhVien = s?.sinhVien || s?.SinhVien || null;
    const nganhObj = s?.nganh || s?.Nganh || null;
    const khoaObj = s?.khoa || s?.Khoa || null;

    const maSinhVien = (sinhVien?.maSinhVien || sinhVien?.MaSinhVien) || s?.maSinhVien || s?.MaSinhVien || '';
    const hoTen = (nguoiDung?.hoTen || nguoiDung?.HoTen) || s?.hoTen || s?.HoTen || '';
    const ngaySinh = (nguoiDung?.ngaySinh || nguoiDung?.NgaySinh) || s?.ngaySinh || s?.NgaySinh || null;
    const namNhapHoc = (sinhVien?.namNhapHoc || sinhVien?.NamNhapHoc) || s?.namNhapHoc || s?.NamNhapHoc || null;
    const nganh = (nganhObj?.tenNganh || nganhObj?.TenNganh) || (nganhObj?.codeNganh) || s?.nganh || s?.Nganh || null;

    return {
      maSinhVien: maSinhVien,
      hoTen: hoTen,
      ngaySinh: ngaySinh,
      lopHanhChinh: s?.lopHanhChinh || s?.LopHanhChinh || null,
      namNhapHoc: namNhapHoc,
      nganh: typeof nganh === 'string' ? nganh : null,
      anhDaiDien: s?.anhDaiDien || s?.AnhDaiDien || (nguoiDung?.anhDaiDien || nguoiDung?.AnhDaiDien) || null
    };
  }

  displayNganh(n: any): string {
    if (!n) return '-';
    if (typeof n === 'string') return n;
    return n?.tenNganh || n?.TenNganh || n?.codeNganh || n?.code || n?.maNganh || JSON.stringify(n) || '-';
  }

  displayNgaySinh(s: any): string {
    if (!s) return '-';
    const d = s?.ngaySinh || s?.NgaySinh || s?.sinhVienNgaySinh || null;
    if (!d) return '-';
    try { const dt = new Date(d); if (!isNaN(dt.getTime())) return dt.toLocaleDateString(); } catch {}
    return String(d);
  }

  displayNamNhapHoc(s: any): string {
    if (!s) return '-';
    const n = s?.namNhapHoc || s?.NamNhapHoc || s?.sinhVien?.namNhapHoc || s?.sinhVien?.NamNhapHoc || null;
    return n ? String(n) : '-';
  }

  private resolveStudentCode(): string | undefined {
    try {
      const u = this.auth.getUser() || (this.authUser || {});
      // Prefer TenDangNhap / username first (this is the login code the backend expects in many endpoints)
      const preferred = u?.tenDangNhap || u?.TenDangNhap || u?.username || u?.userName || u?.maNguoiDung || u?.MaNguoiDung;
      if (preferred) return String(preferred);
      // Next, prefer normalized profile maSinhVien if available
      if (this.userProfile && this.userProfile.maSinhVien) return String(this.userProfile.maSinhVien);
      // fallback to other fields if present
      return (u?.maSinhVien || u?.MaSinhVien || u?.maNguoiDung || u?.MaNguoiDung) ? String(u?.maSinhVien || u?.MaSinhVien || u?.maNguoiDung || u?.MaNguoiDung) : undefined;
    } catch (e) {
      return undefined;
    }
  }


}
