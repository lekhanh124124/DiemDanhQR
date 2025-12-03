import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AttendanceUtilsService {
  constructor() {}

  isPresent(rec: any): boolean {
    const v = rec?.trangThai ?? rec?.TrangThai ?? rec?.isPresent ?? rec?.present;
    if (typeof v === 'boolean') return v === true;
    if (typeof v === 'number') return v === 1;
    if (typeof v === 'string') {
      const s = v.toLowerCase();
      return /(có mặt|co mat|present|đúng giờ|dung gio|sớm|som|trễ|tre|late)/i.test(s);
    }
    const label = (rec?.tenTrangThai || rec?.TenTrangThai || rec?.CodeTrangThai || rec?.codeTrangThai || rec?.trangThai || '').toString().toLowerCase();
    return /(present|có mặt|co mat|đúng giờ|dung gio|sớm|som|trễ|late)/.test(label);
  }

  isAbsent(rec: any): boolean {
    const v = rec?.trangThai ?? rec?.TrangThai ?? rec?.isPresent ?? rec?.present;
    if (typeof v === 'boolean') return v === false;
    if (typeof v === 'number') return v === 0;
    if (typeof v === 'string') return /(vắng|vang|absent|nghỉ|nghi)/i.test(v);
    const code = (rec?.CodeTrangThai || rec?.codeTrangThai || rec?.MaTrangThai || rec?.maTrangThai || '').toString();
    if (code && /(absent|vang|vng|nghi)/i.test(code)) return true;
    return false;
  }

  isPermitted(rec: any): boolean {
    const label = (rec?.tenTrangThai || rec?.TenTrangThai || rec?.trangThai || rec?.TrangThai || '').toString().toLowerCase();
    if (/(không phép|khong phep|khongphep|không- phép|không-phép|no permit|not permitted|not permit)/i.test(label)) return false;
    if (/(có phép|co phep|phép|phep|permit|permitted)/i.test(label)) return true;
    const code = (rec?.CodeTrangThai || rec?.codeTrangThai || rec?.MaTrangThai || rec?.maTrangThai || '').toString().toLowerCase();
    if (code) {
      if (/(absent|absent_unpermitted|unpermitted|no-permit|no_permit|not_permit)/i.test(code)) return false;
      if (/(excused|excuse|excused_absence|permit|perm|phep)/i.test(code)) return true;
    }
    return false;
  }
}
