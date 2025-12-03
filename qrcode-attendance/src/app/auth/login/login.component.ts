import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  tenDangNhap: string = '';
  matKhau: string = '';
  loading = false;
  errorMessage = '';
  showPassword = false;

  constructor(private authService: AuthService, private router: Router, private message: NzMessageService) {}

  togglePassword() {
    this.showPassword=!this.showPassword
  }

  onLogin() {
  this.loading = true;
  this.errorMessage = '';

  this.authService.login(this.tenDangNhap, this.matKhau).subscribe({
    next: (res) => {
      this.loading = false;
      const data = res?.data || res;
      const accessToken = data?.accessToken;
      const refreshToken = data?.refreshToken;
      const nguoiDung = data?.nguoiDung || {};
      const phanQuyen = data?.phanQuyen || {};
      const rawRole = phanQuyen?.codeQuyen || phanQuyen?.CodeQuyen || phanQuyen?.code || phanQuyen?.maQuyen || phanQuyen?.ma || '';
      const role = String(rawRole || '').toUpperCase();

      this.authService.saveTokens(accessToken, refreshToken);
      const flatUser = { ...data, ...nguoiDung, phanQuyen, roleCode: role };
      this.authService.saveUser(flatUser);

      try { this.message.success('Đăng nhập thành công'); } catch (e) {}

      let target = '/';
      if (role && role.startsWith('ADMIN')) target = '/admin';
      else if (role && (role.startsWith('GV') || role === 'GIANGVIEN' || role === 'GIANG_VIEN')) target = '/giangvien';
      else if (role && (role.startsWith('SV') || role === 'S' || role === 'SINHVIEN')) target = '/sinhvien';

      try {
        console.debug('[Login] role=', role, 'navigating to', target);
        this.router.navigateByUrl(target).then((ok) => {
          console.debug('[Login] navigation result:', ok);
          if (!ok) {
            this.router.navigate([target], { replaceUrl: true }).catch((e) => console.error('[Login] fallback navigate error', e));
          }
        }).catch((e) => {
          console.error('[Login] navigate error', e);
        });
      } catch (e) {
        console.error('[Login] navigation thrown', e);
      }
    },

    error: (err) => {
      this.loading = false;
      const msg = err.error?.message || 'Đăng nhập thất bại';
      this.errorMessage = msg;
      try { this.message.error(msg); } catch (e) {}
    }
  });
}

}
