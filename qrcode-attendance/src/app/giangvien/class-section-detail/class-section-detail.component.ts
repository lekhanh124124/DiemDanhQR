import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
import { ScheduleService } from '../../core/services/schedule.service';
import { NzMessageService } from 'ng-zorro-antd/message';
import { NzModalService } from 'ng-zorro-antd/modal';
import { AttendanceService } from '../../core/services/attendance.service';

@Component({
  selector: 'app-class-section-detail',
  templateUrl: './class-section-detail.component.html',
  styleUrls: ['./class-section-detail.component.scss'],
})
export class ClassSectionDetailComponent implements OnInit {
  maLopHocPhan = '';

  students: Array<{ maSinhVien: string; hoTen: string; email?: string; soDienThoai?: string; ngayThamGia?: string; thamGiaLop?: any; thamGiaTrangThai?: boolean }>= [];
  studentsPage = 1;
  studentsPageSize = 10;

  schedules: Array<{ maBuoi: number; ngayHoc: string; tietBatDau: number; soTiet: number; tenPhong?: string; tenMonHoc?: string }>= [];
  schedulesPage = 1;
  schedulesPageSize = 10;

  selectedSchedule: any = null;
  selectedScheduleId: string | null = null;
  selectedTabIndex = 0;

  selectedBuoi?: number;
  attendances: Array<{ maSinhVien: string; hoTen?: string; thoiGianQuet?: string; codeTrangThai?: string; lyDo?: string }>= [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private studentsService: StudentService,
    private scheduleService: ScheduleService,
    private attendanceService: AttendanceService,
    private message: NzMessageService,
    private modal: NzModalService
  ) {}

  ngOnInit(): void {
    this.maLopHocPhan = this.route.snapshot.paramMap.get('id') || '';
    if (!this.maLopHocPhan) return;
    try { this.selectedScheduleId = sessionStorage.getItem('creatingQr'); } catch (e) { this.selectedScheduleId = null; }
    this.loadStudents();
    this.loadSchedules();
  }

  get studentsPaged() {
    const start = (this.studentsPage - 1) * this.studentsPageSize;
    return (this.students || []).slice(start, start + this.studentsPageSize);
  }

  onStudentsPageChange(page: number) {
    this.studentsPage = page;
  }

  get schedulesPaged() {
    const start = (this.schedulesPage - 1) * this.schedulesPageSize;
    return (this.schedules || []).slice(start, start + this.schedulesPageSize);
  }

  onSchedulesPageChange(page: number) {
    this.schedulesPage = page;
  }

  // onScheduleClick(s: any): void {
  //   this.selectedSchedule = s;
  //   this.selectedTabIndex = 0;
  // }

  loadStudents(): void {
    this.studentsService.getStudents({ Page: 1, PageSize: 200, MaLopHocPhan: this.maLopHocPhan }).subscribe(res => {
      const items = res?.data?.items || res?.data || [];
      this.students = (items || []).map((s: any) => {
        const nguoi = s?.nguoiDung || s?.NguoiDung || s;
        const sv = s?.sinhVien || s?.SinhVien || s;
        const thamGia = s?.thamGiaLop || s?.ThamGiaLop || s;
        const rawTrangThai = thamGia?.trangThai ?? thamGia?.TrangThai ?? '';
        const thamGiaTrangThai = String(rawTrangThai).toLowerCase() === 'true';
        return {
          maSinhVien: sv?.maSinhVien || sv?.MaSinhVien || s?.maSinhVien || s?.MaSinhVien || (nguoi?.tenDangNhap || nguoi?.TenDangNhap),
          hoTen: nguoi?.hoTen || nguoi?.HoTen || s?.hoTen || s?.HoTen || '',
          email: nguoi?.email || nguoi?.Email || s?.email || s?.Email || '',
          soDienThoai: nguoi?.soDienThoai || nguoi?.SoDienThoai || s?.soDienThoai || s?.SoDienThoai || '',
          ngayThamGia: thamGia?.ngayThamGia || thamGia?.NgayThamGia || '',
          thamGiaLop: thamGia,
          thamGiaTrangThai
        };
      });
    });
  }

