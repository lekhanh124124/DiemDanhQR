import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ScheduleService } from '../../core/services/schedule.service';
import { CourseService } from '../../core/services/course.service';
import dayjs from 'dayjs';

@Component({
  selector: 'app-room-timetable',
  templateUrl: './room-timetable.component.html',
  styleUrls: ['./room-timetable.component.scss']
})
export class RoomTimetableComponent implements OnInit {
  maPhong: any;
  tenPhong: string = '';
  loading = false;
  selectedDate: Date = new Date();
  currentWeek: { date: Date; key: string; label: string }[] = [];
  caHoc = ['Sáng', 'Chiều', 'Tối'];
  schedules: any[] = [];
  private subjectsByCode: Record<string, any> = {};
  private subjectsByName: Record<string, any> = {};

  constructor(private route: ActivatedRoute, private router: Router, private schedule: ScheduleService, private courseService: CourseService) {}

  ngOnInit(): void {
    const qp: any = this.route.snapshot.queryParams;
    this.maPhong = qp?.MaPhong || qp?.maPhong;
    this.tenPhong = qp?.TenPhong || qp?.tenPhong || '';
    const start = qp?.startDate;
    if (start) this.selectedDate = new Date(start);
    this.generateWeek();
    this.courseService.getSubjects({ Page: 1, PageSize: 1000 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || res?.items || [];
      (items || []).forEach((s: any) => {
        const mh = s?.monHoc || s;
        if (!mh) return;
        const code = (mh?.maMonHoc || mh?.MaMonHoc || mh?.ma || '').toString().trim();
        const name = (mh?.tenMonHoc || mh?.TenMonHoc || mh?.ten || '').toString().toLowerCase().trim();
        if (code) this.subjectsByCode[code] = mh;
        if (name) this.subjectsByName[name] = mh;
      });
      this.loadSessionsForWeek();
    }, error: () => { this.loadSessionsForWeek(); } });
  }

  private toKey(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

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

  onDateChange(): void {
    this.generateWeek();
    this.loadSessionsForWeek();
  }

  prevWeek(): void { this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() - 7)); this.generateWeek(); this.loadSessionsForWeek(); }
  nextWeek(): void { this.selectedDate = new Date(this.selectedDate.setDate(this.selectedDate.getDate() + 7)); this.generateWeek(); this.loadSessionsForWeek(); }
  goToday(): void { this.selectedDate = new Date(); this.generateWeek(); this.loadSessionsForWeek(); }

  private loadSessionsForWeek(): void {
    this.loading = true;
    const params: any = { Page: 1, PageSize: 1000 };
    if (this.maPhong) params.MaPhong = this.maPhong;
    this.schedule.getSchedules(params).subscribe({ next: (res: any) => {
      const raw = Array.isArray(res) ? res : (res?.data?.items || res?.data || res?.items || res || []);
      const items = Array.isArray(raw) ? raw : (raw.items || raw);

      const mapped = (items || []).map((it: any) => {
        const buoi = it?.buoiHoc || it?.BuoiHoc || it;
        const phong = it?.phongHoc || it?.PhongHoc || it;
        const lop = it?.lopHocPhan || it?.LopHocPhan || it;
        const mon = it?.monHoc || it?.MonHoc || it;
        const giang = it?.giangVien || it?.giangVienInfo || it?.nguoiDung || {};

        const flat: any = {
          maBuoi: buoi?.maBuoi || buoi?.MaBuoi || it?.maBuoi || it?.MaBuoi,
          ngayHoc: buoi?.ngayHoc || buoi?.NgayHoc || it?.ngayHoc || it?.NgayHoc,
          tietBatDau: buoi?.tietBatDau || buoi?.TietBatDau || it?.tietBatDau || it?.TietBatDau,
          soTiet: buoi?.soTiet || buoi?.SoTiet || it?.soTiet || it?.SoTiet,
          ghiChu: buoi?.ghiChu || buoi?.GhiChu || it?.ghiChu || it?.GhiChu,
          trangThai: buoi?.trangThai ?? buoi?.TrangThai ?? it?.trangThai ?? it?.TrangThai,
          maPhong: phong?.maPhong || phong?.MaPhong || it?.maPhong || it?.MaPhong,
          tenPhong: phong?.tenPhong || phong?.TenPhong || it?.tenPhong || it?.TenPhong,
          maLopHocPhan: lop?.maLopHocPhan || lop?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan,
          tenLopHocPhan: lop?.tenLopHocPhan || lop?.TenLopHocPhan || it?.tenLopHocPhan || it?.TenLopHocPhan,
          maMonHoc: mon?.maMonHoc || mon?.MaMonHoc || it?.maMonHoc || it?.MaMonHoc,
          tenMonHoc: mon?.tenMonHoc || mon?.TenMonHoc || it?.tenMonHoc || it?.TenMonHoc,
          giangVien: giang?.hoTen || giang?.HoTen || giang?.tenNguoiDung || giang?.TenNguoiDung || giang?.fullName || giang?.name || ''
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

        const merged: any = { ...it, ...flat, ngayKey };
        const mh = mon || it?.monHoc || it?.MonHoc || null;
        let loai: any = null;

        const direct = it?.loaiMon ?? it?.LoaiMon ?? merged?.loaiMon ?? merged?.LoaiMon ?? buoi?.loaiMon ?? buoi?.LoaiMon;
        if (direct !== undefined && direct !== null && String(direct).trim() !== '') {
          loai = direct;
        }

        const code = (mh?.maMonHoc || mh?.MaMonHoc || mh?.ma || merged?.maMonHoc || '').toString().trim();
        if ((loai === null || loai === undefined || String(loai).trim() === '') && code && this.subjectsByCode[code]) {
          loai = this.subjectsByCode[code]?.loaiMon ?? this.subjectsByCode[code]?.LoaiMon;
        }

        if ((loai === null || loai === undefined || String(loai).trim() === '') ) {
          const name = (mh?.tenMonHoc || mh?.TenMonHoc || merged?.tenMonHoc || merged?.tenMonHoc || '').toString().toLowerCase().trim();
          if (name && this.subjectsByName[name]) loai = this.subjectsByName[name]?.loaiMon ?? this.subjectsByName[name]?.LoaiMon;
        }

        merged.subjectClass = '';
        if (loai !== null && loai !== undefined && String(loai).trim() !== '') {
          const n = String(loai).trim().toUpperCase();
          if (n === '1' || n === 'LT') merged.subjectClass = 'lt';
          else if (n === '2' || n === 'TH') merged.subjectClass = 'th';
          else if (n === '3' || n === 'LTH' || n === 'LT+TH' || n === '3') merged.subjectClass = 'lth';
        }

        return merged;
      });

      const weekKeys = new Set(this.currentWeek.map(w => w.key));
      this.schedules = (mapped || []).filter((m: any) => m.ngayKey && weekKeys.has(m.ngayKey));
      this.loading = false;
    }, error: () => { this.loading = false; } });
  }

  getCa(tietBatDau: number): string {
    if (tietBatDau >= 1 && tietBatDau <= 6) return 'Sáng';
    if (tietBatDau >= 7 && tietBatDau <= 12) return 'Chiều';
    return 'Tối';
  }

  getTietRange(buoi: any): string {
    if (!buoi) return '';
    const start = Number(buoi.tietBatDau ?? buoi.TietBatDau ?? buoi?.buoiHoc?.tietBatDau ?? 0) || 0;
    const len = Number(buoi.soTiet ?? buoi.SoTiet ?? buoi?.buoiHoc?.soTiet ?? 1) || 1;
    const end = start > 0 ? (start + Math.max(1, len) - 1) : start;
    if (start <= 0) return '';
    return `${start} - ${end}`;
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
      if (buoi && buoi.subjectClass) return { 'box-shadow': '0 0 0 3px rgba(255,213,79,0.18)' };
      return { 'background-color': '#ffd54f', 'box-shadow': '0 0 0 3px rgba(255,213,79,0.2)' };
    }

    if (buoi && buoi.subjectClass) return {};

    const type = String(buoi.loaiLich || buoi.loaiBuoi || '').toUpperCase();
    const hasLT = type.includes('LT');
    const hasTH = type.includes('TH');
    if (hasLT && hasTH) return { 'background-image': 'linear-gradient(135deg,#e0e0e0 0%, #8bc34a 100%)', color: '#222' };
    if (hasTH) return { 'background-color': '#8bc34a', color: 'white' };
    return { 'background-color': '#e0e0e0' };
  }
}
