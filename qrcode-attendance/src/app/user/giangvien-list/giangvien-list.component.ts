import { Component, OnInit } from '@angular/core';
import { LecturerService } from '../../core/services/lecturer.service';
import { UserService } from '../../core/services/user.service';
import { PermissionService } from '../../core/services/permission.service';
import { AuthService } from '../../core/services/auth.service';
import { AcademicService } from '../../core/services/academic.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-giangvien-list',
  templateUrl: './giangvien-list.component.html',
  styleUrls: ['./giangvien-list.component.scss']
})
export class GiangvienListComponent implements OnInit {
  lecturers: any[] = [];
  pageSize = 5;
  total = 0;
  loading = false;
  keyword = '';
  filterKhoa = '';
  filterHocHam = '';
  filterHocVi = '';
  filterNgayTuyenDungFrom = '';
  filterNgayTuyenDungTo = '';
  filterTrangThaiUser: boolean | undefined = undefined;

  // showAdvanced = false;
  sortBy: string = 'hoTen';
  sortDir: 'asc' | 'desc' = 'asc';
  isCreateModalVisible = false;
  isEditModalVisible = false;
  isDetailModalVisible = false;
  selectedLecturer: any = {};
  detailLecturer: any = {};
  detailLoading = false;
  newLecturer: any = { maQuyen: 2 };
  creating = false;
  editing = false;
  roles: any[] = [];
  departments: any[] = [];
  hocHamOptions = [
    { label: 'Giáo sư', value: 'Giáo sư' },
    { label: 'Phó giáo sư', value: 'Phó giáo sư' },
    { label: 'Không', value: '' }
  ];
  hocViOptions = [
    { label: 'Tiến sĩ', value: 'Tiến sĩ' },
    { label: 'Thạc sĩ', value: 'Thạc sĩ' },
    { label: 'Cử nhân', value: 'Cử nhân' },
    { label: 'Không', value: '' }
  ];
  // rowLoading: { [ma: string]: boolean } = {};
  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;
  pageIndex = 1;

  constructor(
    private lecturerService: LecturerService,
    private userService: UserService,
    private msg: NzMessageService,
    private modalService: NzModalService,
    private permissionService: PermissionService,
    private academicService: AcademicService,
    private auth: AuthService
  ) {}

  private parseBool(v: any): boolean {
    if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
    if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
    return false;
  }

  private formatDateForServer(value: any): string | null {
    if (value === undefined || value === null || value === '') return null;
    if (value instanceof Date) {
      const yyyy = value.getFullYear();
      const mm = String(value.getMonth() + 1).padStart(2, '0');
      const dd = String(value.getDate()).padStart(2, '0');
      return `${yyyy}-${mm}-${dd}`;
    }
    const s = String(value).trim();
    if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
    const m = s.match(/^(\d{1,2})[-\/\.](\d{1,2})[-\/\.](\d{4})$/);
    if (m) {
      const dd = String(Number(m[1])).padStart(2, '0');
      const mm = String(Number(m[2])).padStart(2, '0');
      const yyyy = m[3];
      return `${yyyy}-${mm}-${dd}`;
    }
    const parsed = Date.parse(s);
    if (!isNaN(parsed)) {
      const d = new Date(parsed);
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      return `${yyyy}-${mm}-${dd}`;
    }
    return null;
  }

  private normalizeGender(v: any): number | undefined {
    if (v === undefined || v === null || v === '') return undefined;
    const n = Number(v);
    if (!Number.isNaN(n)) return n;
    const s = String(v).trim();
    if (s === 'Nam' || s.toLowerCase() === 'male') return 1;
    if (s === 'Nữ' || s.toLowerCase() === 'female') return 2;
    return undefined;
  }

  private normalizeStr(v: any): string {
    if (v === undefined || v === null) return '';
    const s = String(v).trim();
    if (s.toLowerCase() === 'null' || s.toLowerCase() === 'undefined') return '';
    return s;
  }

