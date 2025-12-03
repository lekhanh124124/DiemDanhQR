import { Component, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { AttendanceService } from '../../core/services/attendance.service';
import { CourseService } from '../../core/services/course.service';
import { StudentService } from '../../core/services/student.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzMessageService } from 'ng-zorro-antd/message';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-qr-create',
  templateUrl: './qr-create.component.html',
  styleUrls: ['./qr-create.component.scss']
})
export class QrCreateComponent implements OnDestroy {
  @ViewChild('qrHost') qrHost?: ElementRef<HTMLDivElement>;
  selectedClass: string = '';
  selectedDate: string = '';
  tietBatDau: number | null = null;
  soTiet: number | null = null;
  qrData: string = '';
  expiresAt?: string;
  qrImageDataUrl?: string;
  countdownSeconds: number = 0;
  countdownDisplay: string = '';
  private countdownTimer: any;

  classes: Array<{ maLopHocPhan: string; tenLopHocPhan: string }> = [];
  currentMaBuoi?: number;

  classRoster: Array<any> = [];

  getBadgeLabel(item: any): string {
    if (!item) return '';
    if (item.thoiGianQuet && String(item.thoiGianQuet).trim() !== '') {
      const code = String(item.trangThai || '').toUpperCase();
      if (/LATE|ĐI TRỄ|DI TRE/.test(code)) return 'ĐI TRỄ';
      if (/FRAUD|FRAUD_LOCATION|WRONG_GPS|GPS|SAI VỊ TRÍ|SAI VI TRI/.test(code)) return 'ĐIỂM DANH SAI VỊ TRÍ GPS';
      return 'Có mặt';
    }
    const st = (item.trangThai || '').toString().toLowerCase();
    if (!st || st.includes('chưa') || st.includes('chua')) return 'Chưa điểm danh';
    if (st.includes('vắng') || st.includes('vang')) return 'Vắng';
    if (st.trim() !== '') return 'Có mặt';
    return 'Chưa điểm danh';
  }

  getBadgeClass(item: any): string {
    const label = this.getBadgeLabel(item);
    if (label === 'Có mặt') return 'inline-block bg-green-100 text-green-800 px-2 py-0.5 rounded text-sm';
    if (label === 'Vắng') return 'inline-block bg-red-100 text-red-800 px-2 py-0.5 rounded text-sm';
    if (label === 'ĐI TRỄ') return 'inline-block bg-orange-100 text-orange-800 px-2 py-0.5 rounded text-sm';
    if (label === 'ĐIỂM DANH SAI VỊ TRÍ GPS') return 'inline-block bg-yellow-100 text-yellow-800 px-2 py-0.5 rounded text-sm';
    return 'inline-block bg-gray-100 text-gray-800 px-2 py-0.5 rounded text-sm';
  }

  refreshAttendance(): void {
    if (this.attendancePollTimer) {
      clearInterval(this.attendancePollTimer);
      this.attendancePollTimer = undefined;
    }
    this.fetchAttendanceOnce();
    this.attendancePollTimer = setInterval(() => this.fetchAttendanceOnce(), 5000);
  }

  attendanceList: Array<any> = [];
  private attendancePollTimer: any;
  private attendancePollMs = 2000;
  private handleVisibilityChangeBound: any;
  private handleBeforeUnloadBound: any;

  constructor(
    private attendance: AttendanceService,
    private courseService: CourseService,
    private studentService: StudentService,
    private route: ActivatedRoute,
    private scheduleService: ScheduleService,
    private router: Router,
    private modal: NzModalService,
    private msg: NzMessageService,
    private auth: AuthService
  ) {
    this.loadClasses();
    this.route.queryParams.subscribe(p => {
      if (p && p['classId']) this.selectedClass = p['classId'];
      if (p && p['date']) this.selectedDate = p['date'];
      if (p && p['start']) this.tietBatDau = Number(p['start']) || null;
      if (p && p['length']) this.soTiet = Number(p['length']) || null;
      if (p && p['maBuoi']) {
        const maBuoi = Number(p['maBuoi']);
        if (!Number.isNaN(maBuoi)) {
          this.createQrForBuoi(maBuoi, 900, 6);
        }
      }
    });
    try { this.loadSessionFromLocal(); } catch (e) { }
  }

