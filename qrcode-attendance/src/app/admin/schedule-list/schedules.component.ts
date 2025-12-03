import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ScheduleService } from '../../core/services/schedule.service';
import { CourseService } from '../../core/services/course.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import dayjs from 'dayjs';

@Component({
  selector: 'app-schedules',
  templateUrl: './schedules.component.html',
  styleUrls: ['./schedules.component.scss']
})
export class SchedulesComponent implements OnInit {
  schedules: any[] = [];
  loading = false;
  page = 1;
  pageSize = 10;
  total = 0;

  keyword = '';
  showAdvanced = false;
  filterMaLopHocPhan = '';
  filterTenMonHoc = '';
  filterNgayHoc = '';
  filterTietBatDau = '';
  filterTenPhong = '';
  filterMaPhong = '';
  filterTrangThai: any = '';
  sortBy: string = 'NgayHoc';
  sortDir: 'ASC' | 'DESC' = 'ASC';

  isCreateVisible = false;
  isEditVisible = false;
  creating = false;
  updating = false;
  loadingFullSubjects = false;
  newSchedule: any = { MaLopHocPhan: '', MaPhong: '', NgayHoc: '', TietBatDau: '', SoTiet: '', GhiChu: '', TrangThai: true };
  selectedSchedule: any = {};
  coursesOptions: Array<{ label: string; value: string }> = [];
  roomsOptions: Array<{ label: string; value: any }> = [];
  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  constructor(private schedule: ScheduleService, private courseService: CourseService, private msg: NzMessageService, private route: ActivatedRoute, private modal: NzModalService) {}

  private toBoolean(v: any): boolean {
    if (v === undefined || v === null) return false;
    if (typeof v === 'boolean') return v;
    if (typeof v === 'number') return v === 1;
    const s = String(v).trim().toLowerCase();
    if (s === '1' || s === 'true' || s === 't' || s === 'yes') return true;
    return false;
  }

  ngOnInit(): void {
    const qp: any = this.route.snapshot.queryParams;
    if (qp && (qp['MaLopHocPhan'] || qp['maLopHocPhan'])) {
      this.filterMaLopHocPhan = qp['MaLopHocPhan'] || qp['maLopHocPhan'];
    }
    if (qp && (qp['MaPhong'] || qp['maPhong'])) {
      this.filterMaPhong = qp['MaPhong'] || qp['maPhong'];
    }
    if (qp && (qp['TenPhong'] || qp['tenPhong'])) {
      this.filterTenPhong = qp['TenPhong'] || qp['tenPhong'];
    }
    if (qp && (qp['includeAll'] === '1' || qp['includeAll'] === 'true' || qp['includeAll'] === 1)) {
      this.pageSize = 50;
      this.page = 1;
    }
    this.load();
    this.loadCourseOptions();
    this.loadRoomOptions();
  }

  private loadCourseOptions(): void {
    try {
      const params: any = { page: 1, pageSize: 200 };
      this.courseService.getCoursesCached(params).subscribe({ next: (res: any) => {
        const raw = res?.data?.items || res?.data || res || [];
        const items = Array.isArray(raw) ? raw : (raw.items || raw);
        this.coursesOptions = (items || []).map((it: any) => {
          const lop = it?.lopHocPhan || it?.LopHocPhan || it || {};
          const mon = it?.monHoc || it?.MonHoc || {};
          const maLop = lop?.maLopHocPhan || lop?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan || '';
          const label = maLop || '';
          const value = maLop;
          return { label: String(label), value: String(value) };
        }).filter((x: any) => x.value && String(x.value).trim() !== '');
      }, error: () => { } });
    } catch (e) {  }
  }

