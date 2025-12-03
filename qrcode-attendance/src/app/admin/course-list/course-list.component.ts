import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CourseService } from '../../core/services/course.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { SubjectService } from '../../core/services/subject.service';
import { LecturerService } from '../../core/services/lecturer.service';
import { StudentService } from '../../core/services/student.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Observable, lastValueFrom } from 'rxjs';
import * as XLSX from 'xlsx';
import { map } from 'rxjs/operators';

interface Course {
  maMonHoc?: string;
  tenMonHoc?: string;
  maLopHocPhan?: string;
  tenLopHocPhan?: string;
  maLop?: string;
  tenLop?: string;
  soTinChi?: number;
  soTiet?: number;
  maGiangVien?: string;
  tenGiangVien?: string;
  hocKy?: number | string;
  namHoc?: number | string;
  trangThai?: any;
  NamHoc?: number | string;
  maHocKy?: string | number;
  MaHocKy?: string | number;
  HocKy?: number | string;
  Ky?: number | string;
  ky?: number | string;
}

@Component({
  selector: 'app-course-list',
  templateUrl: './course-list.component.html',
  styleUrls: ['./course-list.component.scss']
})
export class CourseListComponent implements OnInit {
  courses: Course[] = [];
  loading = false;
  page = 1;
  pageSize = 20;
  total = 0;

  filterMaLopHocPhan = '';
  filterTenLopHocPhan = '';
  filterTrangThai: any = '';
  filterMaMonHoc = '';
  filterTenMonHoc = '';
  filterMaGiangVien = '';
  filterTenGiangVien = '';
  filterHocKy = '';
  filterNamHoc = '';
  filterSoTinChi = '';
  filterSoTiet = '';


  isCreateCourseVisible = false;
  isEditCourseVisible = false;
  isAddStudentVisible = false;
  creating = false;
  updating = false;
  adding = false;
  selectedCourse: any = {};
  newCourse: any = { TrangThai: false };
  addStudentModel: any = { MaLopHocPhan: '', MaSinhVien: '', NgayThamGia: '', TrangThai: false };

  isImportVisible = false;
  importCourseCode: string = '';
  importFileName: string = '';
  importStudents: Array<{ code: string; status?: 'pending' | 'success' | 'error'; message?: string }> = [];
  importFile?: File | null = null;
  importing = false;

  isDetailVisible = false;
  detailCourseCode: string = '';
  detailStudents: any[] = [];
  detailLoading = false;
  removingStudents: Set<string> = new Set<string>();

  subjects: any[] = [];
  lecturers: any[] = [];
  semesters: any[] = [];
  hocKyOptions: any[] = [
    { value: '1', label: 'HK1' },
    { value: '2', label: 'HK2' },
    { value: '3', label: 'HK3' }
  ];
  years: any[] = [];
  // courseOptions: any[] = [];
  existingCourseForNew: any | null = null;

  expandedRows: Set<string> = new Set<string>();

  sortBy: string = 'TenMonHoc';
  sortDir: 'ASC' | 'DESC' = 'ASC';

  showAdvanced = false;
  searchTerm = '';

  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  constructor(
    private courseService: CourseService,
    private scheduleService: ScheduleService,
    private message: NzMessageService,
    private modal: NzModalService,
    private subjectService: SubjectService,
    private lecturerService: LecturerService,
    private studentService: StudentService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const qp: any = this.route.snapshot.queryParams;
    if (qp) {
      if (qp['MaMonHoc'] || qp['maMonHoc']) this.filterMaMonHoc = qp['MaMonHoc'] || qp['maMonHoc'];
      if (qp['MaGiangVien'] || qp['maGiangVien']) this.filterMaGiangVien = qp['MaGiangVien'] || qp['maGiangVien'];
      if (qp['MaLopHocPhan'] || qp['maLopHocPhan']) this.filterMaLopHocPhan = qp['MaLopHocPhan'] || qp['maLopHocPhan'];
      if (qp['Ky']) this.filterHocKy = qp['Ky'];
      if (qp['NamHoc']) this.filterNamHoc = qp['NamHoc'];
    }
    this.loadSemestersForSelect();
    // this.loadSubjectsForSelect();
    // this.loadLecturersForSelect();
    this.loadCourses();
  }

