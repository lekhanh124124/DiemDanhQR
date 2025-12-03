import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AcademicService {
  private baseUrl = environment.apiBase;

  constructor(private http: HttpClient) {}

  // Lấy danh sách khoa
  getDepartments(params: any = {}) {
    return this.http.get(`${this.baseUrl}/Academic/departments`, { params });
  }

  // Lấy danh sách ngành
  getMajors(params: any = {}) {
    return this.http.get(`${this.baseUrl}/Academic/majors`, { params });
  }
}
