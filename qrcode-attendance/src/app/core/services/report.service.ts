import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly BASE_URL = `${environment.apiBase}/report`;

  constructor(private http: HttpClient, private auth: AuthService) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  getAttendanceReport(maLopHocPhan: string): Observable<any> {
    return this.http.get(`${this.BASE_URL}/attendance/${maLopHocPhan}`, { headers: this.headers() });
  }

  exportAttendanceReport(maLopHocPhan: string): Observable<Blob> {
    return this.http.get(`${this.BASE_URL}/export/${maLopHocPhan}`, {
      headers: this.headers(),
      responseType: 'blob' as const
    });
  }
}


