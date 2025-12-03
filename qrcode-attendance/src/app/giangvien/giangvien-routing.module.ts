import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from '../giangvien/dashboard/dashboard.component';
import { ClassSectionDetailComponent } from './class-section-detail/class-section-detail.component';
import { ClassesGiangdayComponent } from './classes-giangday/classes-giangday.component';
import { AttendanceManageComponent } from './attendance-manage/attendance-manage.component';
import { QrCreateComponent } from './qr-create/qr-create.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'classes-giangday', component: ClassesGiangdayComponent },
  { path: 'class-section-detail/:id', component: ClassSectionDetailComponent },
  { path: 'attendance-manage', component: AttendanceManageComponent},
  { path: 'qr-create', component: QrCreateComponent},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GiangvienRoutingModule { }
