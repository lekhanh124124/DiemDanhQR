import { Component, OnInit } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { AttendanceService } from '../../core/services/attendance.service';
import { CourseService } from '../../core/services/course.service';
import { StudentService } from '../../core/services/student.service';
import { AuthService } from '../../core/services/auth.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { ActivatedRoute } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-attendance-manage',
  templateUrl: './attendance-manage.component.html',
  styleUrls: ['./attendance-manage.component.scss']
})
export class AttendanceManageComponent implements OnInit {
  classes: Array<{ maLopHocPhan: string; tenLopHocPhan: string }> = [];
  selectedClass = '';
  selectedDate = '';
  records: any[] = [];
  loading = false;
  statuses: Array<any> = [];
  processingFill = false;
  processingSaveAll = false;
  pageIndex = 1;
  pageSize = 200;
  tableTotal = 0;
  totalRecords = 0;
  presentCount = 0;
  lateCount = 0;
  absentCount = 0;
  classSize?: number;
  currentMaBuoi?: number;

  constructor(
    private attendance: AttendanceService,
    private courseService: CourseService,
    private studentService: StudentService,
    private route: ActivatedRoute,
    private scheduleService: ScheduleService,
    private msg: NzMessageService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadClasses();
    this.loadStatuses();
    this.route.queryParams.subscribe(p => {
      if (p && p['classId']) this.selectedClass = p['classId'];
      if (p && p['date']) this.selectedDate = p['date'];
      if (p && p['maBuoi']) {
        this.currentMaBuoi = Number(p['maBuoi']);
        this.loadAttendanceByBuoi(this.currentMaBuoi);
      } else if (p && p['classId'] && p['date']) {
        this.loadAttendance();
      }
    });
  }

