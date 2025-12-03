import { Component, OnInit } from '@angular/core';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Router } from '@angular/router';
import { CourseService } from '../../core/services/course.service';

@Component({
  selector: 'app-semesters',
  templateUrl: './semesters.component.html',
  styleUrls: ['./semesters.component.scss']
})
export class SemestersComponent implements OnInit {
  semesters: any[] = [];
  loading = false;
  page = 1;
  pageSize = 10;
  total = 0;

  keyword = '';
  filterNamHoc: string = '';
  filterKy: string = '';

  isCreateVisible = false;
  isEditVisible = false;
  creating = false;
  updating = false;
  newSemester: any = { NamHoc: '', Ky: '' };
  selectedSemester: any = {};

  constructor(private course: CourseService, private msg: NzMessageService, private modal: NzModalService, private router: Router) {}

  ngOnInit(): void { this.load(); }

  private buildParams() {
    return {
      Page: this.page,
      PageSize: this.pageSize,
      NamHoc: this.filterNamHoc || undefined,
      Ky: this.filterKy || undefined
    } as any;
  }

  load(): void {
    this.loading = true;
    this.course.getSemesters(this.buildParams()).subscribe({
      next: res => {
        const items = res?.data?.items || res?.data || [];
        this.semesters = items;
        this.total = res?.data?.total || items.length || 0;
        this.loading = false;
      },
      error: () => { this.loading = false; this.msg.error('Lỗi tải học kỳ'); }
    });
  }

  onPageChange(p: number) { this.page = p; this.load(); }

  onSearch(): void {
    const t = (this.keyword || '').trim();
    if (/^\d{4}$/.test(t)) { this.filterNamHoc = t; this.filterKy = ''; }
    else if (/^\d+$/.test(t)) { this.filterKy = t; this.filterNamHoc = ''; }
    else { this.filterNamHoc = ''; this.filterKy = ''; }
    this.page = 1; this.load();
  }
  refreshList(): void {
    this.keyword = '';
    this.filterNamHoc = '';
    this.filterKy = '';
    this.page = 1; this.load();
  }

  onApplyAdvanced(): void { this.page = 1; this.load(); }

  // create
  openCreate(): void { this.newSemester = { NamHoc: '', Ky: '' }; this.isCreateVisible = true; }
  create(): void {
    const b = this.newSemester;
    const nam = Number(b?.NamHoc); const ky = Number(b?.Ky);
    if (!nam || !ky) { this.msg.warning('Nhập đầy đủ Năm học và Kỳ'); return; }
    this.creating = true;
    this.course.createSemester({ NamHoc: nam, Ky: ky }).subscribe({
      next: () => { this.creating = false; this.isCreateVisible = false; this.msg.success('Tạo học kỳ thành công'); this.load(); },
      error: () => { this.creating = false; this.msg.error('Tạo học kỳ thất bại'); }
    });
  }

  openEdit(row: any): void { this.selectedSemester = { MaHocKy: row.maHocKy, NamHoc: row.namHoc, Ky: row.ky }; this.isEditVisible = true; }
  update(): void {
    const b = this.selectedSemester;
    if (!b?.MaHocKy) { this.msg.warning('Thiếu MaHocKy'); return; }
    this.updating = true;
    const body: any = { MaHocKy: Number(b.MaHocKy) };
    if (b.NamHoc) body.NamHoc = Number(b.NamHoc);
    if (b.Ky) body.Ky = Number(b.Ky);
    this.course.updateSemester(body).subscribe({
      next: () => { this.updating = false; this.isEditVisible = false; this.msg.success('Cập nhật học kỳ thành công'); this.load(); },
      error: () => { this.updating = false; this.msg.error('Cập nhật học kỳ thất bại'); }
    });
  }

  cancel(): void { this.isCreateVisible = false; this.isEditVisible = false; }

  navigateToCourses(row: any): void {
    const ky = row?.ky || row?.Ky;
    const nam = row?.namHoc || row?.NamHoc;
    const queryParams: any = {};
    if (ky) queryParams.Ky = ky;
    if (nam) queryParams.NamHoc = nam;
    this.router.navigate(['/admin/classes'], { queryParams });
  }
}