  processingFinalize = false;

  private async resolveClassByBuoi(maBuoi: number): Promise<string | null> {
    try {
      const res: any = await lastValueFrom(this.scheduleService.getSchedules({ Page: 1, PageSize: 1, MaBuoi: Number(maBuoi) }));
      const items = res?.data?.items || res?.data || [];
      const buoi = items[0] || {};
      const lop = buoi?.lopHocPhan || buoi?.LopHocPhan || buoi || {};
      const ma = lop?.maLopHocPhan || lop?.MaLopHocPhan || buoi?.maLopHocPhan || buoi?.MaLopHocPhan;
      return ma ? String(ma).trim() : null;
    } catch {
      return null;
    }
  }

  private async getClassIdFromRecords(maBuoi: number): Promise<string | null> {
    try {
      const recRes: any = await lastValueFrom(this.attendance.getRecords({ Page: 1, PageSize: 50, MaBuoi: Number(maBuoi) }));
      const recs = recRes?.data?.items || recRes?.data || [];
      for (const it of recs) {
        const lop = it?.lopHocPhan || it?.LopHocPhan || {};
        const ma = lop?.maLopHocPhan || lop?.MaLopHocPhan;
        if (ma) return String(ma).trim();
      }
      return null;
    } catch {
      return null;
    }
  }

  private async finalizeAndCreateMissingThenNavigate(maBuoi: number) {
    if (!maBuoi) return;
    this.processingFinalize = true;
    const loading = this.msg.loading('Đang tổng hợp kết quả...', { nzDuration: 0 });
    try {
      let classIdToUse =
        (this.selectedClass && this.selectedClass.trim()) || null;

      if (!classIdToUse) {
        classIdToUse = await this.getClassIdFromRecords(maBuoi);
      }
      if (!classIdToUse) {
        classIdToUse = await this.resolveClassByBuoi(maBuoi);
      }

      // persist and set selectedClass
      if (classIdToUse) {
        this.selectedClass = classIdToUse;
        try { this.saveSessionToLocal(); } catch {}
      } else {
        this.msg.remove();
        this.msg.warning('Không xác định được lớp học phần của buổi này');
        this.router.navigate(
          ['/giangvien/attendance-manage'],
          { queryParams: { maBuoi, date: this.selectedDate } }
        );
        return;
      }

      // 1. Lấy đúng danh sách lớp theo classIdToUse
      const rosterRes: any = await lastValueFrom(
        this.studentService.getStudents({ MaLopHocPhan: classIdToUse, Page: 1, PageSize: 2000 })
      );
      const roster = rosterRes?.data?.items || rosterRes?.data || [];

      // 2. Lấy danh sách điểm danh hiện có của buổi
      const recRes: any = await lastValueFrom(
        this.attendance.getRecords({ Page: 1, PageSize: 2000, MaBuoi: Number(maBuoi) })
      );
      const recs = recRes?.data?.items || recRes?.data || [];

      const existingIds = new Set(
        (recs || []).map((it: any) => {
          const sv = it?.sinhVien || it?.SinhVien || {};
          return String(
            sv?.maSinhVien || sv?.MaSinhVien || it?.maSinhVien || it?.MaSinhVien || ''
          ).trim();
        }).filter((id: string) => !!id)
      );

      // 3. Tìm sinh viên chưa có bản ghi điểm danh
      const rosterIds = (roster || [])
        .map((s: any) => {
          const sv = s?.sinhVien || s?.SinhVien || {};
          const nd = s?.nguoiDung || s?.NguoiDung || {};
          const id = String(
            sv?.maSinhVien || sv?.MaSinhVien || nd?.maSinhVien || nd?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || ''
          ).trim();
          return id || null;
        })
        .filter((id: string | null): id is string => !!id);

      const missing = rosterIds.filter((id: string) => !existingIds.has(id));

      // 4. Tạo bản ghi điểm danh mặc định (MaTrangThai=8, TrangThai=false) cho các sv thiếu
      let created = 0, failed = 0;
      for (const id of missing) {
        try {
          const payload: any = { MaBuoi: Number(maBuoi), MaSinhVien: id, MaTrangThai: 8, TrangThai: false };
          await lastValueFrom(this.attendance.createRecord(payload));
          created++;
        } catch (e) {
          failed++;
        }
      }

      // 5. Sau khi tạo xong điều hướng sang trang quản lý điểm danh để hiển thị đầy đủ
      this.msg.remove();
      this.msg.success(`Kết thúc điểm danh`);
      this.router.navigate(
        ['/giangvien/attendance-manage'],
        { queryParams: { maBuoi, classId: classIdToUse, date: this.selectedDate } }
      );
    } catch (e) {
      try { this.msg.remove(); } catch {}
      this.msg.error('Kết thúc điểm danh thất bại');
    } finally {
      this.processingFinalize = false;
    }
  }

