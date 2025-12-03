import { Component } from '@angular/core';
// import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { BarcodeFormat } from '@zxing/library';
import { AttendanceService } from '../../core/services/attendance.service';
import { AuthService } from '../../core/services/auth.service';
import { NzMessageService } from 'ng-zorro-antd/message';

@Component({
  selector: 'app-scan',
  templateUrl: './scan.component.html',
  styleUrls: ['./scan.component.scss']
})
export class ScanComponent {
  scanResult: string | null = null;
  selectedDevice?: MediaDeviceInfo;
  availableDevices: MediaDeviceInfo[] = [];
  hasPermission: boolean = false;
  formats: BarcodeFormat[] = [BarcodeFormat.QR_CODE];
  submitting = false;
  scanEnabled = true;
  lastMessage: string | null = null;
  lastMessageType: 'success' | 'error' | 'info' | null = null;

  private extractErrorMessage(err: any): string {
    try {
      if (!err) return 'Lỗi không xác định';
      const body = err.error ?? err;
      if (!body) return String(err.message || err.statusText || 'Lỗi không xác định');
      if (typeof body === 'string') return body;

      if (body.Message) return String(body.Message);
      if (body.message) return String(body.message);
      if (body.error) {
        if (typeof body.error === 'string') return body.error;
        if (body.error.Message) return String(body.error.Message);
        if (body.error.message) return String(body.error.message);
      }

      return JSON.stringify(body);
    } catch (e) {
      return 'Lỗi không xác định';
    }
  }

  constructor(private attendance: AttendanceService, private auth: AuthService, private msg: NzMessageService) {}

  onCamerasFound(devices: MediaDeviceInfo[]): void {
    this.availableDevices = devices;
  }

  onScanSuccess(result: string): void {
    this.scanResult = result;
    const user = this.auth.getUser();
    const maSinhVien = user?.maSinhVien || user?.tenDangNhap || user?.maNguoiDung;
    if (!maSinhVien) { this.msg.warning('Không xác định được mã sinh viên'); return; }
    this.submitting = true;
    this.scanEnabled = false;
    this.getCurrentPosition().then(pos => {
      const lat = pos?.coords?.latitude;
      const lng = pos?.coords?.longitude;
      if (lat === undefined || lng === undefined || lat === null || lng === null) {
        this.submitting = false;
        this.scanEnabled = true;
        this.lastMessageType = 'error';
        this.lastMessage = 'Không lấy được vị trí. Vui lòng bật dịch vụ vị trí và thử lại.';
        this.msg.warning(this.lastMessage);
        return;
      }
      this.attendance.checkin(result, lat, lng).subscribe({
        next: (res) => {
          this.submitting = false;
          this.lastMessageType = 'success';
          this.lastMessage = 'Điểm danh thành công';
          this.msg.success(this.lastMessage);
          setTimeout(() => { this.scanEnabled = true; }, 1500);
        },
        error: (err) => {
          this.submitting = false;
          this.scanEnabled = true;
          this.lastMessageType = 'error';
          const msg = this.extractErrorMessage(err) || 'Lỗi khi gửi điểm danh';
          this.lastMessage = msg;
          this.msg.error(msg);
        }
      });
    }).catch(err => {
      this.submitting = false;
      this.scanEnabled = true;
      this.lastMessageType = 'error';
      this.lastMessage = 'Vui lòng bật quyền vị trí (Location) cho trình duyệt và thử lại để điểm danh.';
      this.msg.error(this.lastMessage);
    });
  }

  private getCurrentPosition(options?: PositionOptions): Promise<GeolocationPosition> {
    return new Promise((resolve, reject) => {
      if (!navigator || !navigator.geolocation) {
        return reject(new Error('Geolocation not available'));
      }
      navigator.geolocation.getCurrentPosition(resolve, reject, options || { enableHighAccuracy: true, timeout: 8000, maximumAge: 0 });
    });
  }

  onPermissionResponse(hasPermission: boolean): void {
    this.hasPermission = hasPermission;
  }
}
