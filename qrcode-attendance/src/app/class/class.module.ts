import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ClassRoutingModule } from './class-routing.module';
import { TeacherScheduleComponent } from './teacher-schedule/teacher-schedule.component';
import { StudentScheduleComponent } from './student-schedule/student-schedule.component';

import { NzCardModule } from 'ng-zorro-antd/card';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzOptionComponent } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    TeacherScheduleComponent,
    StudentScheduleComponent
  ],
  imports: [
    CommonModule,
    ClassRoutingModule,

    FormsModule,
    ReactiveFormsModule,

    NzCardModule,
    NzTableModule,
    NzSpinModule,
    NzInputModule,
    NzButtonModule,
    NzFormModule,
    NzDatePickerModule,
    NzTagModule,
    NzModalModule,
    NzPaginationModule,
    NzOptionComponent,
    NzIconModule,
    SharedModule
  ]
})
export class ClassModule { }