  loadClasses(): void {
    this.classes = [];
    // vẫn giữ khôi phục session & listeners
    try { document.addEventListener('visibilitychange', this.handleVisibilityChangeBound = this.handleVisibilityChange.bind(this)); } catch {}
    try { window.addEventListener('beforeunload', this.handleBeforeUnloadBound = this.handleBeforeUnload.bind(this)); } catch {}
  }

  private loadClassRoster(maLopHocPhan: string): void {
    if (!maLopHocPhan) { this.classRoster = []; return; }
    try {
      this.studentService.getStudents({ MaLopHocPhan: maLopHocPhan, Page: 1, PageSize: 2000 }).subscribe({ next: (res: any) => {
        const items = res?.data?.items || res?.data || [];
        this.classRoster = (items || []).map((s: any) => {
          const sv = s?.sinhVien || s?.SinhVien || {};
          const nd = s?.nguoiDung || s?.NguoiDung || {};
          const id = String(sv?.maSinhVien || sv?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || nd?.maSinhVien || nd?.MaSinhVien || '').trim();
          const name = (nd?.hoTen || nd?.HoTen || sv?.hoTen || sv?.HoTen || s?.hoTen || s?.HoTen || '').trim();
          return { maSinhVien: id, hoTen: name };
        }).filter((r: any) => r.maSinhVien);
        try { this.fillNamesFromRoster(); } catch (e) {}
      }, error: () => { this.classRoster = []; } });
    } catch (e) { this.classRoster = []; }
  }

  private fillNamesFromRoster(): void {
    try {
      if (!this.classRoster || !this.classRoster.length) return;
      const map = new Map<string,string>();
      (this.classRoster || []).forEach((s: any) => { if (s && s.maSinhVien) map.set(String(s.maSinhVien).trim(), (s.hoTen || '').trim()); });
      this.attendanceList = (this.attendanceList || []).map((r: any) => {
        try {
          const id = String(r.maSinhVien || '').trim();
          const rosterName = map.get(id);
          const currentName = (r.tenSinhVien || '').toString().trim();
          return { ...r, tenSinhVien: (rosterName && rosterName.length) ? rosterName : currentName };
        } catch (e) { return r; }
      });
    } catch (e) { }
  }

  private startAttendancePolling(): void {
    // Khôi phục polling: gọi ngay lần đầu rồi lặp lại mỗi 5s
    this.stopAttendancePolling();
    if (!this.currentMaBuoi) return;
    this.fetchAttendanceOnce();
    this.attendancePollTimer = setInterval(() => this.fetchAttendanceOnce(), 5000);
  }