  private buildNormalizedFrom(record: any): any {
    const nd = record?.nguoiDung || record?.NguoiDung || {};
    const gv = record?.giangVien || record?.GiangVien || {};

    const normalized: any = {
      maGiangVien: this.normalizeStr(gv.maGiangVien || gv.MaGiangVien || record?.maGiangVien || record?.MaGiangVien || ''),
      hoTen: this.normalizeStr(nd.hoTen || nd.HoTen || gv.hoTen || gv.HoTen || record?.hoTen || record?.HoTen),
      tenDangNhap: this.normalizeStr(nd.tenDangNhap || nd.TenDangNhap || record?.tenDangNhap || record?.TenDangNhap),
      khoa: this.extractKhoaName(nd.khoa || nd.Khoa || gv.khoa || gv.Khoa || record?.khoa || record?.Khoa),
      maKhoa: this.extractKhoaId(gv.maKhoa || gv.MaKhoa || nd.maKhoa || nd.MaKhoa || record?.maKhoa || record?.MaKhoa || (record?.khoa && (record.khoa.maKhoa || record.khoa.MaKhoa))),
      hocHam: this.normalizeStr(gv.hocHam || gv.HocHam || record?.hocHam || record?.HocHam),
      hocVi: this.normalizeStr(gv.hocVi || gv.HocVi || record?.hocVi || record?.HocVi),
      ngayTuyenDung: this.normalizeStr(gv.ngayTuyenDung || gv.NgayTuyenDung || record?.ngayTuyenDung || record?.NgayTuyenDung),
      email: this.normalizeStr(nd.email || nd.Email || record?.email || record?.Email),
      soDienThoai: this.normalizeStr(nd.soDienThoai || nd.SoDienThoai || record?.soDienThoai || record?.SoDienThoai),
      gioiTinh: this.normalizeGender(nd.gioiTinh ?? nd.GioiTinh ?? gv.gioiTinh ?? gv.GioiTinh ?? record?.gioiTinh ?? record?.GioiTinh),
      ngaySinh: this.normalizeStr(nd.ngaySinh || nd.NgaySinh || record?.ngaySinh || record?.NgaySinh),
      diaChi: this.normalizeStr(nd.diaChi || nd.DiaChi || record?.diaChi || record?.DiaChi),
      anhDaiDien: this.normalizeStr(nd.anhDaiDien || nd.AnhDaiDien || gv.anhDaiDien || gv.AnhDaiDien || record?.anhDaiDien || record?.AnhDaiDien),
      isActive: this.parseBool(nd.trangThai ?? nd.TrangThai ?? record?.trangThai ?? record?.TrangThai)
    };

    if (normalized.ngayTuyenDung && normalized.ngayTuyenDung.includes('T')) {
      normalized.ngayTuyenDung = normalized.ngayTuyenDung.split('T')[0];
    }
    if (normalized.ngaySinh && normalized.ngaySinh.includes('T')) {
      normalized.ngaySinh = normalized.ngaySinh.split('T')[0];
    }

    try {
      if ((!normalized.khoa || normalized.khoa === '') && normalized.maKhoa && Array.isArray(this.departments) && this.departments.length > 0) {
        const found = this.departments.find((d: any) => String(d.value) === String(normalized.maKhoa));
        if (found) normalized.khoa = found.label || normalized.khoa;
      }
    } catch (e) {
    }

    try {
      const key = normalized.maGiangVien || normalized.tenDangNhap || '';
      if ((!normalized.khoa || normalized.khoa === '') && key && Array.isArray(this.lecturers) && this.lecturers.length > 0) {
        const match = this.lecturers.find((l: any) => (l.maGiangVien || l.MaGiangVien) === key || (l.tenDangNhap || l.TenDangNhap) === key);
        if (match) {
          normalized.khoa = normalized.khoa || (match.khoa || match.Khoa || '');
          normalized.maKhoa = normalized.maKhoa || (match.maKhoa || match.MaKhoa || '');
          normalized.isActive = normalized.isActive ?? match.isActive ?? false;
        }
      }

      const rawStatus = (record?.nguoiDung?.trangThai ?? record?.nguoiDung?.TrangThai ?? record?.giangVien?.trangThai ?? record?.giangVien?.TrangThai ?? record?.trangThai ?? record?.TrangThai ?? record?.isActive ?? record?.IsActive);
      let parsedStatus = rawStatus !== undefined ? this.parseBool(rawStatus) : undefined;
      if (parsedStatus === undefined && key && Array.isArray(this.lecturers) && this.lecturers.length > 0) {
        const match2 = this.lecturers.find((l: any) => (l.maGiangVien || l.MaGiangVien) === key || (l.tenDangNhap || l.TenDangNhap) === key);
        if (match2) parsedStatus = this.parseBool(match2.isActive ?? match2.trangThai ?? match2.TrangThai ?? match2.isActive);
      }
      normalized.trangThaiUser = parsedStatus ?? !!normalized.isActive;
    } catch (e) {
      normalized.trangThaiUser = !!normalized.isActive;
    }

    return normalized;
  }