  loadSchedules(): void {
    this.scheduleService.getSchedules({ Page: 1, PageSize: 200, MaLopHocPhan: this.maLopHocPhan }).subscribe(res => {
      console.debug('class-section: schedule service response', res);
      const items = Array.isArray(res) ? res : (res?.data?.items || res?.data || res?.items || []);
      console.debug('class-section: raw schedule items', items);
      this.schedules = (items || []).map((b: any) => {
        const buoi = b?.buoiHoc || b?.BuoiHoc || b;
        const phong = b?.phongHoc || b?.PhongHoc || b;
        const mon = b?.monHoc || b?.MonHoc || b;

        const rawDate = (buoi?.ngayHoc || buoi?.NgayHoc || b?.ngayHoc || b?.NgayHoc || '') as string;
        let isoDate = String(rawDate || '').trim();
        if (isoDate) {
          const parts = isoDate.split(/[-\/]/).map(p => p.trim());
          if (parts.length === 3) {
            if (parts[0].length === 4) {
              isoDate = `${parts[0]}-${parts[1].padStart(2, '0')}-${parts[2].padStart(2, '0')}`;
            } else {
              isoDate = `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`;
            }
          }
        }

        return {
          maBuoi: buoi?.maBuoi || buoi?.MaBuoi || b?.maBuoi || b?.MaBuoi,
          ngayHoc: isoDate,
          tietBatDau: buoi?.tietBatDau || buoi?.TietBatDau || b?.tietBatDau || b?.TietBatDau || null,
          soTiet: buoi?.soTiet || buoi?.SoTiet || b?.soTiet || b?.SoTiet || null,
          tenPhong: phong?.tenPhong || phong?.TenPhong || b?.tenPhong || b?.TenPhong || '',
          tenMonHoc: mon?.tenMonHoc || mon?.TenMonHoc || b?.tenMonHoc || b?.TenMonHoc || (b?.lopHocPhan?.tenLopHocPhan || b?.LopHocPhan?.TenLopHocPhan) || ''
        };
      });
      console.debug('class-section: mapped schedules', this.schedules);
      if (this.selectedScheduleId) {
        const found = this.schedules.find((x: any) => String(x.maBuoi) === String(this.selectedScheduleId) || String(x.MaBuoi) === String(this.selectedScheduleId));
        if (found) this.selectedSchedule = found;
      }
    });
  }

  // viewAttendance(buoi: any): void {
  //   const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
  //   if (!maBuoi) return;
  //   this.selectedBuoi = maBuoi;
  //   this.attendanceService.getRecords({ Page: 1, PageSize: 200, MaBuoi: Number(maBuoi) }).subscribe(r => {
  //     const items = r?.data?.items || r?.data || [];
  //     this.attendances = items.map((x: any) => ({
  //       maSinhVien: x.maSinhVien || x.MaSinhVien,
  //       hoTen: x.hoTen || x.HoTen,
  //       thoiGianQuet: x.thoiGianQuet || x.ThoiGianQuet,
  //       codeTrangThai: x.codeTrangThai || x.CodeTrangThai,
  //       lyDo: x.lyDo || x.LyDo
  //     }));
  //   });
  // }

  goToQr(buoi: any): void {
    const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
    const ngay = buoi?.ngayHoc || '';
    this.router.navigate(['/giangvien/qr-create'], { queryParams: { maBuoi, date: ngay, classId: this.maLopHocPhan } });
  }

  handleCreateQr(buoi: any): void {
    const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
    if (maBuoi) {
      try { sessionStorage.setItem('creatingQr', String(maBuoi)); } catch (e) {}
      this.selectedScheduleId = String(maBuoi);
      this.selectedSchedule = buoi;
    }
    this.goToQr(buoi);
  }

  // openAttendanceManage(buoi: any): void {
  //   const maBuoi = buoi?.maBuoi || buoi?.MaBuoi;
  //   const ngay = buoi?.ngayHoc || '';
  //   this.router.navigate(['/giangvien/attendance-manage'], { queryParams: { classId: this.maLopHocPhan, maBuoi, date: ngay } });
  // }

  // confirmAutoGenerate(): void {
  //   if (!this.maLopHocPhan) {
  //     this.message.warning('Không tìm thấy mã lớp học phần để sinh buổi.');
  //     return;
  //   }
  //   this.modal.confirm({
  //     nzTitle: `Sinh buổi học tự động cho lớp '${this.maLopHocPhan}'?`,
  //     nzContent: 'Hệ thống sẽ tự động tạo các buổi học theo lịch. Bạn có muốn tiếp tục?',
  //     nzOkText: 'Sinh buổi',
  //     nzOkDanger: false,
  //     nzOnOk: () => {
  //       this.scheduleService.autoGenerate({ MaLopHocPhan: this.maLopHocPhan }).subscribe({
  //         next: (res: any) => {
  //           this.message.success('Sinh buổi học tự động thành công');
  //           // reload schedules to show generated sessions
  //           this.loadSchedules();
  //         },
  //         error: (err: any) => {
  //           this.message.error(err?.error?.Message || err?.message || 'Sinh buổi học thất bại');
  //         }
  //       });
  //     },
  //     nzCancelText: 'Hủy'
  //   });
  // }
}