  private fetchAttendanceOnce(): void {
    if (!this.currentMaBuoi) return;
    this.attendance.getRecords({ Page: 1, PageSize: 500, MaBuoi: Number(this.currentMaBuoi) }).subscribe({
      next: (res) => {
        const raw = res?.data?.items || res?.data || [];
        this.attendanceList = (raw || []).map((it: any) => {
          const dd = it.diemDanh || it.DiemDanh || it || {};
          const sv = it.sinhVien || it.SinhVien || {};
          const nd = sv.nguoiDung || sv.NguoiDung || it.nguoiDung || it.NguoiDung || {};
            const maSV = sv.maSinhVien || sv.MaSinhVien || it.maSinhVien || it.MaSinhVien || '';
            const tenSV = nd.hoTen || nd.HoTen || sv.hoTen || sv.HoTen || it.tenSinhVien || it.TenSinhVien || it.hoTen || it.HoTen || '';
            const tg = dd.thoiGianQuet || dd.ThoiGianQuet || dd.thoiGian || dd.ThoiGian || '';
            const stObj = it.trangThaiDiemDanh || it.trangThai || {};
            const code = stObj.codeTrangThai || stObj.CodeTrangThai || dd.trangThai || dd.TrangThai || '';
            return {
              maSinhVien: String(maSV || '').trim(),
              tenSinhVien: String(tenSV || '').trim(),
              thoiGianQuet: tg,
              trangThai: code || ''
            };
        });
        try { this.fillNamesFromRoster(); } catch (e) {}
      },
      error: () => {}
    });
  }

  generateQR(): void {
    if (!this.selectedClass || !this.selectedDate || !this.tietBatDau) return;
    const params: any = {
      Page: 1,
      PageSize: 10,
      MaLopHocPhan: this.selectedClass,
      NgayHoc: this.selectedDate,
      TietBatDau: this.tietBatDau
    };
    this.scheduleService.getSchedules(params).subscribe({
      next: (res) => {
        const items = res?.data?.items || res?.data || [];
        const buoi = items[0];
        const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
        if (!maBuoi) return;
        this.createQrForBuoi(Number(maBuoi), 900, 6);
      },
      error: () => {
      }
    });
  }


  private async createQrForBuoi(maBuoi: number, ttlSeconds = 900, pixelsPerModule = 6): Promise<void> {
    this.clearCountdown();
    try {
      const raw = localStorage.getItem('qrSession');
      if (raw) {
        const obj = JSON.parse(raw);
        if (obj && Number(obj.maBuoi) === Number(maBuoi)) {
          const expiresAtStr = obj.expiresAt;
          if (expiresAtStr) {
            const parts = String(expiresAtStr).split(' ');
            if (parts.length >= 2) {
              const dateParts = parts[0].split('-');
              const timePart = parts[1];
              if (dateParts.length === 3) {
                const iso = `${dateParts[2]}-${dateParts[1].padStart(2,'0')}-${dateParts[0].padStart(2,'0')}T${timePart}`;
                const expires = new Date(iso);
                if (!Number.isNaN(expires.getTime()) && expires.getTime() > Date.now()) {
                  if (obj.pngBase64) this.qrImageDataUrl = String(obj.pngBase64);
                  if (obj.token) this.qrData = String(obj.token || '');
                  this.expiresAt = String(obj.expiresAt || '');
                  this.currentMaBuoi = Number(maBuoi);
                  if (!this.selectedClass) {
                    const clsFromRecords = await this.getClassIdFromRecords(maBuoi);
                    this.selectedClass = clsFromRecords || await this.resolveClassByBuoi(maBuoi) || '';
                  }
                  if (this.selectedClass) this.loadClassRoster(this.selectedClass);
                  if (this.expiresAt) this.startCountdownFromExpires(this.expiresAt);
                  this.startAttendancePolling();
                  try { this.msg.info('Khôi phục QR hiện có.'); } catch (e) {}
                  return;
                }
              }
            }
          }
        }
      }
    } catch (e) {
    }
    this.attendance.generateQrByBuoiJson({ maBuoi, ttlSeconds, pixelsPerModule }).subscribe({
      next: async (res) => {
        const data = res?.data || res;
        if (data?.pngBase64) {
          this.qrImageDataUrl = `data:image/png;base64,${data.pngBase64}`;
          this.qrData = data?.token || '';
          this.expiresAt = data?.expiresAt || '';
          this.currentMaBuoi = Number(maBuoi);
          if (!this.selectedClass) {
            const clsFromRecords = await this.getClassIdFromRecords(maBuoi);
            this.selectedClass = clsFromRecords || await this.resolveClassByBuoi(maBuoi) || '';
          }
          if (this.selectedClass) this.loadClassRoster(this.selectedClass);
          if (this.expiresAt) this.startCountdownFromExpires(this.expiresAt);
          this.startAttendancePolling();
          try { this.saveSessionToLocal(); } catch (e) {}
          return;
        }
        this.currentMaBuoi = Number(maBuoi);
        this.fallbackBlob(maBuoi, ttlSeconds, pixelsPerModule);
      },
      error: () => {
        this.fallbackBlob(maBuoi, ttlSeconds, pixelsPerModule);
      }
    });
  }