  ngOnInit(): void {
    this.loadLecturers();
    this.loadDepartments();
  }

  private extractKhoaName(...sources: any[]): string {
    for (const s of sources) {
      if (s === undefined || s === null) continue;
      if (typeof s === 'object') {
        const name = s.tenKhoa || s.TenKhoa || s.name || s.codeKhoa || s.code || s.label;
        if (name) return String(name);
      } else {
        const v = this.normalizeStr(s);
        if (v) return v;
      }
    }
    return '';
  }

  private extractKhoaId(...sources: any[]): string {
    for (const s of sources) {
      if (s === undefined || s === null) continue;
      if (typeof s === 'object') {
        const id = s.maKhoa || s.MaKhoa || s.codeKhoa || s.code || s.value;
        if (id !== undefined && id !== null && String(id).trim() !== '') return String(id);
      } else {
        const v = this.normalizeStr(s);
        if (v) return v;
      }
    }
    return '';
  }

  private loadDepartments(): void {
  this.academicService.getDepartments({ Page: 1, PageSize: 200 }).subscribe({
    next: (res: any) => {
      console.log('GV Departments raw response:', res);
      console.log('GV Response type:', typeof res);

      if (typeof res === 'string') {
        console.error('API departments trả về HTML thay vì JSON');
        return;
      }

      const items = res?.data?.items?.map((x: any) => x.khoa) || [];
      console.log('GV Departments items (khoa list):', items);

      this.departments = items.map((d: any) => ({
        label: d.tenKhoa || d.TenKhoa || d.name || d.codeKhoa,
        value: d.maKhoa || d.MaKhoa || d.codeKhoa
      }));

      console.log('GV Departments loaded:', this.departments);
    },
    error: (err) => {
      console.error('GV Load departments error:', err);
      console.error('GV Error details:', err.error);
    }
  });
}


  // private loadRoles(): void {
  //   this.permissionService.getRoles({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
  //     const items = res?.data?.items || res?.data || [];
  //     this.roles = (items || []).map((r: any) => ({ label: r.tenQuyen || r.TenQuyen || r.name || r.code || r.codeQuyen, value: r.maQuyen || r.MaQuyen || r.codeQuyen || r.code }));
  //   }, error: () => {  } });
  // }


