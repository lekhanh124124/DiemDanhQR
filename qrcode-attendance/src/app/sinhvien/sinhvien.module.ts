import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SinhvienRoutingModule } from './sinhvien-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ScheduleComponent } from './schedule/schedule.component';

import { NzIconModule } from 'ng-zorro-antd/icon';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTabsModule } from 'ng-zorro-antd/tabs';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';

@NgModule({
  declarations: [
  DashboardComponent,
  ScheduleComponent,
  ],
  imports: [
    CommonModule,
    SinhvienRoutingModule,

    NzIconModule,
    FormsModule,
    NzButtonModule,
    NzInputModule,
    NzSelectModule,
    NzRadioModule,
    NzTableModule,
    NzTabsModule,
    NzDatePickerModule
  ]
})
export class SinhvienModule { }