  private fallbackBlob(maBuoi: number, ttlSeconds = 300, pixelsPerModule = 6) {
    this.attendance.generateQrByBuoi({ maBuoi, ttlSeconds, pixelsPerModule }).subscribe(async (blob) => {
      this.startAttendancePolling();
      this.qrImageDataUrl = await this.blobToDataURL(blob);
      this.qrData = '';
      this.currentMaBuoi = Number(maBuoi);
      try {
        const expires = new Date(Date.now() + (Number(ttlSeconds) * 1000));
        const dd = String(expires.getDate()).padStart(2, '0');
        const mm = String(expires.getMonth() + 1).padStart(2, '0');
        const yyyy = String(expires.getFullYear());
        const hh = String(expires.getHours()).padStart(2, '0');
        const min = String(expires.getMinutes()).padStart(2, '0');
        const sec = String(expires.getSeconds()).padStart(2, '0');
        this.expiresAt = `${dd}-${mm}-${yyyy} ${hh}:${min}:${sec}`;
        this.startCountdownFromExpires(this.expiresAt);
      } catch (e) {
        this.expiresAt = undefined;
      }
      try { this.saveSessionToLocal(); } catch (e) {}
    }, () => {
      this.qrImageDataUrl = undefined;
      this.qrData = '';
      this.expiresAt = undefined;
      this.clearCountdown();
    });
  }

  private startCountdownFromExpires(expiresAtStr?: string) {
    this.clearCountdown();
    if (!expiresAtStr) return;
    const parts = String(expiresAtStr).split(' ');
    if (parts.length < 2) return;
    const dateParts = parts[0].split('-');
    const timePart = parts[1];
    if (dateParts.length !== 3) return;
    const iso = `${dateParts[2]}-${dateParts[1].padStart(2,'0')}-${dateParts[0].padStart(2,'0')}T${timePart}`;
    const expires = new Date(iso);
    if (Number.isNaN(expires.getTime())) return;
    const update = () => {
      const now = new Date();
      let sec = Math.max(0, Math.floor((expires.getTime() - now.getTime()) / 1000));
      this.countdownSeconds = sec;
      const mm = Math.floor(sec / 60).toString().padStart(2, '0');
      const ss = (sec % 60).toString().padStart(2, '0');
      this.countdownDisplay = `${mm}:${ss}`;
      if (sec <= 0) {
        this.attendanceList = [];
        this.clearCountdown();
      }
    };
    update();
    this.countdownTimer = setInterval(update, 1000);
  }

