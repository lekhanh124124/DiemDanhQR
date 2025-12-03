import { Component, OnInit } from '@angular/core';
import { ScheduleService } from '../../core/services/schedule.service';
import { AuthService } from '../../core/services/auth.service';
import { CourseService } from '../../core/services/course.service';

@Component({
  selector: 'app-student-schedule',
  templateUrl: './schedule.component.html',
  styleUrls: ['./schedule.component.scss']
})
export class ScheduleComponent implements OnInit {
  tab: 'week' | 'progress' = 'progress';
  schedules: any[] = [];
  // groupedByWeek: Record<string, any[]> = {};
  // groupedByProgress: Record<string, any[]> = {};
  semesters: Array<{ key: string; HocKy?: any; NamHoc?: any; MaHocKy?: any }> = [];
  selectedSemesterKey: string | null = null;
  selectedDate: Date = new Date();
  currentWeek: { date: Date; key: string; label: string }[] = [];
  caHoc = ['Sáng', 'Chiều', 'Tối'];

  constructor(private scheduleService: ScheduleService, private auth: AuthService, private courseService: CourseService) {}

  ngOnInit(): void {
    this.generateWeek();
    const user = this.auth.getUser();
    const maSV = user?.maSinhVien || user?.MaSinhVien || user?.maNguoiDung || user?.maNguoiDung || user?.username || user?.tenDangNhap || user?.nguoiDung?.maSinhVien;
    if (!maSV) return;

    try {
      const tok = this.auth.getToken();
      if (tok) this.loadSemestersFromApi();
    } catch (e) {
    }

    this.scheduleService.getSchedulesCached({ MaSinhVien: maSV, PageSize: 500 }).subscribe((res: any) => {
      const items = res?.data?.items || res?.data || [];
      const mapped = this.normalizeItems(items || []);
      this.schedules = mapped || [];
      if (!this.semesters || this.semesters.length === 0) {
        this.extractSemesters(items || []);
      }
      this.buildWeek(mapped || []);
      this.buildProgress(mapped || []);
    }, () => {});
  }

