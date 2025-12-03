import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CourseService } from '../../core/services/course.service';
import { AuthService } from '../../core/services/auth.service';

interface LopGiangDay {
  maLopHocPhan: string;
  tenLopHocPhan: string;
  maMonHoc?: string;
  tenMonHoc?: string;
  soSinhVien?: number;
  hocKy?: string | number;
  namHoc?: string | number;
  trangThai?: any;
}

@Component({
  selector: 'app-classes-giangday',
  templateUrl: './classes-giangday.component.html',
  styleUrls: ['./classes-giangday.component.scss']
})
export class ClassesGiangdayComponent implements OnInit {
  danhSachLop: LopGiangDay[] = [];
  keyword = '';
  hocKy?: number;
  namHoc?: number;
  loading = false;
  pageIndex = 1;
  pageSize = 20;
  sortBy = 'TenMonHoc';
  sortDir: 'ASC' | 'DESC' = 'DESC';
  trangThai: any = undefined;
  total = 0;
  listOfData: any[] = [];
  maLopHocPhan?: string;
  tenLopHocPhan?: string;

  search(): void {
    try { console.log('[DEBUG] ClassesGiangday: search() triggered', { keyword: this.keyword, hocKy: this.hocKy, namHoc: this.namHoc }); } catch (e) {}
    this.loadData(true);
  }

  refreshList(): void {
    try { console.log('[DEBUG] ClassesGiangday: refreshList() triggered'); } catch (e) {}
    this.keyword = '';
    this.hocKy = undefined;
    this.namHoc = undefined;
    this.loadData(false);
  }

  private _searchTimeout: any = null;
  onKeywordChange(v: string): void {
    this.keyword = v;
  }

  constructor(
    private courseService: CourseService,
    private auth: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadData(false);
  }

  loadData(manualSearch = false) {
    this.loading = true;
    const baseOpts: any = {
      page: this.pageIndex,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDir: this.sortDir,
      trangThai: this.trangThai,
    };

    if (this.maLopHocPhan && String(this.maLopHocPhan).trim()) {
      baseOpts.maLopHocPhan = String(this.maLopHocPhan).trim();
    }
    if (this.tenLopHocPhan && String(this.tenLopHocPhan).trim()) {
      baseOpts.tenLopHocPhan = String(this.tenLopHocPhan).trim();
    }

    if (this.hocKy !== undefined && this.hocKy !== null && String(this.hocKy).trim() !== '') {
      baseOpts.hocKy = String(this.hocKy);
    }
    if (this.namHoc !== undefined && this.namHoc !== null && String(this.namHoc).trim() !== '') {
      baseOpts.namHoc = String(this.namHoc);
    }

    if (!baseOpts.maLopHocPhan && !baseOpts.tenLopHocPhan && this.keyword && this.keyword.trim() !== '') {
      const kw = this.keyword.trim();
      if (!kw.includes(' ')) {
        baseOpts.maLopHocPhan = kw;
      } else {
        baseOpts.tenLopHocPhan = kw;
      }
    }

    const _u: any = this.auth.getUser ? (this.auth.getUser() || (this.auth as any).user || {}) : ((this.auth as any).user || {});
    const candidates = [
      _u?.nguoiDung?.tenDangNhap,
      _u?.tenDangNhap,
      _u?.username,
      _u?.maGiangVien,
      _u?.MaGiangVien,
    ].filter(x => !!x);

    if (manualSearch) {
      const opts: any = { ...baseOpts };
      if (candidates.length > 0) {
        opts.maGiangVien = candidates[0];
      }

      if (opts.hocKy !== undefined) {
        opts.Ky = String(opts.hocKy);
        delete opts.hocKy;
      }
      if (opts.namHoc !== undefined) {
        opts.NamHoc = String(opts.namHoc);
        delete opts.namHoc;
      }
      opts.exactKeys = true;
      console.log('[DEBUG] ClassesGiangday: manual search calling getCourses', { opts });
      this.courseService.getCourses(opts).subscribe((res: any) => {
        console.log('[DEBUG] ClassesGiangday: raw response', res);
        const items = (res?.data?.items ?? res?.items ?? res) || [];
        this.total = Number(res?.data?.totalRecords ?? items.length);
        this.danhSachLop = (items || []).map((x: any) => ({
          maLopHocPhan: x.lopHocPhan?.maLopHocPhan || x.maLopHocPhan || x.maLop,
          tenLopHocPhan: this.stripLeadingCode(x.lopHocPhan?.tenLopHocPhan || x.tenLopHocPhan || x.tenLop || x.tenMonHoc),
          maMonHoc: x.monHoc?.maMonHoc || x.maMonHoc,
          tenMonHoc: this.stripLeadingCode(x.monHoc?.tenMonHoc || x.tenMonHoc),
          soSinhVien: x.soSinhVien || x.lopHocPhan?.soSinhVien,
          hocKy: x.hocKy?.ky || x.hocKy?.maHocKy || x.hocKy || x.ky,
          namHoc: x.hocKy?.namHoc || x.namHoc,
          trangThai: x.lopHocPhan?.trangThai ?? x.trangThai ?? x.lopHocPhan?.TrangThai ?? x.TrangThai
        }));
        this.loading = false;

        if ((!items || items.length === 0) && this.keyword) {
          console.log('[DEBUG] ClassesGiangday: server returned empty on manual search, falling back to local filter');
          this.fetchAndFilterLocally();
        }
      }, err => {
        this.loading = false;
        console.error(err);
      });
      return;
    }

    const tryIndex = (idx: number) => {
      const opts = { ...baseOpts } as any;
      if (candidates[idx]) {
        opts.maGiangVien = candidates[idx];
      }

      console.log('[DEBUG] ClassesGiangday: calling getCourses', { idx, maGiangVien: opts.maGiangVien, opts });
      this.courseService.getCourses(opts).subscribe((res: any) => {
        console.log('[DEBUG] ClassesGiangday: raw response', res);
        const items = (res?.data?.items ?? res?.items ?? res) || [];
        this.total = Number(res?.data?.totalRecords ?? items.length);
        this.danhSachLop = (items || []).map((x: any) => ({
          maLopHocPhan: x.lopHocPhan?.maLopHocPhan || x.maLopHocPhan || x.maLop,
          tenLopHocPhan: this.stripLeadingCode(x.lopHocPhan?.tenLopHocPhan || x.tenLopHocPhan || x.tenLop || x.tenMonHoc),
          maMonHoc: x.monHoc?.maMonHoc || x.maMonHoc,
          tenMonHoc: this.stripLeadingCode(x.monHoc?.tenMonHoc || x.tenMonHoc),
          soSinhVien: x.soSinhVien || x.lopHocPhan?.soSinhVien,
          hocKy: x.hocKy?.ky || x.hocKy?.maHocKy || x.hocKy || x.ky,
          namHoc: x.hocKy?.namHoc || x.namHoc,
          trangThai: x.lopHocPhan?.trangThai ?? x.trangThai ?? x.lopHocPhan?.TrangThai ?? x.TrangThai
        }));
        this.loading = false;

        if ((!items || items.length === 0) && idx + 1 < candidates.length) {
          tryIndex(idx + 1);
        } else if ((!items || items.length === 0) && this.keyword) {
          console.log('[DEBUG] ClassesGiangday: server returned empty, falling back to local filter');
          this.fetchAndFilterLocally();
        }
      }, err => {
        this.loading = false;
        console.error(err);
      });
    };

    tryIndex(0);
  }

