import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { authGuard } from '../core/guards/auth.guard';
import { RoleGuard } from '../core/guards/role.guard';

import { SubjectComponent } from './subjects/subjects.component';
import { CourseListComponent } from './course-list/course-list.component';
import { RoomsComponent } from './schedule-rooms/rooms.component';
import { RoomTimetableComponent } from './schedule-list/room-timetable.component';
import { SchedulesComponent } from './schedule-list/schedules.component';
import { LogsComponent } from './logs/logs.component';
import { SemestersComponent } from './semesters/semesters.component';

const routes: Routes = [
  { path: '', component: DashboardComponent},
  { path:'classes', component: CourseListComponent},
  { path: 'subjects', component: SubjectComponent},
  { path: 'logs', component: LogsComponent},
  { path: 'schedule/rooms', component: RoomsComponent},
  { path: 'schedule/list', component: SchedulesComponent},
  { path: 'schedule/room-timetable', component: RoomTimetableComponent},
  { path: 'semesters', component: SemestersComponent},
  { path: 'permissions/roles', redirectTo: '/admin/permission/role-list', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
