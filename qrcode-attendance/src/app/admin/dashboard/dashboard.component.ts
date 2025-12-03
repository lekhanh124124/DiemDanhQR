import { Component, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import Chart from 'chart.js/auto';
import { AttendanceService } from '../../core/services/attendance.service';
import { CourseService } from '../../core/services/course.service';
import { StudentService } from '../../core/services/student.service';
import { LecturerService } from '../../core/services/lecturer.service';
import { UserService } from '../../core/services/user.service';


@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements AfterViewInit {
  userName = 'Admin';
  stats = [
    { title: 'Sinh viên', value: 0, icon: 'user', color: 'text-blue-500' },
    { title: 'Giảng viên', value: 0, icon: 'team', color: 'text-green-500' },
    { title: 'Lớp học phần', value: 0, icon: 'book', color: 'text-purple-500' },
  ];
  statsLoading = false;

  activityLogs = [];
  activityCols = [
    { key: 'time', header: 'Thời gian' },
    { key: 'user', header: 'Người dùng' },
    { key: 'action', header: 'Hành động' }
  ];
  logsLoading = false;

  semesters: Array<{ key: string; HocKy?: any; NamHoc?: any; MaHocKy?: any }> = [];
  selectedSemesterKey: string | null = null;
  loadingChart = false;
  private attendanceChart: Chart | null = null;

  constructor(
    private attendanceService: AttendanceService,
    private courseService: CourseService,
    private studentService: StudentService,
    private lecturerService: LecturerService,
    private cdr: ChangeDetectorRef,
    private userService: UserService
  ) {}

  ngAfterViewInit(): void {
    this.loadSemesters();
    setTimeout(() => {
      this.loadTotals();
      this.loadAdminActivity();
    }, 0);
  }

  private loadAdminActivity(): void {
    try {
      this.logsLoading = true;
      let current = this.userService.getCurrentUser();
      const useUsername = (u: any) => u?.tenDangNhap || u?.TenDangNhap || u?.username || u?.maNguoiDung || u?.maNguoiDung || '';
      const username = useUsername(current);
      const fetchActivities = (tenDangNhap: string) => {
        if (!tenDangNhap) { this.activityLogs = []; this.logsLoading = false; return; }
        this.userService.getUserActivity({ page: 1, pageSize: 50, sortBy: 'thoiGian', sortDir: 'desc' }).subscribe({ next: (res2: any) => {
          const data = res2?.data || res2 || [];
          const items = data?.items || data || [];
          const filtered = (items || []).filter((it: any) => {
            const nd = it?.nguoiDung || it?.NguoiDung || it || {};
            const uname = nd?.tenDangNhap || nd?.TenDangNhap || nd?.username || '';
            return uname === tenDangNhap;
          });
          this.activityLogs = (filtered || []);
          this.logsLoading = false;
        }, error: () => { this.activityLogs = []; this.logsLoading = false; } });
      };

      if (username) {
        fetchActivities(username);
      } else {
        this.userService.getThongTinCaNhan().subscribe({ next: (res: any) => {
          const profile = res?.data || res || null;
          const uname = useUsername(profile || {});
          if (uname) fetchActivities(uname);
          else { this.activityLogs = []; this.logsLoading = false; }
        }, error: () => { this.activityLogs = []; this.logsLoading = false; } });
      }
    } catch (e) { this.activityLogs = []; this.logsLoading = false; }
  }

  public getLogTime(a: any): string {
    if (!a) return '-';
    const ls = a?.lichSuHoatDong || a?.LichSuHoatDong || a?.lichSu || a || {};
    return String(ls?.thoiGian || ls?.ThoiGian || ls?.time || ls?.createdAt || ls?.date || a?.time || a?.Time || '-');
  }

  public getLogUser(a: any): string {
    if (!a) return '-';
    const nd = a?.nguoiDung || a?.NguoiDung || a || {};
    return String(nd?.hoTen || nd?.HoTen || nd?.tenDangNhap || nd?.username || a?.user || a?.User || '-');
  }

  public getLogAction(a: any): string {
    if (!a) return '-';
    const ls = a?.lichSuHoatDong || a?.LichSuHoatDong || a?.lichSu || a || {};
    return String(ls?.hanhDong || ls?.HanhDong || ls?.hanh_dong || ls?.action || a?.action || a?.Action || '-');
  }

  private loadTotals(): void {
    this.statsLoading = true;
    let remaining = 3;
    const finishOne = () => {
      remaining--;
      if (remaining <= 0) {
        this.statsLoading = false;
        try { this.cdr.detectChanges(); } catch (e) {}
      }
    };

    // Students
    try {
      this.studentService.getStudents({ Page: 1, PageSize: 1 }).subscribe({ next: (res: any) => {
        const total = Number(res?.data?.totalRecords ?? res?.data?.total ?? 0);
        if (!Number.isNaN(total)) this.stats[0].value = total;
        try { this.cdr.detectChanges(); } catch (e) {}
        finishOne();
      }, error: (err) => { console.warn('Failed to load students total', err); finishOne(); } });
    } catch (e) { console.warn('loadTotals student error', e); finishOne(); }

    // Lecturers
    try {
      this.lecturerService.getLecturers({ Page: 1, PageSize: 1 }).subscribe({ next: (res: any) => {
        const total = Number(res?.data?.totalRecords ?? res?.data?.total ?? 0);
        if (!Number.isNaN(total)) this.stats[1].value = total;
        try { this.cdr.detectChanges(); } catch (e) {}
        finishOne();
      }, error: (err) => { console.warn('Failed to load lecturers total', err); finishOne(); } });
    } catch (e) { console.warn('loadTotals lecturer error', e); finishOne(); }

    // Courses (Lớp học phần)
    try {
      this.courseService.getCourses({ page: 1, pageSize: 1 }).subscribe({ next: (res: any) => {
        const total = Number(res?.data?.totalRecords ?? res?.data?.total ?? 0);
        if (!Number.isNaN(total)) this.stats[2].value = total;
        try { this.cdr.detectChanges(); } catch (e) {}
        finishOne();
      }, error: (err) => { console.warn('Failed to load courses total', err); finishOne(); } });
    } catch (e) { console.warn('loadTotals course error', e); finishOne(); }
  }

  onSemesterChange(): void {
    let maHocKy: any = undefined;
    if (this.selectedSemesterKey) {
      const found = (this.semesters || []).find(s => s.key === this.selectedSemesterKey);
      if (found) maHocKy = found.MaHocKy ?? found.HocKy ?? found.MaHocKy;
    }
    this.loadChart(maHocKy);
  }

  private loadSemesters(): void {
    this.courseService.getSemesters({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      const map = new Map<string, any>();
      (items || []).forEach((it: any) => {
        const hkObj = it?.hocKy || it?.HocKy || null;
        let ky: any = undefined; let nam: any = undefined; let maHocKy: any = undefined;
        if (hkObj && typeof hkObj === 'object') {
          ky = hkObj?.ky ?? hkObj?.Ky;
          nam = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam;
          maHocKy = hkObj?.maHocKy ?? hkObj?.MaHocKy;
        } else {
          ky = it?.Ky ?? it?.hocKy ?? it?.HocKy;
          nam = it?.NamHoc ?? it?.namHoc ?? it?.Nam;
          maHocKy = it?.MaHocKy ?? it?.MaHocKy;
        }
        const key = ky ? `HK${ky} (${nam || ''})` : (it?.Label || it?.Ten || it?.MaHocKy || 'Khác');
        if (!map.has(key)) map.set(key, { key, HocKy: ky, NamHoc: nam, MaHocKy: maHocKy });
      });
      const values = Array.from(map.values());
      setTimeout(() => {
        this.semesters = values;
        if (this.semesters.length) {
          this.selectedSemesterKey = this.semesters[0].key;
          try { this.cdr.detectChanges(); } catch (e) {}
          const found = (this.semesters || []).find(s => s.key === this.selectedSemesterKey);
          const maHocKy = found ? (found.MaHocKy ?? found.HocKy ?? found.MaHocKy) : undefined;
          this.loadChart(maHocKy);
        }
      }, 0);
    }, error: (err) => { console.error('Failed to load semesters', err); } });
  }

  private loadChart(maHocKy?: number): void {
    this.loadingChart = true;
    this.attendanceService.getRatioByKhoa(maHocKy).subscribe({ next: (res: any) => {
      this.loadingChart = false;
      try { this.cdr.detectChanges(); } catch (e) {}

      const items = (res && res.data) ? res.data : (res || []);

      const labels = (items || []).map((it: any) => {
        const k = it?.khoa || it?.Khoa || it?.tenKhoa || it?.TenKhoa || null;
        if (k && typeof k === 'object') {
          return String(k.tenKhoa || k.TenKhoa || k.codeKhoa || k.maKhoa || JSON.stringify(k));
        }
        return String(k || it?.tenKhoa || it?.TenKhoa || it?.label || '');
      });

      const data = (items || []).map((it: any) => {
        const raw = it?.tyLeCoMat ?? it?.tyLeVang ?? it?.tyLe ?? it?.tiLe ?? it?.TiLe ?? it?.ratio ?? it?.percent ?? it?.value ?? 0;
        return Number(raw);
      });

      let canvas = document.getElementById('attendanceChart') as HTMLCanvasElement | null;
      if (!canvas) {
        setTimeout(() => this.createOrUpdateChart(labels, data), 0);
        return;
      }
      this.createOrUpdateChart(labels, data);
    }, error: (err) => { this.loadingChart = false; try { this.cdr.detectChanges(); } catch (e) {} console.error('Failed to load attendance ratio', err); } });
  }

  private createOrUpdateChart(labels: string[], data: number[]): void {
    const canvas = document.getElementById('attendanceChart') as HTMLCanvasElement | null;
    if (!canvas) return;

    try {
      if (this.attendanceChart) {
        try { this.attendanceChart.destroy(); } catch (_) {}
        this.attendanceChart = null;
      }
    } catch (_) {}

    if (!labels || labels.length === 0) {
      this.attendanceChart = new Chart(canvas, { type: 'bar', data: { labels: [], datasets: [] }, options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, max: 100 } } } });
      try { this.cdr.detectChanges(); } catch (e) {}
      return;
    }

    this.attendanceChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [{ label: 'Tỉ lệ điểm danh (%)', data, backgroundColor: labels.map((_: any, i: number) => {
          const palette = ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444', '#06B6D4', '#F97316'];
          return palette[i % palette.length];
        }) }]
      },
      options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, max: 100 } } }
    });
  }
}