  exportPdf(): void {
    (async () => {
      try {
        const STATUS_LABELS = ['CÓ MẶT', 'ĐI TRỄ', 'VẮNG KHÔNG PHÉP', 'VẮNG CÓ PHÉP', 'ĐIỂM DANH SAI VỊ TRÍ GPS'];
        const headers = ['STT','Mã SV','Họ tên','Thời gian quét', ...STATUS_LABELS, 'Lý do'];
        let html = `<div style="font-family: Arial, Helvetica, sans-serif; font-size:12px;">
          <h3>Danh sách điểm danh ${this.currentMaBuoi ? 'Buổi ' + this.currentMaBuoi : ''}</h3>
          <table style="border-collapse:collapse;width:100%"><thead><tr>`;
        headers.forEach(h => { html += `<th style="border:1px solid #333;padding:6px;text-align:left">${h}</th>`; });
        html += '</tr></thead><tbody>';
        (this.records || []).forEach((r: any, i: number) => {
          const status = String(r._editStatus || r.tenTrangThai || r.codeTrangThai || r.trangThai || '').toUpperCase();
          html += '<tr>';
          html += `<td style="border:1px solid #333;padding:6px;">${i+1}</td>`;
          html += `<td style="border:1px solid #333;padding:6px;">${(r.maSinhVien||r.MaSinhVien||'')}</td>`;
          html += `<td style="border:1px solid #333;padding:6px;">${(r.hoTen||r.HoTen||'')}</td>`;
          html += `<td style="border:1px solid #333;padding:6px;">${(r.thoiGianQuet||r.ThoiGianQuet||'')}</td>`;
          for (const lbl of STATUS_LABELS) {
            html += `<td style="border:1px solid #333;padding:6px;text-align:center">${String(lbl).toUpperCase() === status ? '✓' : ''}</td>`;
          }
          html += `<td style="border:1px solid #333;padding:6px;">${(r._editLyDo||r.lyDo||r.LyDo||'')}</td>`;
          html += '</tr>';
        });
        html += '</tbody></table></div>';

        const html2canvas = (await import('html2canvas')).default;
        const container = document.createElement('div');
        container.style.position = 'fixed';
        container.style.left = '-9999px';
        container.style.top = '0';
        container.innerHTML = html;
        document.body.appendChild(container);

        const canvas = await (html2canvas as any)(container, { scale: 2 } as any);
        const imgData = canvas.toDataURL('image/png');
        document.body.removeChild(container);

        const win = window.open('', '_blank');
        if (!win) { this.msg.error('Trình duyệt chặn popup. Vui lòng cho phép popup để xuất (xem trước).'); return; }
        const wHtml = `<!doctype html><html><head><meta charset="utf-8"><title>Attendance Preview</title>` +
          `<style>body{text-align:center;margin:0;padding:10px;font-family:Arial,Helvetica,sans-serif}img{max-width:100%;height:auto}</style>` +
          `</head><body><img src="${imgData}" alt="attendance"/></body></html>`;
        win.document.write(wHtml);
        win.document.close();
        win.focus();
        setTimeout(() => win.print(), 300);
        this.msg.info('Mở xem trước in. Dùng chức năng in của trình duyệt để lưu PDF nếu cần.');
      } catch (e) {
        console.warn('exportPdf: html2canvas not available or failed, falling back to plain print-preview', e);
        try {
          const STATUS_LABELS = ['CÓ MẶT', 'ĐI TRỄ', 'VẮNG KHÔNG PHÉP', 'VẮNG CÓ PHÉP', 'ĐIỂM DANH SAI VỊ TRÍ GPS'];
          const headers = ['STT','Mã SV','Họ tên','Thời gian quét', ...STATUS_LABELS, 'Lý do'];
          let html = `<!doctype html><html><head><meta charset="utf-8"><title>Attendance Export</title>`;
          html += `<style>table{border-collapse:collapse;width:100%}th,td{border:1px solid #333;padding:6px;text-align:left}</style>`;
          html += `</head><body>`;
          html += `<h3>Danh sách điểm danh ${this.currentMaBuoi ? 'Buổi ' + this.currentMaBuoi : ''}</h3>`;
          html += '<table><thead><tr>';
          headers.forEach(h => html += `<th>${h}</th>`);
          html += '</tr></thead><tbody>';
          (this.records || []).forEach((r: any, i: number) => {
            const status = String(r._editStatus || r.tenTrangThai || r.codeTrangThai || r.trangThai || '').toUpperCase();
            html += '<tr>';
            html += `<td>${i+1}</td>`;
            html += `<td>${(r.maSinhVien||r.MaSinhVien||'')}</td>`;
            html += `<td>${(r.hoTen||r.HoTen||'')}</td>`;
            html += `<td>${(r.thoiGianQuet||r.ThoiGianQuet||'')}</td>`;
            for (const lbl of STATUS_LABELS) {
              html += `<td>${String(lbl).toUpperCase() === status ? '✓' : ''}</td>`;
            }
            html += `<td>${(r._editLyDo||r.lyDo||r.LyDo||'')}</td>`;
            html += '</tr>';
          });
          html += '</tbody></table></body></html>';
          const w = window.open('', '_blank');
          if (!w) { this.msg.error('Trình duyệt chặn popup. Vui lòng cho phép popup để sử dụng Export PDF (print preview), hoặc dùng Export Excel.'); return; }
          w.document.write(html);
          w.document.close();
          w.focus();
          setTimeout(() => w.print(), 300);
        } catch (e2) {
          console.error('exportPdf fallback error', e2);
          this.msg.error('Xuất PDF thất bại. Vui lòng dùng Export Excel.');
        }
      }
    })();
  }

  refreshAttendance(): void {
    console.debug('attendance-manage: refreshAttendance called', { currentMaBuoi: this.currentMaBuoi, selectedClass: this.selectedClass, selectedDate: this.selectedDate });
    if (this.currentMaBuoi) {
      this.loadAttendanceByBuoi(this.currentMaBuoi);
      return;
    }
    if (this.selectedClass) {
      this.loadAttendance();
      return;
    }
    this.msg.info('Vui lòng chọn lớp hoặc buổi trước khi làm mới');
  }