  loadSemestersForSelect(): void {
    this.courseService.getSemesters({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      const mapped = (items || []).map((h: any) => {
        const hk = h?.hocKy || h?.HocKy || h?.hoc_ky || h || {};
        const val = hk?.maHocKy || hk?.MaHocKy || hk?.maHocKy || hk?.ky || hk?.Ky || hk;
        const nam = hk?.namHoc || hk?.NamHoc || hk?.nam || hk?.Nam;
        const ky = hk?.ky || hk?.Ky || hk?.hocKy || hk?.HocKy || val;
        const label = nam ? `${nam} - HK ${ky || val}` : `HK ${ky || val}`;
        return { value: val, label, nam };
      });
      this.semesters = mapped.map((m: any) => ({ value: m.value, label: m.label, ky: m.ky, nam: m.nam }));
      const yearsMap: any = {};
      mapped.forEach((m: any) => {
        const y = m.nam;
        if (y !== undefined && y !== null && String(y).toString().trim() !== '') {
          yearsMap[String(y)] = { value: String(y), label: String(y) };
        }
      });
      this.years = Object.values(yearsMap).sort((a: any, b: any) => Number(b.value) - Number(a.value));
    }, error: () => { } });
  }

  loadSubjectsForSelect(): void {
    this.subjectService.getSubjects({ Page: 1, PageSize: 200, SortBy: 'TenMonHoc', SortDir: 'DESC' }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.subjects = items.map((s: any) => {
        const mon = s?.monHoc || s?.MonHoc || s?.mon || s || {};
        const code = mon?.maMonHoc || mon?.MaMonHoc || mon?.maMon || s?.maMonHoc || s?.MaMonHoc || undefined;
        const name = mon?.tenMonHoc || mon?.TenMonHoc || mon?.ten || s?.tenMonHoc || s?.TenMonHoc || undefined;
        return { value: code, label: code && name ? `${code} - ${name}` : (name || code) };
      });
    }, error: () => { /* ignore */ } });
  }

  loadLecturersForSelect(): void {
    this.lecturerService.getLecturers({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.lecturers = items.map((e: any) => {
        const gv = e?.giangVien || e?.giang_vien || e?.GiangVien || e || {};
        const user = e?.nguoiDung || e?.nguoi_dung || e?.user || {};
        const code = gv?.maGiangVien || gv?.MaGiangVien || user?.tenDangNhap || user?.username || undefined;
        const name = user?.hoTen || user?.HoTen || gv?.hoTen || gv?.tenGiangVien || user?.name || undefined;
        return { value: code, label: code && name ? `${code} - ${name}` : (name || code) };
      });
    }, error: () => { } });
  }

  loadCourses(): void {
    this.loading = true;
    this.courseService.getCourses({
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDir: this.sortDir,
      maLopHocPhan: this.filterMaLopHocPhan,
      tenLopHocPhan: this.filterTenLopHocPhan,
      trangThai: this.filterTrangThai,
      maMonHoc: this.filterMaMonHoc,
      tenMonHoc: this.filterTenMonHoc,
      soTinChi: this.filterSoTinChi ? Number(this.filterSoTinChi) : undefined,
      soTiet: this.filterSoTiet ? Number(this.filterSoTiet) : undefined,
      maGiangVien: this.filterMaGiangVien,
      tenGiangVien: this.filterTenGiangVien,
      hocKy: this.filterHocKy ? Number(this.filterHocKy) : undefined,
      namHoc: this.filterNamHoc || undefined,

    }).subscribe({
      next: (res) => {
        const raw = Array.isArray(res) ? res : (res?.data?.items || res?.data || []);
        const totalFromRes = Array.isArray(res) ? raw.length : (res?.data?.totalRecords || res?.data?.total || raw.length);
        let list = this.normalizeCourseItems(raw);

        const hasValue = (v: any) => {
          if (v === undefined || v === null) return false;
          if (typeof v === 'boolean') return true;
          return String(v).trim() !== '';
        };
        const hasFilters = hasValue(this.filterMaLopHocPhan) || hasValue(this.filterTenLopHocPhan) || hasValue(this.filterTrangThai) || hasValue(this.filterMaMonHoc) || hasValue(this.filterTenMonHoc) || hasValue(this.filterMaGiangVien) || hasValue(this.filterTenGiangVien) || hasValue(this.filterHocKy) || hasValue(this.filterNamHoc);
        if (hasFilters) {
          list = this.clientFilterCourses(list);
        }
        this.courses = list;
        this.total = totalFromRes || this.courses.length || 0;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.message.error(err.message || 'Lỗi tải danh sách môn học');
      }
    });
  }

  private clientFilterCourses(items: Course[]): Course[] {
    const mk = (s?: string) => (s || '').toString().toLowerCase();
    return (items || []).filter(it => {
      try {
        if (this.filterTenMonHoc) {
          if (!(it.tenMonHoc && mk(it.tenMonHoc).includes(mk(this.filterTenMonHoc)))) return false;
        }
        if (this.filterMaMonHoc) {
          if (!(it.maMonHoc && mk(it.maMonHoc).includes(mk(this.filterMaMonHoc)))) return false;
        }
        if (this.filterMaLopHocPhan) {
          if (!(it.maLopHocPhan && mk(it.maLopHocPhan).includes(mk(this.filterMaLopHocPhan)))) return false;
        }
        if (this.filterTenLopHocPhan) {
          if (!(it.tenLopHocPhan && mk(it.tenLopHocPhan).includes(mk(this.filterTenLopHocPhan)))) return false;
        }
        if (this.filterMaGiangVien) {
          if (!(it.maGiangVien && mk(it.maGiangVien).includes(mk(this.filterMaGiangVien)))) return false;
        }
        if (this.filterHocKy) {
          const wanted = String(this.filterHocKy).toLowerCase().trim();
          const hkValue = String((it.ky ?? it.hocKy ?? it.HocKy ?? it.maHocKy ?? it.MaHocKy ?? '') || '').toLowerCase().trim();
          if (!hkValue) return false;
          if (hkValue !== wanted && !hkValue.includes(wanted)) return false;
        }
        if (this.filterNamHoc) {
          const wantedY = String(this.filterNamHoc).toLowerCase().trim();
          const yValue = String((it.namHoc ?? it.NamHoc ?? '') || '').toLowerCase().trim();
          if (!yValue) return false;
          if (yValue !== wantedY && !yValue.includes(wantedY)) return false;
        }
        if (this.filterTenGiangVien) {
          if (!(it.tenGiangVien && mk(it.tenGiangVien).includes(mk(this.filterTenGiangVien)))) return false;
        }

        if (this.filterTrangThai !== undefined && this.filterTrangThai !== null && String(this.filterTrangThai).toString().trim() !== '') {
          const want = String(this.filterTrangThai).toLowerCase();
          const val = String(it.trangThai === undefined || it.trangThai === null ? '' : it.trangThai).toLowerCase();
          if (want === 'true' || want === '1' || want === 'hoạt') {
            if (!(val === 'true' || val === '1' || val === 'true')) return false;
          } else if (want === 'false' || want === '0' || want.includes('ngừng')) {
            if (!(val === 'false' || val === '0')) return false;
          }
        }
        return true;
      } catch (e) { return false; }
    });
  }

  private normalizeCourseItems(raw: any[]): Course[] {
    const coerceBool = (v: any) => {
      if (v === undefined || v === null) return undefined;
      if (typeof v === 'boolean') return v;
      if (typeof v === 'number') return v === 1;
      const s = String(v).toLowerCase();
      return s === 'true' || s === '1';
    };
    return (raw || []).map((it: any) => {
      const lop = it?.lopHocPhan || it?.lop || it?.lop_hoc_phan || {};
      const mon = it?.monHoc || it?.monHocHoc || it?.mon || {};
      const gv = it?.giangVien || it?.giang_vien || it?.giangVienInfo || it?.giangVienInfo || it?.giang_vien_info || {};
      const hk = it?.hocKy || it?.hocKyInfo || it?.hoc_ky || {};
      return {
        maLopHocPhan: lop.maLopHocPhan || lop.MaLopHocPhan || it.maLopHocPhan || it.maLop || undefined,
        tenLopHocPhan: lop.tenLopHocPhan || lop.TenLopHocPhan || it.tenLopHocPhan || it.tenLop || undefined,
        maMonHoc: mon.maMonHoc || mon.MaMonHoc || it.maMonHoc || undefined,
        tenMonHoc: mon.tenMonHoc || mon.TenMonHoc || it.tenMonHoc || undefined,
        soTinChi: mon.soTinChi ?? mon.SoTinChi ?? it.soTinChi ?? it.SoTinChi,
        soTiet: mon.soTiet ?? mon.SoTiet ?? it.soTiet ?? it.SoTiet,
        maGiangVien: gv.maGiangVien || gv.MaGiangVien || gv.maGiangVien || it.maGiangVien || undefined,
        tenGiangVien: gv.hoTen || gv.HoTen || gv.tenGiangVien || (gv?.hoTen && String(gv.hoTen)) || it.tenGiangVien || it.giangVienInfo?.hoTen || it.giangVienInfo?.HoTen || undefined,
        namHoc: hk.namHoc || hk.NamHoc || it.namHoc || it.NamHoc,
        NamHoc: hk.namHoc || hk.NamHoc || it.namHoc || it.NamHoc,
        hocKy: hk.hocKy || hk.HocKy || hk.ky || it.hocKy || it.HocKy || it.ky,
        HocKy: hk.hocKy || hk.HocKy || hk.ky || it.hocKy || it.HocKy || it.ky,
        maHocKy: hk.maHocKy || hk.MaHocKy || hk.maHocKy || it.maHocKy || it.MaHocKy,
        MaHocKy: hk.maHocKy || hk.MaHocKy || hk.maHocKy || it.maHocKy || it.MaHocKy,
        ky: hk.ky || hk.Ky || hk.hocKy || hk.HocKy || it.ky || it.Ky,
        Ky: hk.ky || hk.Ky || hk.hocKy || hk.HocKy || it.ky || it.Ky,
        trangThai: coerceBool(lop.trangThai ?? mon.trangThai ?? it.trangThai)
      } as Course;
    });
  }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortBy = column;
      this.sortDir = 'ASC';
    }
    this.loadCourses();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadCourses();
  }

  onFilterChange(): void {
    this.page = 1;
    this.loadCourses();
  }


  applySearchTerm(): void {
    const t = (this.searchTerm || '').trim();
    this.clearAllFiltersOnly();
    if (!t) { this.onFilterChange(); return; }
    this.loading = true;

    const baseParams: any = {
      page: 1,
      pageSize: 100,
      sortBy: this.sortBy,
      sortDir: this.sortDir
    };

    const tryOrder = ['TenMonHoc', 'MaMonHoc'];
    const variantsMap: Record<string, string[]> = {
      MaMonHoc: ['MaMonHoc', 'maMonHoc', 'MaMH', 'maMH'],
      TenMonHoc: ['TenMonHoc', 'tenMonHoc', 'TenMH', 'tenMH', 'TenMon']
    };

    const run = (i: number = 0, v: number = 0) => {
      const kw = t;
      const params: any = { ...baseParams };
      this.filterTenMonHoc = '';
      if (kw) {
        const field = tryOrder[i];
        const variants = variantsMap[field] || [field];
        params[variants[v]] = kw;
      }

      this.courseService.getCourses(params).subscribe({
        next: (res: any) => {
          const raw = Array.isArray(res) ? res : (res?.data?.items || res?.data || []);
          const totalFromRes = Array.isArray(res) ? raw.length : (res?.data?.totalRecords || res?.data?.total || raw.length || 0);
          this.page = 1;
          this.filterTenMonHoc = kw;
          let list = this.normalizeCourseItems(raw || []);
          list = this.clientFilterCourses(list);
          this.courses = list;
          this.total = (Number(totalFromRes) && Number(totalFromRes) === list.length) ? Number(totalFromRes) : list.length;
          if (kw && list.length === 0) {
            const field = tryOrder[i];
            const variants = variantsMap[field] || [field];
            if (v + 1 < variants.length) return run(i, v + 1);
            if (i + 1 < tryOrder.length) return run(i + 1, 0);
          }
          this.loading = false;
        },
        error: (err: any) => {
          if (t) {
            const field = tryOrder[i];
            const variants = variantsMap[field] || [field];
            if (v + 1 < variants.length) return run(i, v + 1);
            if (i + 1 < tryOrder.length) return run(i + 1, 0);
          }
          this.loading = false;
          this.courses = [];
          this.total = 0;
          this.message.error(err?.message || 'Lỗi tìm kiếm');
        }
      });
    };

    run(0, 0);
  }

  private mapFilterKey(k: string): string {
    const map: any = {
      maLopHocPhan: 'filterMaLopHocPhan',
      tenLopHocPhan: 'filterTenLopHocPhan',
      maMonHoc: 'filterMaMonHoc',
      tenMonHoc: 'filterTenMonHoc',
      maGiangVien: 'filterMaGiangVien',
      tenGiangVien: 'filterTenGiangVien',

    };
    return map[k] || k;
  }

  private clearAllFiltersOnly(): void {
    this.filterMaLopHocPhan = '';
    this.filterTenLopHocPhan = '';
    this.filterTrangThai = '';
    this.filterMaMonHoc = '';
    this.filterTenMonHoc = '';
    this.filterMaGiangVien = '';
    this.filterTenGiangVien = '';
    this.filterHocKy = '';
    this.filterNamHoc = '';
    this.filterSoTinChi = '';
    this.filterSoTiet = '';

  }

  openCreateCourse(): void {
    this.newCourse = { TrangThai: false };
    this.existingCourseForNew = null;
    this.loadSubjectsForSelect();
    this.loadLecturersForSelect();
    this.loadSemestersForSelect();
    this.isCreateCourseVisible = true;
  }

  createCourse(): void {
    if (!this.newCourse?.TenLopHocPhan || !this.newCourse?.MaMonHoc || !this.newCourse?.MaGiangVien || !this.newCourse?.MaHocKy) {
      this.message.warning('Vui lòng nhập đầy đủ: Tên lớp học phần, Mã môn học, Mã giảng viên và Mã học kỳ');
      return;
    }
    this.creating = true;
    const body: any = {
      TenLopHocPhan: this.newCourse.TenLopHocPhan,
      MaMonHoc: this.newCourse.MaMonHoc,
      MaGiangVien: this.newCourse.MaGiangVien,
      MaHocKy: this.newCourse.MaHocKy,
      TrangThai: this.newCourse.TrangThai
    };
    this.courseService.createCourse(body as any).subscribe({
      next: () => {
        this.creating = false;
        this.isCreateCourseVisible = false;
        this.message.success('Tạo lớp học phần thành công');
        this.loadCourses();
      },
      error: (err) => { this.creating = false; this.message.error(err?.message || 'Tạo lớp học phần thất bại'); }
    });
  }

  openEditCourse(row: any): void {
    this.selectedCourse = {
      MaLopHocPhan: row.maLopHocPhan || row.maLop,
      TenLopHocPhan: row.tenLopHocPhan || row.tenLop,
      TrangThai: row.trangThai,
      MaMonHoc: row.maMonHoc,
      MaGiangVien: row.maGiangVien,
      MaHocKy: row.maHocKy || row.hocKy
    };
    this.loadSubjectsForSelect();
    this.loadLecturersForSelect();
    this.loadSemestersForSelect();
    this.isEditCourseVisible = true;
  }

  // loadCourseCodesForSelect(): void {
  //   // fetch some courses and extract unique MaLopHocPhan
  //   this.courseService.getCourses({ page: 1, pageSize: 500 }).subscribe({ next: (res: any) => {
  //     const items = res?.data?.items || res?.data || [];
  //     const map: any = {};
  //     items.forEach((c: any) => {
  //       const code = c.maLopHocPhan || c.MaLopHocPhan || c.maLop || c.MaLop;
  //       const label = c.tenLopHocPhan || c.TenLopHocPhan || c.tenLop || c.TenLop;
  //       if (code && !map[code]) map[code] = { value: code, label: label || code };
  //     });
  //     this.courseOptions = Object.values(map);
  //   }, error: () => { /* ignore */ } });
  // }

  private checkCourseExists(maLopHocPhan: string): Observable<any | null> {
    return this.courseService
      .getCourses({ page: 1, pageSize: 1, maLopHocPhan })
      .pipe(map((res: any) => {
        const items = res?.data?.items || [];
        return items?.length ? items[0] : null;
      }));
  }

  // onNewCourseCodeChange(code: string): void {
  //   if (!code) { this.existingCourseForNew = null; return; }
  //   this.checkCourseExists(code).subscribe({
  //     next: (found) => {
  //       this.existingCourseForNew = found;
  //     },
  //     error: () => { this.existingCourseForNew = null; }
  //   });
  // }

  updateCourse(): void {
    if (!this.selectedCourse?.MaLopHocPhan) { this.message.warning('Thiếu MaLopHocPhan'); return; }
    this.updating = true;
    this.courseService.updateCourse(this.selectedCourse).subscribe({
      next: () => {
        this.updating = false;
        this.isEditCourseVisible = false;
        this.message.success('Cập nhật lớp học phần thành công');
        this.loadCourses();
      },
      error: () => { this.updating = false; this.message.error('Cập nhật lớp học phần thất bại'); }
    });
  }

  openAddStudent(row: any): void {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const todayStr = `${yyyy}-${mm}-${dd}`;
    this.addStudentModel = { MaLopHocPhan: row.maLopHocPhan || row.maLop, MaSinhVien: '', NgayThamGia: todayStr, TrangThai: false };
    this.isAddStudentVisible = true;
  }

  addStudent(): void {
    if (!this.addStudentModel?.MaLopHocPhan || !this.addStudentModel?.MaSinhVien) { this.message.warning('Nhập MaLopHocPhan và MaSinhVien'); return; }
    this.adding = true;
    if (!this.addStudentModel.NgayThamGia) {
      const d = new Date();
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      this.addStudentModel.NgayThamGia = `${yyyy}-${mm}-${dd}`;
    }
    this.studentService.addToCourse(this.addStudentModel).subscribe({
      next: () => {
        this.adding = false;
        this.isAddStudentVisible = false;
        this.message.success('Thêm sinh viên vào lớp thành công');
        try {
          if (this.isDetailVisible && this.detailCourseCode === this.addStudentModel.MaLopHocPhan) {
            const code = String(this.addStudentModel.MaSinhVien || '').trim();
            if (code) {
              const exists = this.detailStudents.some(s => String(s.maSinhVien || '').trim() === code);
              if (!exists) {
                this.detailStudents = [{ maSinhVien: code, hoTen: undefined }, ...this.detailStudents];
              }
            }
            this.loadDetailStudents(this.detailCourseCode);
          }
        } catch (e) { }
      },
      error: (err) => {
        if (err?.status === 404) {
          this.checkCourseExists(this.addStudentModel.MaLopHocPhan).subscribe({
            next: (course) => {
              if (!course) {
                this.message.error('Không tìm thấy Lớp học phần (MaLopHocPhan)');
                this.adding = false; return;
              }
              this.studentService.getProfile(this.addStudentModel.MaSinhVien).subscribe({
                next: () => { this.message.error('Không thể thêm. Vui lòng kiểm tra quyền hoặc dữ liệu đầu vào.'); this.adding = false; },
                error: (e2) => { if (e2?.status === 404) this.message.error('Không tìm thấy Sinh viên (MaSinhVien)'); else this.message.error('Không thể xác minh Sinh viên'); this.adding = false; }
              });
            },
            error: () => { this.adding = false; this.message.error(err?.message || 'Thêm sinh viên thất bại'); }
          });
        } else {
          this.adding = false; this.message.error(err?.message || 'Thêm sinh viên thất bại');
        }
      }
    });
  }

  openImportStudents(row: any): void {
    this.importCourseCode = row.maLopHocPhan || row.maLop || '';
    this.importFileName = '';
    this.importStudents = [];
    this.importing = false;
    this.isImportVisible = true;
  }

  openDetail(row: any): void {
    const ma = row.maLopHocPhan || row.maLop || '';
    if (!ma) { this.message.warning('Không có Mã lớp học phần'); return; }
    this.detailCourseCode = ma;
    this.detailStudents = [];
    this.isDetailVisible = true;
    this.loadDetailStudents(ma);
  }

  loadDetailStudents(maLopHocPhan: string): void {
    this.detailLoading = true;
    this.studentService.getStudents({ MaLopHocPhan: maLopHocPhan, Page: 1, PageSize: 500 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.detailStudents = (items || []).map((s: any) => {
        const base = s?.sinhVien || s?.sinh_vien || s?.student || s?.studentInfo || s?.nguoi || s || {};
        const userNested = base?.nguoiDung || base?.user || s?.nguoiDung || s?.user || {};
        const ma = base?.MaSinhVien || base?.maSinhVien || base?.MaSV || base?.maSV || base?.Ma || base?.ma || s?.MaSinhVien || s?.maSinhVien;
        const name = base?.HoTen || base?.hoTen || base?.Ten || base?.ten || base?.name || userNested?.hoTen || userNested?.HoTen || userNested?.name || s?.HoTen || s?.hoTen;
        return {
          maSinhVien: ma || undefined,
          hoTen: name || undefined
        };
      });
      this.detailLoading = false;
    }, error: (err) => { this.detailLoading = false; this.message.error(err?.message || 'Lỗi tải danh sách sinh viên'); } });
  }

  confirmRemoveStudent(maSinhVien: string | undefined): void {
    if (!maSinhVien) { this.message.warning('Thiếu MaSinhVien'); return; }
    this.modal.confirm({
      nzTitle: 'Xác nhận',
      nzContent: `Bạn có chắc muốn xóa sinh viên ${maSinhVien} khỏi lớp ${this.detailCourseCode}?`,
      nzOkText: 'Xóa',
      nzOkDanger: true,
      nzOnOk: () => this.removeStudentFromCourse(String(maSinhVien)),
      nzCancelText: 'Hủy'
    });
  }

  removeStudentFromCourse(maSinhVien: string): void {
    if (!this.detailCourseCode) { this.message.error('Thiếu MaLopHocPhan'); return; }
    try {
      this.removingStudents.add(maSinhVien);
    } catch (e) {}
    this.studentService.removeFromCourse({ MaLopHocPhan: this.detailCourseCode, MaSinhVien: maSinhVien }).subscribe({
      next: () => {
        try { this.removingStudents.delete(maSinhVien); } catch (e) {}
        this.detailStudents = (this.detailStudents || []).filter(s => String(s.maSinhVien || '').trim() !== String(maSinhVien).trim());
        this.message.success(`Đã xóa ${maSinhVien} khỏi lớp`);
      },
      error: (err) => {
        try { this.removingStudents.delete(maSinhVien); } catch (e) {}
        this.message.error(err?.message || 'Xóa thất bại');
      }
    });
  }

  cancelImportModal(): void {
    this.isImportVisible = false;
    this.importing = false;
    this.importFileName = '';
    this.importStudents = [];
  }

  onImportFileChange(event: any): void {
    const file: File | undefined = event?.target?.files?.[0];
    this.importStudents = [];
    this.importFileName = '';
    this.importFile = undefined;
    if (!file) return;
    this.importFileName = file.name;
    this.importFile = file;

    const ext = (file.name.split('.').pop() || '').toLowerCase();
    if (ext === 'xlsx' || ext === 'xls') {
      const reader = new FileReader();
      reader.onload = () => {
        try {
          const data = new Uint8Array(reader.result as ArrayBuffer);
          const workbook = XLSX.read(data, { type: 'array' });
          const firstSheetName = workbook.SheetNames[0];
          const sheet = workbook.Sheets[firstSheetName];
          const rows: any[][] = XLSX.utils.sheet_to_json(sheet, { header: 1 });
          const codes = this.extractCodesFromRows(rows);
          const unique = Array.from(new Set(codes.filter(Boolean)));
          this.importStudents = unique.map(c => ({ code: c }));
          if (!this.importStudents.length) this.message.warning('Không tìm thấy mã sinh viên hợp lệ trong file Excel.');
        } catch (e) {
          this.message.error('Không thể đọc file Excel.');
        }
      };
      reader.onerror = () => { this.message.error('Không thể đọc file Excel.'); };
      reader.readAsArrayBuffer(file);
    } else {
      const reader = new FileReader();
      reader.onload = () => {
        const text = String(reader.result || '');
        const codes = this.parseImportText(text);
        const unique = Array.from(new Set(codes.filter(Boolean)));
        this.importStudents = unique.map(c => ({ code: c }));
        this.importFile = file;
        if (!this.importStudents.length) this.message.warning('Không tìm thấy mã sinh viên hợp lệ trong file.');
      };
      reader.onerror = () => { this.message.error('Không thể đọc file.'); };
      reader.readAsText(file, 'utf-8');
    }
  }

  private parseImportText(text: string): string[] {
    const lines = text.split(/\r?\n/).map(l => l.trim()).filter(l => !!l);
    const out: string[] = [];
    for (const line of lines) {
      if (!line) continue;
      const low = line.toLowerCase();
      if (low.includes('masinhvien') || low.includes('mã sinh viên')) continue;
      let token = line;
      if (line.includes(',')) token = line.split(',')[0];
      else if (line.includes(';')) token = line.split(';')[0];
      token = token.replace(/^\"|\"$/g, '').trim();
      out.push(token);
    }
    return out;
  }

  private extractCodesFromRows(rows: any[][]): string[] {
    if (!rows || !rows.length) return [];
    const header = (rows[0] || []).map((x: any) => String(x || '').trim().toLowerCase());
    let idx = 0;
    for (let i = 0; i < header.length; i++) {
      const h = header[i];
      if (h === 'masinhvien' || h.includes('mã sinh viên') || h.includes('ma sinh vien')) { idx = i; break; }
    }
    const out: string[] = [];
    for (let r = 1; r < rows.length; r++) {
      const row = rows[r] || [];
      let val = row[idx];
      if (val === undefined || val === null) continue;
      if (typeof val !== 'string') val = String(val);
      val = val.replace(/^\"|\"$/g, '').trim();
      if (val) out.push(val);
    }
    return out;
  }

  async startImport(): Promise<void> {
  if (!this.importCourseCode) { this.message.warning('Thiếu Mã lớp học phần'); return; }
  if (!this.importStudents?.length && !this.importFile) { this.message.warning('Chưa có dữ liệu import'); return; }
  this.importing = true;
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const todayStr = `${yyyy}-${mm}-${dd}`;

    if (this.importFile) {
      const fd = new FormData();
      fd.append('file', this.importFile, this.importFile.name);
      fd.append('MaLopHocPhan', this.importCourseCode);
      try {
        await lastValueFrom(this.studentService.addToCourseBulk(fd));
        this.importing = false;
        this.message.success('Import hàng loạt đã được gửi. Vui lòng kiểm tra kết quả.');
        this.isImportVisible = false;
        try {
          if (this.isDetailVisible && this.detailCourseCode === this.importCourseCode) {
            this.loadDetailStudents(this.detailCourseCode);
          }
        } catch (e) { /* ignore */ }
        return;
      } catch (bulkErr) {
        try { console.warn('Bulk import failed, falling back to per-item import', bulkErr); } catch (e) {}
      }
    }

    let ok = 0, fail = 0;
    for (const item of this.importStudents) {
      item.status = 'pending'; item.message = 'Đang thêm...';
      try {
        await lastValueFrom(this.studentService.addToCourse({ MaLopHocPhan: this.importCourseCode, MaSinhVien: item.code, NgayThamGia: todayStr, TrangThai: false }));
        item.status = 'success'; item.message = 'Đã thêm'; ok++;
        try {
          if (this.isDetailVisible && this.detailCourseCode === this.importCourseCode) {
            const code = String(item.code || '').trim();
            if (code) {
              const exists = this.detailStudents.some(s => String(s.maSinhVien || '').trim() === code);
              if (!exists) {
                this.detailStudents = [{ maSinhVien: code, hoTen: undefined }, ...this.detailStudents];
              }
            }
          }
        } catch (e) { /* ignore */ }
      } catch (err: any) {
        item.status = 'error';
        if (err?.status === 404) item.message = 'Không tìm thấy sinh viên';
        else if (err?.status === 400) item.message = err?.error?.Message || 'Không hợp lệ / có thể đã tồn tại';
        else item.message = err?.message || 'Lỗi không xác định';
        fail++;
      }
    }
    this.importing = false;
    this.message.info(`Import hoàn tất: thành công ${ok}, thất bại ${fail}`);
    try {
      if (this.isDetailVisible && this.detailCourseCode === this.importCourseCode) {
        this.loadDetailStudents(this.detailCourseCode);
      }
    } catch (e) { }
  }

  cancelModals(): void {
    this.isCreateCourseVisible = false;
    this.isEditCourseVisible = false;
    this.isAddStudentVisible = false;
  }

  resetFilters(): void {
    this.filterMaLopHocPhan = '';
    this.filterTenLopHocPhan = '';
    this.filterTrangThai = '';
    this.filterMaMonHoc = '';
    this.filterTenMonHoc = '';
    this.filterMaGiangVien = '';
    this.filterTenGiangVien = '';
    this.filterHocKy = '';
    this.filterNamHoc = '';
    this.filterSoTinChi = '';
    this.filterSoTiet = '';

    this.searchTerm = '';
    this.onFilterChange();
  }

  // Nút làm mới: reset toàn bộ filter + ô tìm kiếm, và tải lại danh sách
  refreshList(): void { this.resetFilters(); }

  goToScheduleForCourse(row: any): void {
    const ma = row?.maLopHocPhan || row?.maLop; if (!ma) return;
    this.router.navigate(['/admin/schedule/list'], { queryParams: { MaLopHocPhan: ma } });
  }

  toggleExpand(row: any): void {
    const key = String(row?.maLopHocPhan || row?.maLop || (row?.maMonHoc || '') + '::' + (row?.tenLopHocPhan || ''));
    if (this.expandedRows.has(key)) this.expandedRows.delete(key);
    else this.expandedRows.add(key);
  }

  isExpanded(row: any): boolean {
    const key = String(row?.maLopHocPhan || row?.maLop || (row?.maMonHoc || '') + '::' + (row?.tenLopHocPhan || ''));
    return this.expandedRows.has(key);
  }

  confirmAutoGenerateCourse(row: any): void {
    const ma = row?.maLopHocPhan || row?.maLop || '';
    if (!ma) { this.message.warning('Thiếu Mã lớp học phần'); return; }
    this.modal.confirm({
      nzTitle: `Sinh buổi học tự động cho lớp '${ma}'?`,
      nzContent: 'Hệ thống sẽ tự động tạo các buổi học theo lịch. Bạn có muốn tiếp tục?',
      nzOkText: 'Sinh buổi',
      nzOkDanger: false,
      nzOnOk: () => {
        this.scheduleService.autoGenerate({ MaLopHocPhan: ma }).subscribe({
          next: (res: any) => {
            this.message.success('Sinh buổi học tự động thành công');
            try { this.loadCourses(); } catch (e) {}
          },
          error: (err: any) => {
            this.message.error(err?.error?.Message || err?.message || 'Sinh buổi học thất bại');
          }
        });
      },
      nzCancelText: 'Hủy'
    });
  }

  confirmDisableCourse(row: any): void {
    const ma = row?.maLopHocPhan || row?.maLop || '';
    if (!ma) { this.message.warning('Thiếu Mã lớp học phần'); return; }
    this.modal.confirm({
      nzTitle: 'Xác nhận',
      nzContent: `Bạn có chắc muốn vô hiệu hóa lớp '${ma}'?`,
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        this.courseService.updateCourse({ MaLopHocPhan: ma, TrangThai: false }).subscribe({
          next: () => { this.message.success('Đã vô hiệu hóa lớp học phần'); try { this.loadCourses(); } catch (e) {} },
          error: (err) => { this.message.error(err?.message || 'Vô hiệu hóa thất bại'); }
        });
      },
      nzCancelText: 'Hủy'
    });
  }

  public getRowKey(row: any): string {
    return String(row?.maLopHocPhan || row?.maLop || (row?.maMonHoc || '') + '::' + (row?.tenLopHocPhan || ''));
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
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} lớp học phần đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các lớp học phần đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(k => {
          const ma = k.includes('::') ? undefined : k;
          if (!ma) { remaining--; if (remaining === 0) this.loadCourses(); return; }
          this.courseService.updateCourse({ MaLopHocPhan: ma, TrangThai: false }).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(k);
              if (remaining === 0) {
                this.message.success('Đã vô hiệu hóa các lớp đã chọn');
                this.loadCourses();
              }
            },
            error: () => { remaining--; if (remaining === 0) this.loadCourses(); }
          });
        });
      }
    ,
      nzCancelText: 'Hủy'
    });
  }
}