  private loadRoomOptions(): void {
    try {
      const params: any = { Page: 1, PageSize: 500 };
      this.schedule.getRooms(params).subscribe({ next: (res: any) => {
        const raw = res?.data?.items || res?.data || res || [];
        const items = Array.isArray(raw) ? raw : (raw.items || raw);
        this.roomsOptions = (items || []).map((it: any) => {
          const p = it?.phongHoc || it || {};
          const label = p?.tenPhong || p?.TenPhong || p?.ten || p?.ma || p?.maPhong || p?.MaPhong || '';
          const value = p?.maPhong ?? p?.MaPhong ?? p?.ma ?? '';
          return { label: String(label), value: value };
        }).filter((x: any) => x.value !== undefined && x.value !== null && String(x.value).trim() !== '');
      }, error: () => { } });
    } catch (e) {  }
  }

  load(): void {
    this.loading = true;
    const params: any = { Page: this.page, PageSize: this.pageSize, MaLopHocPhan: this.filterMaLopHocPhan, TenMonHoc: this.filterTenMonHoc, NgayHoc: this.filterNgayHoc, TietBatDau: this.filterTietBatDau, TenPhong: this.filterTenPhong, MaPhong: this.filterMaPhong, SortBy: this.sortBy, SortDir: this.sortDir };
    if (this.filterTrangThai !== '' && this.filterTrangThai !== undefined && this.filterTrangThai !== null) {
      params.TrangThai = (this.filterTrangThai === true || String(this.filterTrangThai) === 'true') ? true : (this.filterTrangThai === false || String(this.filterTrangThai) === 'false' ? false : this.filterTrangThai);
    }
    this.schedule.getSchedules(params).subscribe({
      next: res => {
        let rawItems: any[] = [];
        if (Array.isArray(res)) {
          rawItems = res;
          this.total = res.length;
        } else {
          rawItems = res?.items || res?.data?.items || res?.data || [];

          const totalFromRes = Number(res?.data?.totalRecords ?? res?.data?.total ?? res?.total ?? rawItems.length) || rawItems.length;
          this.total = totalFromRes;
        }
        this._rawItems = rawItems || [];

        // this.buildSubjectsSummaryIfNeeded();

        this.schedules = (rawItems || []).map((it: any) => {
          const buoi = it?.buoiHoc || it?.BuoiHoc || it;
          const phong = it?.phongHoc || it?.PhongHoc || it;
          const lop = it?.lopHocPhan || it?.LopHocPhan || it;
          const mon = it?.monHoc || it?.MonHoc || it;
          return {
            maBuoi: buoi?.maBuoi || buoi?.MaBuoi || it?.maBuoi || it?.MaBuoi,
            maLopHocPhan: lop?.maLopHocPhan || lop?.MaLopHocPhan || it?.maLopHocPhan || it?.MaLopHocPhan,
            tenLopHocPhan: lop?.tenLopHocPhan || lop?.TenLopHocPhan || it?.tenLopHocPhan || it?.TenLopHocPhan,
            tenMonHoc: mon?.tenMonHoc || mon?.TenMonHoc || it?.tenMonHoc || it?.TenMonHoc,
            maPhong: phong?.maPhong || phong?.MaPhong || it?.maPhong || it?.MaPhong,
            tenPhong: phong?.tenPhong || phong?.TenPhong || it?.tenPhong || it?.TenPhong,
            ngayHoc: (() => {
              const raw = buoi?.ngayHoc || buoi?.NgayHoc || it?.ngayHoc || it?.NgayHoc;
              if (!raw) return '';
              const parsed = dayjs(raw, ['DD-MM-YYYY', 'D-M-YYYY', 'YYYY-MM-DD'], true);
              if (parsed.isValid()) return parsed.format('YYYY-MM-DD');
              const loose = dayjs(raw);
              return loose.isValid() ? loose.format('YYYY-MM-DD') : raw;
            })(),
            tietBatDau: buoi?.tietBatDau || buoi?.TietBatDau || it?.tietBatDau || it?.TietBatDau,
            soTiet: buoi?.soTiet || buoi?.SoTiet || it?.soTiet || it?.SoTiet,
            ghiChu: buoi?.ghiChu || buoi?.GhiChu || it?.ghiChu || it?.GhiChu,
            trangThai: this.toBoolean(buoi?.trangThai ?? buoi?.TrangThai ?? it?.trangThai ?? it?.TrangThai),
            _raw: it
          };
        });
        this.loading = false;
      },
      error: () => { this.loading = false; this.msg.error('Lỗi tải buổi học'); }
    });
  }

