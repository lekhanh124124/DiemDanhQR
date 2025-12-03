import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TeacherScheduleComponent } from './teacher-schedule/teacher-schedule.component';
import { StudentScheduleComponent } from './student-schedule/student-schedule.component';

const routes: Routes = [
  { path: 'teacher-schedule', component: TeacherScheduleComponent},
  { path: 'student-schedule', component: StudentScheduleComponent},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClassRoutingModule { }
