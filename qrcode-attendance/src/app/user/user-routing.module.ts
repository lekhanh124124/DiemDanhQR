import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RoleGuard } from '../core/guards/role.guard';
import { GiangvienListComponent } from './giangvien-list/giangvien-list.component';
import { SinhvienListComponent } from './sinhvien-list/sinhvien-list.component';
import { AdminListComponent } from './admin-list/admin-list.component';

const routes: Routes = [
  { path: '', redirectTo: 'user-list', pathMatch: 'full' },
  { path: 'giangvien-list', component: GiangvienListComponent },
  { path: 'admin-list', component: AdminListComponent, canActivate: [RoleGuard], data: { roles: ['ADMIN'] } },
  { path: 'sinhvien-list', component: SinhvienListComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