  loadClasses(): void {
    const user: any = this.auth.getUser() || {};
    const rawCandidates = [
      user?.tenDangNhap,
      user?.TenDangNhap,
      user?.username,
      user?.nguoiDung?.tenDangNhap,
      user?.nguoiDung?.TenDangNhap,
      user?.maGiangVien,
      user?.MaGiangVien,
      user?.nguoiDung?.maGiangVien,
      user?.nguoiDung?.MaGiangVien,
      user?.maNguoiDung,
      user?.MaNguoiDung
    ].filter((v: any) => v !== undefined && v !== null && String(v).trim() !== '').map((v: any) => String(v));

    const nonNumeric = rawCandidates.filter((c: string) => /\D/.test(c));
    const candidates = nonNumeric.length ? nonNumeric : rawCandidates;

    const baseOpts: any = { page: 1, pageSize: 50 };

    const tryIdx = (i: number) => {
      const opts: any = { ...baseOpts };
      if (candidates[i]) { opts.maGiangVien = candidates[i]; }
      this.courseService.getCourses(opts).subscribe(res => {
        console.debug('attendance-manage: getCourses response for tryIdx', i, res);
        const items = res?.data?.items || res?.data || [];
        if ((items || []).length) {
          this.classes = items.map((x: any) => {
            const lop = x?.lopHocPhan || x?.LopHocPhan || x?.lop || x;
            const mon = x?.monHoc || x?.MonHoc || x?.subject || {};
            const ma = lop?.maLopHocPhan || lop?.MaLopHocPhan || lop?.maLop || x?.maLopHocPhan || x?.maLop || '';
            const ten = lop?.tenLopHocPhan || lop?.TenLopHocPhan || lop?.tenLop || x?.tenLopHocPhan || x?.tenLop || mon?.tenMonHoc || mon?.TenMonHoc || '';
            return { maLopHocPhan: ma, tenLopHocPhan: ten };
          }).filter((c: any) => c.maLopHocPhan && c.tenLopHocPhan);
          console.debug('attendance-manage: classes mapped sample', this.classes.slice(0,5));
          return;
        }
        if (i + 1 < candidates.length) tryIdx(i + 1);
        else this.classes = [];
      }, err => {
        if (i + 1 < candidates.length) tryIdx(i + 1); else this.classes = [];
      });
    };

    if (candidates.length) tryIdx(0);
    else {
      this.courseService.getCourses(baseOpts).subscribe(res => {
        console.debug('attendance-manage: getCourses fallback response', res);
        const items = res?.data?.items || res?.data || [];
        this.classes = (items || []).map((x: any) => {
          const lop = x?.lopHocPhan || x?.LopHocPhan || x?.lop || x;
          const mon = x?.monHoc || x?.MonHoc || x?.subject || {};
          const ma = lop?.maLopHocPhan || lop?.MaLopHocPhan || lop?.maLop || x?.maLopHocPhan || x?.maLop || '';
          const ten = lop?.tenLopHocPhan || lop?.TenLopHocPhan || lop?.tenLop || x?.tenLopHocPhan || x?.tenLop || mon?.tenMonHoc || mon?.TenMonHoc || '';
          return { maLopHocPhan: ma, tenLopHocPhan: ten };
        }).filter((c: any) => c.maLopHocPhan && c.tenLopHocPhan);
        console.debug('attendance-manage: classes populated (fallback), count=', this.classes.length, this.classes.slice(0,5));
      }, () => { this.classes = []; });
    }
  }