  private fetchAndFilterLocally(maGiangVien?: string): void {
    const opts: any = {
      page: 1,
      pageSize: 2000,
      sortBy: 'TenMonHoc',
      sortDir: 'DESC'
    };
    if (maGiangVien) opts.maGiangVien = maGiangVien;

    try { console.log('[DEBUG] ClassesGiangday: fetchAndFilterLocally calling getCourses', { opts }); } catch (e) {}
    this.courseService.getCourses(opts).subscribe(res => {
      try { console.log('[DEBUG] ClassesGiangday: fetchAndFilterLocally raw response', res); } catch (e) {}
      const raw = Array.isArray(res) ? res : (res?.data?.items || res?.data || []);
      const list = (raw || []).map((x: any) => ({
        maLopHocPhan: x.lopHocPhan?.maLopHocPhan || x.maLopHocPhan || x.maLop,
        tenLopHocPhan: this.stripLeadingCode(x.lopHocPhan?.tenLopHocPhan || x.tenLopHocPhan || x.tenLop || x.tenMonHoc),
        maMonHoc: x.monHoc?.maMonHoc || x.maMonHoc,
        tenMonHoc: this.stripLeadingCode(x.monHoc?.tenMonHoc || x.tenMonHoc),
        soSinhVien: x.soSinhVien || x.lopHocPhan?.soSinhVien,
        hocKy: x.hocKy?.ky || x.hocKy?.maHocKy || x.hocKy || x.ky,
        namHoc: x.hocKy?.namHoc || x.namHoc,
        trangThai: x.lopHocPhan?.trangThai ?? x.trangThai ?? x.lopHocPhan?.TrangThai ?? x.TrangThai
      }));

      const kw = String(this.keyword || '').trim().toLowerCase();
      if (!kw) {
        this.danhSachLop = list;
        return;
      }

      const filtered = list.filter((it: any) => {
        return (String(it.maLopHocPhan || '').toLowerCase().includes(kw)
          || String(it.tenLopHocPhan || '').toLowerCase().includes(kw)
          || String(it.maMonHoc || '').toLowerCase().includes(kw)
          || String(it.tenMonHoc || '').toLowerCase().includes(kw));
      });

      this.danhSachLop = filtered;
    }, () => { this.danhSachLop = []; });
  }

  private stripLeadingCode(v?: string): string {
    if (!v) return '';
    return String(v).replace(/^[A-Za-z0-9._-]+\s*[-–—:]\s*/, '').trim();
  }

  xemChiTiet(lop: LopGiangDay): void {
    if (this.isDisabled(lop)) return;
    this.router.navigate(['/giangvien/class-section-detail', lop.maLopHocPhan]);
  }

  private parseBooleanStatus(v: any): boolean {
    if (v === undefined || v === null) return true;
    if (typeof v === 'boolean') return v;
    const s = String(v).trim().toLowerCase();
    if (s === 'true' || s === '1' || s === 'yes') return true;
    if (s === 'false' || s === '0' || s === 'no') return false;
    return s.length > 0;
  }

  isDisabled(item: any): boolean {
    try {
      const candidates = [
        item?.lopHocPhan?.trangThai,
        item?.monHoc?.trangThai,
        item?.trangThai,
        item?.TrangThai
      ];
      for (const c of candidates) {
        if (c === undefined || c === null) continue;
        const parsed = this.parseBooleanStatus(c);
        if (parsed === false) return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }

  // taoQR(lop: LopGiangDay): void {
  //   this.router.navigate(['/giangvien/qr-create'], { queryParams: { classId: lop.maLopHocPhan } });
  // }

  // quanLyDiemDanh(lop: LopGiangDay): void {
  //   this.router.navigate(['/giangvien/attendance-manage'], { queryParams: { classId: lop.maLopHocPhan } });
  // }

  // xemLichDay(): void {
  //   this.router.navigate(['/giangvien/class/teacher-schedule']);
  // }
}
