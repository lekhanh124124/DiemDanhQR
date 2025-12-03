import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { StudentService } from '../../core/services/student.service';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-student-schedule',
  templateUrl: './student-schedule.component.html',
  styleUrls: ['./student-schedule.component.scss']
})
export class StudentScheduleComponent implements OnInit {
  selectedDate: Date = new Date();
  currentWeek: { date: Date; key: string; label: string }[] = [];
  caHoc = ['Sáng', 'Chiều', 'Tối'];
  schedules: any[] = [];
  isLoading = false;
  @ViewChild('studentListTpl', { static: true }) studentListTpl!: TemplateRef<any>;
  modalStudents: Array<{ maSinhVien: string; hoTen: string }> = [];
  modalLoading = false;
  private modalRef: any = null;
  modalClassId: string = '';

  constructor(
    private scheduleService: ScheduleService,
    private auth: AuthService,
    private studentService: StudentService,
    private modal: NzModalService
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
      return false;
    } catch (e) { return false; }
  }

  onDateChange(): void {
    this.generateWeek();
    this.loadSchedule();
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

  // Gọi API lấy lịch học
  loadSchedule(): void {
    this.isLoading = true;
    const user = this.auth.getUser();
    const candidates = [
      user?.maSinhVien,
      user?.MaSinhVien,
      user?.nguoiDung?.maSinhVien,
      user?.nguoiDung?.MaSinhVien,
      user?.tenDangNhap,
      user?.TenDangNhap,
      user?.username,
      user?.maNguoiDung,
      user?.maNguoiDung
    ].filter((v: any) => v !== undefined && v !== null && String(v).trim() !== '').map(v => String(v));

    try { console.debug('[DEBUG] StudentSchedule user=', user, 'candidates=', candidates); } catch (e) {}

    const baseParams: any = { Page: 1, PageSize: 200 };

    const normalize = (raw: any): any[] => {
      const items = Array.isArray(raw) ? raw : (raw?.data?.items || raw?.data || raw?.items || raw || []);
      return (items || []).map((it: any) => {
        const flat: any = {
          maBuoi: it?.buoiHoc?.maBuoi || it?.maBuoi || it?.MaBuoi,
          ngayHoc: it?.buoiHoc?.ngayHoc || it?.NgayHoc || it?.ngayHoc,
          tietBatDau: it?.buoiHoc?.tietBatDau || it?.tietBatDau,
          soTiet: it?.buoiHoc?.soTiet || it?.soTiet,
          tenPhong: it?.phongHoc?.tenPhong || it?.tenPhong,
          maLopHocPhan: it?.lopHocPhan?.maLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan,
          tenMonHoc: it?.monHoc?.tenMonHoc || it?.tenMonHoc || it?.tenMonHoc || it?.monHoc?.tenMonHoc,
          tenGiangVien: it?.giangVienInfo?.hoTen || it?.giangVien?.hoTen || it?.tenGiangVien || it?.giangVien?.tenGiangVien
        };
        flat.trangThai = it?.buoiHoc?.trangThai ?? it?.trangThai ?? it?.lopHocPhan?.trangThai ?? it?.lopHocPhan?.TrangThai;
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
    };

    const tryIndex = (idx: number) => {
      const candidate = candidates[idx];
      const params: any = { ...baseParams };
      if (candidate) params.MaSinhVien = candidate;

      try { console.debug('[DEBUG] StudentSchedule calling getSchedules with params=', params); } catch (e) {}

      this.scheduleService.getSchedules(params).subscribe({
        next: (res) => {
          const mapped = normalize(res);
          try { console.debug('[DEBUG] StudentSchedule result count=', (mapped || []).length, 'for candidate=', candidate); } catch (e) {}
          if ((mapped || []).length) {
            this.schedules = mapped;
            this.isLoading = false;
            return;
          }
          if (idx + 1 < candidates.length) tryIndex(idx + 1);
          else { this.schedules = []; this.isLoading = false; }
        },
        error: (err) => {
          console.error('Load schedules error', err);
          if (idx + 1 < candidates.length) tryIndex(idx + 1);
          else { this.schedules = []; this.isLoading = false; }
        }
      });
    };

    if (candidates.length) tryIndex(0); else tryIndex(0);
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

  showClassStudents(buoi: any): void {
    if (!buoi) return;
    if (this.isDisabled(buoi)) {
      this.modal.error({ nzTitle: 'Lớp đã bị vô hiệu hóa', nzContent: 'Lớp học phần này đã bị vô hiệu hóa và không thể xem danh sách.', nzOkText: 'Đóng' });
      return;
    }
    const maLop = buoi?.maLopHocPhan || buoi?.MaLopHocPhan || buoi?.lopHocPhan?.maLopHocPhan || buoi?.lopHocPhan?.MaLopHocPhan || null;
    const ten = buoi?.tenMonHoc || buoi?.TenMonHoc || buoi?.lopHocPhan?.tenLopHocPhan || buoi?.tenLopHocPhan || maLop;
    if (!maLop) {
      this.modal.error({ nzTitle: 'Không xác định lớp', nzContent: 'Không tìm thấy mã lớp học phần.', nzOkText: 'Xác nhận' });
      return;
    }

    this.modalLoading = true;
    this.modalStudents = [];
    this.modalClassId = maLop;
    this.modalRef = this.modal.create({ nzTitle: `Danh sách lớp - ${ten}`, nzContent: this.studentListTpl, nzWidth: 500, nzFooter: null });

    this.studentService.getStudents({ MaLopHocPhan: maLop, Page: 1, PageSize: 1000 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.modalStudents = (items || []).map((s: any) => {
        const nguoi = s?.nguoiDung || s?.NguoiDung || s;
        const sv = s?.sinhVien || s?.SinhVien || s;
        return {
          maSinhVien: sv?.maSinhVien || sv?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || (nguoi?.tenDangNhap || nguoi?.TenDangNhap) || '',
          hoTen: nguoi?.hoTen || nguoi?.HoTen || s?.hoTen || s?.HoTen || ''
        };
      });
      this.modalLoading = false;
    }, error: (err: any) => {
      this.modalLoading = false;
      this.modalStudents = [];
      this.modal.error({ nzTitle: 'Lỗi tải danh sách', nzContent: err?.message || 'Không thể tải danh sách sinh viên.', nzOkText: 'Xác nhận' });
      if (this.modalRef) this.modalRef.close();
    } });
  }

  closeStudentModal(): void {
    if (this.modalRef) this.modalRef.close();
    this.modalRef = null;
    this.modalClassId = '';
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

  // Trở về tuần hiện tại
  goToToday(): void {
    this.selectedDate = new Date();
    this.generateWeek();
    this.loadSchedule();
  }
}
