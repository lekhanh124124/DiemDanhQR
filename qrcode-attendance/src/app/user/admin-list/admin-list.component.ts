import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { UserService } from '../../core/services/user.service';
import { PermissionService } from '../../core/services/permission.service';

@Component({
  selector: 'app-admin-user-list',
  templateUrl: './admin-list.component.html',
  styleUrls: ['./admin-list.component.scss']
})
export class AdminListComponent implements OnInit {
  loading = false;
  users: any[] = [];
  page = 1;
  pageSize = 10;
  total = 0;

  search = '';
  filterMaQuyen: any = '';
  filterTrangThaiUser: boolean | undefined = undefined;
  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  isCreateModalVisible = false;
  isEditModalVisible = false;
  isEditing = false;
  editingUser: any = null;
  form!: FormGroup;
  saving = false;
  avatarFile?: File | null = null;
  roles: any[] = [];
  sortBy: string = 'hoTen';
  sortDir: 'asc' | 'desc' = 'asc';

  constructor(private userService: UserService, private fb: FormBuilder, private msg: NzMessageService, private permissionService: PermissionService, private modalService: NzModalService) {
    this.form = this.fb.group({
      TenDangNhap: [''],
      MaQuyen: [''],
      TrangThai: [undefined],
      HoTen: [''],
      GioiTinh: [''],
      Email: [''],
      SoDienThoai: [''],
      NgaySinh: [''],
      DiaChi: ['']
    });
  }

  ngOnInit(): void {
    this.load();
    this.loadRoles();
  }

  private loadRoles(): void {
    try {
      this.permissionService.getRoles({ page: 1, pageSize: 200, sortBy: 'TenQuyen', sortDir: 'ASC' }).subscribe({ next: (res: any) => {
        const items = res?.data?.items || res?.data || [];
        this.roles = (items || []).map((r: any) => {
          const p = r?.phanQuyen || r?.PhanQuyen || r?.phan_quyen || r || {};
          const rawMa = p?.maQuyen ?? p?.MaQuyen ?? p?.codeQuyen ?? p?.CodeQuyen ?? '';
          const maQuyen = (rawMa !== null && rawMa !== undefined && String(rawMa).trim() !== '' && !Number.isNaN(Number(rawMa))) ? Number(rawMa) : String(rawMa);
          return {
            maQuyen,
            codeQuyen: p?.codeQuyen || p?.CodeQuyen || p?.code || '',
            tenQuyen: p?.tenQuyen || p?.TenQuyen || p?.ten || ''
          };
        });
      }, error: () => { this.roles = []; } });
    } catch (e) { this.roles = []; }
  }