  loadAttendance(): void {
    console.debug('attendance-manage: loadAttendance start', { selectedClass: this.selectedClass, selectedDate: this.selectedDate });
    if (!this.selectedClass) {
      console.debug('attendance-manage: loadAttendance aborted, no selectedClass');
      return;
    }

    this.loading = true;

    const formattedDate = this.normalizeDateForApi(this.selectedDate);


    if (formattedDate) {
      const schedParams: any = { Page: 1, PageSize: 10, MaLopHocPhan: this.selectedClass, NgayHoc: formattedDate };
      console.debug('attendance-manage: checking schedule for class+date', schedParams);
      this.scheduleService.getSchedules(schedParams).pipe(finalize(() => { if (!this.currentMaBuoi) this.loading = false; })).subscribe({ next: (schRes: any) => {
        const sitems = schRes?.data?.items || schRes?.data || [];
        const buoi = sitems && sitems.length ? sitems[0] : null;
        const maBuoi = buoi ? (buoi?.maBuoi || buoi?.MaBuoi || buoi?.buoiHoc?.maBuoi || buoi?.buoiHoc?.MaBuoi) : undefined;
        if (maBuoi) {
          this.currentMaBuoi = Number(maBuoi);
          this.loadAttendanceByBuoi(this.currentMaBuoi);
          return;
        }
        console.debug('attendance-manage: no scheduled session found for selected date/class', { selectedClass: this.selectedClass, NgayHoc: formattedDate });
        this.records = [];
        this.classSize = undefined;
        this.tableTotal = 0;
        this.currentMaBuoi = undefined;
        this.computeSummary();
        this.msg.info('Không có buổi học cho lớp này vào ngày đã chọn');
      }, error: (err: any) => {
        console.error('attendance-manage: schedule lookup error', err);
        this.records = [];
        this.classSize = undefined;
        this.tableTotal = 0;
        this.currentMaBuoi = undefined;
        this.computeSummary();
        this.msg.error('Lỗi khi kiểm tra lịch học');
        this.loading = false;
      } });
      return;
    }

    this.tryLoadBySchedule(undefined);
  }

  private normalizeDateForApi(raw?: string): string | undefined {
    if (!raw) return undefined;
    const s = String(raw).trim();
    if (!s) return undefined;
    const parts = s.split(/[-\/]/).map(p => p.trim());
    if (parts.length !== 3) return s;
    if (parts[0].length === 4) {
      return `${parts[0]}-${parts[1].padStart(2,'0')}-${parts[2].padStart(2,'0')}`;
    }
    return `${parts[2]}-${parts[1].padStart(2,'0')}-${parts[0].padStart(2,'0')}`;
  }

  private tryLoadBySchedule(formattedDate?: string | undefined): void {
    const params: any = { Page: this.pageIndex, PageSize: this.pageSize, MaLopHocPhan: this.selectedClass };
    if (formattedDate) params.NgayHoc = formattedDate;
    this.loading = true;
    this.scheduleService.getSchedules(params).pipe(finalize(() => { this.loading = false; })).subscribe({ next: (res) => {
      console.debug('attendance-manage: getSchedules response', res);
      const items = res?.data?.items || res?.data || [];
      const buoi = items[0];
      const maBuoi = buoi?.maBuoi || buoi?.MaBuoi || buoi?.buoiHoc?.maBuoi || buoi?.buoiHoc?.MaBuoi;
      if (maBuoi) {
        this.currentMaBuoi = Number(maBuoi);
        this.attendance.getRecords({ Page: this.pageIndex, PageSize: this.pageSize, MaBuoi: Number(maBuoi) })
          .pipe(finalize(() => { this.loading = false; }))
          .subscribe(r => {
            const items = r?.data?.items || r?.data || r || [];
            const totalRecordsVal = r?.data?.totalRecords || r?.data?.total || r?.data?.totalItems;
            const totalPagesVal = r?.data?.totalPages;
            const serverPageSize = r?.data?.pageSize || r?.data?.PageSize || this.pageSize;
            if (typeof totalRecordsVal === 'number') {
              this.tableTotal = totalRecordsVal;
            } else if (totalRecordsVal) {
              this.tableTotal = Number(totalRecordsVal);
            } else if (typeof totalPagesVal === 'number') {
              this.tableTotal = (serverPageSize && Number(serverPageSize) > 0) ? Number(totalPagesVal) * Number(serverPageSize) : Number(totalPagesVal);
            } else if (totalPagesVal) {
              this.tableTotal = (serverPageSize && Number(serverPageSize) > 0) ? Number(totalPagesVal) * Number(serverPageSize) : Number(totalPagesVal);
            } else {
              this.tableTotal = items ? items.length : 0;
            }
            this.records = this.normalizeRecords(items);
            this.computeSummary();
            this.loadClassSizeIfNeeded();
            this.fillNamesFromRoster();
          });
      } else {
        this.records = [];
      }
    }, error: () => { this.records = []; } });
  }