  private startCountdownFromRemainingSeconds(seconds: number) {
    this.clearCountdown();
    let sec = Math.max(0, Math.floor(Number(seconds) || 0));

    const ex = new Date(Date.now() + sec * 1000);
    const dd = String(ex.getDate()).padStart(2, '0');
    const mm = String(ex.getMonth() + 1).padStart(2, '0');
    const yyyy = String(ex.getFullYear());
    const hh = String(ex.getHours()).padStart(2, '0');
    const min = String(ex.getMinutes()).padStart(2, '0');
    const ss = String(ex.getSeconds()).padStart(2, '0');
    this.expiresAt = `${dd}-${mm}-${yyyy} ${hh}:${min}:${ss}`;

    const update = () => {
      if (sec <= 0) {
        this.attendanceList = [];
        this.clearCountdown();
        return;
      }
      sec = sec - 1;
      this.countdownSeconds = sec;
      const mmTxt = Math.floor(sec / 60).toString().padStart(2, '0');
      const ssTxt = (sec % 60).toString().padStart(2, '0');
      this.countdownDisplay = `${mmTxt}:${ssTxt}`;
      if (sec <= 0) {
        this.attendanceList = [];
        this.clearCountdown();
      }
    };
    this.countdownSeconds = sec;
    const mm0 = Math.floor(sec / 60).toString().padStart(2, '0');
    const ss0 = (sec % 60).toString().padStart(2, '0');
    this.countdownDisplay = `${mm0}:${ss0}`;
    this.countdownTimer = setInterval(update, 1000);
  }

  private handleVisibilityChange(): void {
    try {
      if (typeof document === 'undefined') return;
      if (document.hidden) {
        this.pauseCountdownAndPersist();
      } else {
        this.resumeCountdownFromPersistIfNeeded();
      }
    } catch (e) { }
  }

  private pauseCountdownAndPersist(): void {
    try {
      const raw = localStorage.getItem('qrSession');
      const obj: any = raw ? JSON.parse(raw) : {};
      obj.pngBase64 = obj.pngBase64 || this.qrImageDataUrl || null;
      obj.token = obj.token || this.qrData || null;
      obj.expiresAt = obj.expiresAt || this.expiresAt || null;
      obj.maBuoi = obj.maBuoi || this.currentMaBuoi || null;
      obj.classId = obj.classId || this.selectedClass || null;
      obj.remainingSeconds = Number(this.countdownSeconds) || 0;
      obj.savedAt = Date.now();
      obj.paused = true;
      localStorage.setItem('qrSession', JSON.stringify(obj));
    } catch (e) { }
    if (this.countdownTimer) { clearInterval(this.countdownTimer); this.countdownTimer = undefined; }
  }

  private resumeCountdownFromPersistIfNeeded(): void {
    try {
      const raw = localStorage.getItem('qrSession');
      if (!raw) return;
      const obj: any = JSON.parse(raw);
      if (!obj) return;
      // khôi phục dữ liệu QR
      if (obj.pngBase64) this.qrImageDataUrl = this.qrImageDataUrl || String(obj.pngBase64);
      if (obj.token) this.qrData = this.qrData || String(obj.token);
      if (obj.classId) this.selectedClass = this.selectedClass || String(obj.classId);
      if (!this.currentMaBuoi && obj.maBuoi) this.currentMaBuoi = Number(obj.maBuoi);
      if (this.selectedClass) this.loadClassRoster(this.selectedClass);

      // tính lại remainingSeconds dựa trên elapsed
      if (obj.paused) {
        const savedAt = Number(obj.savedAt) || Date.now();
        const savedRem = Math.max(0, Number(obj.remainingSeconds) || 0);
        const elapsedSec = Math.floor((Date.now() - savedAt) / 1000);
        const rem = Math.max(0, savedRem - elapsedSec);

        if (rem > 0) {
          const expiresMs = Date.now() + rem * 1000;
          const ex = new Date(expiresMs);
          const dd = String(ex.getDate()).padStart(2, '0');
          const mm = String(ex.getMonth() + 1).padStart(2, '0');
          const yyyy = String(ex.getFullYear());
          const hh = String(ex.getHours()).padStart(2, '0');
          const min = String(ex.getMinutes()).padStart(2, '0');
          const sec = String(ex.getSeconds()).padStart(2, '0');
          this.expiresAt = `${dd}-${mm}-${yyyy} ${hh}:${min}:${sec}`;
          // start countdown from the recomputed remaining time
          this.startCountdownFromRemainingSeconds(rem);
        } else if (obj.expiresAt) {
          // fallback: khởi động lại từ expiresAt nếu còn hạn
          this.startCountdownFromExpires(String(obj.expiresAt));
        } else {
          this.clearCountdown();
        }
        this.startAttendancePolling();
        try {
          obj.paused = false;
          obj.remainingSeconds = undefined;
          obj.savedAt = undefined;
          obj.expiresAt = this.expiresAt || obj.expiresAt || null;
          localStorage.setItem('qrSession', JSON.stringify(obj));
        } catch (e) {}
      } else {
        // nếu không ở trạng thái paused, nhưng có expiresAt thì đảm bảo countdown chạy
        if (obj.expiresAt && !this.countdownTimer) {
          this.startCountdownFromExpires(String(obj.expiresAt));
        }
        this.startAttendancePolling();
      }
    } catch (e) { }
  }