  //  Chuyển Date sang yyyy-MM-dd để so sánh
  private toKey(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  //  Tạo danh sách ngày trong tuần (T2 → CN)
  generateWeek(): void {
    const start = new Date(this.selectedDate);
    const day = start.getDay();
    const diff = start.getDate() - day + (day === 0 ? -6 : 1);
    const startOfWeek = new Date(start.setDate(diff));

    const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật'];
    this.currentWeek = Array.from({ length: 7 }, (_, i) => {
      const d = new Date(startOfWeek);
      d.setDate(d.getDate() + i);
      return { date: d, key: this.toKey(d), label: daysOfWeek[i] };
    });
  }

  //  Xác định ca học dựa theo tiết bắt đầu
  getCa(tietBatDau: number): string {
    if (tietBatDau >= 1 && tietBatDau <= 5) return 'Sáng';
    if (tietBatDau >= 6 && tietBatDau <= 10) return 'Chiều';
    return 'Tối';
  }

  private tietStartTimes: string[] = [
    // '07:00','07:50','08:40','09:30','10:20', // tiết 1-5 (sáng)
    // '13:00','13:50','14:40','15:30','16:20', // tiết 6-10 (chiều)
    // '18:00','18:50' // tối
  ];

  getCurrentTiet(): number | null {
    const now = new Date();
    const hhmm = `${String(now.getHours()).padStart(2,'0')}:${String(now.getMinutes()).padStart(2,'0')}`;
    for (let i = 0; i < this.tietStartTimes.length; i++) {
      const start = this.tietStartTimes[i];
      const [sh, sm] = start.split(':').map(Number);
      const startMs = new Date(now.getFullYear(), now.getMonth(), now.getDate(), sh, sm).getTime();
      const endMs = startMs + (45 * 60 * 1000);
      const nowMs = now.getTime();
      if (nowMs >= startMs && nowMs < endMs) return i + 1;
    }
    return null;
  }

  getStyle(buoi: any) {
    const todayKey = this.toKey(new Date());
    const currentTiet = this.getCurrentTiet();
    const isToday = buoi.ngayKey === todayKey;
    const buoiStart = Number(buoi.tietBatDau) || 0;
    const buoiLen = Number(buoi.soTiet) || 1;

    if (isToday && currentTiet !== null && currentTiet >= buoiStart && currentTiet < buoiStart + buoiLen) {
      return { 'background-color': '#ffd54f', 'box-shadow': '0 0 0 3px rgba(255,213,79,0.2)' };
    }

    if (isToday) return { opacity: '0.6', 'background-color': '#f5f5f5' };

    const type = String(buoi.loaiLich || buoi.loaiBuoi || '').toUpperCase();
    const hasLT = type.includes('LT');
    const hasTH = type.includes('TH');
    if (hasLT && hasTH) return { 'background-image': 'linear-gradient(135deg,#e0e0e0 0%, #8bc34a 100%)', color: '#222' };
    if (hasTH) return { 'background-color': '#8bc34a', color: 'white' };
    return { 'background-color': '#e0e0e0' };
  }

  nextWeek(): void { this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() + 7)); this.generateWeek(); }
  prevWeek(): void { this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() - 7)); this.generateWeek(); }
  goToToday(): void { this.selectedDate = new Date(); this.generateWeek(); }

  private loadSemestersFromApi() {
    this.courseService.getSemesters({ Page: 1, PageSize: 200 }).subscribe({
      next: (res: any) => {
        const items = res?.data?.items || res?.data || [];
        const map = new Map<string, { key: string; HocKy?: any; NamHoc?: any; MaHocKy?: any }>();
        (items || []).forEach((it: any) => {
          const hkObj = it?.hocKy || it?.HocKy || null;
          let ky: any = undefined;
          let nam: any = undefined;
          let maHocKy: any = undefined;
          if (hkObj && typeof hkObj === 'object') {
            ky = hkObj?.ky ?? hkObj?.Ky ?? hkObj?.ky ?? hkObj?.ky;
            nam = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam ?? hkObj?.namHoc;
            maHocKy = hkObj?.maHocKy ?? hkObj?.MaHocKy ?? hkObj?.MaHocKy;
          } else {
            ky = it?.Ky ?? it?.hocKy ?? it?.MaHocKy ?? it?.MaHocKy;
            nam = it?.NamHoc ?? it?.namHoc ?? it?.Nam;
          }
          const key = ky ? `HK${ky} (${nam || ''})` : (it?.Label || it?.Ten || it?.MaHocKy || 'Khác');
          if (!map.has(key)) map.set(key, { key, HocKy: ky, NamHoc: nam, MaHocKy: maHocKy });
        });
        this.semesters = Array.from(map.values());
      },
      error: () => {
      }
    });
  }

  private extractSemesters(items: any[]) {
    const map = new Map<string, { key: string; HocKy?: any; NamHoc?: any; MaHocKy?: any }>();
    (items || []).forEach(it => {
      const hkObj = it?.hocKy || it?.HocKy || null;
      let ky: any = undefined;
      let nam: any = undefined;
      let maHocKy: any = undefined;
      if (hkObj && typeof hkObj === 'object') {
        ky = hkObj?.ky ?? hkObj?.Ky ?? hkObj?.ky;
        nam = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam ?? hkObj?.namHoc;
        maHocKy = hkObj?.maHocKy ?? hkObj?.MaHocKy ?? hkObj?.MaHocKy;
      } else {
        ky = it?.HocKy ?? it?.hocKy ?? it?.hk;
        nam = it?.NamHoc ?? it?.namHoc ?? it?.nam;
      }
      const key = ky ? `HK${ky} (${nam || ''})` : (it?.HocKyStr || it?.NamHoc || it?.MaLopHocPhan || 'Khác');
      if (!map.has(key)) map.set(key, { key, HocKy: ky, NamHoc: nam, MaHocKy: maHocKy });
    });
    this.semesters = Array.from(map.values());
  }

  loadSchedule() {
    const user = this.auth.getUser();
    const maSV = user?.maSinhVien || user?.MaSinhVien || user?.maNguoiDung || user?.maNguoiDung || user?.username || user?.tenDangNhap || user?.nguoiDung?.maSinhVien;
    if (!maSV) return;
    try { console.debug('[DEBUG] ScheduleComponent will call getSchedules with MaSinhVien=', maSV, 'selectedSemester=', this.selectedSemesterKey); } catch (e) {}
    let maHocKy: any = undefined;
    if (this.selectedSemesterKey) {
      const found = (this.semesters || []).find(s => s.key === this.selectedSemesterKey);
      if (found) maHocKy = found.MaHocKy ?? found.HocKy ?? found.MaHocKy;
    }

    const params: any = { PageSize: 500, MaSinhVien: maSV };
    if (maHocKy !== undefined && maHocKy !== null && String(maHocKy).trim() !== '') params.MaHocKy = maHocKy;

    this.scheduleService.getSchedulesCached(params).subscribe((res: any) => {
      const items = res?.data?.items || res?.data || [];
      const mapped = this.normalizeItems(items || []);
      this.schedules = mapped || [];
      this.buildWeek(mapped || []);
      this.buildProgress(mapped || []);
      if (!this.semesters || this.semesters.length === 0) this.extractSemesters(items || []);
    }, () => {});
  }

  private normalizeItems(rawItems: any): any[] {
    const items = Array.isArray(rawItems) ? rawItems : (rawItems?.data?.items || rawItems?.data || rawItems?.items || rawItems || []);
    return (items || []).map((it: any) => {
      const flat: any = {
        maBuoi: it?.buoiHoc?.maBuoi || it?.maBuoi || it?.MaBuoi,
        ngayHoc: it?.buoiHoc?.ngayHoc || it?.NgayHoc || it?.ngayHoc,
        tietBatDau: it?.buoiHoc?.tietBatDau || it?.tietBatDau || it?.TietBatDau,
        soTiet: it?.buoiHoc?.soTiet || it?.soTiet || it?.SoTiet,
        tenPhong: it?.phongHoc?.tenPhong || it?.tenPhong || it?.TenPhong,
        maLopHocPhan: it?.lopHocPhan?.maLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan,
        tenMonHoc: it?.monHoc?.tenMonHoc || it?.tenMonHoc || it?.TenMonHoc,
        giangVien: it?.giangVien || it?.giangVienInfo || it?.nguoiDung
      };
      const ngayRaw = String(flat.ngayHoc || '').trim();
      let ngayKey: string | null = null;
      if (ngayRaw) {
        if (ngayRaw.includes('-')) {
          const parts = ngayRaw.split('-');
          if (parts.length === 3 && parts[0].length === 2) ngayKey = `${parts[2]}-${parts[1].padStart(2,'0')}-${parts[0].padStart(2,'0')}`;
          else ngayKey = parts.join('-');
        } else if (ngayRaw.includes('/')) {
          const p = ngayRaw.split('/'); if (p.length === 3) ngayKey = `${p[2]}-${p[1].padStart(2,'0')}-${p[0].padStart(2,'0')}`;
        }
      }
      return { ...it, ...flat, ngayKey };
    });
  }

  get progressList(): any[] {
    if (!this.selectedSemesterKey) return [];
    return (this.schedules || []).filter(it => {
      const hkObj = it?.hocKy || it?.HocKy || null;
      let ky: any = undefined;
      let nam: any = undefined;
      if (hkObj && typeof hkObj === 'object') {
        ky = hkObj?.ky ?? hkObj?.Ky ?? hkObj?.ky;
        nam = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam ?? hkObj?.namHoc;
      } else {
        ky = it?.HocKy ?? it?.hocKy ?? it?.hk;
        nam = it?.NamHoc ?? it?.namHoc ?? it?.nam;
      }
      const key = ky ? `HK${ky} (${nam || ''})` : (it?.HocKyStr || it?.NamHoc || it?.MaLopHocPhan || 'Khác');
      return key === this.selectedSemesterKey;
    });
  }

  weekdayLabel(item: any): string {
    const d = item?.NgayHoc || item?.ngay || item?.Ngay || null;
    if (!d) return item?.Thu || item?.thu || '-';
    try {
      const dt = new Date(d);
      const labels = ['Chủ Nhật','Thứ 2','Thứ 3','Thứ 4','Thứ 5','Thứ 6','Thứ 7'];
      return labels[dt.getDay()] || '-';
    } catch {
      return String(d);
    }
  }

  formatTiet(item: any): string {
    if (item?.Tiet) return item.Tiet;
    if (item?.tiet) return item.tiet;
    if (item?.TietBatDau) {
      const start = Number(item.TietBatDau);
      const len = Number(item.SoTiet || item.soTiet || 1);
      return `${start}${len>1 ? ' - ' + (start + len - 1) : ''}`;
    }
    return '-';
  }

  private buildWeek(items: any[]) {
    const map: Record<string, any[]> = {};
    (items || []).forEach(it => {
      const d = it?.NgayHoc || it?.ngayHoc || it?.ngay || it?.Ngay || null;
      let key = 'Không rõ';
      if (d) {
        try { const dt = new Date(d); key = ['Chủ Nhật','Thứ 2','Thứ 3','Thứ 4','Thứ 5','Thứ 6','Thứ 7'][dt.getDay()]; } catch { key = String(d); }
      }
      if (!map[key]) map[key] = [];
      map[key].push(it);
    });
    // this.groupedByWeek = map;
  }

  private buildProgress(items: any[]) {
    const map: Record<string, any[]> = {};
    (items || []).forEach(it => {
      let ky: any = undefined;
      let nam: any = undefined;
      const hkObj = it?.hocKy || it?.HocKy || null;
      if (hkObj && typeof hkObj === 'object') {
        ky = hkObj?.ky ?? hkObj?.Ky ?? hkObj?.ky;
        nam = hkObj?.namHoc ?? hkObj?.NamHoc ?? hkObj?.nam ?? hkObj?.namHoc;
      } else {
        ky = it?.HocKy ?? it?.hocKy ?? it?.hk;
        nam = it?.NamHoc ?? it?.namHoc ?? it?.Nam;
      }
      const key = ky ? `HK${ky} (${nam||''})` : (it?.HocKyStr || it?.NamHoc || it?.MaLopHocPhan || 'Khác');
      if (!map[key]) map[key] = [];
      map[key].push(it);
    });
    // this.groupedByProgress = map;
  }
  // switchTab(t: 'week'|'progress') { this.tab = t; }
}