  // onPageIndexChange(newIndex: number): void {
  //   this.pageIndex = newIndex;
  //   if (this.currentMaBuoi) this.loadAttendanceByBuoi(this.currentMaBuoi);
  //   else this.loadAttendance();
  // }

  loadAttendanceByBuoi(maBuoi: number): void {
    if (!maBuoi) { console.debug('attendance-manage: loadAttendanceByBuoi aborted, invalid maBuoi', maBuoi); return; }
    console.debug('attendance-manage: loadAttendanceByBuoi start', { maBuoi, pageIndex: this.pageIndex, pageSize: this.pageSize });
    this.loading = true;
    this.attendance.getRecords({ Page: this.pageIndex, PageSize: this.pageSize, MaBuoi: Number(maBuoi) })
      .pipe(finalize(() => { this.loading = false; }))
      .subscribe(r => {
        const items = r?.data?.items || r?.data || r || [];
        const normalized = this.normalizeRecords(items);
        try {
          if (!this.selectedClass) {
            this.records = normalized;
            this.computeSummary();
            return;
          }
          this.studentService.getStudents({ MaLopHocPhan: this.selectedClass, Page: 1, PageSize: 1000 }).subscribe({ next: (res: any) => {
            const roster = res?.data?.items || res?.data || [];
            const map = new Map<string, any>();
            (normalized || []).forEach((rec: any) => { if (rec.maSinhVien) map.set(String(rec.maSinhVien), rec); });
            const merged = (roster || []).map((s: any, idx: number) => {
              const sv = s?.sinhVien || s?.SinhVien || {};
              const nd = s?.nguoiDung || s?.NguoiDung || {};
              const id = String(sv?.maSinhVien || sv?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || nd?.maSinhVien || nd?.MaSinhVien || '').trim();
              const name = (nd?.hoTen || nd?.HoTen || sv?.hoTen || sv?.HoTen || s?.hoTen || s?.HoTen || '').trim();
              const existing = map.get(id) || null;
              if (existing) return { ...existing, _index: idx + 1, hoTen: (existing.hoTen && existing.hoTen.trim() !== '') ? existing.hoTen : name, _editStatus: existing._editStatus || existing.codeTrangThai || existing.CodeTrangThai || '', _editLyDo: existing._editLyDo || (existing.lyDo || '') };
              return {
                _index: idx + 1,
                maDiemDanh: '',
                maSinhVien: id,
                hoTen: name,
                thoiGianQuet: '',
                lyDo: '',
                codeTrangThai: '',
                tenTrangThai: '',
                buoiHoc: null,
                lopHocPhan: null,
                _editStatus: '',
                _originalEditStatus: '',
                _editLyDo: ''
              };
            });
            this.records = merged;
            this.classSize = (roster || []).length || undefined;
            this.tableTotal = this.classSize || (this.records || []).length;
            this.computeSummary();
          }, error: () => {
            this.records = normalized;
            this.classSize = undefined;
            this.tableTotal = (this.records || []).length;
            this.computeSummary();
          } });
        } catch (e) {
          this.records = normalized;
          this.computeSummary();
        }
      });
  }

