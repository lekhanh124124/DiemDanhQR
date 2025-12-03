import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-scroll-table',
  templateUrl: './scroll-table.component.html',
  styleUrls: ['./scroll-table.component.scss']
})
export class ScrollTableComponent {
  @Input() data: any[] = [];
  @Input() height: string = '240px';
  @Input() columns: Array<{ key: string; header: string }> = [];

}