  private handleBeforeUnload(evt?: any): void {
    try { this.pauseCountdownAndPersist(); } catch (e) {}
  }

  private clearCountdown() {
    if (this.countdownTimer) { clearInterval(this.countdownTimer); this.countdownTimer = undefined; }
    this.countdownSeconds = 0;
    this.countdownDisplay = '';
  }

  // // Live attendance polling
  // private startAttendancePolling(): void {
  //   this.stopAttendancePolling();
  //   if (!this.currentMaBuoi) return;
  //   // fetch immediately then schedule
  //   this.fetchAttendanceOnce();
  //   this.attendancePollTimer = setInterval(() => this.fetchAttendanceOnce(), this.attendancePollMs);
  // }

  private stopAttendancePolling(): void {
    if (this.attendancePollTimer) { clearInterval(this.attendancePollTimer); this.attendancePollTimer = undefined; }
  }

  // private fetchAttendanceOnce(): void {

    // Bỏ gọi /api/attendance/records, giữ nguyên cấu trúc để UI không lỗi
    // Giữ nguyên danh sách hiện có (không cập nhật)
  // }

  endAttendance(): void {
    const maBuoi = this.currentMaBuoi;
    // Bỏ phần tính toán thống kê hiển thị, chỉ còn confirm kết thúc

    // const content = `
    //   <div style="line-height:1.6">
    //     <div><strong>Tổng SV:</strong> ${total}</div>
    //     <div><strong>Có mặt:</strong> ${present} — <strong>Vắng:</strong> ${absent} — <strong>Muộn:</strong> ${late}</div>
    //     <div style="margin-top:8px;color:#666;font-size:12px">Bạn có thể xem chi tiết và chỉnh sửa lý do/điểm danh bù trong trang Quản lý điểm danh.</div>
    //   </div>
    // `;

    this.modal.confirm({
      nzTitle: 'Điểm danh đã kết thúc',
      // nzContent: content,
      nzOkText: 'Xem chi tiết',
      nzCancelText: 'Đóng',
      nzOnOk: () => {
        if (maBuoi) {
          try { this.stopAttendancePolling(); } catch (e) {}
          try { this.clearCountdown(); } catch (e) {}
          try { this.clearSessionFromLocal(); } catch (e) {}
          try { sessionStorage.removeItem('creatingQr'); } catch (e) {}
          this.finalizeAndCreateMissingThenNavigate(maBuoi);
        } else {
          this.msg.info('Không tìm thấy buổi học để xem kết quả');
        }
      },
      nzOnCancel: () => {
        try { this.stopAttendancePolling(); } catch (e) {}
        this.qrImageDataUrl = undefined;
        this.qrData = '';
        this.expiresAt = undefined;
        try { this.clearCountdown(); } catch (e) {}
        try { this.clearSessionFromLocal(); } catch (e) {}
        try { sessionStorage.removeItem('creatingQr'); } catch (e) {}
      }
    });
  }