  private normalizeRecords(items: any[]): any[] {
    if (!items || !items.length) return [];
    return (items || []).map((it: any, idx: number) => {
      const dd = it.diemDanh || it.DiemDanh || {};
      const sv = it.sinhVien || it.SinhVien || {};
      const st = it.trangThaiDiemDanh || it.trangThai || it.trangThaiObj || {};
      const buoi = it.buoiHoc || it.buoi || it.BuoiHoc || {};
      const lop = it.lopHocPhan || it.lop || it.LopHocPhan || {};
      const maDiemDanh = dd?.maDiemDanh || dd?.MaDiemDanh || dd?.id || '';
      const maSinhVien = sv?.maSinhVien || sv?.MaSinhVien || it?.maSinhVien || it?.MaSinhVien || '';
      const tenSinhVien = sv?.hoTen || sv?.HoTen || it?.hoTen || it?.HoTen || '';
      const thoiGianQuet = dd?.thoiGianQuet || dd?.ThoiGianQuet || dd?.thoiGian || dd?.ThoiGian || '';
      const lyDo = dd?.lyDo || dd?.LyDo || dd?.lydo || dd?.lydo || dd?.ghiChu || '';
      const codeTrangThai = (st?.codeTrangThai || st?.CodeTrangThai || dd?.codeTrangThai || dd?.CodeTrangThai || st?.code || '') || '';
      const tenTrangThai = st?.tenTrangThai || st?.TenTrangThai || '';
      const scannedFlag = dd?.trangThai === true || String(dd?.trangThai).toLowerCase() === 'true' || String(dd?.trangThai) === '1';
      const scanned = Boolean(maDiemDanh) || Boolean(thoiGianQuet) || scannedFlag;
      return {
        _index: idx + 1,
        maDiemDanh: maDiemDanh,
        maSinhVien: String(maSinhVien || '').trim(),
        hoTen: String(tenSinhVien || '').trim(),
        thoiGianQuet: thoiGianQuet,
        lyDo: lyDo === 'null' ? '' : (lyDo || ''),
        codeTrangThai: codeTrangThai,
        tenTrangThai: tenTrangThai,
        buoiHoc: buoi,
        lopHocPhan: lop,
        // chỉ gán radio cho bản ghi đã quét, bản ghi tạo thiếu sẽ để trống cho giảng viên tự chọn
        _editStatus: scanned ? this.mapToRadioLabel({ codeTrangThai, tenTrangThai }) || '' : '',
        _originalEditStatus: scanned ? this.mapToRadioLabel({ codeTrangThai, tenTrangThai }) || '' : '',
        _editLyDo: (lyDo === 'null' ? '' : lyDo) || '',
        _originalLyDo: (lyDo === 'null' ? '' : lyDo) || '',
        _trangThai: scanned
      };
    });
  }


