import { Component, OnInit } from '@angular/core';
import { SubjectService } from '../../core/services/subject.service';
import { CourseService } from '../../core/services/course.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Router } from '@angular/router';

@Component({
  selector: 'app-subject',
  templateUrl: './subjects.component.html',
  styleUrls: ['./subjects.component.scss']
})
export class SubjectComponent implements OnInit {
  subjects: any[] = [];
  page = 1;
  pageSize = 20;
  total = 0;
  keyword = '';
  loading = false;

  filterMaMonHoc: any = '';
  filterTenMonHoc: any = '';
  filterLoaiMon: any = '';
  filterTrangThai: any = '';
  filterSoTinChi: any = '';
  filterSoTiet: any = '';
  sortBy: string = 'TenMonHoc';
  sortDir: 'ASC' | 'DESC' = 'ASC';

  isCreateVisible = false;
  isEditVisible = false;
  creating = false;
  updating = false;
  newSubject: any = { TenMonHoc: '', SoTinChi: '', SoTiet: '', MoTa: '', TrangThai: true, LoaiMon: 1 };
  selectedSubject: any = {};
  uiDeleted = new Set<string>();

  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  constructor(
    private subjectService: SubjectService,
    private courseService: CourseService,
    private message: NzMessageService,
    private modal: NzModalService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadSubjects();
  }