  loadLecturers(): void {
    this.loading = true;
    const params: any = {
      Page: this.pageIndex,
      PageSize: this.pageSize,
      SortBy: this.sortBy === 'hoTen' ? 'hoten' : this.sortBy,
      SortDir: this.sortDir
    };

    if (this.keyword) this.keyword = String(this.keyword).trim();


    if (this.keyword) {
      const kw = String(this.keyword).trim();
      params.HoTen = kw;
      params.TenGiangVien = kw;
    }

    if (this.filterKhoa) params.MaKhoa = this.filterKhoa;
    if (this.filterHocHam) params.HocHam = this.filterHocHam;
    if (this.filterHocVi) params.HocVi = this.filterHocVi;
    if (this.filterNgayTuyenDungFrom) params.NgayTuyenDungFrom = this.filterNgayTuyenDungFrom;
    if (this.filterNgayTuyenDungTo) params.NgayTuyenDungTo = this.filterNgayTuyenDungTo;
    if (this.filterTrangThaiUser !== undefined) params.TrangThaiUser = this.filterTrangThaiUser;

  console.log('Lecturer search params:', params);

    this.lecturerService.getLecturers(params).subscribe({
      next: (res) => {
        const raw = res?.data?.items || res?.data || [];
        const coerce = (v: any): boolean | undefined => {
          if (v === undefined || v === null || v === '') return undefined;
          if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
          if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
          return undefined;
        };

        this.lecturers = (raw || []).map((item: any) => {
          const nguoiDung = item.nguoiDung || item.NguoiDung || {};
          const giangVien = item.giangVien || item.GiangVien || {};
          const khoa = item.khoa || item.Khoa || {};

          const isActive = coerce(nguoiDung.trangThai ?? nguoiDung.TrangThai ?? item.trangThai ?? item.TrangThai) ?? true;

          const email = (nguoiDung.email || nguoiDung.Email || item.email || item.Email || giangVien.email || giangVien.Email) || '';
          const phone = (nguoiDung.soDienThoai || nguoiDung.SoDienThoai || item.soDienThoai || item.SoDienThoai || giangVien.soDienThoai || giangVien.SoDienThoai) || '';
          const rawGioiTinh = (nguoiDung.gioiTinh ?? nguoiDung.GioiTinh ?? item.gioiTinh ?? item.GioiTinh ?? giangVien.gioiTinh ?? giangVien.GioiTinh);
          const gioiTinh = this.normalizeGender(rawGioiTinh);

          return {
            maGiangVien: giangVien.maGiangVien || giangVien.MaGiangVien || item.maGiangVien || item.MaGiangVien,
            hoTen: nguoiDung.hoTen || nguoiDung.HoTen || giangVien.hoTen || giangVien.HoTen || item.hoTen || item.HoTen,
            tenDangNhap: nguoiDung.tenDangNhap || nguoiDung.TenDangNhap || item.tenDangNhap || item.TenDangNhap,
            khoa: khoa.tenKhoa || khoa.TenKhoa || item.khoa || item.Khoa,
            maKhoa: khoa.maKhoa || khoa.MaKhoa || item.maKhoa || item.MaKhoa,
            codeKhoa: khoa.codeKhoa || khoa.CodeKhoa,
            hocHam: (giangVien.hocHam === 'null' ? '' : (giangVien.hocHam || giangVien.HocHam || '')),
            hocVi: (giangVien.hocVi === 'null' ? '' : (giangVien.hocVi || giangVien.HocVi || '')),
            ngayTuyenDung: giangVien.ngayTuyenDung === 'null' ? '' : (giangVien.ngayTuyenDung || giangVien.NgayTuyenDung || ''),
            email: email,
            soDienThoai: phone,
            gioiTinh: gioiTinh,
            isActive: isActive
          };
        });
        this.total = res?.data?.totalRecords || this.lecturers.length;
        this.loading = false;

      },
      error: (err) => {
        this.loading = false;
        this.msg.error(err?.error?.message || 'Lỗi tải danh sách giảng viên');
      }
    });
  }

  onFilterChange(): void { this.pageIndex = 1; this.loadLecturers(); }
  onPageChange(p: number): void { this.pageIndex = p; this.loadLecturers(); }

  onCurrentPageDataChange(listOfCurrentPageData: readonly any[]): void {
    this.listOfCurrentPageData = (listOfCurrentPageData || []) as any[];
    this.refreshCheckedStatus();
  }

  onAllChecked(checked: boolean): void {
    this.listOfCurrentPageData.forEach(item => this.updateCheckedSet(item.maGiangVien, checked));
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
    this.checked = this.listOfCurrentPageData.length > 0 && this.listOfCurrentPageData.every(item => this.setOfCheckedId.has(item.maGiangVien));
    this.indeterminate = this.listOfCurrentPageData.some(item => this.setOfCheckedId.has(item.maGiangVien)) && !this.checked;
  }

