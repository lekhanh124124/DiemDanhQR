import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-teacher-schedule',
  templateUrl: './teacher-schedule.component.html',
  styleUrls: ['./teacher-schedule.component.scss']
})
export class TeacherScheduleComponent implements OnInit {
 selectedDate: Date = new Date();
  currentWeek: { date: Date; key: string; label: string }[] = [];
  caHoc = ['Sáng', 'Chiều', 'Tối'];
  schedules: any[] = [];
  isLoading = false;

  constructor(
    private scheduleService: ScheduleService,
    private auth: AuthService,
    private router: Router
    , private message: NzMessageService
  ) {}

  ngOnInit(): void {
    this.generateWeek();
    this.loadSchedule();
  }

  private parseBooleanStatus(v: any): boolean {
    if (v === undefined || v === null) return true;
    if (typeof v === 'boolean') return v;
    const s = String(v).trim().toLowerCase();
    if (s === 'true' || s === '1' || s === 'yes') return true;
    if (s === 'false' || s === '0' || s === 'no') return false;
    return s.length > 0;
  }

  isDisabled(buoi: any): boolean {
    try {
      const candidates = [
        buoi?.lopHocPhan?.trangThai,
        buoi?.monHoc?.trangThai,
        buoi?.buoiHoc?.trangThai,
        buoi?.phongHoc?.trangThai,
        buoi?.trangThai,
        buoi?.TrangThai
      ];

      for (const c of candidates) {
        if (c === undefined || c === null) continue;
        const parsed = this.parseBooleanStatus(c);
        if (parsed === false) return true;
      }

      // no explicit false found -> enabled
      return false;
    } catch (e) {
      return false;
    }
  }

  onDateChange(): void {
    this.generateWeek();
    this.loadSchedule();
  }