  loadSubjects(): void {
    this.loading = true;
    const baseParams: any = {
      Page: this.page,
      PageSize: this.pageSize,
      SortBy: this.sortBy,
      SortDir: this.sortDir
    };

    const tryOrder = ['MaMonHoc', 'TenMonHoc'];
    const variantsMap: Record<string, string[]> = {
      MaMonHoc: ['MaMonHoc', 'maMonHoc', 'MaMH', 'maMH'],
      TenMonHoc: ['TenMonHoc', 'tenMonHoc', 'TenMH', 'tenMH', 'TenMon']
    };

    const run = (i: number = 0, v: number = 0) => {
      const kw = (this.keyword || '').trim();
      const params: any = { ...baseParams };
      if (this.filterMaMonHoc !== undefined && this.filterMaMonHoc !== '') params.MaMonHoc = this.filterMaMonHoc;
      if (this.filterTenMonHoc !== undefined && this.filterTenMonHoc !== '') params.TenMonHoc = this.filterTenMonHoc;
      if (this.filterTrangThai !== undefined && this.filterTrangThai !== '') params.TrangThai = this.filterTrangThai;
      if (this.filterLoaiMon !== undefined && this.filterLoaiMon !== '') params.LoaiMon = Number(this.filterLoaiMon);
      if (this.filterSoTinChi !== undefined && this.filterSoTinChi !== '') params.SoTinChi = Number(this.filterSoTinChi);
      if (this.filterSoTiet !== undefined && this.filterSoTiet !== '') params.SoTiet = Number(this.filterSoTiet);
      if (kw) {
        const field = tryOrder[i];
        const variants = variantsMap[field] || [field];
        params[variants[v]] = kw;
      }
      this.subjectService.getSubjects(params).subscribe({
        next: (res: any) => {
          const raw = res?.data?.items || res?.data || [];
          const coerceBool = (v: any) => {
            if (v === undefined || v === null) return undefined;
            if (typeof v === 'boolean') return v;
            if (typeof v === 'number') return v === 1;
            const s = String(v).toLowerCase();
            return s === 'true' || s === '1';
          };
          let list = (raw || []).map((s: any) => {
            const m = s?.monHoc || s?.MonHoc || s || {};
            return {
              maMonHoc: m.maMonHoc || m.MaMonHoc || s.maMonHoc || s.MaMonHoc,
              tenMonHoc: m.tenMonHoc || m.TenMonHoc || s.tenMonHoc || s.TenMonHoc,
              soTinChi: m.soTinChi ?? m.SoTinChi ?? s.soTinChi ?? s.SoTinChi,
              soTiet: m.soTiet ?? m.SoTiet ?? s.soTiet ?? s.SoTiet,
              trangThai: coerceBool(m.trangThai) ?? coerceBool(m.TrangThai) ?? coerceBool(s.trangThai) ?? coerceBool(s.TrangThai) ?? true,
              moTa: m.moTa || m.MoTa || s.moTa || s.MoTa,
              loaiMon: m.loaiMon ?? m.LoaiMon ?? m.LoaiMonHoc ?? s.loaiMon ?? s.LoaiMon ?? 1
            };
          });
          if (kw) {
            const field = tryOrder[i];
            const kwl = kw.toLowerCase();
            list = list.filter((it: any) => {
              if (field === 'MaMonHoc') return String(it.maMonHoc || '').toLowerCase().includes(kwl);
              return String(it.tenMonHoc || '').toLowerCase().includes(kwl);
            });
          }
          this.subjects = list.filter((it: any) => !this.uiDeleted.has(it.maMonHoc));
          this.total = res?.data?.total || res?.data?.totalRecords || list.length;
          if (kw && list.length === 0) {
            const field = tryOrder[i];
            const variants = variantsMap[field] || [field];
            if (v + 1 < variants.length) return run(i, v + 1);
            if (i + 1 < tryOrder.length) return run(i + 1, 0);
          }
          this.loading = false;
        },
        error: (err: any) => {
          if (this.keyword) {
            const field = tryOrder[i];
            const variants = variantsMap[field] || [field];
            if (v + 1 < variants.length) return run(i, v + 1);
            if (i + 1 < tryOrder.length) return run(i + 1, 0);
          }
          console.error('Lỗi khi load môn học:', err);
          this.loading = false;
        }
      });
    };

    run(0, 0);
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadSubjects();
  }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortBy = column;
      this.sortDir = 'ASC';
    }
    this.loadSubjects();
  }

  onSearch(): void {
    this.page = 1;
    this.loadSubjects();
  }

  refreshList(): void {
    this.keyword = '';
    this.filterMaMonHoc = '';
    this.filterTenMonHoc = '';
    this.filterLoaiMon = '';
    this.filterTrangThai = '';
    this.filterSoTinChi = '';
    this.filterSoTiet = '';
    this.page = 1;
    this.uiDeleted.clear();
    this.loadSubjects();
  }

  openCreate(): void { this.newSubject = { TenMonHoc: '', SoTinChi: '', SoTiet: '', MoTa: '', TrangThai: true, LoaiMon: 1 }; this.isCreateVisible = true; }
  create(): void {
    const b = this.newSubject;
    if (!b?.TenMonHoc || b?.SoTinChi === '' || b?.SoTiet === '' || b?.LoaiMon === undefined || b?.LoaiMon === null) {
      this.message.warning('Vui lòng nhập đủ: Tên môn học, Số tín chỉ, Số tiết và Loại môn');
      return;
    }
    this.creating = true;
    const body: any = {
      TenMonHoc: b.TenMonHoc,
      SoTinChi: Number(b.SoTinChi),
      SoTiet: Number(b.SoTiet),
      MoTa: b.MoTa,
      TrangThai: b.TrangThai,
      LoaiMon: Number(b.LoaiMon)
    };
    this.courseService.createSubject(body).subscribe({
      next: () => {
        this.creating = false;
        this.isCreateVisible = false;
        this.message.success('Tạo môn học thành công');
        this.loadSubjects();
      },
      error: (err) => {
        this.creating = false;
        this.message.error(err?.error?.Message || err?.message || 'Tạo môn học thất bại');
      }
    });
  }

  openEdit(s: any): void {
    this.selectedSubject = {
      MaMonHoc: s.maMonHoc,
      TenMonHoc: s.tenMonHoc,
      SoTinChi: s.soTinChi,
      SoTiet: s.soTiet,
      MoTa: s.moTa,
      TrangThai: s.trangThai,
      LoaiMon: s.loaiMon ?? s.LoaiMon ?? 1
    };
    this.isEditVisible = true;
  }
  update(): void {
    const b = this.selectedSubject; if (!b?.MaMonHoc) { this.message.warning('Thiếu MaMonHoc'); return; }
    this.updating = true;
    const body: any = {
      MaMonHoc: b.MaMonHoc,
      TenMonHoc: b.TenMonHoc,
      SoTinChi: b.SoTinChi !== undefined && b.SoTinChi !== '' ? Number(b.SoTinChi) : undefined,
      SoTiet: b.SoTiet !== undefined && b.SoTiet !== '' ? Number(b.SoTiet) : undefined,
      MoTa: b.MoTa,
      TrangThai: b.TrangThai,
      LoaiMon: b.LoaiMon !== undefined && b.LoaiMon !== '' ? Number(b.LoaiMon) : undefined
    };
    this.courseService.updateSubject(body).subscribe({
      next: () => {
        this.updating = false;
        this.isEditVisible = false;
        this.message.success('Cập nhật môn học thành công');
        this.loadSubjects();
      },
      error: (err) => {
        this.updating = false;
        this.message.error(err?.error?.Message || err?.message || 'Cập nhật môn học thất bại');
      }
    });
  }

  setStatus(s: any, active: boolean): void {
    const ma = s?.maMonHoc || s?.MaMonHoc; if (!ma) return;
    const prev = s.trangThai;
    s.trangThai = active;
    this.courseService.updateSubject({ MaMonHoc: ma, TrangThai: active }).subscribe({
      next: () => {
        this.message.success(active ? 'Đã kích hoạt môn học' : 'Đã vô hiệu hóa môn học');
        if (active === false) {
          this.cascadeDisableClasses(ma);
        }
      },
      error: (err) => {
        s.trangThai = prev;
        this.message.error(err?.error?.Message || err?.message || 'Cập nhật trạng thái thất bại');
      }
    });
  }

  private cascadeDisableClasses(maMonHoc: string): void {
    if (!maMonHoc) return;
    this.message.info('Đang vô hiệu hóa các lớp thuộc môn học...');
    this.courseService.getCourses({ maMonHoc: maMonHoc, page: 1, pageSize: 1000 }).subscribe({
      next: (res: any) => {
        const items = res?.data?.items || res?.items || res || [];
        const updates = (items || []).map((it: any) => {
          const maLop = it?.maLopHocPhan || it?.MaLopHocPhan || it?.lopHocPhan?.maLopHocPhan || it?.maLop || null;
          if (!maLop) return of(null);
          return this.courseService.updateCourse({ MaLopHocPhan: maLop, TrangThai: false }).pipe(catchError(err => of(null)));
        });
        if (!updates.length) {
          this.message.info('Không tìm thấy lớp học phần để vô hiệu hóa.');
          return;
        }
        forkJoin(updates).subscribe({ next: () => { this.message.success('Đã vô hiệu hóa các lớp thuộc môn học'); this.loadSubjects(); }, error: () => { this.message.warning('Một số lớp không thể vô hiệu hóa'); this.loadSubjects(); } });
      }, error: (err) => {
        console.error('Lỗi khi lấy lớp thuộc môn:', err);
        this.message.warning('Không thể lấy danh sách lớp để vô hiệu hóa.');
      }
    });
  }

  confirmDeactivate(s: any): void {
    const label = s.tenMonHoc || s.TenMonHoc || s.maMonHoc || '';
    this.modal.confirm({
      nzTitle: `Bạn có chắc muốn vô hiệu hóa môn học '${label}'?`,
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => this.setStatus(s, false),
      nzCancelText: 'Hủy'
    });
  }

  confirmActivate(s: any): void { this.setStatus(s, true); }

  // deleteUI(s: any): void {
  //   const ma = s?.maMonHoc || s?.MaMonHoc; if (!ma) return;
  //   this.modal.confirm({
  //     nzTitle: 'Xác nhận',
  //     nzContent: 'Bạn có chắc chắn muốn vô hiệu hóa mục này khỏi danh sách?',
  //     nzOkText: 'Xóa',
  //     nzOkDanger: true,
  //     nzOnOk: () => {
  //       this.uiDeleted.add(ma);
  //       this.subjects = this.subjects.filter(x => x.maMonHoc !== ma);
  //       this.message.success('Đã xóa khỏi danh sách.');
  //     },
  //     nzCancelText: 'Hủy'
  //   });
  // }

  navigateToCourses(s: any): void {
    const ma = s?.maMonHoc || s?.MaMonHoc; if (!ma) return;
    this.router.navigate(['/admin/classes'], { queryParams: { MaMonHoc: ma } });
  }

  ngAfterViewInit(): void {
  }

  public getRowKey(row: any): string {
    return String(row?.maMonHoc || row?.MaMonHoc || (row?.tenMonHoc || '') + '::' + (row?.moTa || ''));
  }

  public getLoaiLabel(loai: any): string {
    const n = Number(loai);
    if (n === 1) return 'Lý thuyết';
    if (n === 2) return 'Thực hành';
    if (n === 3) return 'Lý thuyết & Thực hành';
    return '-';
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
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} môn học đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các môn học đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(k => {
          const ma = k.includes('::') ? undefined : k;
          if (!ma) { remaining--; if (remaining === 0) this.loadSubjects(); return; }
          this.courseService.updateSubject({ MaMonHoc: ma, TrangThai: false }).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(k);
              if (remaining === 0) {
                this.message.success('Đã vô hiệu hóa các môn đã chọn');
                this.loadSubjects();
              }
            },
            error: () => { remaining--; if (remaining === 0) this.loadSubjects(); }
          });
        });
      }
    ,
      nzCancelText: 'Hủy'
    });
  }
}
