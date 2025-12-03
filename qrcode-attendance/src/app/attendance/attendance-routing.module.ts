import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HistoryComponent } from './history/history.component';
import { ScanComponent } from './scan/scan.component';

const routes: Routes = [
  { path:'history', component: HistoryComponent},
  { path: 'scan', component: ScanComponent}
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AttendanceRoutingModule { }