  formatDisplayDate(raw: any): string {
    if (!raw && raw !== 0) return '';
    try {
      const parsed = dayjs(String(raw), ['YYYY-MM-DD', 'DD-MM-YYYY', 'D-M-YYYY', 'DD/MM/YYYY', 'D/M/YYYY'], true);
      if (parsed.isValid()) return parsed.format('DD-MM-YYYY');
      const loose = dayjs(String(raw));
      if (loose.isValid()) return loose.format('DD-MM-YYYY');
      return String(raw);
    } catch (e) {
      return String(raw);
    }
  }

  private _rawItems: any[] = [];
  private _fetchedFullSubjects = false;

  // distinctSubjects: Array<any> = [];

  // private buildSubjectsSummaryIfNeeded(): void {
  //   try {
  //     if (!this._rawItems || this._rawItems.length === 0) {
  //       this.distinctSubjects = [];
  //       return;
  //     }

  //     if (!this._fetchedFullSubjects && this.total && Number(this.total) > (this._rawItems || []).length) {
  //       this._fetchedFullSubjects = true; // mark to avoid retry loops
  //       const fullPageSize = Math.min(Number(this.total) || 0, 1000);
  //       const params: any = { Page: 1, PageSize: fullPageSize, MaLopHocPhan: this.filterMaLopHocPhan, NgayHoc: this.filterNgayHoc, TietBatDau: this.filterTietBatDau, TenPhong: this.filterTenPhong, MaPhong: this.filterMaPhong };
  //       this.loadingFullSubjects = true;
  //       this.schedule.getSchedules(params).subscribe({
  //         next: (fullRes: any) => {
  //           let fullItems: any[] = [];
  //           if (Array.isArray(fullRes)) fullItems = fullRes;
  //           else fullItems = fullRes?.items || fullRes?.data?.items || fullRes?.data || [];
  //           if (fullItems && fullItems.length) {
  //             this._rawItems = fullItems;
  //           }
  //           // rebuild summary using full items
  //           this.buildSubjectsSummaryIfNeeded();
  //           this.loadingFullSubjects = false;
  //         },
  //         error: () => {
  //           // ignore and continue with what we have
  //           this.loadingFullSubjects = false;
  //         }
  //       });
  //       return; // wait for async fetch to rebuild
  //     }

  //     const map = new Map<string, any>();
  //     for (const it of this._rawItems) {
  //       const lop = it?.lopHocPhan || it?.LopHocPhan || it;
  //       const mon = it?.monHoc || it?.MonHoc || it;
  //       const giang = it?.giangVienInfo || it?.giangVien || {};
  //       const key = lop?.maLopHocPhan || lop?.MaLopHocPhan || mon?.maMonHoc || mon?.MaMonHoc || String(mon?.tenMonHoc || '');
  //       const existing = map.get(key) || { key, maLopHocPhan: lop?.maLopHocPhan || lop?.MaLopHocPhan || '', maMonHoc: mon?.maMonHoc || mon?.MaMonHoc || '', tenMonHoc: mon?.tenMonHoc || mon?.TenMonHoc || '', count: 0, giangVien: giang?.hoTen || giang?.HoTen || giang?.maGiangVien || '' };
  //       existing.count = (existing.count || 0) + 1;
  //       map.set(key, existing);
  //     }

  //     this.distinctSubjects = Array.from(map.values()).map((s: any) => ({ ...s }));


