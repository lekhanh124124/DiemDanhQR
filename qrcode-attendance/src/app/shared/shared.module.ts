import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TableEmptyComponent } from './table-empty/table-empty.component';
import { StatusPillComponent } from './status-pill/status-pill.component';
import { ScrollTableComponent } from './scroll-table/scroll-table.component';
import { ScrollComponent } from './scroll/scroll.component';

import { NzTableModule } from 'ng-zorro-antd/table';
import { NzSpinModule } from 'ng-zorro-antd/spin';
import { FormsModule } from '@angular/forms';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzEmptyModule } from 'ng-zorro-antd/empty';


@NgModule({
  declarations: [TableEmptyComponent, StatusPillComponent, ScrollTableComponent, ScrollComponent],
  imports: [
    CommonModule,
    NzEmptyModule,
    NzTableModule,
    NzSpinModule,
    NzButtonModule,
    FormsModule
  ],
  exports: [TableEmptyComponent, StatusPillComponent, ScrollTableComponent, ScrollComponent]
})
export class SharedModule { }