  async exportExcel(): Promise<void> {
    try {
      const XLSX = await import('xlsx');
      const STATUS_LABELS = ['CÓ MẶT', 'ĐI TRỄ', 'VẮNG KHÔNG PHÉP', 'VẮNG CÓ PHÉP', 'ĐIỂM DANH SAI VỊ TRÍ GPS'];
      const rows = (this.records || []).map((r: any, i: number) => {
        const status = String(r._editStatus || r.tenTrangThai || r.codeTrangThai || r.trangThai || '').toUpperCase();
        const base: any = {
          STT: i + 1,
          MaSV: r.maSinhVien || r.MaSinhVien || '',
          HoTen: r.hoTen || r.HoTen || '',
          ThoiGianQuet: r.thoiGianQuet || r.ThoiGianQuet || '',
          LyDo: r._editLyDo || r.lyDo || r.LyDo || ''
        };
        for (const lbl of STATUS_LABELS) {
          base[lbl] = (String(lbl).toUpperCase() === status) ? '✓' : '';
        }
        return base;
      });
      const ws = XLSX.utils.json_to_sheet(rows);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Attendance');
      const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
      const blob = new Blob([wbout], { type: 'application/octet-stream' });
      const a = document.createElement('a');
      const url = URL.createObjectURL(blob);
      a.href = url;
      a.download = `attendance-${this.currentMaBuoi || this.selectedClass || 'export'}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      console.error('exportExcel error', e);
      this.msg.error('Xuất Excel thất bại. Hãy chắc chắn đã cài package "xlsx"');
    }
  }

  // get attendancePercent(): string {
  //   const base = (this.classSize && this.classSize > 0) ? this.classSize : (this.totalRecords || 0);
  //   if (!base) return '0%';
  //   const pct = Math.round((this.presentCount / base) * 10000) / 100;
  //   return `${pct}%`;
  // }

  private mapToRadioLabel(st: { codeTrangThai?: string; tenTrangThai?: string }): string {
    const code = String(st.codeTrangThai || '').toUpperCase();
    const name = String(st.tenTrangThai || '').toUpperCase();
    const src = code + ' ' + name;
    if (/LATE/.test(src) || /ĐI TRỄ/.test(src) || /DI TRE/.test(src)) return 'ĐI TRỄ';
    if (/FRAUD|FRAUD_LOCATION/.test(src) || /WRONG_GPS/.test(src) || /GPS/.test(src) || /SAI VỊ TRÍ/.test(src) || /SAI VI TRI/.test(src)) return 'ĐIỂM DANH SAI VỊ TRÍ GPS';
    if (/ABSENT_EXCUSED/.test(src) || /VẮNG CÓ PHÉP/.test(src) || /VANG CO PHEP/.test(src) || /EXCUSED/.test(src)) return 'VẮNG CÓ PHÉP';
    if (/ABSENT_UNEXCUSED/.test(src) || /VẮNG KHÔNG PHÉP/.test(src) || /VANG KHONG PHEP/.test(src) || (/ABSENT/.test(src) && !/EXCUSED/.test(src))) return 'VẮNG KHÔNG PHÉP';
    if (/PRESENT/.test(src) || /CÓ MẶT/.test(src) || /CO MAT/.test(src) || code === 'P') return 'CÓ MẶT';
    return '';
  }

  private loadStatuses(): void {
    try {
      this.attendance.getStatuses({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
        const raw = res?.data?.items || res?.data || res || [];
        this.statuses = (raw || []).map((it: any) => it?.trangThaiDiemDanh || it?.trangThai || it?.status || it || {})
          .filter((s: any) => s && Object.keys(s).length > 0);
      }, error: () => { this.statuses = []; } });
    } catch { this.statuses = []; }
  }

  private labelToCode(label: string): string {
    const L = String(label || '').toUpperCase();
    switch (L) {
      case 'CÓ MẶT': return 'PRESENT';
      case 'ĐI TRỄ': return 'LATE';
      case 'VẮNG KHÔNG PHÉP': return 'ABSENT_UNEXCUSED';
      case 'VẮNG CÓ PHÉP': return 'ABSENT_EXCUSED';
      case 'ĐIỂM DANH SAI VỊ TRÍ GPS': return 'WRONG_GPS';
      default: return L;
    }
  }

  private getMaTrangThaiFromValue(val: string): number | undefined {
    const code = this.labelToCode(val);
    const found = (this.statuses || []).find((s: any) => {
      const sc = String(s.codeTrangThai || s.CodeTrangThai || s.code || '').toUpperCase();
      const sn = String(s.tenTrangThai || s.TenTrangThai || '').toUpperCase();
      return sc === code || sn === code || sn === val.toUpperCase();
    });
    if (!found) return undefined;
    const idRaw = found.maTrangThai || found.MaTrangThai || found.id;
    const num = Number(idRaw);
    return isNaN(num) ? undefined : num;
  }

  private computeSummary(): void {
    const base = this.records || [];
    this.totalRecords = this.classSize && this.classSize > 0 ? this.classSize : base.length;
    let present = 0, late = 0, absent = 0;
    for (const r of base) {
      const label = String(r._editStatus || '').toUpperCase();
      if (!label) continue; // chưa chọn => chưa điểm danh thủ công
      if (label === 'CÓ MẶT') present++;
      else if (label === 'ĐI TRỄ') late++;
      else if (label === 'VẮNG KHÔNG PHÉP') absent++;
      else if (label === 'VẮNG CÓ PHÉP') absent++;
      else if (label === 'ĐIỂM DANH SAI VỊ TRÍ GPS') present++; // vẫn tính là có mặt
    }
    this.presentCount = present;
    this.lateCount = late;
    // absent tổng = tổng lớp - (present+late) nếu thiếu
    const baseTotal = this.totalRecords || 0;
    const accounted = present + late + absent;
    this.absentCount = absent + Math.max(0, baseTotal - accounted);
  }

  async saveAllHandmarks(): Promise<void> {
    if (!this.currentMaBuoi) { this.msg.error('Không có buổi học'); return; }
    // Các bản ghi cần lưu: chưa điểm danh (_trangThai false) nhưng đã chọn radio, hoặc thay đổi radio so với ban đầu.
    const targets = (this.records || []).filter(r => {
      const hasId = !!(r.maDiemDanh);
      if (!hasId) return false;
      const lyDoChanged = (String(r._editLyDo || '').trim() !== String(r._originalLyDo || '').trim()) && String(r._editLyDo || '').trim() !== '';
      if (lyDoChanged) return true;
      if (r._trangThai === false && r._editStatus) return true;
      return r._editStatus && r._editStatus !== r._originalEditStatus;
    });
    if (!targets.length) { this.msg.info('Không có thay đổi cần lưu'); return; }
    let ok = 0, fail = 0;
    for (const row of targets) {
      try {
        const ma = row.maDiemDanh;
        const label = row._editStatus;
        const maTrangThai = this.getMaTrangThaiFromValue(label);
        const body: any = { MaDiemDanh: ma, TrangThai: true };
        if (row._editLyDo && String(row._editLyDo).trim() !== '') {
          body.LyDo = String(row._editLyDo).trim();
        }
        if (maTrangThai !== undefined) body.MaTrangThai = maTrangThai; else body.CodeTrangThai = this.labelToCode(label);
        console.debug('attendance-manage: saving record', body);
        await lastValueFrom(this.attendance.updateRecord(body));
        ok++;
      } catch (e) {
        fail++;
      }
    }
    this.msg.success(`Lưu xong: thành công ${ok}, lỗi ${fail}`);
    this.loadAttendanceByBuoi(this.currentMaBuoi || 0);
  }

  private loadClassSizeIfNeeded(): void {
    try {
      if (!this.selectedClass) { this.classSize = undefined; return; }
      this.studentService.getStudents({ MaLopHocPhan: this.selectedClass, Page: 1, PageSize: 1 }).subscribe({
        next: (res: any) => {
          const total = res?.data?.totalRecords || res?.data?.total || res?.data?.totalItems || undefined;
          this.classSize = (typeof total === 'number') ? total : undefined;
        },
        error: () => { this.classSize = undefined; }
      });
    } catch { this.classSize = undefined; }
  }

  private fillNamesFromRoster(): void {
    try {
      if (!this.selectedClass) return;
      this.studentService.getStudents({ MaLopHocPhan: this.selectedClass, Page: 1, PageSize: 500 }).subscribe({
        next: (res: any) => {
          const items = res?.data?.items || res?.data || [];
          const map = new Map<string,string>();
          (items || []).forEach((s: any) => {
            const sv = s?.sinhVien || s?.SinhVien || {};
            const nd = s?.nguoiDung || s?.NguoiDung || {};
            const id = String(sv?.maSinhVien || sv?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || nd?.maSinhVien || nd?.MaSinhVien || '').trim();
            const name = (nd?.hoTen || nd?.HoTen || sv?.hoTen || sv?.HoTen || s?.hoTen || s?.HoTen || '').trim();
            if (id) map.set(id, name);
          });
          this.records = (this.records || []).map(r => ({ ...r, hoTen: (r.hoTen && r.hoTen.trim() !== '') ? r.hoTen : (map.get(String(r.maSinhVien)) || r.hoTen) }));
        },
        error: () => { }
      });
    } catch { }
  }
}
