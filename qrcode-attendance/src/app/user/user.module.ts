import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { UserRoutingModule } from './user-routing.module';
import { GiangvienListComponent } from './giangvien-list/giangvien-list.component';
import { SinhvienListComponent } from './sinhvien-list/sinhvien-list.component';
import { AdminListComponent } from './admin-list/admin-list.component';

import { NzCardModule } from 'ng-zorro-antd/card';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzBadgeModule } from 'ng-zorro-antd/badge';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    GiangvienListComponent,
    SinhvienListComponent,
    AdminListComponent
  ],
  imports: [
    CommonModule,
    UserRoutingModule,
    FormsModule,
    ReactiveFormsModule,

    NzCardModule,
    NzFormModule,
    NzInputModule,
    NzButtonModule,
    NzTableModule,
    NzSpinModule,
    NzMessageModule,
    NzIconModule,
    NzModalModule,
    NzTagModule,
    NzPaginationModule,
    NzSelectModule,
    NzBadgeModule,
    NzRadioModule,
    NzMenuModule,
    NzDropDownModule,
    SharedModule
  ]
})
export class UserModule { }