  deleteSelected(): void {
    if (this.setOfCheckedId.size === 0) return;
    this.modalService.confirm({
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} giảng viên đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các giảng viên đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzCancelText: 'Hủy',
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(id => {
          this.lecturerService.deleteLecturer(id).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(id);
              const idx = this.lecturers.findIndex(l => (l.maGiangVien || l.MaGiangVien) === id);
              if (idx !== -1) this.lecturers[idx] = { ...this.lecturers[idx], isActive: false, trangThai: false, TrangThai: false, trangThaiUser: false };
              if (remaining === 0) {
                this.msg.success('Đã vô hiệu hóa các giảng viên đã chọn');
                this.loadLecturers();
              }
            },
            error: () => {
              remaining--;
              if (remaining === 0) {
                this.loadLecturers();
              }
            }
          });
        });
      }
    });
  }

  onAdd(): void { this.showCreateModal(); }

  showDeleteConfirm(id: string): void {
    const lec = this.lecturers.find(l => (l.maGiangVien || l.MaGiangVien) === id);
    if (lec) this.handleDelete(lec);
  }

  onSearch(): void {
    this.pageIndex = 1;
    this.loadLecturers();
  }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }
    this.pageIndex = 1;
    this.loadLecturers();
  }

  clearAdvanced(): void {
    this.filterKhoa = '';
    this.filterHocHam = '';
    this.filterHocVi = '';
    this.filterNgayTuyenDungFrom = '';
    this.filterNgayTuyenDungTo = '';
  // Để làm mới về trạng thái mặc định "Tất cả"
  this.filterTrangThaiUser = undefined as any;
    this.sortBy = 'hoTen';
    this.sortDir = 'asc';
  }

  refreshList(): void {
    this.keyword = '';
    this.clearAdvanced();
    this.pageIndex = 1;
    this.loadLecturers();
  }

    showCreateModal(): void {
      // Khi tạo, backend sẽ sinh mã giảng viên và đặt NgàyTuyenDung mặc định
      this.newLecturer = {
        maQuyen: 2,
        hoTen: '',
        maKhoa: '',
        hocHam: '',
        hocVi: '',
        email: '',
        gioiTinh: undefined,
        soDienThoai: '',
        diaChi: '',
        ngaySinh: undefined,
        anhDaiDien: undefined
      };
    this.isCreateModalVisible = true;
  }

  handleCancel(): void {
    this.isCreateModalVisible = false;
    this.isEditModalVisible = false;
  }

  handleCreate(): void {
    console.log('newLecturer:', this.newLecturer);
    console.log('maKhoa value:', this.newLecturer.maKhoa, 'type:', typeof this.newLecturer.maKhoa);
    if (this.newLecturer.maKhoa === null || this.newLecturer.maKhoa === undefined || this.newLecturer.maKhoa === '') {
      this.msg.warning('Vui lòng nhập mã khoa!');
      return;
    }
    if (!this.newLecturer.hoTen) {
      this.msg.warning('Vui lòng nhập họ tên!');
      return;
    }

    this.creating = true;
    const fd = new FormData();
    // API: POST /api/lecturer/create
    fd.append('HoTen', String(this.newLecturer.hoTen));
    fd.append('MaKhoa', String(this.newLecturer.maKhoa));
    if (this.newLecturer.hocHam) fd.append('HocHam', String(this.newLecturer.hocHam));
    if (this.newLecturer.hocVi) fd.append('HocVi', String(this.newLecturer.hocVi));
    if (this.newLecturer.gioiTinh !== undefined) fd.append('GioiTinh', String(this.newLecturer.gioiTinh));
    if (this.newLecturer.anhDaiDien instanceof File) fd.append('AnhDaiDien', this.newLecturer.anhDaiDien);
    if (this.newLecturer.email) fd.append('Email', String(this.newLecturer.email));
    if (this.newLecturer.soDienThoai) fd.append('SoDienThoai', String(this.newLecturer.soDienThoai));
    const ngayTuyenForCreate = this.formatDateForServer(this.newLecturer.ngayTuyenDung) || this.formatDateForServer(new Date());
    if (ngayTuyenForCreate) fd.append('NgayTuyenDung', ngayTuyenForCreate);
    if (this.newLecturer.ngaySinh) fd.append('NgaySinh', String(this.newLecturer.ngaySinh));
    if (this.newLecturer.diaChi) fd.append('DiaChi', String(this.newLecturer.diaChi));

    this.lecturerService.createLecturer(fd).subscribe({
      next: (res) => {
        this.creating = false;
        this.isCreateModalVisible = false;
        this.msg.success('Tạo giảng viên thành công');
        this.loadLecturers();
      },
      error: (err) => {
        this.creating = false;
        this.msg.error(err?.error?.message || 'Lỗi khi tạo giảng viên');
      }
    });
  }

  showEditModal(data: any): void {
    this.selectedLecturer = { ...data };
    this.selectedLecturer.gioiTinh = this.selectedLecturer.gioiTinh ?? this.selectedLecturer.GioiTinh;
    this.selectedLecturer.soDienThoai = this.selectedLecturer.soDienThoai ?? this.selectedLecturer.SoDienThoai;
    this.selectedLecturer.diaChi = this.selectedLecturer.diaChi ?? this.selectedLecturer.DiaChi;
    this.selectedLecturer.ngaySinh = this.selectedLecturer.ngaySinh ?? this.selectedLecturer.NgaySinh;
    this.isEditModalVisible = true;
  }

  showDetailModal(ma: string): void {
  if (!ma) return;
  this.detailLoading = true;
  this.detailLecturer = {};
  this.isDetailModalVisible = true;

  this.userService.getUserInfoByUsername(ma).subscribe({
    next: (uRes: any) => {
      console.log('User info response:', uRes);
      const userDetail = uRes?.data ?? uRes;
      if (userDetail && Object.keys(userDetail).length > 0) {
        const normalized = this.buildNormalizedFrom(userDetail);
        this.detailLoading = false;
        const hasData = (normalized.hoTen || normalized.maGiangVien || normalized.email || normalized.soDienThoai);
        if (!hasData) {
        } else {
          this.detailLecturer = normalized;
          return;
        }
      }

      this.lecturerService.getProfile(ma).subscribe({
        next: (res: any) => {
          this.detailLoading = false;
          const detail = res?.data ?? res;
          if (!detail || Object.keys(detail).length === 0) {
            this.msg.info('Không tìm thấy thông tin chi tiết cho giảng viên này');
            return;
          }
          const normalized = this.buildNormalizedFrom(detail);
          const hasData = (normalized.hoTen || normalized.maGiangVien || normalized.email || normalized.soDienThoai);
          if (!hasData) {
            this.msg.info('Không tìm thấy thông tin chi tiết cho giảng viên này');
            return;
          }
          this.detailLecturer = normalized;
        },
        error: (err) => {
          this.detailLoading = false;
          this.msg.error(err?.message || err?.error?.message || 'Lỗi khi tải thông tin chi tiết');
          this.detailLecturer = {};
        }
      });
    },
    error: (_err) => {
      this.lecturerService.getProfile(ma).subscribe({
        next: (res: any) => {
          this.detailLoading = false;
          const detail = res?.data ?? res;
          const normalized = this.buildNormalizedFrom(detail);
          const hasData = (normalized.hoTen || normalized.maGiangVien || normalized.email || normalized.soDienThoai);
          if (!hasData) {
            this.msg.info('Không tìm thấy thông tin chi tiết cho giảng viên này');
            return;
          }
          this.detailLecturer = normalized;
        },
        error: (err) => {
          this.detailLoading = false;
          this.msg.error(err?.message || err?.error?.message || 'Lỗi khi tải thông tin chi tiết');
          this.detailLecturer = {};
        }
      });
    }
  });
}

  handleEdit(): void {
    if (!this.selectedLecturer.maGiangVien) return;
    this.editing = true;

    const fd = new FormData();
    // API: POST /api/lecturer/profile
    fd.append('MaGiangVien', String(this.selectedLecturer.maGiangVien));
    if (this.selectedLecturer.hoTen) fd.append('TenGiangVien', String(this.selectedLecturer.hoTen));
    if (this.selectedLecturer.trangThai !== undefined) fd.append('TrangThai', String(this.selectedLecturer.trangThai));
    if (this.selectedLecturer.maKhoa) {
      fd.append('MaKhoa', String(this.selectedLecturer.maKhoa));
      fd.append('Khoa', String(this.selectedLecturer.maKhoa));
    }
    if (this.selectedLecturer.hocHam) fd.append('HocHam', String(this.selectedLecturer.hocHam));
    if (this.selectedLecturer.hocVi) fd.append('HocVi', String(this.selectedLecturer.hocVi));
    const ngayTuyen = this.formatDateForServer(this.selectedLecturer.ngayTuyenDung);
    if (ngayTuyen) fd.append('NgayTuyenDung', ngayTuyen);
    if (this.selectedLecturer.gioiTinh !== undefined) fd.append('GioiTinh', String(this.selectedLecturer.gioiTinh));
    if (this.selectedLecturer.anhDaiDien instanceof File) fd.append('AnhDaiDien', this.selectedLecturer.anhDaiDien);
    if (this.selectedLecturer.email) fd.append('Email', String(this.selectedLecturer.email));
    if (this.selectedLecturer.soDienThoai) fd.append('SoDienThoai', String(this.selectedLecturer.soDienThoai));
    const ngaySinh = this.formatDateForServer(this.selectedLecturer.ngaySinh);
    if (ngaySinh) fd.append('NgaySinh', ngaySinh);
    if (this.selectedLecturer.diaChi) fd.append('DiaChi', String(this.selectedLecturer.diaChi));

    this.lecturerService.updateProfile(fd).subscribe({
      next: () => {
        this.editing = false;
        this.isEditModalVisible = false;
        this.msg.success('Cập nhật thông tin giảng viên thành công');
        this.loadLecturers();
      },
      error: (err) => {
        this.editing = false;
        this.msg.error(err?.error?.message || 'Lỗi khi cập nhật');
      }
    });
  }

  onNewAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length ? input.files[0] : undefined;
    if (file) this.newLecturer.anhDaiDien = file;
  }

  onEditAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length ? input.files[0] : undefined;
    if (file) this.selectedLecturer.anhDaiDien = file;
  }

  handleDelete(lecturer: any): void {
    this.modalService.confirm({
      nzTitle: `Vô hiệu hóa giảng viên "${lecturer.hoTen}"?`,
      nzContent: 'Bạn có chắc chắn muốn vô hiệu hóa giảng viên này?',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzCancelText: 'Hủy',
      nzOnOk: () => {
        this.lecturerService.deleteLecturer(lecturer.maGiangVien).subscribe({
          next: () => {
            this.msg.success('Đã vô hiệu hóa giảng viên thành công');
            const idx = this.lecturers.findIndex(l => (l.maGiangVien || l.MaGiangVien) === lecturer.maGiangVien);
            if (idx !== -1) this.lecturers[idx] = { ...this.lecturers[idx], isActive: false, trangThai: false, TrangThai: false, trangThaiUser: false };
            this.loadLecturers();
          },
          error: () => {
            this.msg.error('Vô hiệu hóa thất bại');
          }
        });
      }
    });
  }

  get isAdmin(): boolean {
    try {
      const u = this.auth.getUser();
      console.debug('isAdmin check - current user:', u);
      if (!u) return false;

      const maQ = u.maQuyen ?? u.MaQuyen ?? u.maQuyenId ?? u.roleId;
      if (maQ !== undefined && maQ !== null && Number(maQ) === 1) return true;

      const roleCode = u.roleCode || u.RoleCode || u.phanQuyen?.codeQuyen || u.phanQuyen?.CodeQuyen || u.role?.codeQuyen || u.role?.code;
      if (roleCode && String(roleCode).toUpperCase() === 'ADMIN') return true;

      const roles = u.roles || u.role || [];
      if (Array.isArray(roles) && roles.length > 0) {
        for (const r of roles) {
          if (!r) continue;
          const rc = (r.codeQuyen || r.code || r.codeRole || r).toString?.() || '';
          if (rc && rc.toUpperCase().includes('ADMIN')) return true;
        }
      }

      const textual = (u.tenQuyen || u.TenQuyen || u.roleName || u.roleNames || u.username || u.userName || '');
      if (String(textual).toLowerCase().includes('admin')) return true;

      return false;
    } catch (e) {
      return false;
    }
  }

  onResetPassword(tenDangNhapOrKey: string): void {
    if (!tenDangNhapOrKey) {
      this.msg.error('Không có tên đăng nhập để đặt lại mật khẩu');
      return;
    }

    const ten = String(tenDangNhapOrKey || '').trim();
    this.modalService.confirm({
      nzTitle: `Đặt lại mật khẩu cho "${ten}"?`,
      nzOkText: 'Đặt lại',
      nzOkType: 'primary',
      nzOnOk: () => {
        this.auth.refreshPassword({ TenDangNhap: ten }).subscribe({
          next: (res: any) => {
            this.modalService.success({ nzTitle: 'Mật khẩu đã được đặt lại', nzContent: '' });
          },
          error: (err: any) => {
            this.msg.error(err?.error?.message || err?.message || 'Lỗi khi đặt lại mật khẩu');
          }
        });
      }
    });
  }
}
