import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const user = this.authService.getUser();
    const allowedRoles = route.data['roles'] as string[];

    if (!user) {
      console.warn('RoleGuard: no user -> redirect to login');
      this.router.navigate(['/auth/login']);
      return false;
    }

    const roleRaw = user.roleCode || user.phanQuyen?.codeQuyen || user.codeQuyen || user.vaiTro || '';
    const role = String(roleRaw || '').toUpperCase();
    console.log('RoleGuard: user role =', role, 'allowed =', allowedRoles);

    if (role && Array.isArray(allowedRoles) && allowedRoles.length > 0) {
      const ok = allowedRoles.some((ar: string) => {
        if (!ar) return false;
        const a = String(ar || '').toUpperCase();
        if (role === a) return true;
        if (role.startsWith(a)) return true;
        return false;
      });
      if (ok) return true;
    }

    console.warn('RoleGuard: role not allowed -> redirect to login');
    this.router.navigate(['/auth/login']);
    return false;
  }
}
