import { Component } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { NzMessageService } from 'ng-zorro-antd/message';
import { Router } from '@angular/router';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
  matKhauCu = '';
  matKhauMoi = '';
  xacNhanMatKhau = '';
  loading = false;

  showOld = false;
  showNew = false;
  showConfirm = false;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private message: NzMessageService,
    private router: Router
  ) {}

  doiMatKhau() {
    if (!this.matKhauCu || !this.matKhauMoi) {
      this.message.warning('Vui lòng nhập đầy đủ mật khẩu cũ và mới!');
      return;
    }

    if (this.matKhauMoi !== this.xacNhanMatKhau) {
      this.message.error('Xác nhận mật khẩu không khớp!');
      return;
    }

    if ((this.matKhauMoi || '').length < 6) {
      this.message.warning('Mật khẩu mới phải có ít nhất 6 ký tự.');
      return;
    }

    const token = this.authService.getToken();
    if (!token) {
      this.message.error('Bạn cần đăng nhập lại!');
      return;
    }

    this.loading = true;
    this.authService.changePassword({ matkhaucu: this.matKhauCu, matkhaumoi: this.matKhauMoi }).subscribe({
      next: () => {
        this.message.success('Đổi mật khẩu thành công!');
        this.loading = false;
        this.matKhauCu = this.matKhauMoi = this.xacNhanMatKhau = '';
        try {
          setTimeout(() => {
            try {
              const user = this.authService.getUser() || {};
              const role = String(user.roleCode || user.role || '').trim();
              if (role === 'ADMIN') this.router.navigate(['/admin']);
              else if (role === 'GV') this.router.navigate(['/giangvien']);
              else if (role === 'SV') this.router.navigate(['/sinhvien']);
              else this.router.navigate(['/']);
            } catch (e) {}
          }, 0);
        } catch (e) {}
      },
      error: (err) => {
        console.error(err);
        this.message.error(err.error?.message || 'Đổi mật khẩu thất bại!');
        this.loading = false;
      }
    });
  }
}
