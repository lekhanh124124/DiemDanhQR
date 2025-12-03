import { Component, OnInit } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
})
export class ProfileComponent implements OnInit {
  userInfo: any;
  roleCode: string = '';
  avatarUrl: string = 'assets/images/avatar.jpg';
  isEditVisible = false;
  saving = false;
  editModel: any = {};
  avatarFile?: File | null = null;
  avatarPreview: string | null = null;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private msg: NzMessageService
  ) {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  onAvatarError(event: any): void {
    try {
      const img = event?.target as HTMLImageElement;
      if (img) img.src = 'assets/images/avatar.jpg';
    } catch (e) {
    }
  }

  openEdit(): void {
    const s = (v: any) => this.sanitizeString(v);
    this.editModel = {
      Email: s(this.userInfo?.email ?? this.userInfo?.Email),
      SoDienThoai: s(this.userInfo?.soDienThoai ?? this.userInfo?.SoDienThoai),
      NgaySinh: s(this.userInfo?.ngaySinh ?? this.userInfo?.NgaySinh),
      DiaChi: s(this.userInfo?.diaChi ?? this.userInfo?.DiaChi),
      TrangThai: this.userInfo?.trangThai ?? this.userInfo?.TrangThai ?? true
    };
    this.avatarFile = null;
    this.isEditVisible = true;
  }

  private sanitizeString(v: any): string {
    if (v === undefined || v === null) return '';
    const s = String(v).trim();
    if (s.toLowerCase() === 'null') return '';
    return s;
  }

  onFileChange(event: any): void {
    const f = event?.target?.files?.[0];
    if (f) this.avatarFile = f;
    else this.avatarFile = null;
    if (this.avatarFile) {
      const reader = new FileReader();
      reader.onload = (e: any) => { this.avatarPreview = e.target.result; };
      reader.readAsDataURL(this.avatarFile);
    } else {
      this.avatarPreview = null;
    }
  }

  saveProfile(): void {
    const payload: any = { ...this.editModel };
    const tenDangNhap = this.userInfo?.tenDangNhap || this.userInfo?.TenDangNhap || this.userInfo?.username || '';
    const maNguoiDung = this.userInfo?.maNguoiDung || this.userInfo?.MaNguoiDung || '';
    if (tenDangNhap) payload.TenDangNhap = String(tenDangNhap);
    if (maNguoiDung) payload.MaNguoiDung = String(maNguoiDung);
    if (this.avatarFile) payload.AnhDaiDien = this.avatarFile;
    this.saving = true;
    this.userService.updateUserProfile(payload).subscribe({ next: () => {
      this.saving = false;
      this.isEditVisible = false;
      this.msg.success('Cập nhật thông tin cá nhân thành công');
      this.loadUserInfo();
    }, error: (err) => {
      this.saving = false;
      this.msg.error(err?.error?.Message || err?.message || 'Cập nhật thất bại');
    } });
  }

  private computeAvatarUrlFromPath(path: string): string {
    const p = String(path || '').trim();
    if (!p) return 'assets/images/avatar.jpg';
    if (p.startsWith('http') || p.startsWith('data:')) return p;
    const apiHost = (environment.apiBase || '').replace(/\/api\/?$/, '').replace(/\/$/, '');
    const seg = p.startsWith('/') ? p : `/${p}`;
    const ts = Date.now();
    return `${apiHost}${seg}?t=${ts}`;
  }

  loadUserInfo(): void {
    const currentUser = this.authService.getUser();
    if (!currentUser) return;

    this.roleCode = currentUser.roleCode;

    this.userService.getThongTinCaNhan().subscribe({
      next: (res) => {
        if (res && res.data !== undefined) {
          const data = res.data || {};
          const nguoiDung = data.nguoiDung || data.NguoiDung || {};
          const giangVien = data.giangVien || data.GiangVien || {};
          const sinhVien = data.sinhVien || data.SinhVien || {};
          const khoa = data.khoa || data.Khoa || data.department || {};
          const nganh = data.nganh || data.Nganh || (sinhVien && (sinhVien.nganh || sinhVien.Nganh)) || {};
          const phanQuyen = data.phanQuyen || data.PhanQuyen || {};

          const flattened: any = { ...nguoiDung };
          Object.assign(flattened, giangVien, sinhVien);

          if (khoa) {
            flattened.khoa = (khoa.tenKhoa || khoa.ten || khoa.codeKhoa || khoa.code || khoa.maKhoa) || khoa;
          }

          if (nganh) {
            flattened.nganh = (nganh.tenNganh || nganh.TenNganh || nganh.codeNganh || nganh.code || nganh.maNganh) || nganh;
          }

          flattened.roleCode = (phanQuyen?.codeQuyen || phanQuyen?.code || flattened.roleCode || this.roleCode) as string;

          if (res.fallback) flattened._isFallback = true;

          this.userInfo = flattened;
          this.sanitizeDisplayFields(this.userInfo);
          this.avatarUrl = this.computeAvatarUrlFromPath(flattened.anhDaiDien || flattened.anh || '');
        } else {
            this.userInfo = res;
            this.sanitizeDisplayFields(this.userInfo);
            this.avatarUrl = this.computeAvatarUrlFromPath(res?.anhDaiDien || res?.anh || '');
        }
      },
      error: (err) => {
        console.error('Lỗi khi tải thông tin người dùng:', err);
        this.msg.error('Không thể tải thông tin người dùng (lỗi server). Hiển thị thông tin cơ bản.');
        const fallback = currentUser || {};
        this.userInfo = { ...(fallback || {}), _isFallback: true };
        this.avatarUrl = this.computeAvatarUrlFromPath(this.userInfo?.anhDaiDien || this.userInfo?.anh || '');
      },
    });
  }

  private sanitizeDisplayFields(obj: any): void {
    if (!obj || typeof obj !== 'object') return;
    const keys = ['email', 'soDienThoai', 'gioiTinh', 'ngaySinh', 'diaChi', 'hoTen', 'maGiangVien', 'maSinhVien', 'khoa', 'nganh', 'namNhapHoc'];
    for (const k of keys) {
      const v = obj[k] ?? obj[this.toPascalOrCamel(k)];
      if (v === undefined || v === null) obj[k] = '';
      else if (String(v).toLowerCase() === 'null') obj[k] = '';
      else obj[k] = v;
    }
  }

  private toPascalOrCamel(key: string): string {
    if (!key) return key;
    return key.charAt(0).toUpperCase() + key.slice(1);
  }
}