  // Chuyển Date sang yyyy-MM-dd để so sánh
  private toKey(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  // Tạo danh sách ngày trong tuần (T2 → CN)
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

  // Gọi API lấy lịch học
  loadSchedule(): void {
    this.isLoading = true;
    const user = this.auth.getUser();
    const rawCandidates = [
      user?.username,
      user?.tenDangNhap,
      user?.maGiangVien,
      user?.MaGiangVien,
      user?.nguoiDung?.maGiangVien,
      user?.maNguoiDung,
      user?.maNguoiDung,
      user?.nguoiDung?.maNguoiDung
    ];

    const seen = new Set<string>();
    const candidates: string[] = [];
    for (const v of rawCandidates) {
      if (v === undefined || v === null) continue;
      const s = String(v).trim();
      if (!s) continue;
      if (!seen.has(s)) {
        seen.add(s);
        candidates.push(s);
      }
    }

    try { console.debug('[DEBUG] TeacherSchedule user=', user, 'candidates=', candidates); } catch (e) {}

    const baseParams: any = { Page: 1, PageSize: 200 };

    const normalizeAndMap = (itemsRaw: any): any[] => {
      const items = Array.isArray(itemsRaw) ? itemsRaw : (itemsRaw?.data?.items || itemsRaw?.data || itemsRaw?.items || itemsRaw?.Items || []);
      const mapped = (items || []).map((it: any) => {
        const flat: any = {
          maBuoi: it?.buoiHoc?.maBuoi || it?.maBuoi || it?.MaBuoi,
          ngayHoc: it?.buoiHoc?.ngayHoc || it?.NgayHoc || it?.ngayHoc,
          tietBatDau: it?.buoiHoc?.tietBatDau || it?.tietBatDau || it?.TietBatDau,
          soTiet: it?.buoiHoc?.soTiet || it?.soTiet || it?.SoTiet,
          ghiChu: it?.buoiHoc?.ghiChu || it?.ghiChu || it?.GhiChu,
          trangThai: it?.buoiHoc?.trangThai ?? it?.trangThai ?? it?.lopHocPhan?.trangThai ?? it?.lopHocPhan?.TrangThai,
          maPhong: it?.phongHoc?.maPhong || it?.maPhong || it?.MaPhong,
          tenPhong: it?.phongHoc?.tenPhong || it?.tenPhong,
          maLopHocPhan: it?.lopHocPhan?.maLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan || it?.lopHocPhan,
          tenLopHocPhan: it?.lopHocPhan?.tenLopHocPhan || it?.tenLopHocPhan || it?.tenLop || it?.lopHocPhan?.tenLopHocPhan,
          maMonHoc: it?.monHoc?.maMonHoc || it?.maMonHoc,
          tenMonHoc: it?.monHoc?.tenMonHoc || it?.tenMonHoc,
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

        let subjectClass: string | null = null;
        try {
          const loai = it?.monHoc?.loaiMon ?? it?.loaiMon ?? it?.buoiHoc?.loaiMon ?? it?.LoaiMon ?? flat?.loaiMon ?? null;
          if (loai !== null && loai !== undefined) {
            const n = Number(String(loai).trim());
            if (!isNaN(n)) subjectClass = (n === 2) ? 'th' : (n === 3 ? 'lth' : 'lt');
          }
        } catch (e) { subjectClass = null; }

        return { ...it, ...flat, ngayKey, subjectClass };
      });

      try { if (mapped && mapped.length) console.debug('[DEBUG] TeacherSchedule first mapped item=', mapped[0]); } catch (e) {}
      return mapped;
    };

    const tryIndex = (idx: number) => {
      const candidate = candidates[idx];
      const params = { ...baseParams } as any;
      if (candidate) params.MaGiangVien = candidate;
      else {
      }

      try { console.debug('[DEBUG] TeacherSchedule calling getSchedules with params=', params); } catch (e) {}

      this.scheduleService.getSchedules(params).subscribe({
        next: (res) => {
          const rawItems = Array.isArray(res) ? res : (res?.data?.items || res?.data || res?.items || res?.Items || res);
          const mapped = normalizeAndMap(rawItems);
          try { console.debug('[DEBUG] TeacherSchedule result count=', (mapped || []).length, 'for candidate=', candidate); } catch (e) {}
          if ((mapped || []).length) {
            this.schedules = mapped;
            this.isLoading = false;
            return;
          }
          if (idx + 1 < candidates.length) {
            tryIndex(idx + 1);
          } else {
            this.schedules = [];
            this.isLoading = false;
          }
        },
        error: (err) => {
          console.error('Load schedules error', err);
          if (idx + 1 < candidates.length) tryIndex(idx + 1);
          else { this.schedules = []; this.isLoading = false; const msg = err?.error?.message || err?.message || `Không thể tải thời khóa biểu (lỗi ${err?.status || ''})`; this.message.error(msg); }
        }
      });
    };
    tryIndex(0);
  }

  // Xác định ca học dựa theo tiết bắt đầu
  getCa(tietBatDau: number): string {
    if (tietBatDau >= 1 && tietBatDau <= 6) return 'Sáng';
    if (tietBatDau >= 7 && tietBatDau <= 12) return 'Chiều';
    return 'Tối';
  }

  // Trả về khoảng tiết hiển thị "start - end"
  getTietRange(buoi: any): string {
    if (!buoi) return '';
    const start = Number(buoi.tietBatDau ?? buoi.TietBatDau ?? buoi?.buoiHoc?.tietBatDau ?? 0) || 0;
    const len = Number(buoi.soTiet ?? buoi.SoTiet ?? buoi?.buoiHoc?.soTiet ?? 1) || 1;
    const end = start > 0 ? (start + Math.max(1, len) - 1) : start;
    if (start <= 0) return '';
    return `${start} - ${end}`;
  }

  private tietStartTimes: string[] = [
    // '07:00','07:45','08:30','09:15','10:00',
    // '13:00','13:50','14:40','15:30','16:20',
    // '18:00','18:50'
  ];

  getCurrentTiet(): number | null {
    const now = new Date();
    const hhmm = `${String(now.getHours()).padStart(2,'0')}:${String(now.getMinutes()).padStart(2,'0')}`;
    for (let i = 0; i < this.tietStartTimes.length; i++) {
      const start = this.tietStartTimes[i];
      const [sh, sm] = start.split(':').map(Number);
      const startMs = new Date(now.getFullYear(), now.getMonth(), now.getDate(), sh, sm).getTime();
      const endMs = startMs + (45 * 60 * 1000); // 45 phút mỗi tiết
      const nowMs = now.getTime();
      if (nowMs >= startMs && nowMs < endMs) {
        return i + 1;
      }
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

    const type = String(buoi.loaiLich || buoi.loaiBuoi || '').toUpperCase();
    const hasLT = type.includes('LT');
    const hasTH = type.includes('TH');
    if (hasLT && hasTH) return { 'background-image': 'linear-gradient(135deg,#e0e0e0 0%, #8bc34a 100%)', color: '#222' };
    if (hasTH) return { 'background-color': '#8bc34a', color: 'white' };
    return { 'background-color': '#e0e0e0' };
  }

  // Tuần kế tiếp
  nextWeek(): void {
    this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() + 7));
    this.generateWeek();
    this.loadSchedule();
  }

  // Tuần trước
  prevWeek(): void {
    this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() - 7));
    this.generateWeek();
    this.loadSchedule();
  }

  /**  Trở về tuần hiện tại */
  goToToday(): void {
    this.selectedDate = new Date();
    this.generateWeek();
    this.loadSchedule();
  }

  // openQr(buoi: any): void {
  //   const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
  //   if (!maBuoi) return;
  //   this.router.navigate(['/giangvien/qr-create'], { queryParams: { maBuoi } });
  // }

  // openAttendance(buoi: any): void {
  //   const maLopHocPhan = buoi?.maLopHocPhan || buoi?.MaLopHocPhan;
  //   const ngay = buoi?.ngayHoc || '';
  //   this.router.navigate(['/giangvien/attendance-manage'], { queryParams: { classId: maLopHocPhan, date: ngay } });
  // }

  viewStudents(buoi: any): void {
    if (this.isDisabled(buoi)) {
      try { this.message.warning('Lớp này đã bị vô hiệu hóa'); } catch (e) {}
      return;
    }
    const maLop = buoi?.maLopHocPhan || buoi?.MaLopHocPhan || buoi?.maLop || buoi?.maLopHocPhanChiTiet || '';
    if (!maLop) return;
    this.router.navigate(['/giangvien/class-section-detail', maLop]);
  }
}