  private saveSessionToLocal(): void {
    try {
      const obj: any = {
        pngBase64: this.qrImageDataUrl || null,
        token: this.qrData || null,
        expiresAt: this.expiresAt || null,
        maBuoi: this.currentMaBuoi || null,
        classId: this.selectedClass || null
      };
      localStorage.setItem('qrSession', JSON.stringify(obj));
    } catch (e) { }
  }

  private loadSessionFromLocal(): void {
    try {
      const raw = localStorage.getItem('qrSession');
      if (!raw) return;
      const obj = JSON.parse(raw);
      if (!obj) return;

      const savedMaBuoi = obj.maBuoi != null ? Number(obj.maBuoi) : undefined;
      const routeMaBuoi = Number(this.route.snapshot.queryParamMap.get('maBuoi') || NaN);
      const hasRouteMaBuoi = !Number.isNaN(routeMaBuoi);

      const buoiMatches =
        (this.currentMaBuoi && savedMaBuoi && Number(this.currentMaBuoi) === Number(savedMaBuoi)) ||
        (!this.currentMaBuoi && savedMaBuoi && (!hasRouteMaBuoi || Number(routeMaBuoi) === Number(savedMaBuoi)));

      if (obj.pngBase64) this.qrImageDataUrl = String(obj.pngBase64);
      if (obj.token) this.qrData = String(obj.token || '');
      if (obj.expiresAt) this.expiresAt = String(obj.expiresAt || '');

      if (buoiMatches) {
        if (savedMaBuoi) this.currentMaBuoi = Number(savedMaBuoi);
        if (obj.classId) this.selectedClass = String(obj.classId || '');
      } else {
        this.selectedClass = this.selectedClass || '';
      }

      if (buoiMatches && this.selectedClass) this.loadClassRoster(this.selectedClass);

      if (obj.paused && obj.remainingSeconds != null && buoiMatches) {
        try { this.startCountdownFromRemainingSeconds(Number(obj.remainingSeconds)); } catch (e) {}
      } else {
        if (this.expiresAt && buoiMatches) this.startCountdownFromExpires(this.expiresAt);
      }
      if (this.currentMaBuoi && buoiMatches) this.startAttendancePolling();
    } catch (e) { }
  }

  private clearSessionFromLocal(): void {
    try { localStorage.removeItem('qrSession'); } catch (e) { }
  }

  enterFullscreen(): void {
    try {
      const el = this.qrHost?.nativeElement as HTMLElement | undefined;
      if (!el) return;
      if (el.requestFullscreen) el.requestFullscreen();
      else if ((el as any).webkitRequestFullscreen) (el as any).webkitRequestFullscreen();
    } catch { }
  }

  extractBase64(): void {
    try {
      const host = this.qrHost?.nativeElement;
      if (!host) return;
      const canvas = host.querySelector('canvas') as HTMLCanvasElement | null;
      const img = host.querySelector('img') as HTMLImageElement | null;
      if (canvas) {
        this.qrImageDataUrl = canvas.toDataURL('image/png');
      } else if (img && img.src && img.src.startsWith('data:image')) {
        this.qrImageDataUrl = img.src;
      }
    } catch {
      this.qrImageDataUrl = undefined;
    }
  }

  downloadQrPng(): void {
    if (!this.qrImageDataUrl) return;
    const a = document.createElement('a');
    a.href = this.qrImageDataUrl;
    a.download = `qr-${this.selectedClass}-${this.selectedDate}.png`;
    a.click();
  }

  private blobToDataURL(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result));
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  ngOnDestroy(): void {
    try { this.pauseCountdownAndPersist(); } catch (e) {}
    this.clearCountdown();
    this.stopAttendancePolling();
    try { if (this.handleVisibilityChangeBound) document.removeEventListener('visibilitychange', this.handleVisibilityChangeBound); } catch (e) {}
    try { if (this.handleBeforeUnloadBound) window.removeEventListener('beforeunload', this.handleBeforeUnloadBound); } catch (e) {}
  }
}

