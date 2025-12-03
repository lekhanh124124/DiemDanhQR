import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RoleGuard } from './core/guards/role.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

const routes: Routes = [

  { path: 'auth', loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule) },

  {
    path: 'admin',
    component: MainLayoutComponent,
    canActivate: [RoleGuard],
    data: { roles: ['ADMIN'] },
    children: [
      { path: '', loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule) },
      { path: 'user', loadChildren: () => import('./user/user.module').then(m => m.UserModule) },
      { path: 'class', loadChildren: () => import('./class/class.module').then(m => m.ClassModule)},
      { path: 'permissions-new', loadChildren: () => import('./permission/permission.module').then(m => m.PermissionModule) }
    ]
  },

  {
    path: 'giangvien',
    component: MainLayoutComponent,
    canActivate: [RoleGuard],
    data: { roles: ['GV'] },
    children: [
      { path: '', loadChildren: () => import('./giangvien/giangvien.module').then(m => m.GiangvienModule) },
      { path: 'class', loadChildren: () => import('./class/class.module').then(m => m.ClassModule) }
    ]
  },

  {
    path: 'sinhvien',
    component: MainLayoutComponent,
    canActivate: [RoleGuard],
    data: { roles: ['SV'] },
    children: [
      { path: '', loadChildren: () => import('./sinhvien/sinhvien.module').then(m => m.SinhvienModule) },
      { path: 'class', loadChildren: () => import('./class/class.module').then(m => m.ClassModule) },
      { path: 'attendance', loadChildren: () => import('./attendance/attendance.module').then(m => m.AttendanceModule) }
    ]
  },

  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: 'auth/login' }
];


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