  //     if (this.filterMaLopHocPhan) {
  //       const filterVal = String(this.filterMaLopHocPhan || '').trim();
  //       if (filterVal) {
  //         for (const s of this.distinctSubjects) {
  //           const matches = (s.maLopHocPhan && String(s.maLopHocPhan) === filterVal) || (s.maMonHoc && String(s.maMonHoc) === filterVal) || (s.key && String(s.key) === filterVal);
  //           if (matches) {
  //             const serverTotal = Number(this.total) || undefined;
  //             if (serverTotal && serverTotal > 0) {
  //               s.count = serverTotal;
  //             }
  //             break;
  //           }
  //         }
  //       }
  //     }
  //   } catch (e) {
  //     console.error('buildSubjectsSummary error', e);
  //     this.distinctSubjects = [];
  //   }
  // }

  // viewSubject(maLopHocPhan: string): void {
  //   if (!maLopHocPhan) return;
  //   this.filterMaLopHocPhan = maLopHocPhan;
  //   this.page = 1;
  //   this.load();
  // }

  // clearSubjectFilter(): void {
  //   this.filterMaLopHocPhan = '';
  //   this.page = 1;
  //   this.load();
  // }


  // showAllSubjects(): void {
  //   this.filterMaLopHocPhan = '';
  //   this.page = 1;
  //   const fullPageSize = 1000;
  //   const params: any = { Page: 1, PageSize: fullPageSize, MaLopHocPhan: '', NgayHoc: this.filterNgayHoc, TietBatDau: this.filterTietBatDau, TenPhong: this.filterTenPhong, MaPhong: this.filterMaPhong };
  //   this.loadingFullSubjects = true;
  //   this._fetchedFullSubjects = true;
  //   this.schedule.getSchedules(params).subscribe({
  //     next: (fullRes: any) => {
  //       let fullItems: any[] = [];
  //       if (Array.isArray(fullRes)) fullItems = fullRes;
  //       else fullItems = fullRes?.items || fullRes?.data?.items || fullRes?.data || [];
  //       this._rawItems = fullItems || [];
  //       const totalFromRes = Number(fullRes?.data?.totalRecords ?? fullRes?.data?.total ?? fullRes?.total ?? this._rawItems.length) || this._rawItems.length;
  //       this.total = totalFromRes;
  //       // this.buildSubjectsSummaryIfNeeded();
  //       this.loadingFullSubjects = false;
  //       this.pageSize = this._rawItems.length || 10;
  //     },
  //     error: () => { this.loading = false; this.msg.error('Không tải được danh sách đầy đủ'); }
  //   });
  // }

