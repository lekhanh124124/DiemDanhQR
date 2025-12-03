import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { AdminRoutingModule } from './admin-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { MainLayoutComponent } from '../layout/main-layout/main-layout.component';
import { MainLayoutModule } from '../layout/main-layout.module';
import { CourseListComponent } from './course-list/course-list.component';
import { SubjectComponent } from './subjects/subjects.component';
import { LogsComponent } from './logs/logs.component';
import { RoomsComponent } from './schedule-rooms/rooms.component';
import { SchedulesComponent } from './schedule-list/schedules.component';
import { RoomTimetableComponent } from './schedule-list/room-timetable.component';
import { SemestersComponent } from './semesters/semesters.component';

import { NzCardModule } from 'ng-zorro-antd/card';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzAutocompleteModule } from 'ng-zorro-antd/auto-complete';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzPaginationModule } from 'ng-zorro-antd/pagination';
import { NzBadgeModule } from 'ng-zorro-antd/badge';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [
    DashboardComponent,
    SubjectComponent,
    CourseListComponent,
    LogsComponent,
    RoomsComponent,
    SchedulesComponent,
    RoomTimetableComponent,
    SemestersComponent
  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    MainLayoutModule,
    FormsModule,

    NzCardModule,
    NzIconModule,
    NzTableModule,
    NzCardModule,
    NzTableModule,
    NzSpinModule,
    NzCardModule,
    NzInputModule,
    NzDatePickerModule,
    NzAutocompleteModule,
    NzPaginationModule,
    NzBadgeModule,
    NzModalModule,
    NzSelectModule,
    NzButtonModule,
    NzTagModule,
    NzDropDownModule,
    NzPopconfirmModule,
    SharedModule,
  ]
})
export class AdminModule { }