  load(): void {
    this.loading = true;
    const maQuyenParam = this.filterMaQuyen && String(this.filterMaQuyen).trim() !== '' ? this.filterMaQuyen : undefined;
    const codeQuyenParam = undefined;

    console.debug('[AdminList] load called', { page: this.page, pageSize: this.pageSize, filterMaQuyen: this.filterMaQuyen });

    this.userService.listUsers({
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDir: this.sortDir,
      tenDangNhap: undefined,
      hoTen: this.search && this.search.trim() !== '' ? this.search.trim() : undefined,
      maQuyen: maQuyenParam,
      codeQuyen: codeQuyenParam,
      trangThai: this.filterTrangThaiUser
    }).subscribe({ next: (res: any) => {
      const raw = res?.data?.items || res?.data || res || [];
      const mapped = (raw || []).map((it: any) => {
        const nd = it?.nguoiDung || it?.NguoiDung || it || {};
        const pq = it?.phanQuyen || it?.PhanQuyen || it?.phan_quyen || {};
        const tenDangNhap = nd?.tenDangNhap || nd?.TenDangNhap || nd?.username || nd?.maNguoiDung || '';
        const hoTen = nd?.hoTen || nd?.HoTen || nd?.name || '';
        const email = nd?.email || nd?.Email || '';
        const maQuyen = pq?.maQuyen || pq?.MaQuyen || pq?.codeQuyen || pq?.CodeQuyen || '';
        const tenQuyen = pq?.tenQuyen || pq?.TenQuyen || pq?.ten || '';
        const rawStatus = nd?.trangThai ?? nd?.TrangThai ?? it?.trangThai ?? it?.TrangThai ?? nd?.isActive ?? it?.isActive;
        const parseBool = (v: any): boolean => {
          if (v === true || v === 'true' || v === 'True' || v === 1 || v === '1') return true;
          if (v === false || v === 'false' || v === 'False' || v === 0 || v === '0') return false;
          return false;
        };
        const isActive = rawStatus !== undefined ? parseBool(rawStatus) : true;
        return {
          tenDangNhap,
          hoTen,
          email,
          maQuyen,
          tenQuyen,
          isActive,
          trangThaiUser: isActive,
          _raw: it
        };
      });
      this.users = mapped;
      const pageFromRes = res?.data?.page ?? res?.page;
      const pageSizeFromRes = res?.data?.pageSize ?? res?.pageSize;
      const totalFromRes = res?.data?.totalRecords ?? res?.data?.total;
      this.page = pageFromRes ? Number(pageFromRes) : this.page;
      this.pageSize = pageSizeFromRes ? Number(pageSizeFromRes) : this.pageSize;
      this.total = totalFromRes ? Number(totalFromRes) : this.users.length;
      this.loading = false;
    }, error: (err) => { this.loading = false; this.msg.error('Không thể tải danh sách người dùng'); } });
  }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }
    this.page = 1;
    this.load();
  }

  onCurrentPageDataChange(listOfCurrentPageData: readonly any[]): void {
    this.listOfCurrentPageData = (listOfCurrentPageData || []) as any[];
    this.refreshCheckedStatus();
  }

  onAllChecked(checked: boolean): void {
    this.listOfCurrentPageData.forEach(item => this.updateCheckedSet(item.tenDangNhap || item.TenDangNhap || item.maNguoiDung || item.MaNguoiDung, checked));
    this.refreshCheckedStatus();
  }

  onItemChecked(id: string, checked: boolean): void {
    this.updateCheckedSet(id, checked);
    this.refreshCheckedStatus();
  }

  private updateCheckedSet(id: string, checked: boolean): void {
    if (!id) return;
    if (checked) this.setOfCheckedId.add(id);
    else this.setOfCheckedId.delete(id);
  }

  private refreshCheckedStatus(): void {
    this.checked = this.listOfCurrentPageData.length > 0 && this.listOfCurrentPageData.every(item => this.setOfCheckedId.has(item.tenDangNhap || item.TenDangNhap || item.maNguoiDung || item.MaNguoiDung));
    this.indeterminate = this.listOfCurrentPageData.some(item => this.setOfCheckedId.has(item.tenDangNhap || item.TenDangNhap || item.maNguoiDung || item.MaNguoiDung)) && !this.checked;
  }

  openCreate(): void {
    this.isEditing = false;
    this.editingUser = null;
    this.form.reset();
    this.avatarFile = null;
    this.isCreateModalVisible = true;
  }

  openEdit(u: any): void {
    this.isEditing = true;
    this.editingUser = u;
    const source = u && u._raw ? (u._raw.nguoiDung || u._raw.NguoiDung || u._raw) : u || {};
    this.form.patchValue({
      TenDangNhap: source?.tenDangNhap || source?.TenDangNhap || source?.username || source?.maNguoiDung || '',
      MaQuyen: (u && u._raw && (u._raw.phanQuyen || u._raw.PhanQuyen)) ? (u._raw.phanQuyen?.maQuyen || u._raw.PhanQuyen?.MaQuyen || '') : (u?.maQuyen || u?.MaQuyen || ''),
      TrangThai: ((): any => {
        const rs = source?.trangThai ?? source?.TrangThai ?? u?.trangThai ?? u?.TrangThai ?? source?.isActive ?? u?.isActive;
        if (rs === true || rs === 'true' || rs === 'True' || rs === 1 || rs === '1') return true;
        if (rs === false || rs === 'false' || rs === 'False' || rs === 0 || rs === '0') return false;
        return undefined;
      })(),
      HoTen: source?.hoTen || source?.HoTen || source?.name || '',
      GioiTinh: source?.gioiTinh || source?.GioiTinh || null,
      Email: source?.email || source?.Email || '',
      SoDienThoai: source?.soDienThoai || source?.SoDienThoai || source?.phone || '',
      NgaySinh: source?.ngaySinh || source?.NgaySinh || '',
      DiaChi: source?.diaChi || source?.DiaChi || ''
    });
    this.avatarFile = null;
    this.isEditModalVisible = true;
  }

  refreshList(): void {
    this.search = '';
    this.clearAdvanced();
    this.page = 1;
    this.load();
  }

  clearAdvanced(): void {
    this.filterMaQuyen = '';
    this.filterTrangThaiUser = undefined as any;
  }

  onFileChange(event: any): void {
    const f = event?.target?.files?.[0];
    if (f) this.avatarFile = f;
    else this.avatarFile = null;
  }

  create(): void {
    const v = this.form.value;
    this.saving = true;
    const payload: any = { ...v };
    if (this.avatarFile) payload.AnhDaiDien = this.avatarFile;
    payload.TrangThai = true;
    this.userService.createUser(payload).subscribe({ next: (res: any) => {
      try {
        const created = res?.data || res;
        const pq = created?.phanQuyen || created?.PhanQuyen || created?.phan_quyen || {};
        const rawMa = pq?.maQuyen ?? pq?.MaQuyen ?? pq?.codeQuyen ?? pq?.CodeQuyen ?? '';
        const maVal = (rawMa !== null && rawMa !== undefined && String(rawMa).trim() !== '' && !Number.isNaN(Number(rawMa))) ? Number(rawMa) : String(rawMa);
        if (maVal) {
          this.filterMaQuyen = maVal;
          console.debug('[AdminList] create response, setting filterMaQuyen=', maVal, 'created=', created);
        } else {
          console.debug('[AdminList] create response, no maQuyen found in response', 'created=', created);
        }
      } catch (e) {}

      this.msg.success('Tạo người dùng thành công');
      this.saving = false;
      this.isCreateModalVisible = false;
      this.load();
    }, error: () => { this.saving = false; this.msg.error('Tạo thất bại'); } });
  }

  update(): void {
    const v = this.form.value;
    this.saving = true;
    const payload: any = { ...v };
    if (this.avatarFile) payload.AnhDaiDien = this.avatarFile;
    if (payload.TrangThai !== undefined && payload.TrangThai !== null) {
      payload.TrangThaiUser = payload.TrangThai;
    }
    this.userService.updateUserProfile(payload).subscribe({ next: () => {
      this.msg.success('Cập nhật thành công');
      this.saving = false;
      this.isEditModalVisible = false;
      this.load();
    }, error: (err) => { this.saving = false; this.msg.error('Cập nhật thất bại'); } });
  }

  onSearch(): void { this.page = 1; this.load(); }

  onPageChange(pageIndex: number): void { this.page = pageIndex; this.load(); }

  deleteSelected(): void {
    if (this.setOfCheckedId.size === 0) return;
    this.modalService.confirm({
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} người dùng đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các người dùng đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzCancelText: 'Hủy',
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(id => {
          const payload: any = {};
          if (id !== undefined && id !== null && String(id).trim() !== '' && !Number.isNaN(Number(String(id)))) {
            payload.MaNguoiDung = id;
          } else {
            payload.TenDangNhap = id;
          }
          payload.TrangThaiUser = false;
          payload.TrangThai = false;
          this.userService.updateUserProfile(payload).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(id);
              const idx = this.users.findIndex(u => (u.tenDangNhap || u.TenDangNhap || u.maNguoiDung || u.MaNguoiDung) === id);
              if (idx !== -1) this.users[idx] = { ...this.users[idx], trangThaiUser: false, isActive: false };
              if (remaining === 0) {
                this.msg.success('Đã vô hiệu hóa các người dùng đã chọn');
                this.load();
              }
            },
            error: () => {
              remaining--;
              if (remaining === 0) this.load();
            }
          });
        });
      }
    });
  }

  handleDisable(u: any): void {
    const label = u.hoTen || u.HoTen || u.tenDangNhap || u.TenDangNhap || (u._raw && (u._raw.nguoiDung?.tenDangNhap || u._raw.NguoiDung?.TenDangNhap)) || '';
    this.modalService.confirm({
      nzTitle: `Vô hiệu hóa người dùng "${label}"?`,
      nzContent: 'Bạn có chắc chắn muốn vô hiệu hóa người dùng này?',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzCancelText: 'Hủy',
      nzOnOk: () => {
        const key = u.tenDangNhap || u.TenDangNhap || u.maNguoiDung || u.MaNguoiDung || (u._raw && (u._raw.nguoiDung?.tenDangNhap || u._raw.NguoiDung?.TenDangNhap || u._raw.nguoiDung?.maNguoiDung || u._raw.NguoiDung?.MaNguoiDung));
        const payload: any = { TrangThaiUser: false, TrangThai: false };
        if (key !== undefined && key !== null && String(key).trim() !== '' && !Number.isNaN(Number(String(key)))) payload.MaNguoiDung = key;
        else payload.TenDangNhap = key;

        this.userService.updateUserProfile(payload).subscribe({
          next: () => {
            this.msg.success('Đã vô hiệu hóa người dùng thành công');
            const idx = this.users.findIndex(x => (x.tenDangNhap || x.TenDangNhap || x.maNguoiDung || x.MaNguoiDung) === key);
            if (idx !== -1) this.users[idx] = { ...this.users[idx], trangThaiUser: false, isActive: false };
            this.load();
          },
          error: (err) => {
            this.msg.error(err?.error?.message || 'Vô hiệu hóa thất bại');
          }
        });
      }
    });
  }
}