  onFilterChange(): void { this.page = 1; this.load(); }
  onPageChange(p: number): void { this.page = p; this.load(); }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortBy = column;
      this.sortDir = 'ASC';
    }
    this.page = 1;
    this.load();
  }

  onSearch(): void {
    const t = (this.keyword || '').trim();
    this.filterTenMonHoc = t;
    this.filterMaLopHocPhan = '';
    this.filterNgayHoc = '';
    this.filterTietBatDau = '';
    this.filterTenPhong = '';
    this.page = 1;
    this.load();
  }
  refreshList(): void {
    this.keyword = '';
    this.filterMaLopHocPhan = '';
    this.filterTenMonHoc = '';
    this.filterNgayHoc = '';
    this.filterTietBatDau = '';
    this.filterTenPhong = '';
    this.filterTrangThai = '';
    this.page = 1;
    this.load();
  }

  public getRowKey(row: any): string {
    return String(row?.maBuoi || row?.MaBuoi || (row?.maLopHocPhan || '') + '::' + (row?.ngayHoc || ''));
  }

  onCurrentPageDataChange(listOfCurrentPageData: readonly any[]): void {
    this.listOfCurrentPageData = (listOfCurrentPageData || []) as any[];
    this.refreshCheckedStatus();
  }

  onAllChecked(checked: boolean): void {
    this.listOfCurrentPageData.forEach(item => this.updateCheckedSet(this.getRowKey(item), checked));
    this.refreshCheckedStatus();
  }

  onItemChecked(id: string, checked: boolean): void {
    this.updateCheckedSet(id, checked);
    this.refreshCheckedStatus();
  }

  private updateCheckedSet(id: string, checked: boolean): void {
    if (checked) this.setOfCheckedId.add(id);
    else this.setOfCheckedId.delete(id);
  }

  private refreshCheckedStatus(): void {
    this.checked = this.listOfCurrentPageData.length > 0 && this.listOfCurrentPageData.every(item => this.setOfCheckedId.has(this.getRowKey(item)));
    this.indeterminate = this.listOfCurrentPageData.some(item => this.setOfCheckedId.has(this.getRowKey(item))) && !this.checked;
  }

  deleteSelected(): void {
    if (this.setOfCheckedId.size === 0) return;
    this.modal.confirm({
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} buổi học đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các buổi học đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(k => {
          const ma = k.includes('::') ? undefined : k;
          if (!ma) { remaining--; if (remaining === 0) this.load(); return; }
          this.schedule.updateSchedule({ MaBuoi: Number(ma), TrangThai: false }).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(k);
              if (remaining === 0) { this.msg.success('Đã vô hiệu hóa các buổi đã chọn'); this.load(); }
            },
            error: () => { remaining--; if (remaining === 0) this.load(); }
          });
        });
      }
    ,
      nzCancelText: 'Hủy'
    });
  }

  openCreate(): void { this.newSchedule = { MaLopHocPhan: '', MaPhong: '', NgayHoc: '', TietBatDau: '', SoTiet: '', GhiChu: '', TrangThai: true }; this.isCreateVisible = true; }
  create(): void {
    const b = this.newSchedule; if (!b?.MaLopHocPhan || !b?.MaPhong || !b?.NgayHoc || !b?.TietBatDau || !b?.SoTiet) { this.msg.warning('Điền đủ thông tin'); return; }
    this.creating = true;
    const body = { ...b, MaPhong: Number(b.MaPhong), TietBatDau: Number(b.TietBatDau), SoTiet: Number(b.SoTiet) };
    this.schedule.createSchedule(body).subscribe({
      next: () => { this.creating = false; this.isCreateVisible = false; this.msg.success('Tạo buổi học thành công'); this.load(); },
      error: () => { this.creating = false; this.msg.error('Tạo buổi học thất bại'); }
    });
  }

  openEdit(row: any): void {
    this.selectedSchedule = { MaBuoi: row.maBuoi, MaPhong: row.maPhong, NgayHoc: row.ngayHoc, TietBatDau: row.tietBatDau, SoTiet: row.soTiet, GhiChu: row.ghiChu, TrangThai: row.trangThai };
    this.isEditVisible = true;
  }
  update(): void {
    if (!this.selectedSchedule?.MaBuoi) { this.msg.warning('Thiếu MaBuoi'); return; }
    this.updating = true;
    const b = this.selectedSchedule; const body = { ...b, MaPhong: b.MaPhong ? Number(b.MaPhong) : undefined, TietBatDau: b.TietBatDau ? Number(b.TietBatDau) : undefined, SoTiet: b.SoTiet ? Number(b.SoTiet) : undefined };
    this.schedule.updateSchedule(body).subscribe({
      next: () => { this.updating = false; this.isEditVisible = false; this.msg.success('Cập nhật buổi học thành công'); this.load(); },
      error: () => { this.updating = false; this.msg.error('Cập nhật buổi học thất bại'); }
    });
  }

  cancel(): void { this.isCreateVisible = false; this.isEditVisible = false; }

  delete(row: any): void {
    const ma = row?.maBuoi;
    if (!ma) return;
    this.modal.confirm({
      nzTitle: 'Xác nhận',
      nzContent: `Bạn có chắc muốn vô hiệu hóa buổi ${ma}?`,
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        this.schedule.deleteSchedule(ma).subscribe({
          next: () => { this.msg.success('Đã vô hiệu hóa'); this.load(); },
          error: () => { this.msg.error('Vô hiệu hóa thất bại'); }
        });
      },
      nzCancelText: 'Hủy'
    });
  }
}




