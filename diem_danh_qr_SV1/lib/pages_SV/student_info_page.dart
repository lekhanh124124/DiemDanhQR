import 'package:flutter/material.dart';
import '../services/api_service.dart';

class StudentInfoPage extends StatefulWidget {
  const StudentInfoPage({super.key});

  @override
  State<StudentInfoPage> createState() => _StudentInfoPageState();
}

class _StudentInfoPageState extends State<StudentInfoPage> {
  // Màu sắc và Styles
  static const Color _primaryColor = Color(0xFF4C7AF3); // Blue
  static const Color _backgroundColor = Color(0xFFF8F9FA); // Light Grey

  late Future<Map<String, dynamic>> _futureInfo;

  // MARK: - Helper Functions (Giữ nguyên logic gốc)
  String _present(dynamic v) {
    if (v == null) return 'Chưa có';
    final s = v.toString().trim();
    if (s.isEmpty) return 'Chưa có';
    if (s.toLowerCase() == 'null') return 'Chưa có';
    return s;
  }

  String _formatDateString(dynamic v) {
    final raw = _present(v);
    if (raw == 'Chưa có') return raw;
    try {
      DateTime dt = DateTime.parse(raw);
      return '${dt.day.toString().padLeft(2, '0')}/${dt.month.toString().padLeft(2, '0')}/${dt.year}';
    } catch (_) {}

    final seps = ['/', '-', '.'];
    for (final sep in seps) {
      if (raw.contains(sep)) {
        final parts = raw.split(sep).map((p) => p.trim()).toList();
        if (parts.length == 3) {
          if (parts[0].length == 4) {
            final y = int.tryParse(parts[0]) ?? 0;
            final m = int.tryParse(parts[1]) ?? 0;
            final d = int.tryParse(parts[2]) ?? 0;
            if (y > 0 && m > 0 && d > 0) return '${d.toString().padLeft(2, '0')}/${m.toString().padLeft(2, '0')}/${y.toString().padLeft(4, '0')}';
          } else if (parts[2].length == 4) {
            final d = int.tryParse(parts[0]) ?? 0;
            final m = int.tryParse(parts[1]) ?? 0;
            final y = int.tryParse(parts[2]) ?? 0;
            if (y > 0 && m > 0 && d > 0) return '${d.toString().padLeft(2, '0')}/${m.toString().padLeft(2, '0')}/${y.toString().padLeft(4, '0')}';
          }
        }
      }
    }
    return raw;
  }

