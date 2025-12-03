import { Component, OnInit } from '@angular/core';
import { StudentService } from '../../core/services/student.service';
import { UserService } from '../../core/services/user.service';
import { AcademicService } from '../../core/services/academic.service';
import { PermissionService } from '../../core/services/permission.service';
import { AuthService } from '../../core/services/auth.service';
import * as XLSX from 'xlsx';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';

@Component({
  selector: 'app-sinhvien-list',
  templateUrl: './sinhvien-list.component.html',
  styleUrls: ['./sinhvien-list.component.scss']
})
export class SinhvienListComponent implements OnInit {
  students: any[] = [];
  loading = false;
  rowLoading: Record<string, boolean> = {};
  pageSize = 5;
  total = 0;
  importValid = false;
  importError: string | null = null;
  pageIndex = 1;

  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  keyword = '';
  filterKhoa = '';
  filterNganh = '';
  filterNamNhapHoc = '';
  filterTrangThaiUser?: boolean;
  // showAdvanced = false;
  sortBy: string = 'maSinhVien';
  sortDir: 'asc' | 'desc' = 'asc';

  isCreateModalVisible = false;
  isEditModalVisible = false;
  isImportModalVisible = false;
  isDetailModalVisible = false;
  creating = false;
  editing = false;
  detailLoading = false;

  newStudent: any = {};
  selectedStudent: any = {};
  detailStudent: any = {};
  departments: any[] = [];
  majors: any[] = [];
  majorsLoading = false;
  years: number[] = [];
  roles: any[] = [];
  importFile?: File;
  importing = false;
  importDefaultNamNhapHoc?: number | string;

