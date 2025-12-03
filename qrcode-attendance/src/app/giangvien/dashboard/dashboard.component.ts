import { Component, AfterViewInit, ChangeDetectorRef, OnDestroy, ViewChild, ElementRef, HostListener } from '@angular/core';
import Chart from 'chart.js/auto';
import { AttendanceService } from '../../core/services/attendance.service';
import { CourseService } from '../../core/services/course.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements AfterViewInit, OnDestroy{
  private attendanceChart: Chart | null = null;
  constructor(
    private attendanceService: AttendanceService,
    private courseService: CourseService,
    private scheduleService: ScheduleService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  semesters: Array<{ key: string; HocKy?: any; NamHoc?: any; MaHocKy?: any }> = [];
  selectedSemesterKey: string | null = null;
  loadingChart = false;

  public todayClasses: any[] = [];
  public loadingUpcoming = false;
  public currentIndex = 0;
  private carouselTimer: any = null;
  private readonly carouselIntervalMs = 3000;
  @ViewChild('todayContainer', { static: false }) private todayContainerRef?: ElementRef<HTMLDivElement>;
  @ViewChild('todayInner', { static: false }) private todayInnerRef?: ElementRef<HTMLDivElement>;


  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadSemesters();
      this.loadTodayClasses();
    }, 0);
  }

  ngOnDestroy(): void {
    this.stopCarousel();
  }

  private toKeyFromRaw(rawDate: string): string | null {
    if (!rawDate) return null;
    const s = String(rawDate).trim();
    if (!s) return null;
    if (s.includes('/')) {
      const p = s.split('/'); if (p.length === 3) return `${p[2]}-${p[1].padStart(2,'0')}-${p[0].padStart(2,'0')}`;
    }
    if (s.includes('-')) {
      const p = s.split('-'); if (p.length === 3) {
        if (p[0].length === 2) return `${p[2]}-${p[1].padStart(2,'0')}-${p[0].padStart(2,'0')}`;
        return s;
      }
    }
    try { const d = new Date(s); if (!isNaN(d.getTime())) return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; } catch (e) {}
    return null;
  }

  public formatDateDisplay(rawDate: string): string {
    const key = this.toKeyFromRaw(rawDate);
    if (!key) return rawDate || '';
    const [yy, mm, dd] = key.split('-');
    return `${dd}/${mm}/${yy}`;
  }

  private startCarousel(): void {
    this.stopCarousel();
    this.carouselTimer = setInterval(() => {
      try {
        this.currentIndex = (this.currentIndex + 1) % (this.todayClasses.length || 1);
        try { this.updateInnerTransform(); this.cdr.detectChanges(); } catch (e) {}
      } catch (e) {}
    }, this.carouselIntervalMs);
  }

  private stopCarousel(): void {
    if (this.carouselTimer) { clearInterval(this.carouselTimer); this.carouselTimer = null; }
    this.currentIndex = 0;
    try { if (this.todayInnerRef && this.todayInnerRef.nativeElement) this.todayInnerRef.nativeElement.style.transform = 'translateY(0px)'; } catch (e) {}
  }

  private loadTodayClasses(): void {
    this.loadingUpcoming = true;
    const user = this.auth.getUser();
    const rawCandidates = [user?.username, user?.tenDangNhap, user?.maGiangVien, user?.MaGiangVien, user?.maNguoiDung];
    const seen = new Set<string>(); const candidates: string[] = [];
    for (const v of rawCandidates) { if (!v) continue; const s = String(v).trim(); if (!s) continue; if (!seen.has(s)) { seen.add(s); candidates.push(s); } }

    const normalize = (itemsRaw: any): any[] => {
      const items = Array.isArray(itemsRaw) ? itemsRaw : (itemsRaw?.data?.items || itemsRaw?.data || itemsRaw?.items || []);
      return (items || []).map((it: any) => {
        const buoi = it?.buoiHoc || it || {};
        const lop = it?.lopHocPhan || it?.lop || it?.LopHocPhan || {};
        const phong = it?.phongHoc || it?.phong || {};
        return {
          raw: it,
          tenLop: lop?.tenLopHocPhan || lop?.TenLopHocPhan || lop?.tenLop || it?.tenLop || it?.TenLop || '-',
          ngayRaw: buoi?.ngayHoc || it?.ngayHoc || it?.NgayHoc || '',
          ngayKey: this.toKeyFromRaw(buoi?.ngayHoc || it?.ngayHoc || it?.NgayHoc || ''),
          gio: (() => { const start = Number(buoi?.tietBatDau || buoi?.TietBatDau || 0) || 0; const len = Number(buoi?.soTiet || buoi?.SoTiet || 1) || 1; if (!start) return ''; return `${start} - ${Math.max(1, start + len - 1)}`; })(),
          phong: phong?.tenPhong || phong?.TenPhong || phong?.maPhong || it?.maPhong || ''
        };
      });
    };

    const tryIndex = (idx: number) => {
      const candidate = candidates[idx];
      const todayKey = (() => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`; })();
      const params: any = { Page: 1, PageSize: 200, NgayHocFrom: todayKey };
      if (candidate) params.MaGiangVien = candidate;

      this.scheduleService.getSchedulesCached(params).subscribe({ next: (res: any) => {
        const mapped = normalize(res) || [];
        const today = mapped.filter((m: any) => m.ngayKey === todayKey);
        this.todayClasses = today || [];
        this.loadingUpcoming = false;
        this.stopCarousel();
        setTimeout(() => {
          try { this.updateInnerTransform(); } catch (e) {}
          if (this.todayClasses.length > 1) this.startCarousel();
        }, 0);
      }, error: (err) => {
        if (idx + 1 < candidates.length) tryIndex(idx + 1);
        else { this.todayClasses = []; this.loadingUpcoming = false; }
      } });
    };

    if (candidates.length) tryIndex(0); else { this.todayClasses = []; this.loadingUpcoming = false; }
  }

  private updateInnerTransform(): void {
    try {
      const container = this.todayContainerRef && this.todayContainerRef.nativeElement;
      const inner = this.todayInnerRef && this.todayInnerRef.nativeElement;
      if (!container || !inner) return;
      const offset = this.currentIndex * container.clientHeight;
      inner.style.transform = `translateY(-${offset}px)`;
    } catch (e) {}
  }

  @HostListener('window:resize') onWindowResize(): void {
    try { this.updateInnerTransform(); } catch (e) {}
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
      setTimeout(() => {
        this.semesters = Array.from(map.values());
        if (this.semesters.length) {
          this.selectedSemesterKey = this.semesters[0].key;
          try { this.cdr.detectChanges(); } catch (e) {}
          const found = (this.semesters || []).find(s => s.key === this.selectedSemesterKey);
          const maHocKy = found ? (found.MaHocKy ?? found.HocKy ?? found.MaHocKy) : undefined;
          this.loadChart(maHocKy);
        } else {
          this.loadChart(undefined);
        }
      }, 0);
    }, error: (err) => { console.error('Failed to load semesters for giangvien dashboard', err); this.loadChart(undefined); } });
  }

  private loadChart(maHocKy?: number): void {
    this.loadingChart = true;
    this.attendanceService.getRatioByLopHocPhanForGV(maHocKy).subscribe({ next: (res: any) => {
      this.loadingChart = false;
      const items = (res && res.data) ? res.data : (res || []);
      const labels = (items || []).map((it: any) => {
        const lop = it?.lopHocPhan || it?.LopHocPhan || null;
        const mon = it?.monHoc || it?.MonHoc || null;
        return lop?.tenLopHocPhan || lop?.TenLopHocPhan || mon?.tenMonHoc || mon?.TenMonHoc || it.tenLopHocPhan || it.TenLopHocPhan || it.tenLop || it.TenLop || it.MaLopHocPhan || 'Lớp';
      });
      const data = (items || []).map((it: any) => {
        const presentRaw = it?.tyLeCoMat ?? it?.TyLeCoMat ?? it?.tyLeCoMat?.toString?.() ?? null;
        if (presentRaw !== null && presentRaw !== undefined) {
          const n = Number(presentRaw);
          return Number.isFinite(n) ? Math.max(0, Math.min(100, n)) : 0;
        }
        const absentRaw = it?.tyLeVang ?? it?.TyLeVang ?? it?.tyLeVang?.toString?.() ?? null;
        if (absentRaw !== null && absentRaw !== undefined) {
          const n = Number(absentRaw);
          if (Number.isFinite(n)) return Math.max(0, Math.min(100, 100 - n));
        }
        const other = Number(it?.tiLe ?? it?.TiLe ?? it?.ratio ?? it?.percent ?? it?.value ?? 0);
        return Number.isFinite(other) ? Math.max(0, Math.min(100, other)) : 0;
      });
      const canvas = document.getElementById('attendanceRateChart') as HTMLCanvasElement | null;
      if (!canvas) return;

      try { if (this.attendanceChart) { this.attendanceChart.destroy(); this.attendanceChart = null; } } catch (e) {}

      if (!items || items.length === 0) {
        try { this.attendanceChart = new Chart(canvas, { type: 'bar', data: { labels: [], datasets: [] }, options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, max: 100 } } } }); } catch (e) {}
        try { this.cdr.detectChanges(); } catch (e) {}
        return;
      }

      try {
        const px = Math.min(350, Math.max(140, labels.length * 32));
        canvas.style.height = px + 'px';
        canvas.height = px;
        canvas.style.maxHeight = '350px';
      } catch (e) {}

      try { console.debug('[gv-dashboard] attendance chart labels:', labels); console.debug('[gv-dashboard] attendance chart data:', data); } catch (e) {}

      try { canvas.style.width = '100%'; } catch (e) {}
      this.attendanceChart = new Chart(canvas, {
        type: 'bar',
        data: {
          labels,
          datasets: [{
            label: 'Tỉ lệ điểm danh (%)',
            data,
            backgroundColor: labels.map((_: any, i: number) => {
              const palette = ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444', '#06B6D4', '#F97316'];
              return palette[i % palette.length];
            }),
            barPercentage: 0.6,
            categoryPercentage: 0.6,
            maxBarThickness: 80
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: { legend: { display: false } },
          scales: {
            x: {
              ticks: {
                maxRotation: 45,
                minRotation: 25,
                autoSkip: false
              },
              grid: { display: false }
            },
            y: { beginAtZero: true, max: 100 }
          }
        }
      });
      try { this.cdr.detectChanges(); } catch (e) {}
    }, error: (err) => { this.loadingChart = false; try { this.cdr.detectChanges(); } catch (e) {} console.error('Failed to load attendance ratio by class (gv)', err); } });
  }
}