  @override
  void initState() {
    super.initState();
    _futureInfo = ApiService.fetchUserInfo();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureInfo = ApiService.fetchUserInfo();
    });
    await _futureInfo;
  }

  Future<void> _handleLogout(BuildContext context) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xác nhận đăng xuất'),
        content: const Text('Bạn có chắc chắn muốn đăng xuất?'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Hủy')),
          TextButton(onPressed: () => Navigator.pop(context, true), child: const Text('Đăng xuất', style: TextStyle(color: Colors.red))),
        ],
      ),
    );

    if (confirm == true) {
      if (!context.mounted) return;
      final navigator = Navigator.of(context);
      final messenger = ScaffoldMessenger.of(context);
      // Sử dụng showDialog cho loading
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: CircularProgressIndicator()),
      );
      final result = await ApiService.logout();
      if (navigator.mounted) navigator.pop();
      if (result['success']) {
        messenger.showSnackBar(const SnackBar(content: Text('Đăng xuất thành công!'), backgroundColor: Colors.green));
        // Đảm bảo '/login' là route hợp lệ
        navigator.pushNamedAndRemoveUntil('/login', (route) => false);
      } else {
        messenger.showSnackBar(SnackBar(content: Text(result['message'] ?? 'Đăng xuất thất bại'), backgroundColor: Colors.red));
      }
    }
  }

  // MARK: - UI Components
  
  // Widget tái sử dụng cho mỗi dòng thông tin
  Widget _buildInfoRow(IconData icon, String title, String value) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 5,
            offset: const Offset(0, 3), 
          ),
        ],
      ),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, color: _primaryColor, size: 22),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: TextStyle(
                    fontWeight: FontWeight.w600,
                    fontSize: 14,
                    color: Colors.grey.shade600,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  value,
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // Widget xây dựng Header chứa Avatar, Tên, MSSV
  Widget _buildProfileHeader(String fullName, String mssv, String? avatarUrl) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.only(top: 16, bottom: 32),
      child: Column(
        children: <Widget>[
          // Avatar
          Container(
            padding: const EdgeInsets.all(5),
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(color: _primaryColor.withOpacity(0.5), width: 3),
            ),
            child: ClipOval(
              child: avatarUrl != null
                  ? Image.network(
                      avatarUrl.toString(),
                      width: 110,
                      height: 110,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) => Container(
                        width: 110,
                        height: 110,
                        decoration: BoxDecoration(
                          color: Colors.grey.shade300,
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(Icons.person, size: 60, color: Colors.white),
                      ),
                    )
                  : Container(
                      width: 110,
                      height: 110,
                      decoration: BoxDecoration(
                        color: Colors.grey.shade300,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(Icons.person, size: 60, color: Colors.white),
                    ),
            ),
          ),
          const SizedBox(height: 16),
          
          // Tên
          Text(
            _present(fullName),
            textAlign: TextAlign.center,
            style: const TextStyle(
              fontSize: 26,
              fontWeight: FontWeight.w900,
              color: Colors.black87,
            ),
          ),
          const SizedBox(height: 4),
          
          // MSSV
          Text(
            'MSSV: ${_present(mssv)}',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 16, color: Colors.grey.shade600, fontWeight: FontWeight.w500),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _backgroundColor,
      appBar: AppBar(
        title: const Text('Thông tin cá nhân', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
        backgroundColor: _primaryColor,
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white),
        actions: [
          IconButton(
            onPressed: _refresh, 
            icon: const Icon(Icons.refresh, color: Colors.white),
            tooltip: 'Làm mới',
          ),
        ],
      ),
      body: FutureBuilder<Map<String, dynamic>>(
        future: _futureInfo,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator(color: _primaryColor));
          }
          if (snap.hasError) {
            return Center(child: Text('Lỗi khi tải thông tin: ${snap.error}'));
          }

          final res = snap.data ?? {};
          if (res['success'] != true) {
            final msg = res['message'] ?? 'Không thể tải thông tin';
            return Center(child: Text(msg));
          }

          final Map<String, dynamic> info = Map<String, dynamic>.from(res['data'] ?? {});

          final Map<String, dynamic> nguoiDung = info['nguoiDung'] is Map ? Map<String, dynamic>.from(info['nguoiDung']) : {};
          final Map<String, dynamic> sinhVien = info['sinhVien'] is Map ? Map<String, dynamic>.from(info['sinhVien']) : {};

          // Lấy thông tin
          final fullName = nguoiDung['hoTen'] ?? nguoiDung['HoTen'] ?? nguoiDung['name'];
          final mssv = sinhVien['maSinhVien'] ?? sinhVien['MaSinhVien'] ?? nguoiDung['username'];
          final rawAvatar = nguoiDung['anhDaiDien'] ?? nguoiDung['anhDaiien'] ?? nguoiDung['avatar'];
          String? avatarUrl;
          if (rawAvatar is String && rawAvatar.isNotEmpty && rawAvatar.toLowerCase() != 'null') {
            var a = rawAvatar;
            if (!a.toLowerCase().startsWith('http')) {
              final base = ApiService.baseUrl.replaceAll(RegExp(r'/$'), '');
              a = base + (a.startsWith('/') ? a : '/$a');
            }
            avatarUrl = a;
          } else {
            avatarUrl = null;
          }
          final gioiTinh = nguoiDung['gioiTinh'] ?? '—';
          final email = nguoiDung['email'] ?? '—';
          final soDienThoai = nguoiDung['soDienThoai'] ?? nguoiDung['so_dien_thoai'] ?? '—';
          final dob = nguoiDung['ngaySinh'] ?? '—';
          final diaChi = nguoiDung['diaChi'] ?? '—';
          final namNhapHoc = sinhVien['namNhapHoc'] ?? sinhVien['NamNhapHoc'] ?? '—';

          return SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                // 1. Profile Header
                _buildProfileHeader(fullName, mssv, avatarUrl),
                
                const SizedBox(height: 16),
                
                // 2. Thông tin chi tiết
                _buildInfoRow(Icons.person_outline, 'Giới tính', _present(gioiTinh)),
                _buildInfoRow(Icons.cake_outlined, 'Ngày sinh', _formatDateString(dob)),
                _buildInfoRow(Icons.email_outlined, 'Email', _present(email)),
                _buildInfoRow(Icons.phone_outlined, 'Số điện thoại', _present(soDienThoai)),
                
                // Nhóm thông tin học tập
                const SizedBox(height: 20),
                Text('Thông tin học tập', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.grey.shade700)),
                const SizedBox(height: 12),
                
                _buildInfoRow(Icons.school_outlined, 'Năm nhập học', _present(namNhapHoc)),
                _buildInfoRow(Icons.location_on_outlined, 'Địa chỉ', _present(diaChi)),
                
                const SizedBox(height: 30),
                
                // 3. Nút Đăng xuất
                ElevatedButton.icon(
                  onPressed: () => _handleLogout(context),
                  icon: const Icon(Icons.logout, size: 24),
                  label: const Text('Đăng xuất', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.red.shade600,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                    elevation: 5,
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}