  constructor(
    private studentService: StudentService,
    private message: NzMessageService,
    private modal: NzModalService,
    private academicService: AcademicService,
    private permissionService: PermissionService,
    private userService: UserService,
    private auth: AuthService
  ) {}

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
      this.message.error('Không có tên đăng nhập để đặt lại mật khẩu');
      return;
    }
    const ten = String(tenDangNhapOrKey || '').trim();
    this.modal.confirm({
      nzTitle: `Đặt lại mật khẩu cho "${ten}"?`,
      nzOkText: 'Đặt lại',
      nzOkType: 'primary',
      nzOnOk: () => {
        this.auth.refreshPassword({ TenDangNhap: ten }).subscribe({
          next: (res: any) => {
            this.modal.success({ nzTitle: 'Mật khẩu đã được đặt lại', nzContent: '', nzOkText: 'Xác nhận' });
          },
          error: (err: any) => { this.message.error(err?.error?.message || err?.message || 'Lỗi khi đặt lại mật khẩu'); }
        });
      },
      nzCancelText: 'Hủy'
    });

  }

  ngOnInit(): void {
    this.years = this.generateYears();
    this.loadStudents();
    this.loadDepartments();
    this.loadMajors();
    this.loadRoles();
  }

  private generateYears(): number[] {
    const current = new Date().getFullYear();
    const years: number[] = [];
    for (let y = current; y >= current - 20; y--) years.push(y);
    return years;
  }

  private loadDepartments(): void {
    this.academicService.getDepartments({ Page: 1, PageSize: 200 }).subscribe({
      next: (res: any) => {
        console.log('Departments raw response:', res);
        console.log('Response type:', typeof res);

        if (typeof res === 'string') {
          console.error('API departments trả về HTML thay vì JSON');
          return;
        }

        const items = res?.data?.items || res?.data || [];
        console.log('Departments items:', items);

        this.departments = (items || []).map((d: any) => ({
          label: d.tenKhoa || d.TenKhoa || d.name || d.codeKhoa,
          value: d.maKhoa || d.MaKhoa || d.codeKhoa
        }));
        console.log('Departments loaded:', this.departments);
      },
      error: (err) => {
        console.error('Load departments error:', err);
        console.error('Error details:', err.error);
      }
    });
  }

  private loadMajors(): void {
    this.academicService.getMajors({ Page: 1, PageSize: 200 }).subscribe({
      next: (res: any) => {
        console.log('Majors raw response:', res);
        console.log('Response type:', typeof res);

        if (typeof res === 'string') {
          console.error('API majors trả về HTML thay vì JSON');
          return;
        }

        const items = res && (res.data?.items || res.data || []) ? (res.data?.items || res.data || []) : [];
        console.log('Majors items:', items);

        this.majors = (items || []).map((it: any) => {
          const n = it?.nganh || it || {};
          const code = n?.codeNganh || n?.CodeNganh || '';
          const id = n?.maNganh || n?.MaNganh || code || '';
          const name = n?.tenNganh || n?.TenNganh || '';
          const label = code ? `${code} — ${name || code}` : (name || id);
          return { label, value: id, code };
        }).filter((x: any) => (x.value || '').toString().trim() !== '');
        console.log('Majors loaded:', this.majors.length, 'items');
      },
      error: (err) => {
        console.error('Load majors error:', err);
        console.error('Error details:', err.error);
      }
    });
  }

  private loadRoles(): void {
    this.permissionService.getRoles({ Page: 1, PageSize: 200 }).subscribe({ next: (res: any) => {
      const items = res?.data?.items || res?.data || [];
      this.roles = (items || []).map((r: any) => ({ label: r.tenQuyen || r.TenQuyen || r.name || r.code, value: r.maQuyen || r.MaQuyen || r.code }));
    }, error: () => { } });
  }

  loadStudents(): void {
    this.loading = true;
    const baseParams: any = {
      Page: this.pageIndex,
      PageSize: this.pageSize,
      SortBy: this.sortBy,
      SortDir: this.sortDir
    };

    if (this.filterKhoa) baseParams.Khoa = this.filterKhoa;
    if (this.filterNganh) {
      baseParams.Nganh = this.filterNganh;
      baseParams.MaNganh = this.filterNganh;
      baseParams.CodeNganh = this.filterNganh;
    }
    if (this.filterNamNhapHoc) baseParams.NamNhapHoc = this.filterNamNhapHoc;
    if (this.filterTrangThaiUser !== undefined) baseParams.TrangThaiUser = this.filterTrangThaiUser;

    const kw = (this.keyword || '').trim();
    if (kw) {
      baseParams.HoTen = kw;
    }

    this.studentService.getStudents(baseParams).subscribe({
      next: (res: any) => {
        const raw = res?.data?.items || res?.data || [];
        const coerce = (v: any): boolean | undefined => {
          if (v === undefined || v === null || v === '') return undefined;
          if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
          if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
          return undefined;
        };

        this.students = (raw || []).map((item: any) => {
          const nguoiDung = item.nguoiDung || {};
          const sinhVien = item.sinhVien || {};
          const nganh = item.nganh || {};
          const khoa = item.khoa || {};

          const isActive = coerce(nguoiDung.trangThai) ?? true;

          const nguoiDungObj = nguoiDung || {};
          const sinhVienObj = sinhVien || {};
          const nganhObj = nganh || {};
          const khoaObj = khoa || {};

          const email = (nguoiDungObj.email || nguoiDungObj.Email || item.email || item.Email || sinhVienObj.email || '') || '';
          const phone = (nguoiDungObj.soDienThoai || nguoiDungObj.SoDienThoai || item.soDienThoai || item.SoDienThoai || sinhVienObj.soDienThoai || '') || '';
          const gioiTinhVal = (nguoiDungObj.gioiTinh ?? nguoiDungObj.GioiTinh ?? item.gioiTinh ?? item.GioiTinh ?? sinhVienObj.gioiTinh);
          const normalizedGioiTinh = (() => {
            if (gioiTinhVal === undefined || gioiTinhVal === null || gioiTinhVal === '') return undefined;
            const n = Number(gioiTinhVal);
            if (!Number.isNaN(n)) return n;
            const s = String(gioiTinhVal).trim().toLowerCase();
            if (s === 'nam' || s === 'male') return 1;
            if (s === 'nữ' || s === 'nu' || s === 'female') return 2;
            return undefined;
          })();

          return {
            maSinhVien: sinhVienObj.maSinhVien || sinhVienObj.MaSinhVien || item.maSinhVien || item.MaSinhVien,
            hoTen: nguoiDungObj.hoTen || nguoiDungObj.HoTen || sinhVienObj.hoTen || sinhVienObj.HoTen || item.hoTen || item.HoTen,
            tenDangNhap: nguoiDungObj.tenDangNhap || nguoiDungObj.TenDangNhap || item.tenDangNhap || item.TenDangNhap,
            email: email,
            soDienThoai: phone,
            gioiTinh: normalizedGioiTinh,
            khoa: khoaObj.tenKhoa || khoaObj.TenKhoa || item.khoa || item.Khoa,
            maKhoa: khoaObj.maKhoa || khoaObj.MaKhoa || item.maKhoa || item.MaKhoa,
            codeKhoa: khoaObj.codeKhoa || khoaObj.CodeKhoa,
            nganh: nganhObj.tenNganh || nganhObj.TenNganh || item.nganh || item.Nganh,
            maNganh: nganhObj.maNganh || nganhObj.MaNganh || item.maNganh || item.MaNganh,
            codeNganh: nganhObj.codeNganh || nganhObj.CodeNganh,
            namNhapHoc: sinhVienObj.namNhapHoc || sinhVienObj.NamNhapHoc || item.namNhapHoc,
            isActive
          };
        });


        this.total = res?.data?.totalRecords || this.students.length;
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        this.message.error(err?.error?.message || 'Lỗi tải danh sách sinh viên');
      }
    });
  }

  onCurrentPageDataChange(listOfCurrentPageData: readonly any[]): void {
    this.listOfCurrentPageData = (listOfCurrentPageData || []) as any[];
    this.refreshCheckedStatus();
  }

  onAllChecked(checked: boolean): void {
    this.listOfCurrentPageData.forEach(item => this.updateCheckedSet(item.maSinhVien, checked));
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
    this.checked = this.listOfCurrentPageData.length > 0 && this.listOfCurrentPageData.every(item => this.setOfCheckedId.has(item.maSinhVien));
    this.indeterminate = this.listOfCurrentPageData.some(item => this.setOfCheckedId.has(item.maSinhVien)) && !this.checked;
  }

  deleteSelected(): void {
    if (this.setOfCheckedId.size === 0) return;
    this.modal.confirm({
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} sinh viên đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các sinh viên đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(id => {
          this.studentService.deleteStudent(id).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(id);
              const idx = this.students.findIndex(s => (s.maSinhVien || s.MaSinhVien) === id);
              if (idx !== -1) this.students[idx] = { ...this.students[idx], isActive: false, trangThai: false, TrangThai: false, trangThaiUser: false };
              if (remaining === 0) {
                this.message.success('Đã vô hiệu hóa các sinh viên đã chọn');
                this.loadStudents();
              }
            },
            error: () => {
              remaining--;
              if (remaining === 0) this.loadStudents();
            }
          });
        });
      }
    ,
      nzCancelText: 'Hủy'
    });
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.loadStudents();
  }

  onFilterChange(): void {
    this.pageIndex = 1;
    this.loadStudents();
  }

  onSearch(): void {
    this.pageIndex = 1;
    this.loadStudents();
  }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }
    this.pageIndex = 1;
    this.loadStudents();
  }

  clearAdvanced(): void {
    this.filterKhoa = '';
    this.filterNganh = '';
    this.filterNamNhapHoc = '';
    this.filterTrangThaiUser = undefined;
    this.sortBy = 'hoTen';
    this.sortDir = 'asc';
  }

  refreshList(): void {
    this.keyword = '';
    this.clearAdvanced();
    this.pageIndex = 1;
    this.loadStudents();
  }

  /** =============== CRUD =============== **/

  showCreateModal(): void {
    this.newStudent = {};
    this.newStudent.MaQuyen = this.newStudent.MaQuyen ?? 3;
    this.isCreateModalVisible = true;
  }

  showImportModal(): void {
    this.importFile = undefined;
    this.isImportModalVisible = true;
  }

  onImportFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length ? input.files[0] : undefined;
    if (file) {
      this.importFile = file;
    } else this.importFile = undefined;

    this.importValid = false;
    this.importError = null;

    if (!file) return;

    const name = (file.name || '').toLowerCase();
    if (!name.endsWith('.xlsx') && !name.endsWith('.xls')) {
      this.importError = 'Vui lòng chọn file Excel (.xlsx hoặc .xls)';
      this.importFile = undefined;
      return;
    }

    const reader = new FileReader();
    reader.onload = (e: any) => {
      try {
        const data = e.target.result;
        const wb = XLSX.read(data, { type: 'array' });
        const firstSheetName = wb.SheetNames && wb.SheetNames[0];
        if (!firstSheetName) {
          this.importError = 'File không chứa sheet nào';
          this.importFile = undefined;
          return;
        }
        const ws = wb.Sheets[firstSheetName];
        const sheetJson: any[] = XLSX.utils.sheet_to_json(ws, { header: 1, defval: '' });
        if (!sheetJson || sheetJson.length === 0) {
          this.importError = 'File Excel trống';
          this.importFile = undefined;
          return;
        }

            const rows: any[] = sheetJson;
            const normalize = (arr: any[]) => (arr || []).map((h: any) => String(h || '').trim());

            const expected = ['HoTen','GioiTinh','Email','SoDienThoai','NgaySinh','DiaChi','NamNhapHoc','CodeNganh','MaSinhVien'];

            let headerRow: string[] = normalize(rows[0] || []);
            let headerRowIndex = 0;
            const maxScan = Math.min(5, rows.length);
            let found = false;
            for (let r = 0; r < maxScan; r++) {
              const candidate = normalize(rows[r] || []);
              const candidateLC = candidate.map(c => c.toLowerCase());
              const matches = expected.filter(h => candidateLC.includes(h.toLowerCase())).length;
              if (matches >= Math.max(1, Math.floor(expected.length / 2)) || (candidateLC.includes('masinhvien'.toLowerCase()) && candidateLC.includes('codenganh'.toLowerCase()))) {
                headerRow = candidate;
                headerRowIndex = r;
                found = true;
                break;
              }
            }

            console.log('Detected header row index:', headerRowIndex, 'headers:', headerRow);

            const headerRowLC = headerRow.map(h => String(h || '').trim().toLowerCase());
            const missing = expected.filter(h => !headerRowLC.includes(h.toLowerCase()));
            if (missing.length > 0) {
              this.importError = `Header thiếu cột: ${missing.join(', ')}. Mong đợi (tên cột, thứ tự không bắt buộc): ${expected.join(', ')}. Tìm thấy (hàng ${headerRowIndex + 1}): ${headerRow.join(', ')}`;
              this.importValid = false;
              return;
            }

        this.importValid = true;
        this.importError = null;
      } catch (err) {
        console.error('Error parsing excel', err);
        this.importError = 'Không thể đọc file Excel. Vui lòng kiểm tra định dạng.';
        this.importFile = undefined;
        this.importValid = false;
      }
    };
    reader.onerror = () => {
      this.importError = 'Lỗi khi đọc file';
      this.importFile = undefined;
      this.importValid = false;
    };
    reader.readAsArrayBuffer(file);
  }

  handleImport(): void {
    if (!this.importFile) {
      this.message.warning('Vui lòng chọn file Excel để import');
      return;
    }
    if (!this.importValid) {
      this.message.warning(this.importError || 'File chưa hợp lệ. Vui lòng kiểm tra header.');
      return;
    }
    this.importing = true;
    const fd = new FormData();
    fd.append('file', this.importFile, this.importFile.name);
    if (this.importDefaultNamNhapHoc !== undefined && this.importDefaultNamNhapHoc !== null && String(this.importDefaultNamNhapHoc).trim() !== '') {
      fd.append('DefaultNamNhapHoc', String(this.importDefaultNamNhapHoc));
    }
    this.studentService.bulkImport(fd).subscribe({
      next: (res) => {
        this.importing = false;
        this.isImportModalVisible = false;
        this.message.success('Import sinh viên thành công');
        this.loadStudents();
      },
      error: (err) => {
        this.importing = false;
        this.message.error(err?.error?.message || err?.message || 'Import thất bại');
      }
    });
  }

  handleCreate(): void {
    const formData = new FormData();
  if (this.newStudent.HoTen) formData.append('HoTen', this.newStudent.HoTen);
    if (this.newStudent.TrangThai !== undefined) formData.append('TrangThai', String(this.newStudent.TrangThai));
    if (this.newStudent.NamNhapHoc !== undefined) formData.append('NamNhapHoc', String(this.newStudent.NamNhapHoc));
    if (this.newStudent.MaNganh) formData.append('MaNganh', String(this.newStudent.MaNganh));
    if (this.newStudent.AnhDaiDien instanceof File) formData.append('AnhDaiDien', this.newStudent.AnhDaiDien);
    if (this.newStudent.GioiTinh !== undefined) formData.append('GioiTinh', String(this.newStudent.GioiTinh));
    if (this.newStudent.Email) formData.append('Email', this.newStudent.Email);
    if (this.newStudent.SoDienThoai) formData.append('SoDienThoai', this.newStudent.SoDienThoai);
    if (this.newStudent.NgaySinh) formData.append('NgaySinh', this.newStudent.NgaySinh);
    if (this.newStudent.DiaChi) formData.append('DiaChi', this.newStudent.DiaChi);    this.creating = true;
    this.studentService.createStudent(formData).subscribe({
      next: () => {
        this.creating = false;
        this.isCreateModalVisible = false;
        this.message.success('Tạo sinh viên thành công');
        this.newStudent = {};
        this.loadStudents();
      },
      error: (err) => {
        this.creating = false;
        this.message.error(err?.error?.message || err?.message || 'Tạo sinh viên thất bại');
      }
    });
  }

  showEditModal(student: any): void {
    const maSV = student?.maSinhVien || student?.MaSinhVien;
    if (!maSV) {
      this.message.error('Mã sinh viên không hợp lệ');
      return;
    }

    const parsedRow = this.normalizeStudentFrom(student);
    this.selectedStudent = {
      MaSinhVien: parsedRow.maSinhVien || student?.maSinhVien || student?.MaSinhVien || '',
      HoTen: parsedRow.hoTen || student?.hoTen || student?.HoTen || '',
      Email: parsedRow.email || student?.email || student?.Email || '',
      SoDienThoai: parsedRow.soDienThoai || student?.soDienThoai || student?.SoDienThoai || '',
      GioiTinh: parsedRow.gioiTinh ?? student?.gioiTinh ?? student?.GioiTinh,
      NamNhapHoc: parsedRow.namNhapHoc || student?.namNhapHoc || student?.NamNhapHoc,
      MaNganh: parsedRow.maNganh || student?.maNganh || student?.MaNganh,
      TrangThai: parsedRow.isActive !== undefined ? !!parsedRow.isActive : undefined,
      TrangThaiUser: parsedRow.trangThaiUser,
      AnhDaiDien: parsedRow.anhDaiDien || student?.anhDaiDien || student?.AnhDaiDien
    } as any;

    this.isEditModalVisible = true;

    this.rowLoading[maSV] = true;
    this.studentService.getProfile(maSV).subscribe({
      next: (s) => {
        const sx: any = s as any;
        if (!sx) {
          this.rowLoading[maSV] = false;
          return;
        }
        this.selectedStudent = {
          ...this.selectedStudent,
          MaSinhVien: this.selectedStudent.MaSinhVien || sx.maSinhVien || sx.MaSinhVien,
          HoTen: this.selectedStudent.HoTen || sx.hoTen || sx.HoTen,
          Email: this.selectedStudent.Email || sx.email || sx.Email,
          SoDienThoai: this.selectedStudent.SoDienThoai || sx.soDienThoai || sx.SoDienThoai || sx.sdt || sx.Sdt,
          GioiTinh: this.selectedStudent.GioiTinh ?? sx.gioiTinh ?? sx.GioiTinh,
          NamNhapHoc: this.selectedStudent.NamNhapHoc || sx.namNhapHoc || sx.NamNhapHoc,
          MaNganh: this.selectedStudent.MaNganh || sx.maNganh || sx.MaNganh,
          AnhDaiDien: this.selectedStudent.AnhDaiDien || sx.anhDaiDien || sx.AnhDaiDien,
        } as any;

        const coerce = (v: any): boolean | undefined => {
          if (v === undefined || v === null || v === '') return undefined;
          if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
          if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
          return undefined;
        };
        const srcStatus = sx.trangThai ?? sx.TrangThai ?? sx.trangThaiUser ?? sx.TrangThaiUser ?? student.trangThai ?? student.TrangThai ?? student.trangThaiUser ?? student.TrangThaiUser;
        const parsedStatus = coerce(srcStatus);
        if (parsedStatus !== undefined) {
          this.selectedStudent.TrangThai = parsedStatus;
          this.selectedStudent.TrangThaiUser = parsedStatus;
        }

        this.rowLoading[maSV] = false;
      },
      error: () => {
        this.rowLoading[maSV] = false;
      }
    });
  }

  handleEdit(): void {
    const maSV = this.selectedStudent?.MaSinhVien || this.selectedStudent?.maSinhVien;
    if (!maSV) {
      this.message.warning('Mã sinh viên không hợp lệ');
      return;
    }

    const formData = new FormData();
    formData.append('MaSinhVien', maSV);
    // API: PUT /api/student/update
    if (this.selectedStudent.HoTen || this.selectedStudent.hoTen) formData.append('TenSinhVien', this.selectedStudent.HoTen || this.selectedStudent.hoTen);
    if (this.selectedStudent.TrangThai !== undefined) formData.append('TrangThai', String(this.selectedStudent.TrangThai));
    if (this.selectedStudent.TrangThai !== undefined) formData.append('TrangThaiUser', String(this.selectedStudent.TrangThai));
    if (this.selectedStudent.NamNhapHoc !== undefined) formData.append('NamNhapHoc', String(this.selectedStudent.NamNhapHoc));
    if (this.selectedStudent.MaNganh) formData.append('MaNganh', String(this.selectedStudent.MaNganh));
    if (this.selectedStudent.GioiTinh !== undefined) formData.append('GioiTinh', String(this.selectedStudent.GioiTinh));
    if (this.selectedStudent.AnhDaiDien instanceof File) formData.append('AnhDaiDien', this.selectedStudent.AnhDaiDien);
    if (this.selectedStudent.Email) formData.append('Email', this.selectedStudent.Email);
    if (this.selectedStudent.SoDienThoai) formData.append('SoDienThoai', this.selectedStudent.SoDienThoai);
    if (this.selectedStudent.NgaySinh) formData.append('NgaySinh', this.selectedStudent.NgaySinh);
    if (this.selectedStudent.DiaChi) formData.append('DiaChi', this.selectedStudent.DiaChi);

    this.editing = true;
    this.rowLoading[maSV] = true;
    this.studentService.updateStudent(formData).subscribe({
      next: () => {
        this.editing = false;
        this.isEditModalVisible = false;
        this.message.success('Cập nhật sinh viên thành công');
        const idx = this.students.findIndex(it => (it.maSinhVien || it.MaSinhVien) === maSV);
        if (idx !== -1) {
          const current = this.students[idx];
          const merged = {
            ...current,
            hoTen: this.selectedStudent.HoTen ?? current.hoTen ?? current.HoTen,
            TrangThai: this.selectedStudent.TrangThai ?? current.TrangThai,
            trangThai: this.selectedStudent.TrangThai ?? current.trangThai,
            trangThaiUser: this.selectedStudent.TrangThai ?? current.trangThaiUser,
            isActive: (this.selectedStudent.TrangThai !== undefined) ? !!this.selectedStudent.TrangThai : current.isActive,
          } as any;
          this.students[idx] = merged;
        }
        this.rowLoading[maSV] = false;
        this.loadStudents();
      },
      error: (err) => {
        this.editing = false;
        this.rowLoading[maSV] = false;
        this.message.error(err?.error?.message || err?.message || 'Cập nhật thất bại');
      }
    });
  }

  onNewAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length ? input.files[0] : undefined;
    if (file) this.newStudent.AnhDaiDien = file;
  }

  onEditAvatarChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input?.files && input.files.length ? input.files[0] : undefined;
    if (file) this.selectedStudent.AnhDaiDien = file;
  }

  showDetailModal(student: any): void {
    const maSV = student?.maSinhVien || student?.MaSinhVien;
    const tenDangNhap = student?.tenDangNhap || student?.TenDangNhap;
    this.detailStudent = {};
    this.isDetailModalVisible = true;
    this.detailLoading = true;

    const finish = (obj: any) => {
      const parsed = this.normalizeStudentFrom(obj || {});
      if ((obj && (obj.tenDangNhap || obj.TenDangNhap)) && !parsed.tenDangNhap) parsed.tenDangNhap = obj.tenDangNhap || obj.TenDangNhap;
      parsed.maSinhVien = parsed.maSinhVien || obj?.maSinhVien || obj?.MaSinhVien || maSV || '';
      parsed.hoTen = parsed.hoTen || obj?.hoTen || obj?.HoTen || '';
      const coerce = (v: any): boolean | undefined => {
        if (v === undefined || v === null || v === '') return undefined;
        if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
        if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
        return undefined;
      };
      const srcStatus = obj?.nguoiDung?.trangThai ?? obj?.nguoiDung?.TrangThai ?? obj?.trangThai ?? obj?.TrangThai ?? obj?.trangThaiUser ?? obj?.TrangThaiUser;
      parsed.trangThaiUser = coerce(srcStatus) ?? parsed.isActive;
      this.detailStudent = parsed;
      this.detailLoading = false;
    };

    if (tenDangNhap) {
      this.userService.getUserInfoByUsername(tenDangNhap).subscribe({
        next: (res: any) => {
          const data = (res && (res.data || res)) || {};
          const nguoiDung = data.nguoiDung || data.NguoiDung || {};
          const sinhVien = data.sinhVien || data.SinhVien || {};
          const merged: any = { ...nguoiDung, ...sinhVien };
          if (data.khoa) merged.khoa = data.khoa;
          if (data.Khoa) merged.khoa = merged.khoa || data.Khoa;
          if (data.nganh) merged.nganh = data.nganh;
          if (data.Nganh) merged.nganh = merged.nganh || data.Nganh;
          merged.tenDangNhap = merged.tenDangNhap || tenDangNhap;
          finish(merged);
        },
        error: () => {
          if (maSV) {
            this.studentService.getProfile(maSV).subscribe({ next: (p: any) => finish(p), error: () => { finish(student); } });
          } else {
            finish(student);
          }
        }
      });
    } else if (maSV) {
      this.studentService.getProfile(maSV).subscribe({ next: (p: any) => finish(p), error: () => { finish(student); } });
    } else {
      finish(student);
    }
  }

  getMajorLabel(maNganh?: string): string | undefined {
    if (!maNganh) return undefined;
    const found = (this.majors || []).find(m => String(m.value) === String(maNganh));
    return found ? found.label : undefined;
  }

  private normalizeStudentFrom(record: any): any {
    const nd = record?.nguoiDung || record?.NguoiDung || {};
    const sv = record?.sinhVien || record?.SinhVien || record || {};
    const ng = record?.nganh || record?.Nganh || {};
    const kh = record?.khoa || record?.Khoa || {};

    const normalizeStr = (v: any) => {
      if (v === undefined || v === null) return '';
      const s = String(v).trim();
      if (s.toLowerCase() === 'null' || s.toLowerCase() === 'undefined') return '';
      return s;
    };

    const parsed: any = {
      maSinhVien: normalizeStr(sv.maSinhVien || sv.MaSinhVien || record.maSinhVien || record.MaSinhVien),
      tenDangNhap: normalizeStr(nd.tenDangNhap || nd.TenDangNhap || record.tenDangNhap || record.TenDangNhap),
      hoTen: normalizeStr(nd.hoTen || nd.HoTen || sv.hoTen || sv.HoTen || record.hoTen || record.HoTen),
      email: normalizeStr(nd.email || nd.Email || sv.email || sv.Email || record.email || record.Email),
      soDienThoai: normalizeStr(nd.soDienThoai || nd.SoDienThoai || sv.soDienThoai || sv.SoDienThoai || record.soDienThoai || record.SoDienThoai),
      gioiTinh: (() => {
        const v = nd.gioiTinh ?? nd.GioiTinh ?? sv.gioiTinh ?? sv.GioiTinh ?? record.gioiTinh ?? record.GioiTinh;
        if (v === undefined || v === null || v === '') return undefined;
        const n = Number(v);
        if (!Number.isNaN(n)) return n;
        const s = String(v).trim().toLowerCase();
        if (s === 'nam' || s === 'male') return 1;
        if (s === 'nữ' || s === 'nu' || s === 'female') return 2;
        return undefined;
      })(),
      khoa: normalizeStr(kh.tenKhoa || kh.TenKhoa || record.khoa || record.Khoa),
      maKhoa: normalizeStr(kh.maKhoa || kh.MaKhoa || record.maKhoa || record.MaKhoa),
      nganh: normalizeStr(ng.tenNganh || ng.TenNganh || record.nganh || record.Nganh),
      maNganh: normalizeStr(ng.maNganh || ng.MaNganh || record.maNganh || record.MaNganh),
      namNhapHoc: sv.namNhapHoc || sv.NamNhapHoc || record.namNhapHoc || record.NamNhapHoc,
      diaChi: normalizeStr(nd.diaChi || nd.DiaChi || sv.diaChi || sv.DiaChi || record.diaChi || record.DiaChi),
      isActive: (nd.trangThai ?? nd.TrangThai ?? record.trangThai ?? record.TrangThai) === undefined ? true : ((nd.trangThai ?? nd.TrangThai ?? record.trangThai ?? record.TrangThai) === true || (nd.trangThai ?? nd.TrangThai ?? record.trangThai ?? record.TrangThai) === 'true' || (nd.trangThai ?? nd.TrangThai ?? record.trangThai ?? record.TrangThai) === 1 || (nd.trangThai ?? nd.TrangThai ?? record.trangThai ?? record.TrangThai) === '1')
    };

    return parsed;
  }

  onToggleStatus(student: any, newVal: boolean): void {
    const ma = student.maSinhVien || student.MaSinhVien;
    if (!ma) {
      this.message.error('Mã sinh viên không hợp lệ');
      return;
    }
    this.rowLoading[ma] = true as any;
    this.studentService.setStatus(ma, newVal).subscribe({
      next: () => {
        student.isActive = !!newVal;
        this.rowLoading[ma] = false as any;
        this.message.success(newVal ? 'Đã kích hoạt sinh viên' : 'Đã vô hiệu hóa sinh viên');
      },
      error: (err) => {
        this.rowLoading[ma] = false as any;
        this.message.error(err?.error?.message || 'Cập nhật trạng thái thất bại');
      }
    });
  }

  handleDelete(student: any): void {
    this.modal.confirm({
      nzTitle: `Vô hiệu hóa sinh viên "${student.hoTen || student.HoTen}"?`,
      nzContent: 'Bạn có chắc chắn muốn vô hiệu hóa sinh viên này không?',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzCancelText: 'Hủy',
      nzOnOk: () => {
        const maSV = student.maSinhVien || student.MaSinhVien;
        if (!maSV) {
          this.message.error('Mã sinh viên không hợp lệ');
          return;
        }

        this.rowLoading[maSV] = true;
        this.studentService.deleteStudent(maSV).subscribe({
          next: () => {
            this.message.success('Đã vô hiệu hóa sinh viên thành công');
            const idx = this.students.findIndex(it => (it.maSinhVien || it.MaSinhVien) === maSV);
            if (idx !== -1) this.students[idx] = { ...this.students[idx], isActive: false, trangThai: false, TrangThai: false, trangThaiUser: false };
            this.rowLoading[maSV] = false;
            this.loadStudents();
          },
          error: (err) => {
            this.rowLoading[maSV] = false;
            this.message.error(err?.error?.message || 'Vô hiệu hóa thất bại');
          }
        });
      }
    });
  }

  handleCancel(): void {
    this.isCreateModalVisible = false;
    this.isEditModalVisible = false;
  }
}
