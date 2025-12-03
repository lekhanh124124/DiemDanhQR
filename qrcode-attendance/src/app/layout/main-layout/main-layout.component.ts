import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { PermissionService } from '../../core/services/permission.service';
import { environment } from '../../../environments/environment';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit {
  isCollapsed = false;
  isMobile = false;
  userName = '';
  userRole = '';
  avatarUrl = ''
  defaultAvatar = 'assets/images/avatar.jpg';
  menuItems: any[] = [];
  currentTitle = 'Trang chủ'

  private adminTopOrder: string[] = ['trang chủ', 'quản lý người dùng', 'quản lý học phần', 'phân quyền', 'nhật ký hoạt động', 'nhật ký hệ thống'];

  private childOrderMap: Record<string, string[]> = {
    'quản lý người dùng': ['quản trị viên', 'giảng viên', 'sinh viên'],
    'quản lý học phần': ['môn học', 'lớp học phần', 'phòng học', 'buổi học'],
    'phân quyền': ['quyền']
  };

  private allowedIcons = new Set<string>([
    'menu-fold','menu-unfold','dashboard','form','user','idcard','read','team','calendar','file-text','bar-chart','qrcode','check-circle','pie-chart','history','book','bell','home','eye','eye-invisible','lock','user-add','logout','filter','plus','reload','sync','search','down','ordered-list','unordered-list','more','bars','edit','delete','upload','link'
  ]);

  private routerSub?: Subscription;
  private permissionsListener?: any;
  constructor(private authService: AuthService, private router: Router, private userService: UserService, private permissionService: PermissionService, private message: NzMessageService) {}

  ngOnInit() {
    const user = this.authService.getUser();
    console.log('User info:', user);
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.userName = user.hoTen || user.tenDangNhap || 'Người dùng';
    const rawRole = user?.roleCode || user?.phanQuyen?.codeQuyen || user?.phanQuyen?.CodeQuyen || user?.codeQuyen || user?.code || '';
    this.userRole = String(rawRole || '').toUpperCase();

    try {
      const path = user?.anhDaiDien || user?.anh || user?.avatar || '';
      if (path) {
        this.avatarUrl = this.computeAvatarUrlFromPath(path);
      }
    } catch (e) {}
    try {
      this.userService.getThongTinCaNhan().subscribe({
        next: (res: any) => {
          const data = res?.data || res;
          const nguoiDung = data?.nguoiDung || data || {};
          const path = nguoiDung?.anhDaiDien || nguoiDung?.anh || nguoiDung?.avatar || '';
          if (path) {
            const url = this.computeAvatarUrlFromPath(path);
            this.avatarUrl = `${url}${url.includes('?') ? '&' : '?'}t=${Date.now()}`;
          }
        },
        error: () => {}
      });
    } catch (e) {}

    // Menu theo role
    if (this.userRole && this.userRole.startsWith('ADMIN')) {
      this.menuItems = [];
    } else if (this.userRole === 'GV' || (this.userRole || '').startsWith('GV')) {
      this.menuItems = [
        { title: 'Trang chủ', route: '/giangvien', icon: 'home' },
        {
          title: 'Lớp giảng dạy',
          icon: 'book',
          children: [
            { title: 'Danh sách lớp học phần', route: '/giangvien/classes-giangday', icon: 'team' },
            { title: 'Thời khóa biểu giảng dạy', route: '/giangvien/class/teacher-schedule', icon: 'calendar' }
          ]
        },
        { title: 'Quản lý điểm danh', route: '/giangvien/attendance-manage', icon: 'check-circle' },
      ];

    } else if (this.userRole === 'SV') {
      this.menuItems = [
        { title: 'Trang chủ', route: '/sinhvien', icon: 'home' },
        {
          title: 'Lịch học',
          icon: 'book',
          children: [
            { title: 'Lịch theo tuần', route: '/sinhvien/class/student-schedule', icon: 'calendar' }
          ]
        },
        { title: 'Điểm danh bằng QR', route: '/sinhvien/attendance/scan', icon: 'qrcode' },
        { title: 'Lịch sử điểm danh', route: '/sinhvien/attendance/history', icon: 'history' },
      ];
    }

    this.routerSub = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((ev: any) => {
        const url = ev.urlAfterRedirects || this.router.url;
        this.setCurrentTitle(url);
      });

    this.setCurrentTitle(this.router.url);

    this.permissionsListener = () => {
      try { this.tryLoadMenu(this.authService.getUser()); } catch (e) {}
    };
    window.addEventListener('permissions:changed', this.permissionsListener as EventListener);

    this.tryLoadMenu(user);
  }

  onAvatarError(ev: Event) {
    try {
      const img = ev?.target as HTMLImageElement | null;
      if (img) img.src = this.defaultAvatar;
    } catch (e) {}
  }

  ngOnDestroy() {
    if (this.routerSub) {
      this.routerSub.unsubscribe();
    }
    try {
      if (this.permissionsListener) window.removeEventListener('permissions:changed', this.permissionsListener as EventListener);
    } catch (e) {}
  }
  private tryLoadMenu(userObj: any) {
    const roleParamMa = userObj?.MaQuyen || userObj?.maQuyen || userObj?.phanQuyen?.maQuyen || userObj?.phanQuyen?.MaQuyen || '';
    const roleParamCode = userObj?.roleCode || userObj?.phanQuyen?.codeQuyen || userObj?.phanQuyen?.CodeQuyen || userObj?.codeQuyen || '';
    const params: any = {};
    if (roleParamMa) params.maQuyen = roleParamMa;
    if (roleParamCode) params.codeQuyen = roleParamCode;
    params._t = Date.now();

    console.debug('[tryLoadMenu] roleParamMa=', roleParamMa, 'roleParamCode=', roleParamCode, 'params=', params);
    this.permissionService.getRoleFunctions(params).subscribe({
      next: (res: any) => {
        const items = Array.isArray(res?.data?.items) ? res.data.items : (Array.isArray(res?.data) ? res.data : (Array.isArray(res) ? res : []));
        console.debug('[tryLoadMenu] role-functions response items count=', Array.isArray(items) ? items.length : 'N/A', 'raw=', items);
        if (!Array.isArray(items) || items.length === 0) {
          console.warn('Role-functions returned no items');
          return;
        }

        const roleCode = String(this.userRole || '').trim();
        const filtered = items.filter((it: any) => {
          const pq = it?.phanQuyen || {};
          const code = String(pq?.codeQuyen || pq?.CodeQuyen || pq?.code || '').trim();
          const ma = String(pq?.maQuyen || pq?.MaQuyen || pq?.ma || '').trim();
          return (roleCode && (code === roleCode || ma === roleCode)) || !roleCode;
        });

        if (!Array.isArray(filtered) || filtered.length === 0) {
          console.warn('No role-functions entries for current role', { roleCode: this.userRole, filtered, items });
          return;
        }

        const nodes: Record<string, any> = {};
        for (const it of filtered) {
          const cn = it?.chucNang || {};
          const groupEnabled = String((it?.nhomChucNang?.trangThai || it?.nhomChucNang?.TrangThai) ?? '').toLowerCase() !== 'false';
          const funcFlagRaw = (it?.trangThai ?? it?.TrangThai) ?? (cn?.trangThai ?? cn?.TrangThai) ?? '';
          const funcEnabled = String(funcFlagRaw).toLowerCase() !== 'false';
          if (!groupEnabled || !funcEnabled) {
            console.debug('[tryLoadMenu] skipping function due to disabled flag', { chucNang: cn, nhomChucNang: it?.nhomChucNang, funcFlagRaw });
            continue;
          }
          const id = String(cn?.maChucNang || cn?.MaChucNang || cn?.ma || '').trim();
          if (!id) continue;
          const parentRaw = String(cn?.parentChucNangId || cn?.parent || cn?.parentId || '').trim();
          const parentId = (parentRaw && /^null$/i.test(parentRaw)) ? '' : parentRaw;
          const routeRaw = String(cn?.url || cn?.duongDan || cn?.duongdan || cn?.route || cn?.path || '').trim();
          const route = (routeRaw && /^null$/i.test(routeRaw)) ? '' : routeRaw;
          const rawIcon = String(cn?.icon || cn?.iconName || '').trim();
          nodes[id] = {
            id,
            title: String(cn?.tenChucNang || cn?.TenChucNang || cn?.ten || cn?.name || '').trim() || 'N/A',
            code: String(cn?.codeChucNang || cn?.CodeChucNang || cn?.code || '').trim(),
            route,
            icon: this.sanitizeIcon(rawIcon),
            parentId: parentId || ''
          };
        }

        const groups: any[] = [];
        const orphans: any[] = [];
        for (const id of Object.keys(nodes)) {
          const node = nodes[id];
            if (!node.parentId) {
            let grp = groups.find(g => g.title === node.title || (g.code && g.code === node.code));
            if (!grp) {
              grp = { title: node.title, code: node.code || '', icon: node.icon || '', children: [] };
              groups.push(grp);
            }
          } else {
            const parent = nodes[node.parentId];
            const item = { title: node.title, route: node.route || '', icon: node.icon || '' };
            if (parent) {
              let grp = groups.find(g => g.title === parent.title || (g.code && g.code === parent.code));
              if (!grp) {
                grp = { title: parent.title, code: parent.code || '', icon: parent.icon || '', children: [] };
                groups.push(grp);
              }
              grp.children.push(item);
            } else {
              orphans.push(item);
            }
          }
        }

        const built = [...orphans, ...groups];

        if (String(this.userRole || '').trim().startsWith('ADMIN')) {
          const adminDynamicTitles = ['Trang chủ', 'Quản lý người dùng', 'Quản lý học phần', 'Phân quyền', 'Nhật ký hệ thống'];
          const builtMap: Record<string, any[]> = {};
          const parentInfoMap: Record<string, any> = {};
          for (const g of groups) {
            if (g && g.title) {
              builtMap[g.title] = g.children || [];
              if (g.code) builtMap[`code:${g.code}`] = g.children || [];
              parentInfoMap[g.title] = g;
              if (g.code) parentInfoMap[`code:${g.code}`] = g;
            }
          }

          if (!(this.menuItems && this.menuItems.length)) {
            const builtAdmin = (groups || []).map((g: any) => {
              let defaultRoute: string | undefined = undefined;
              const t = String(g.title || '').trim().toLowerCase();
                if (t === 'trang chủ' || t === 'trang chu' || t === 'home') defaultRoute = '/admin';
              else if (t === 'nhật ký hệ thống' || t === 'nhat ky he thong') defaultRoute = '/admin/logs';
              else if (t === 'phân quyền' || t === 'phan quyen') defaultRoute = '/admin/permissions-new/roles';
                return { title: g.title, icon: this.sanitizeIcon(String(g.icon || '')), route: defaultRoute, children: (g.children || []) };
            });
            const existingRoutes = new Set((builtAdmin || []).map((i: any) => i.route).filter(Boolean));
            const orphansToPrepend = (orphans || []).filter((o: any) => o.route && !existingRoutes.has(o.route));
            this.menuItems = this.reorderAdminMenu([...orphansToPrepend, ...builtAdmin]);
            this.applyDefaultAdminIcons();
            return;
          }

          const merged = (this.menuItems || []).map((item: any) => {
            if (adminDynamicTitles.includes(item.title)) {
              const children = builtMap[item.title] || builtMap[`code:${(item.code||'').toString()}`];
              if (Array.isArray(children) && children.length > 0) {

                  const parentInfo = parentInfoMap[item.title] || parentInfoMap[`code:${(item.code||'').toString()}`];
                  const preservedRoute = item.route || (parentInfo && parentInfo.route) || undefined;
                  return { ...item, children, route: preservedRoute };
              }
              const foundKey = Object.keys(builtMap).find(k => k.toLowerCase() === item.title.toLowerCase() || (k.startsWith('code:') && k.slice(5).toLowerCase() === item.title.toLowerCase()));
              if (foundKey) return { ...item, children: builtMap[foundKey] };
              const parentInfo = parentInfoMap[item.title] || parentInfoMap[`code:${(item.code||'').toString()}`];
              if (parentInfo && parentInfo.route) {
                return { ...item, route: parentInfo.route };
              }
            }
            return { ...item, icon: this.sanitizeIcon(String(item.icon || '')) };
          });

          const existingRoutes = new Set((merged || []).map((i: any) => i.route).filter(Boolean));
          const orphansToPrepend = (orphans || []).filter((o: any) => o.route && !existingRoutes.has(o.route));

          this.menuItems = this.reorderAdminMenu([...orphansToPrepend, ...merged]);
          this.applyDefaultAdminIcons();
          console.debug('[tryLoadMenu] final menuItems (merged)', this.menuItems);
        } else {
          console.debug('[tryLoadMenu] non-admin, leaving static menuItems', this.menuItems);
        }
      },
      error: (err: any) => {
        console.error('Failed to load role-functions', err);
      }
    });
  }

  private norm(t: string): string {
    return String(t || '').trim().toLowerCase().replace(/\s+/g, ' ');
  }

  private reorderAdminMenu(items: any[]): any[] {
    if (!Array.isArray(items)) return items;
    const orderIndex: Record<string, number> = {};
    this.adminTopOrder.forEach((k, i) => orderIndex[k] = i);

    const keyFor = (title: string) => this.norm(title);

    for (const it of items) {
      const key = keyFor(it.title);
      const childOrder = this.childOrderMap[key];
      if (Array.isArray(it.children) && childOrder && childOrder.length) {
        const orderMap: Record<string, number> = {};
        childOrder.forEach((c, idx) => orderMap[this.norm(c)] = idx);
        it.children.sort((a: any, b: any) => {
          const ak = this.norm(a.title || a);
          const bk = this.norm(b.title || b);
          const ai = (orderMap[ak] !== undefined) ? orderMap[ak] : 9999;
          const bi = (orderMap[bk] !== undefined) ? orderMap[bk] : 9999;
          if (ai !== bi) return ai - bi;
          return (a.title || '').localeCompare(b.title || '');
        });
      }
    }

    const known: any[] = [];
    const unknown: any[] = [];
    for (const it of items) {
      const k = keyFor(it.title);
      if (orderIndex[k] !== undefined) known.push(it);
      else unknown.push(it);
    }
    known.sort((a: any, b: any) => {
      return orderIndex[keyFor(a.title)] - orderIndex[keyFor(b.title)];
    });
    unknown.sort((a: any, b: any) => (a.title || '').localeCompare(b.title || ''));
    return [...known, ...unknown];
  }
  private computeAvatarUrlFromPath(path: string): string {
    const p = String(path || '').trim();
    if (!p) return '';
    if (p.startsWith('http') || p.startsWith('data:')) return p;
    const apiHost = (environment.apiBase || '').replace(/\/api\/?$/, '').replace(/\/$/, '');
    const seg = p.startsWith('/') ? p : `/${p}`;
    return `${apiHost}${seg}`;
  }

  private sanitizeIcon(iconName: string): string {
    let n = String(iconName || '').trim();
    if (!n) return '';

    n = n.replace(/-o$/i, ''); 

    const key = n.replace(/\s+/g, '').toLowerCase();

    const kebab = n.replace(/([a-z0-9])([A-Z])/g, '$1-$2').replace(/[_\s]+/g, '-').toLowerCase();
    if (this.allowedIcons.has(kebab)) return kebab;

    if (this.allowedIcons.has(key)) return key;

    const stripped = key.replace(/outline$/i, '').replace(/-outline$/i, '');
    if (this.allowedIcons.has(stripped)) return stripped;

    console.debug('[sanitizeIcon] unknown icon', iconName, '-> falling back to file-text');
    return 'file-text';
  }

  private applyDefaultAdminIcons() {
    if (!Array.isArray(this.menuItems)) return;
    const map: Record<string, string> = {
      'trang chủ': 'home',
      'quản lý người dùng': 'team',
      'quản trị viên': 'user-add',
      'giảng viên': 'idcard',
      'sinh viên': 'user',
      'quản lý học phần': 'book',
      'môn học': 'read',
      'lớp học phần': 'ordered-list',
      'phòng học': 'link',
      'buổi học': 'calendar',
      'phân quyền': 'lock',
      'quyền': 'file-text',
      'nhật ký hệ thống': 'history'
    };

    const applyTo = (it: any) => {
      try {
        const key = this.norm(String(it.title || ''));
        if ((!it.icon || it.icon === 'file-text') && map[key]) {
          it.icon = this.sanitizeIcon(map[key]);
        }
        if (Array.isArray(it.children)) {
          for (const c of it.children) applyTo(c);
        }
      } catch (e) {}
    };

    for (const it of this.menuItems) applyTo(it);
  }

  setCurrentTitle(url: string) {
    const cleanUrl = (url || '').split('?')[0].split('#')[0];
    const allRoutes: { title: string, route: string }[] = [];

    for (let item of this.menuItems) {
      if (item.children) {
        for (let child of item.children) {
          allRoutes.push({ title: child.title, route: child.route });
        }
      }
      if (item.route) {
        allRoutes.push({ title: item.title, route: item.route });
      }
    }

    allRoutes.sort((a, b) => b.route.length - a.route.length);

    for (let r of allRoutes) {
      if (cleanUrl.startsWith(r.route)) {
        this.currentTitle = r.title;
        return;
      }
    }
    this.currentTitle = 'Trang chủ';
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  onBreakpoint(ev: boolean | MediaQueryListEvent | Event | any) {
    let isBelow = false;
    if (typeof ev === 'boolean') {
      isBelow = ev;
    } else if (ev && typeof (ev as any).matches === 'boolean') {
      isBelow = !!(ev as any).matches;
    } else {
      isBelow = !!ev;
    }

    this.isMobile = isBelow;
    if (this.isMobile) this.isCollapsed = true;
  }


  goProfile() {
    this.router.navigate(['/auth/profile']);
  }

  goChangePassword() {
    this.router.navigate(['/auth/change-password']);
  }

  logout() {
    try { this.message.success('Đăng xuất thành công'); } catch (e) {}
    this.authService.logout();
  }

  goHome() {
    try {
      const role = String(this.userRole || '').trim();
      if (role && role.startsWith('ADMIN')) this.router.navigate(['/admin']);
      else if (role && (role === 'GV' || role.startsWith('GV'))) this.router.navigate(['/giangvien']);
      else if (role && (role === 'SV' || role.startsWith('SV'))) this.router.navigate(['/sinhvien']);
      else this.router.navigate(['/']);
    } catch (e) {
      this.router.navigate(['/']);
    }
  }
}
