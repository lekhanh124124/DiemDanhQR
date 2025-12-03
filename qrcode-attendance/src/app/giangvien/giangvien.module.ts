import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GiangvienRoutingModule } from './giangvien-routing.module';
import { QRCodeModule } from 'angularx-qrcode';

import { DashboardComponent } from './dashboard/dashboard.component';
import { ClassesGiangdayComponent } from './classes-giangday/classes-giangday.component';
import { ClassSectionDetailComponent } from './class-section-detail/class-section-detail.component';
import { QrCreateComponent } from './qr-create/qr-create.component';
import { AttendanceManageComponent } from './attendance-manage/attendance-manage.component';

import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
@NgModule({
  declarations: [
    DashboardComponent,
    ClassesGiangdayComponent,
    ClassSectionDetailComponent,
    QrCreateComponent,
    AttendanceManageComponent,
  ],
  imports: [
    CommonModule,
    GiangvienRoutingModule,
    QRCodeModule,
    FormsModule,

    NzIconModule,
    NzSpinModule,
    NzTableModule,
    NzButtonModule,
    NzPaginationModule,
    NzTabsModule,
    NzTagModule,
    NzSelectModule,
    NzRadioModule,
    NzDropDownModule,
    NzInputModule,
    NzModalModule,
    NzMessageModule,
    SharedModule
  ]
})
export class GiangvienModule { }




