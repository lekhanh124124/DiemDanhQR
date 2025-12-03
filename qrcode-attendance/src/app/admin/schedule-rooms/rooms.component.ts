import { Component, OnInit } from '@angular/core';
import { ScheduleService } from '../../core/services/schedule.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { Router } from '@angular/router';

@Component({
  selector: 'app-rooms',
  templateUrl: './rooms.component.html',
  styleUrls: ['./rooms.component.scss']
})
export class RoomsComponent implements OnInit {
  rooms: any[] = [];
  loading = false;
  page = 1;
  pageSize = 10;
  total = 0;

  keyword = '';
  showAdvanced = false;
  filterTenPhong = '';
  filterToaNha = '';
  filterTang = '';
  filterSucChua = '';
  filterTrangThai: boolean | undefined;
  sortBy: string = 'TenPhong';
  sortDir: 'ASC' | 'DESC' = 'ASC';

  isCreateVisible = false;
  isEditVisible = false;
  creating = false;
  updating = false;
  selectedRoom: any = {};
  newRoom: any = { TenPhong: '', ToaNha: '', Tang: '', SucChua: '', TrangThai: true };
  setOfCheckedId = new Set<string>();
  listOfCurrentPageData: any[] = [];
  checked = false;
  indeterminate = false;

  constructor(private schedule: ScheduleService, private msg: NzMessageService, private modal: NzModalService, private router: Router) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    const params: any = { Page: this.page, PageSize: this.pageSize, TenPhong: this.filterTenPhong, ToaNha: this.filterToaNha, Tang: this.filterTang, SucChua: this.filterSucChua, TrangThai: this.filterTrangThai, SortBy: this.sortBy, SortDir: this.sortDir };
    this.schedule.getRooms(params).subscribe({
      next: res => {
        const raw = res?.data?.items || res?.data || [];
        const coerceBool = (v: any) => {
          if (v === undefined || v === null) return false;
          if (typeof v === 'boolean') return v;
          if (typeof v === 'number') return v === 1;
          const s = String(v).toLowerCase();
          return s === '1' || s === 'true';
        };

        this.rooms = (raw || []).map((it: any) => {
          const p = it?.phongHoc || it || {};
          return {
            maPhong: p?.maPhong ?? p?.MaPhong ?? p?.ma,
            tenPhong: p?.tenPhong ?? p?.TenPhong ?? p?.ten,
            toaNha: p?.toaNha ?? p?.ToaNha,
            tang: p?.tang ?? p?.Tang,
            sucChua: p?.sucChua ?? p?.SucChua,
            trangThai: coerceBool(p?.trangThai ?? p?.TrangThai ?? p?.TrangThai)
          };
        });

        this.total = Number(res?.data?.totalRecords ?? res?.data?.total ?? this.rooms.length) || this.rooms.length;
        this.page = Number(res?.data?.page ?? this.page) || this.page;
        this.pageSize = Number(res?.data?.pageSize ?? this.pageSize) || this.pageSize;
        this.loading = false;
      },
      error: () => { this.loading = false; this.msg.error('Lỗi tải phòng học'); }
    });
  }

  onFilterChange(): void { this.page = 1; this.load(); }
  onPageChange(p: number): void { this.page = p; this.load(); }

  changeSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortBy = column;
      this.sortDir = 'ASC';
    }
    this.page = 1;
    this.load();
  }

  openCreate(): void { this.newRoom = { TenPhong: '', ToaNha: '', Tang: '', SucChua: '', TrangThai: true }; this.isCreateVisible = true; }
  create(): void {
    if (!this.newRoom?.TenPhong) { this.msg.warning('Nhập Tên phòng'); return; }
    this.creating = true;
    const body = { ...this.newRoom, Tang: this.newRoom.Tang ? Number(this.newRoom.Tang) : undefined, SucChua: this.newRoom.SucChua ? Number(this.newRoom.SucChua) : undefined };
    this.schedule.createRoom(body).subscribe({
      next: () => { this.creating = false; this.isCreateVisible = false; this.msg.success('Tạo phòng thành công'); this.load(); },
      error: () => { this.creating = false; this.msg.error('Tạo phòng thất bại'); }
    });
  }

  openEdit(room: any): void {
    this.selectedRoom = { MaPhong: room.maPhong, TenPhong: room.tenPhong, ToaNha: room.toaNha, Tang: room.tang, SucChua: room.sucChua, TrangThai: room.trangThai };
    this.isEditVisible = true;
  }
  update(): void {
    if (!this.selectedRoom?.MaPhong) { this.msg.warning('Thiếu MaPhong'); return; }
    this.updating = true;
    const body = { ...this.selectedRoom, Tang: this.selectedRoom.Tang ? Number(this.selectedRoom.Tang) : undefined, SucChua: this.selectedRoom.SucChua ? Number(this.selectedRoom.SucChua) : undefined };
    this.schedule.updateRoom(body).subscribe({
      next: () => { this.updating = false; this.isEditVisible = false; this.msg.success('Cập nhật phòng thành công'); this.load(); },
      error: () => { this.updating = false; this.msg.error('Cập nhật phòng thất bại'); }
    });
  }

  cancel(): void { this.isCreateVisible = false; this.isEditVisible = false; }

  onSearch(): void {
    const t = (this.keyword || '').trim();
    this.filterTenPhong = t; this.filterToaNha = ''; this.filterTang = ''; this.filterSucChua = ''; this.page = 1;
    this.load();
  }
  refreshList(): void {
    this.keyword = '';
    this.filterTenPhong = '';
    this.filterToaNha = '';
    this.filterTang = '';
    this.filterSucChua = '';
    this.filterTrangThai = undefined;
    this.page = 1;
    this.load();
  }

  // Selection helpers
  public getRowKey(row: any): string {
    return String(row?.maPhong || row?.MaPhong || (row?.tenPhong || '') + '::' + (row?.toaNha || ''));
  }

  onCurrentPageDataChange(listOfCurrentPageData: readonly any[]): void {
    this.listOfCurrentPageData = (listOfCurrentPageData || []) as any[];
    this.refreshCheckedStatus();
  }

  onAllChecked(checked: boolean): void {
    this.listOfCurrentPageData.forEach(item => this.updateCheckedSet(this.getRowKey(item), checked));
    this.refreshCheckedStatus();
  }

  onItemChecked(id: string, checked: boolean): void {
    this.updateCheckedSet(id, checked);
    this.refreshCheckedStatus();
  }

  private updateCheckedSet(id: string, checked: boolean): void {
    if (checked) this.setOfCheckedId.add(id);
    else this.setOfCheckedId.delete(id);
  }

  private refreshCheckedStatus(): void {
    this.checked = this.listOfCurrentPageData.length > 0 && this.listOfCurrentPageData.every(item => this.setOfCheckedId.has(this.getRowKey(item)));
    this.indeterminate = this.listOfCurrentPageData.some(item => this.setOfCheckedId.has(this.getRowKey(item))) && !this.checked;
  }

  deleteSelected(): void {
    if (this.setOfCheckedId.size === 0) return;
    this.modal.confirm({
      nzTitle: `Vô hiệu hóa ${this.setOfCheckedId.size} phòng đã chọn?`,
      nzContent: 'Hành động này sẽ vô hiệu hóa các phòng đã chọn.',
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        const ids = Array.from(this.setOfCheckedId);
        let remaining = ids.length;
        ids.forEach(k => {
          const ma = k.includes('::') ? undefined : k;
          if (!ma) { remaining--; if (remaining === 0) this.load(); return; }
          this.schedule.updateRoom({ MaPhong: Number(ma), TrangThai: false }).subscribe({
            next: () => {
              remaining--;
              this.setOfCheckedId.delete(k);
              if (remaining === 0) { this.msg.success('Đã vô hiệu hóa các phòng đã chọn'); this.load(); }
            },
            error: () => { remaining--; if (remaining === 0) this.load(); }
          });
        });
      }
    ,
      nzCancelText: 'Hủy'
    });
  }

  delete(room: any): void {
    const ma = room?.maPhong;
    if (!ma) return;
    this.modal.confirm({
      nzTitle: 'Xác nhận',
      nzContent: `Bạn có chắc chắn muốn vô hiệu hóa phòng ${room.tenPhong || ma}?`,
      nzOkText: 'Vô hiệu hóa',
      nzOkDanger: true,
      nzOnOk: () => {
        this.schedule.deleteRoom(ma).subscribe({
          next: () => { this.msg.success('Đã vô hiệu hóa'); this.load(); },
          error: () => { this.msg.error('Vô hiệu hóa thất bại'); }
        });
      },
      nzCancelText: 'Hủy'
    });
  }

  navigateToSchedules(room: any): void {
    const ma = room?.maPhong;
    const ten = room?.tenPhong;
    const queryParams: any = {};
    if (ma) queryParams.MaPhong = ma;
    if (ten) queryParams.TenPhong = ten;

    queryParams.includeAll = '1';
    this.router.navigate(['/admin/schedule/room-timetable'], { queryParams });
  }
}



