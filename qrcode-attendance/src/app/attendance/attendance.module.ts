import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AttendanceRoutingModule } from './attendance-routing.module';
import { ScanComponent } from './scan/scan.component';
import { HistoryComponent } from './history/history.component';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { FormsModule } from '@angular/forms';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    ScanComponent,
    HistoryComponent
  ],
  imports: [
    CommonModule,
    AttendanceRoutingModule,
    ZXingScannerModule,
    FormsModule,

    NzTableModule,
    NzSelectModule,
    NzIconModule,
    NzButtonModule,
    NzModalModule,
    SharedModule
  ]
})
export class AttendanceModule { }
