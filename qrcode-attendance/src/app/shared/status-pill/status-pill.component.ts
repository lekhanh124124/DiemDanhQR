import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-pill',
  templateUrl: './status-pill.component.html',
  styleUrls: ['./status-pill.component.scss']
})
export class StatusPillComponent {
  @Input() state: any;
  @Input() activeLabel = 'Hoạt động';
  @Input() inactiveLabel = 'Ngừng hoạt động';
  @Input() minWidth?: string | number;
  @Input() compact = false;

  get styleObject(): any {
    const s: any = {};
    if (this.minWidth !== undefined && this.minWidth !== null) {
      s['min-width'] = typeof this.minWidth === 'number' ? `${this.minWidth}px` : this.minWidth;
    }
    return s;
  }

  isActive(): boolean {
    const v = this.state;
    if (v === true || v === 1) return true;
    if (v === false || v === 0) return false;
    if (typeof v === 'string') {
      const s = v.toLowerCase();
      if (s === 'true' || s === '1' || s === 'active' || s === 'hoạt động' || s === 'dang hoat dong') return true;
      return false;
    }
    return !!v;
  }
}
