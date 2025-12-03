import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter/foundation.dart';
import '../config/app_config.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  // URL server - DevTunnel (HTTPS)
  static const String baseUrl = AppConfig.apiBaseUrl;

  // Helper: authorized HTTP request with auto refresh-once
  static Future<http.Response> _authorizedRequest(
    Uri url, {
    String method = 'GET',
    Map<String, String>? headers,
    Object? body,
  }) async {
    final prefs = await SharedPreferences.getInstance();
    final accessToken = prefs.getString('accessToken');
    if (accessToken == null) {
      return http.Response('{"message":"Missing token"}', 401);
    }

    final authHeaders = <String, String>{
      'Authorization': 'Bearer $accessToken',
      if (headers != null) ...headers,
    };
    // Chỉ đặt Content-Type cho phương thức có body
    final upper = method.toUpperCase();
    if (upper == 'POST' || upper == 'PUT' || upper == 'DELETE') {
      authHeaders['Content-Type'] = 'application/json';
    }

    Future<http.Response> doCall() async {
      switch (upper) {
        case 'POST':
          return http.post(url, headers: authHeaders, body: body);
        case 'PUT':
          return http.put(url, headers: authHeaders, body: body);
        case 'DELETE':
          return http.delete(url, headers: authHeaders, body: body);
        default:
          return http.get(url, headers: authHeaders);
      }
    }

    var response = await doCall();
    if (response.statusCode == 401) {
      final refreshed = await refreshAccessToken();
      if (refreshed['success'] == true) {
        final newAccess = (await SharedPreferences.getInstance()).getString('accessToken');
        if (newAccess != null) {
          final retryHeaders = {
            ...authHeaders,
            'Authorization': 'Bearer $newAccess',
          };
          switch (upper) {
            case 'POST':
              response = await http.post(url, headers: retryHeaders, body: body);
              break;
            case 'PUT':
              response = await http.put(url, headers: retryHeaders, body: body);
              break;
            case 'DELETE':
              response = await http.delete(url, headers: retryHeaders, body: body);
              break;
            default:
              response = await http.get(url, headers: retryHeaders);
          }
        }
      }
    }

    return response;
  }

  // Làm mới access token bằng refresh token
  static Future<Map<String, dynamic>> refreshAccessToken() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final currentAccess = prefs.getString('accessToken');
      final refresh = prefs.getString('refreshToken');

      // Also read stored username if available (needed by backend)
      final tenDangNhap = prefs.getString('username');

      if (currentAccess == null || refresh == null) {
        return {
          'success': false,
          'message': 'Thiếu token để làm mới',
        };
      }

      final url = Uri.parse('$baseUrl/api/auth/refreshtoken');
      // Backend expects TenDangNhap and RefreshToken (see API spec)
      final body = <String, dynamic>{
        if (tenDangNhap != null) 'TenDangNhap': tenDangNhap,
        'RefreshToken': refresh,
      };

      final response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $currentAccess',
        },
        body: jsonEncode(body),
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        String? newAccess = data['accessToken']
            ?? (data['data'] is Map ? data['data']['accessToken'] : null)
            ?? data['token']
            ?? (data['data'] is Map ? data['data']['token'] : null)
            ?? data['jwt']
            ?? (data['data'] is Map ? data['data']['jwt'] : null)
            ?? data['access_token']
            ?? (data['data'] is Map ? data['data']['access_token'] : null);

        String? newRefresh = data['refreshToken']
            ?? (data['data'] is Map ? data['data']['refreshToken'] : null)
            ?? data['refresh_token']
            ?? (data['data'] is Map ? data['data']['refresh_token'] : null);

        if (newAccess != null) await prefs.setString('accessToken', newAccess);
        if (newRefresh != null) await prefs.setString('refreshToken', newRefresh);

        return {
          'success': true,
          'accessToken': newAccess,
          'refreshToken': newRefresh,
          'data': data,
        };
      } else {
        return {
          'success': false,
          'message': 'Làm mới token thất bại',
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi làm mới token: $e',
      };
    }
  }

  // Lấy danh sách lớp học (Lịch học)
  static Future<Map<String, dynamic>> fetchClassList({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    String? maLop,
    String? tenLop,
    String? trangThai,
    String? maMonHoc,
    String? tenMonHoc,
    String? hocKy,
    String? soTinChi,
    String? maGiangVien,
    String? tenGiangVien,
    String? khoa,
    Map<String, String>? extraParams,
  }) async {
    try {
      final body = <String, dynamic>{
        'page': page,
        'pageSize': pageSize,
        if (sortBy != null && sortBy.isNotEmpty) 'sortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
        if (maLop != null && maLop.isNotEmpty) 'MaLop': maLop,
        if (tenLop != null && tenLop.isNotEmpty) 'TenLop': tenLop,
        if (trangThai != null && trangThai.isNotEmpty) 'TrangThai': trangThai,
        if (maMonHoc != null && maMonHoc.isNotEmpty) 'MaMonHoc': maMonHoc,
        if (tenMonHoc != null && tenMonHoc.isNotEmpty) 'TenMonHoc': tenMonHoc,
        if (hocKy != null && hocKy.isNotEmpty) 'HocKy': hocKy,
        if (soTinChi != null && soTinChi.isNotEmpty) 'SoTinChi': soTinChi,
        if (maGiangVien != null && maGiangVien.isNotEmpty) 'MaGiangVien': maGiangVien,
        if (tenGiangVien != null && tenGiangVien.isNotEmpty) 'TenGiangVien': tenGiangVien,
        if (khoa != null && khoa.isNotEmpty) 'Khoa': khoa,
      };

      final urlPost = Uri.parse('$baseUrl/api/lophoc/laydanhsach');

      // Thử GET trước (một số backend dùng GET với query)
      final queryParams = body.map((k, v) => MapEntry(k, v.toString()));
      final urlGet = urlPost.replace(queryParameters: queryParams);

      http.Response response = await _authorizedRequest(urlGet, method: 'GET');

      // Nếu GET thất bại (không 2xx), fallback sang POST JSON
      if (response.statusCode < 200 || response.statusCode >= 300) {
        response = await _authorizedRequest(
          urlPost,
          method: 'POST',
          body: jsonEncode(body),
        );
      }

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final data = jsonDecode(response.body);

        // Tìm mảng items từ nhiều cấu trúc khác nhau
        List<dynamic>? items;
        if (data is List) {
          items = data;
        } else if (data is Map<String, dynamic>) {
          // Thử các key thường gặp và cả lồng data.items
          dynamic candidates = data['items'] ?? data['result'] ?? data['records'] ?? data['content'];
          if (candidates == null && data['data'] is List) {
            candidates = data['data'];
          }
          if (candidates == null && data['data'] is Map) {
            final m = data['data'] as Map;
            candidates = m['items'] ?? m['result'] ?? m['records'] ?? m['content'];
          }
          items = candidates is List ? candidates : null;
        }

        return {
          'success': true,
          'data': items ?? [],
          'raw': data,
        };
      } else {
        final msg = _tryParseMessage(response.body) ?? 'Không thể lấy danh sách lớp học';
        return {
          'success': false,
          'message': msg,
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  static String? _tryParseMessage(String body) {
    try {
      final json = jsonDecode(body);
      if (json is Map<String, dynamic>) {
        // Try a list of common keys that may contain an error/message
        for (final k in ['message', 'error', 'detail', 'status', 'errors']) {
          if (json.containsKey(k) && json[k] != null) {
            final v = json[k];
            if (v is String) return v.toString();
            if (v is Map || v is List) return v.toString();
          }
        }
      }
    } catch (_) {}
    return null;
  }

  // Extract a friendly message from a standard service response map.
  // Checks common locations: top-level 'message', 'data.message', 'raw' JSON, or status.
  static String extractFriendlyMessage(Map<String, dynamic>? res, {String fallback = 'Điểm danh thất bại'}) {
    if (res == null) return fallback;
    try {
      // Normalize top-level keys to lowercase to support responses like 'Message'/'Status'
      final lowerRes = <String, dynamic>{};
      for (final e in res.entries) {
        lowerRes[e.key.toString().toLowerCase()] = e.value;
      }

      // find status/code if present
      String? status;
      if (lowerRes['status'] != null) status = lowerRes['status'].toString();
      else if (lowerRes['code'] != null) status = lowerRes['code'].toString();

      // find a human-friendly message in common places
      String? message;
      if (lowerRes['message'] != null && lowerRes['message'].toString().trim().isNotEmpty) {
        message = lowerRes['message'].toString().trim();
      } else {
        final data = lowerRes['data'] ?? res['data'];
        if (data is Map && data['message'] != null && data['message'].toString().trim().isNotEmpty) {
          message = data['message'].toString().trim();
        } else {
          final raw = lowerRes['raw'] ?? res['raw'] ?? res;
          if (raw is String && raw.trim().isNotEmpty) {
            final parsed = _tryParseMessage(raw);
            if (parsed != null && parsed.isNotEmpty) message = parsed;
            else message = raw.trim();
          } else {
            final found = _findMessageInDynamic(raw);
            if (found != null && found.trim().isNotEmpty) message = found.trim();
          }
        }
      }

      // Compose final output: prefer message; do NOT include status codes in the returned text
      if (message != null && message.isNotEmpty) {
        return message;
      }

      // If only status is available, fall back to the provided fallback (do not surface raw status)
      if (status != null && status.isNotEmpty) return fallback;
    } catch (_) {}
    return fallback;
  }

  // Recursively search a dynamic JSON-like object (Map/List/String) for a human-readable message.
  // Prioritizes common keys; ignores purely numeric/status-only strings.
  static String? _findMessageInDynamic(dynamic obj) {
    if (obj == null) return null;

    final candidates = ['message', 'msg', 'error', 'detail', 'description', 'status', 'title', 'reason'];

    String? normalize(dynamic v) {
      if (v == null) return null;
      if (v is String) {
        final s = v.trim();
        // ignore purely numeric strings like '400' or 'status=400'
        if (RegExp(r'^\d{1,4} *$').hasMatch(s)) return null;
        if (RegExp(r'^(status[:=]?\s*)?\d{1,4}\s*$').hasMatch(s.toLowerCase())) return null;
        return s.isNotEmpty ? s : null;
      }
      if (v is num) return v.toString();
      return null;
    }

    try {
      // If it's a Map, try keys first (case-insensitive)
      if (obj is Map) {
        // check candidate keys (case-insensitive)
        final lowerMap = <String, dynamic>{};
        for (final e in obj.entries) {
          lowerMap[e.key.toString().toLowerCase()] = e.value;
        }
        for (final k in candidates) {
          if (lowerMap.containsKey(k)) {
            final v = normalize(lowerMap[k]);
            if (v != null) return v;
            // if value is complex, search inside it
            final sub = _findMessageInDynamic(lowerMap[k]);
            if (sub != null && sub.isNotEmpty) return sub;
          }
        }

        // fallback: search all values recursively
        for (final v in obj.values) {
          final found = _findMessageInDynamic(v);
          if (found != null && found.isNotEmpty) return found;
        }
      }

      // If it's a List, iterate
      if (obj is List) {
        for (final item in obj) {
          final found = _findMessageInDynamic(item);
          if (found != null && found.isNotEmpty) return found;
        }
      }

      // If it's a primitive string/number, return normalized
      final norm = normalize(obj);
      if (norm != null) return norm;
    } catch (_) {}
    return null;
  }

  // Lấy danh sách buổi học
  static Future<Map<String, dynamic>> fetchSchedule({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    Map<String, String>? filters,
  }) async {
    try {
      final params = <String, String>{
        'Page': page.toString(),
        'PageSize': pageSize.toString(),
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
      };

      // Các tham số tìm kiếm hợp lệ
      final allowed = {
        'MaBuoi','MaPhong','TenPhong','MaLopHocPhan','TenLopHocPhan','TenMonHoc',
        'MaHocKy','NgayHoc','Nam','Tuan','Thang','TietBatDau','SoTiet','TrangThai',
        'MaSinhVien','MaGiangVien'
      };
      if (filters != null) {
        filters.forEach((k, v) {
          if (v.isNotEmpty && allowed.contains(k)) {
            params[k] = v;
          }
        });
      }

      final url = Uri.parse('$baseUrl/api/schedule/list').replace(queryParameters: params);
      final response = await _authorizedRequest(url, method: 'GET');

      final rawBody = response.body;
      Map<String, dynamic>? json;
      try { json = jsonDecode(rawBody) as Map<String, dynamic>; } catch (_) {}

      if (response.statusCode >= 200 && response.statusCode < 300 && json != null) {
        // Thành công chuẩn: status == "200" và có data.items
        final statusStr = (json['status'] ?? json['Status'])?.toString();
        if (statusStr == '200') {
          final data = json['data'] is Map ? json['data'] as Map : {};
          final items = (data['items'] is List) ? List<Map<String,dynamic>>.from(data['items']) : <Map<String,dynamic>>[];
          return {
            'success': true,
            'items': items,
            'page': data['page']?.toString(),
            'pageSize': data['pageSize']?.toString(),
            'totalRecords': data['totalRecords']?.toString(),
            'totalPages': data['totalPages']?.toString(),
            'message': json['message'] ?? json['Message'],
            'raw': json,
          };
        }
      }

      // Thất bại theo mẫu: { "Status": "...", "Message": "...", "Data": null }
      final message = json?['Message'] ?? json?['message'] ?? 'Không thể lấy danh sách buổi học';
      final statusOut = json?['Status'] ?? json?['status'] ?? response.statusCode.toString();
      return {
        'success': false,
        'status': statusOut.toString(),
        'message': message.toString(),
        'raw': json ?? rawBody,
      };
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // Lấy danh sách phòng học (schedule rooms)
  static Future<Map<String, dynamic>> fetchScheduleRooms({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    String? keyword,
    String? maPhong,
    String? tenPhong,
    String? toaNha,
    String? tang,
    String? sucChua,
    String? trangThai,
    Map<String, String>? extraParams,
  }) async {
    try {
      final params = <String, String>{
        'Page': page.toString(),
        'PageSize': pageSize.toString(),
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
        if (keyword != null && keyword.isNotEmpty) 'Keyword': keyword,
        if (maPhong != null && maPhong.isNotEmpty) 'MaPhong': maPhong,
        if (tenPhong != null && tenPhong.isNotEmpty) 'TenPhong': tenPhong,
        if (toaNha != null && toaNha.isNotEmpty) 'ToaNha': toaNha,
        if (tang != null && tang.isNotEmpty) 'Tang': tang,
        if (sucChua != null && sucChua.isNotEmpty) 'SucChua': sucChua,
        if (trangThai != null && trangThai.isNotEmpty) 'TrangThai': trangThai,
      };
      if (extraParams != null) params.addAll(extraParams);

      final url = Uri.parse('$baseUrl/api/schedule/rooms').replace(queryParameters: params);
      final response = await _authorizedRequest(url, method: 'GET');

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final data = jsonDecode(response.body);
        List<dynamic>? items;
        if (data is List) {
          items = data;
        } else if (data is Map<String, dynamic>) {
          dynamic candidates = data['items'] ?? data['result'] ?? data['records'] ?? data['data'] ?? data['content'];
          if (candidates is Map) {
            candidates = candidates['items'] ?? candidates['result'] ?? candidates['records'] ?? candidates['content'];
          }
          items = candidates is List ? candidates : null;
        }

        return {
          'success': true,
          'data': items ?? [],
          'raw': data,
        };
      } else {
        final msg = _tryParseMessage(response.body) ?? 'Không thể lấy danh sách phòng học';
        return {
          'success': false,
          'message': msg,
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // Lấy danh sách lớp học phần (course list)
  static Future<Map<String, dynamic>> fetchCourseList({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    String? keyword,
    String? maLopHocPhan,
    String? tenLopHocPhan,
    String? trangThai,
    String? maMonHoc,
    String? tenMonHoc,
    String? soTinChi,
    String? soTiet,
    String? maGiangVien,
    String? hocKy,
    String? tenGiangVien,
    String? maSinhVien,
    Map<String, String>? extraParams,
  }) async {
    try {
      final params = <String, String>{
        'Page': page.toString(),
        'PageSize': pageSize.toString(),
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
        if (keyword != null && keyword.isNotEmpty) 'Keyword': keyword,
        if (maLopHocPhan != null && maLopHocPhan.isNotEmpty) 'maLopHocPhan': maLopHocPhan,
        if (tenLopHocPhan != null && tenLopHocPhan.isNotEmpty) 'tenLopHocPhan': tenLopHocPhan,
        if (trangThai != null && trangThai.isNotEmpty) 'trangThai': trangThai,
        if (maMonHoc != null && maMonHoc.isNotEmpty) 'MaMonHoc': maMonHoc,
        if (tenMonHoc != null && tenMonHoc.isNotEmpty) 'tenMonHoc': tenMonHoc,
        if (soTinChi != null && soTinChi.isNotEmpty) 'soTinChi': soTinChi,
        if (soTiet != null && soTiet.isNotEmpty) 'soTiet': soTiet,
        if (maGiangVien != null && maGiangVien.isNotEmpty) 'maGiangVien': maGiangVien,
        if (hocKy != null && hocKy.isNotEmpty) 'hocKy': hocKy,
        if (tenGiangVien != null && tenGiangVien.isNotEmpty) 'tenGiangVien': tenGiangVien,
        if (maSinhVien != null && maSinhVien.isNotEmpty) 'maSinhVien': maSinhVien,
      };
      if (extraParams != null) params.addAll(extraParams);

      final url = Uri.parse('$baseUrl/api/course/list').replace(queryParameters: params);
      final response = await _authorizedRequest(url, method: 'GET');

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final data = jsonDecode(response.body);
        List<dynamic>? items;
        if (data is List) {
          items = data;
        } else if (data is Map<String, dynamic>) {
          dynamic candidates = data['items'] ?? data['result'] ?? data['records'] ?? data['data'] ?? data['content'];
          if (candidates is Map) {
            candidates = candidates['items'] ?? candidates['result'] ?? candidates['records'] ?? candidates['content'];
          }
          items = candidates is List ? candidates : null;
        }

        return {
          'success': true,
          'data': items ?? [],
          'raw': data,
        };
      } else {
        final msg = _tryParseMessage(response.body) ?? 'Không thể lấy danh sách lớp học phần';
        return {
          'success': false,
          'message': msg,
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // Lấy danh sách môn học (course subjects)
  static Future<Map<String, dynamic>> fetchCourseSubjects({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    String? keyword,
    String? maMonHoc,
    String? tenMonHoc,
    String? soTinChi,
    String? soTiet,
    String? hocKy,
    String? trangThai,
    Map<String, String>? extraParams,
  }) async {
    try {
      final params = <String, String>{
        'Page': page.toString(),
        'PageSize': pageSize.toString(),
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
        if (keyword != null && keyword.isNotEmpty) 'Keyword': keyword,
        if (maMonHoc != null && maMonHoc.isNotEmpty) 'MaMonHoc': maMonHoc,
        if (tenMonHoc != null && tenMonHoc.isNotEmpty) 'TenMonHoc': tenMonHoc,
        if (soTinChi != null && soTinChi.isNotEmpty) 'SoTinChi': soTinChi,
        if (soTiet != null && soTiet.isNotEmpty) 'SoTiet': soTiet,
        if (hocKy != null && hocKy.isNotEmpty) 'HocKy': hocKy,
        if (trangThai != null && trangThai.isNotEmpty) 'TrangThai': trangThai,
      };
      if (extraParams != null) params.addAll(extraParams);

      final url = Uri.parse('$baseUrl/api/course/subjects').replace(queryParameters: params);
      final response = await _authorizedRequest(url, method: 'GET');

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final data = jsonDecode(response.body);
        List<dynamic>? items;
        if (data is List) {
          items = data;
        } else if (data is Map<String, dynamic>) {
          dynamic candidates = data['items'] ?? data['result'] ?? data['records'] ?? data['data'] ?? data['content'];
          if (candidates is Map) {
            candidates = candidates['items'] ?? candidates['result'] ?? candidates['records'] ?? candidates['content'];
          }
          items = candidates is List ? candidates : null;
        }

        return {
          'success': true,
          'data': items ?? [],
          'raw': data,
        };
      } else {
        final msg = _tryParseMessage(response.body) ?? 'Không thể lấy danh sách môn học';
        return {
          'success': false,
          'message': msg,
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // API Đổi mật khẩu
  static Future<Map<String, dynamic>> changePassword(String oldPassword, String newPassword) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('accessToken');
      if (accessToken == null) {
        return {
          'success': false,
          'message': 'Không tìm thấy token',
        };
      }
  // standardized endpoint
  final url = Uri.parse('$baseUrl/api/auth/changepassword');
      // Send Vietnamese field names as backend expects
      var response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $accessToken',
        },
        body: jsonEncode({
          'MatKhauCu': oldPassword,
          'MatKhauMoi': newPassword,
        }),
      );
      // Nếu 401, thử làm mới token và gọi lại 1 lần
      if (response.statusCode == 401) {
        final refreshed = await refreshAccessToken();
        if (refreshed['success'] == true) {
          final newAccess = (await SharedPreferences.getInstance()).getString('accessToken');
          if (newAccess != null) {
            response = await http.post(
              url,
              headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer $newAccess',
              },
              body: jsonEncode({
                'MatKhauCu': oldPassword,
                'MatKhauMoi': newPassword,
              }),
            );
          }
        }
      }
      if (response.statusCode == 200) {
        return {
          'success': true,
          'message': 'Đổi mật khẩu thành công',
        };
      } else {
        final errorData = jsonDecode(response.body);
        return {
          'success': false,
          'message': errorData['message'] ?? 'Đổi mật khẩu thất bại',
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }
  static Future<Map<String, dynamic>> login(String tenDangNhap, String matKhau) async {
    try {
      final url = Uri.parse('$baseUrl/api/auth/login');

      // Send credentials as form data (application/x-www-form-urlencoded)
      // Some backends expect multipart/form-data (form-data in Postman). Use
      // MultipartRequest to send form-data fields exactly as the server expects.
      final req = http.MultipartRequest('POST', url);
      // Use lowercase field names as shown in API examples (tendangnhap/matkhau)
      req.fields['tendangnhap'] = tenDangNhap.trim();
      req.fields['matkhau'] = matKhau.trim();
      req.headers['Accept'] = 'application/json';

      if (kDebugMode) {
        // Helpful debug: show what we'll send (avoid printing secrets in prod)
        print('[DEBUG] LOGIN multipart fields: ${req.fields}');
      }

      final streamed = await req.send();
      final respBody = await streamed.stream.bytesToString();
      final status = streamed.statusCode;

      if (kDebugMode) {
        print('[DEBUG] LOGIN multipart response status: $status');
        print('[DEBUG] LOGIN multipart response body: $respBody');
      }

      // If multipart fails (some servers prefer x-www-form-urlencoded),
      // try a fallback attempt with urlencoded form data.
      if (status != 200) {
        if (kDebugMode) print('[DEBUG] LOGIN multipart failed, trying x-www-form-urlencoded fallback');
        try {
          final fallbackResp = await http.post(
            url,
            headers: {'Content-Type': 'application/x-www-form-urlencoded', 'Accept': 'application/json'},
            body: {'tendangnhap': tenDangNhap.trim(), 'matkhau': matKhau.trim()},
          );
          final fbStatus = fallbackResp.statusCode;
          final fbBody = fallbackResp.body;
          if (kDebugMode) {
            print('[DEBUG] LOGIN fallback status: $fbStatus');
            print('[DEBUG] LOGIN fallback body: $fbBody');
          }
          // Use fallback response instead of multipart response
          // by replacing respBody/status for subsequent parsing.
          // (fall through to the existing parsing logic)
          // assign to local vars via shadowing
          final respBodyFb = fbBody;
          final statusFb = fbStatus;
          // reuse below parsing by setting respBody/status via temporary variables
          // we'll jump into parsing block by checking _status
          if (statusFb == 200) {
            final data = jsonDecode(respBodyFb);

            String? accessToken;
            String? refreshToken;
            if (data is Map<String, dynamic>) {
              accessToken = data['accessToken']
                  ?? (data['data'] is Map ? data['data']['accessToken'] : null)
                  ?? data['token']
                  ?? (data['data'] is Map ? data['data']['token'] : null)
                  ?? data['jwt']
                  ?? (data['data'] is Map ? data['data']['jwt'] : null)
                  ?? data['access_token']
                  ?? (data['data'] is Map ? data['data']['access_token'] : null);

              refreshToken = data['refreshToken']
                  ?? (data['data'] is Map ? data['data']['refreshToken'] : null)
                  ?? data['refresh_token']
                  ?? (data['data'] is Map ? data['data']['refresh_token'] : null);
            }

            return {
              'success': true,
              'data': data,
              'accessToken': accessToken,
              'refreshToken': refreshToken,
            };
          } else {
            Map<String, dynamic>? errorData;
            try {
              errorData = jsonDecode(respBodyFb) as Map<String, dynamic>;
            } catch (_) {}
            final message = errorData != null
                ? (errorData['message']?.toString() ?? errorData['error']?.toString())
                : 'Đăng nhập thất bại (mã $statusFb)';
            final raw = respBodyFb;
            final snippet = raw.length > 180 ? '${raw.substring(0, 180)}…' : raw;
            return {
              'success': false,
              'message': message,
              'status': statusFb,
              'raw': snippet,
              'endpoint': url.toString(),
            };
          }
        } catch (e) {
          if (kDebugMode) print('[DEBUG] LOGIN fallback failed: $e');
          // fall through to return original multipart response failure below
        }
      }

      if (status == 200) {
        final data = jsonDecode(respBody);

        String? accessToken;
        String? refreshToken;
        if (data is Map<String, dynamic>) {
          accessToken = data['accessToken']
              ?? (data['data'] is Map ? data['data']['accessToken'] : null)
              ?? data['token']
              ?? (data['data'] is Map ? data['data']['token'] : null)
              ?? data['jwt']
              ?? (data['data'] is Map ? data['data']['jwt'] : null)
              ?? data['access_token']
              ?? (data['data'] is Map ? data['data']['access_token'] : null);

          refreshToken = data['refreshToken']
              ?? (data['data'] is Map ? data['data']['refreshToken'] : null)
              ?? data['refresh_token']
              ?? (data['data'] is Map ? data['data']['refresh_token'] : null);
        }

        return {
          'success': true,
          'data': data,
          'accessToken': accessToken,
          'refreshToken': refreshToken,
        };
      } else {
        Map<String, dynamic>? errorData;
        try {
          errorData = jsonDecode(respBody) as Map<String, dynamic>;
        } catch (_) {}
        final message = errorData != null
            ? (errorData['message']?.toString() ?? errorData['error']?.toString())
            : 'Đăng nhập thất bại (mã $status)';
        final raw = respBody;
        final snippet = raw.length > 180 ? '${raw.substring(0, 180)}…' : raw;
        return {
          'success': false,
          'message': message,
          'status': status,
          'raw': snippet,
          'endpoint': url.toString(),
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi kết nối: $e',
      };
    }
  }

  // API Đăng xuất
  static Future<Map<String, dynamic>> logout() async {
    try {
      // Lấy accessToken từ SharedPreferences
      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('accessToken');

      if (accessToken == null) {
        return {
          'success': false,
          'message': 'Không tìm thấy token',
        };
      }

  final url = Uri.parse('$baseUrl/api/auth/logout');

      var response = await http.post(
        url,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $accessToken',
        },
      );
      // Nếu 401, thử làm mới token và gọi lại 1 lần
      if (response.statusCode == 401) {
        final refreshed = await refreshAccessToken();
        if (refreshed['success'] == true) {
          final newAccess = (await SharedPreferences.getInstance()).getString('accessToken');
          if (newAccess != null) {
            response = await http.post(
              url,
              headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer $newAccess',
              },
            );
          }
        }
      }

      if (response.statusCode == 200) {
        // Xóa token khỏi SharedPreferences
        await prefs.remove('accessToken');
        await prefs.remove('refreshToken');
        
        return {
          'success': true,
          'message': 'Đăng xuất thành công',
        };
      } else {
        return {
          'success': false,
          'message': 'Đăng xuất thất bại',
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // Ví dụ: GET request
  static Future<Map<String, dynamic>> getStudentInfo(String mssv) async {
    try {
      final url = Uri.parse('$baseUrl/students/$mssv');
      
      final response = await http.get(
        url,
        headers: {
          'Content-Type': 'application/json',
          // Nếu cần token: 'Authorization': 'Bearer $token',
        },
      );

      if (response.statusCode == 200) {
        return {
          'success': true,
          'data': jsonDecode(response.body),
        };
      } else {
        return {
          'success': false,
          'message': 'Không thể lấy thông tin sinh viên',
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  // Lấy thông tin người dùng hiện tại (thông tin cá nhân)
  // Endpoint: GET /api/user/info (authorized)
  static Future<Map<String, dynamic>> fetchUserInfo() async {
    try {
      final url = Uri.parse('$baseUrl/api/user/info');
      final response = await _authorizedRequest(url, method: 'GET');

      if (response.statusCode >= 200 && response.statusCode < 300) {
        final data = jsonDecode(response.body);

        // Normalize to a Map<String, dynamic> representing the user info
        Map<String, dynamic> info = {};
        if (data is Map<String, dynamic>) {
          if (data['data'] is Map<String, dynamic>) {
            info = Map<String, dynamic>.from(data['data']);
          } else {
            info = Map<String, dynamic>.from(data);
          }
        } else {
          info = {'raw': data};
        }

        if (kDebugMode) {
          try {
            print('[DEBUG] fetchUserInfo RAW: ${response.body}');
          } catch (_) {}
        }

        return {
          'success': true,
          'data': info,
          'raw': data,
        };
      } else {
        if (kDebugMode) {
          try {
            print('[DEBUG] fetchUserInfo ERROR. STATUS: ${response.statusCode} RAW: ${response.body}');
          } catch (_) {}
        }
        final msg = _tryParseMessage(response.body) ?? 'Không thể lấy thông tin người dùng';
        return {
          'success': false,
          'message': msg,
          'status': response.statusCode,
        };
      }
    } catch (e) {
      return {
        'success': false,
        'message': 'Lỗi: $e',
      };
    }
  }

  static Future<Map<String, dynamic>> attendanceCheckin(String token, double latitude, double longitude) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('accessToken');
      if (accessToken == null) {
        return {
          'Status': '401',
          'Message': 'Thiếu accessToken',
          'Data': null,
        };
      }

      final base = Uri.parse('$baseUrl/api/attendance/checkin');
      final url = base.replace(queryParameters: {
        'Token': token,
        'Latitude': latitude.toString(),
        'Longitude': longitude.toString(),
      });

      http.Response response = await http.post(
        url,
        headers: {
          'Authorization': 'Bearer $accessToken',
          'Accept': 'application/json',
        },
      );

      if (response.statusCode == 401) {
        final refreshed = await refreshAccessToken();
        if (refreshed['success'] == true) {
          final newAccess = (await SharedPreferences.getInstance()).getString('accessToken');
            if (newAccess != null) {
            response = await http.post(
              url,
              headers: {
                'Authorization': 'Bearer $newAccess',
                'Accept': 'application/json',
              },
            );
          }
        }
      }

      final rawBody = response.body;
      Map<String, dynamic>? parsed;
      try {
        final j = jsonDecode(rawBody);
        if (j is Map<String, dynamic>) parsed = j;
      } catch (_) {}

      // Nếu parse được Map -> trả nguyên; nếu không -> bọc tối thiểu.
      if (parsed != null) return parsed;

      return {
        'Status': response.statusCode.toString(),
        'Message': 'Không đọc được JSON phản hồi',
        'Data': null,
        'Raw': rawBody,
      };
    } catch (e) {
      return {
        'Status': '500',
        'Message': 'Lỗi điểm danh: $e',
        'Data': null,
      };
    }
  }

  // Lấy danh sách điểm danh (attendance records)
  static Future<Map<String, dynamic>> fetchAttendanceRecords({
    int page = 1,
    int pageSize = 20,
    String? sortBy,
    String? sortDir,
    String? maDiemDanh,
    String? thoiGianQuet,
    String? maTrangThai,
    String? trangThai,
    String? maBuoi,
    String? maLopHocPhan,
    String? maSinhVien,
  }) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final accessToken = prefs.getString('accessToken');
      maSinhVien ??= prefs.getString('username');

      if (accessToken == null) {
        return {
          'Status': '401',
          'Message': 'Thiếu accessToken',
          'Data': null,
        };
      }

      final params = <String, String>{
        'Page': page.toString(),
        'PageSize': pageSize.toString(),
        if (sortBy != null && sortBy.isNotEmpty) 'SortBy': sortBy,
        if (sortDir != null && sortDir.isNotEmpty) 'SortDir': sortDir,
        if (maDiemDanh != null && maDiemDanh.isNotEmpty) 'MaDiemDanh': maDiemDanh,
        if (thoiGianQuet != null && thoiGianQuet.isNotEmpty) 'ThoiGianQuet': thoiGianQuet,
        if (maTrangThai != null && maTrangThai.isNotEmpty) 'maTrangThai': maTrangThai, // theo đặc tả
        if (trangThai != null && trangThai.isNotEmpty) 'TrangThai': trangThai,
        if (maBuoi != null && maBuoi.isNotEmpty) 'MaBuoi': maBuoi,
        if (maLopHocPhan != null && maLopHocPhan.isNotEmpty) 'MaLopHocPhan': maLopHocPhan,
        if (maSinhVien != null && maSinhVien.isNotEmpty) 'MaSinhVien': maSinhVien!,
      };

      final url = Uri.parse('$baseUrl/api/attendance/records').replace(queryParameters: params);

      http.Response response = await http.get(
        url,
        headers: {
          'Authorization': 'Bearer $accessToken',
          'Accept': 'application/json',
        },
      );

      // Refresh nếu 401
      if (response.statusCode == 401) {
        final refreshed = await refreshAccessToken();
        if (refreshed['success'] == true) {
          final newAccess = (await SharedPreferences.getInstance()).getString('accessToken');
          if (newAccess != null) {
            response = await http.get(
              url,
              headers: {
                'Authorization': 'Bearer $newAccess',
                'Accept': 'application/json',
              },
            );
          }
        }
      }

      Map<String, dynamic>? parsed;
      try {
        final j = jsonDecode(response.body);
        if (j is Map<String, dynamic>) parsed = j;
      } catch (_) {}

      if (parsed != null) return parsed;

      return {
        'Status': response.statusCode.toString(),
        'Message': 'Không đọc được JSON phản hồi',
        'Data': null,
        'Raw': response.body,
      };
    } catch (e) {
      return {
        'Status': '500',
        'Message': 'Lỗi lấy lịch sử điểm danh: $e',
        'Data': null,
      };
    }
  }
}
