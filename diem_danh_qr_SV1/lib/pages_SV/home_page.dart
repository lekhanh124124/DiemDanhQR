import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import '../services/api_service.dart';
import 'qr_scanner_page.dart';
import 'attendance_records_page.dart';
import 'attendance_page.dart';
import 'personal_page.dart';
import 'schedule_list_page.dart';

// ======================= HELPER DIALOG =======================

// Helper hiển thị popup kết quả điểm danh đẹp
Future<void> _showCheckinDialog(BuildContext context, Map<String, dynamic> res) async {
  final data = res['data'] ?? res['Data'];
  if (data is! Map) return;
  final diemDanh = data['diemDanh'] as Map? ?? {};
  final trangThai = data['trangThaiDiemDanh'] as Map? ?? {};
  // final sinhVien = data['sinhVien'] as Map? ?? {}; // (Có thể dùng nếu cần hiển thị tên SV)
  final buoiHoc = data['buoiHoc'] as Map? ?? {};
  final lopHocPhan = data['lopHocPhan'] as Map? ?? {};

  String safe(dynamic v) => (v == null || v.toString().toLowerCase() == 'null') ? '' : v.toString();

  await showDialog(
    context: context,
    builder: (c) {
      return Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        elevation: 0,
        backgroundColor: Colors.transparent,
        child: Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
            boxShadow: const [BoxShadow(color: Colors.black26, blurRadius: 10, offset: Offset(0, 4))],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Icon thành công
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(color: Colors.green.withOpacity(0.1), shape: BoxShape.circle),
                child: const Icon(Icons.check_circle, color: Colors.green, size: 40),
              ),
              const SizedBox(height: 12),
              const Text('Điểm danh thành công!', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.green)),
              const SizedBox(height: 16),
              const Divider(),
              
              // Thông tin chi tiết
              Align(alignment: Alignment.centerLeft, child: Text('Thông tin chi tiết:', style: TextStyle(color: Colors.grey[600], fontSize: 13))),
              const SizedBox(height: 8),
              if (safe(trangThai['tenTrangThai']).isNotEmpty)
                _infoRow(Icons.info_outline, 'Trạng thái', safe(trangThai['tenTrangThai']), color: Colors.blue),
              _infoRow(Icons.class_, 'Lớp HP', safe(lopHocPhan['tenLopHocPhan'])),
              _infoRow(Icons.access_time, 'Thời gian', safe(diemDanh['thoiGianQuet'])),
              _infoRow(Icons.calendar_today, 'Ngày học', safe(buoiHoc['ngayHoc'])),
              _infoRow(Icons.schedule, 'Tiết', '${safe(buoiHoc['tietBatDau'])} - Số tiết: ${safe(buoiHoc['soTiet'])}'),
              
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.blueAccent,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    padding: const EdgeInsets.symmetric(vertical: 12),
                  ),
                  onPressed: () => Navigator.pop(c),
                  child: const Text('Đóng', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                ),
              )
            ],
          ),
        ),
      );
    },
  );
}

Widget _infoRow(IconData icon, String label, String value, {Color? color}) {
  if (value.isEmpty) return const SizedBox.shrink();
  return Padding(
    padding: const EdgeInsets.symmetric(vertical: 4),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: color ?? Colors.grey[600]),
        const SizedBox(width: 8),
        Expanded(
          child: RichText(
            text: TextSpan(
              style: const TextStyle(color: Colors.black87, fontSize: 14),
              children: [
                TextSpan(text: '$label: ', style: TextStyle(color: Colors.grey[600])),
                TextSpan(text: value, style: const TextStyle(fontWeight: FontWeight.w600)),
              ],
            ),
          ),
        ),
      ],
    ),
  );
}

bool _isCheckinSuccess(Map<String, dynamic> res) {
  final st = (res['status'] ?? res['Status'])?.toString();
  return st == '200' && (res['data'] ?? res['Data']) is Map;
}

// ======================= MAIN PAGE =======================

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int _selectedIndex = 0;

  static const List<Widget> _pages = <Widget>[
    HomeContent(),
    ScheduleListPage(),
    AttendancePage(),
    PersonalPage(),
  ];

  void _onItemTapped(int index) {
    setState(() => _selectedIndex = index);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBody: true, // Cho phép body tràn xuống dưới navbar
      body: _pages[_selectedIndex],
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: const BorderRadius.only(topLeft: Radius.circular(20), topRight: Radius.circular(20)),
          boxShadow: [
            BoxShadow(color: Colors.blue.withOpacity(0.08), blurRadius: 10, offset: const Offset(0, -5))
          ],
        ),
        child: ClipRRect(
          borderRadius: const BorderRadius.only(topLeft: Radius.circular(20), topRight: Radius.circular(20)),
          child: BottomNavigationBar(
            type: BottomNavigationBarType.fixed,
            currentIndex: _selectedIndex,
            selectedItemColor: const Color(0xFF2563EB), // Xanh đậm
            unselectedItemColor: const Color(0xFF94A3B8), // Xám xanh
            backgroundColor: Colors.white,
            selectedLabelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
            showUnselectedLabels: true,
            elevation: 0,
            onTap: _onItemTapped,
            items: const [
              BottomNavigationBarItem(icon: Icon(Icons.home_rounded), label: "Trang chủ"),
              BottomNavigationBarItem(icon: Icon(Icons.calendar_month_rounded), label: "Lịch học"),
              BottomNavigationBarItem(icon: Icon(Icons.qr_code_scanner_rounded), label: "Điểm danh"),
              BottomNavigationBarItem(icon: Icon(Icons.person_rounded), label: "Cá nhân"),
            ],
          ),
        ),
      ),
    );
  }
}

class HomeContent extends StatefulWidget {
  const HomeContent({super.key});

  @override
  State<HomeContent> createState() => _HomeContentState();
}

class _HomeContentState extends State<HomeContent> {
  late Future<Map<String, dynamic>> _futureInfo;

  @override
  void initState() {
    super.initState();
    _futureInfo = ApiService.fetchUserInfo();
  }

  // --- LOGIC ---

  Future<bool> _ensureLocationPermission() async {
    bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      if (!mounted) return false;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vui lòng bật GPS / Location Service')));
      return false;
    }

    LocationPermission perm = await Geolocator.checkPermission();
    if (perm == LocationPermission.denied) {
      perm = await Geolocator.requestPermission();
    }
    if (perm == LocationPermission.deniedForever) {
      if (!mounted) return false;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Quyền vị trí bị từ chối vĩnh viễn. Mở cài đặt để cấp quyền.')));
      await Geolocator.openAppSettings();
      return false;
    }
    if (perm == LocationPermission.denied) {
      if (!mounted) return false;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Bạn đã từ chối quyền vị trí')));
      return false;
    }
    return true;
  }

  Future<void> _handleQRScan() async {
    if (!await _ensureLocationPermission()) return;
    
    // ignore: use_build_context_synchronously
    final code = await Navigator.push(context, MaterialPageRoute(builder: (_) => const QRScannerPage()));
    if (code == null || !mounted) return;

    // Show loading dialog
    showDialog(context: context, barrierDismissible: false, builder: (c) => const Center(child: CircularProgressIndicator()));

    try {
      final pos = await Geolocator.getCurrentPosition(desiredAccuracy: LocationAccuracy.high);
      final res = await ApiService.attendanceCheckin(code, pos.latitude, pos.longitude);
      
      if (!mounted) return;
      Navigator.pop(context); // Tắt loading

      if (_isCheckinSuccess(res)) {
        await _showCheckinDialog(context, res);
      } else {
        final msg = (res['message'] ?? res['Message'] ?? 'Điểm danh thất bại').toString();
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg), backgroundColor: Colors.red[600]));
      }
    } catch (e) {
      if (!mounted) return;
      Navigator.pop(context); // Tắt loading
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Lỗi: $e'), backgroundColor: Colors.red[600]));
    }
  }

  // --- UI COMPONENTS ---

  Widget _buildBackgroundDecoration() {
    return Stack(
      children: [
        Container(
          decoration: const BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: [
                Color(0xFFE0F2FE), // Sky 100
                Color(0xFFF0F9FF), // Sky 50
                Colors.white,
              ],
            ),
          ),
        ),
        Positioned(
          top: 60,
          right: -40,
          child: Container(
            width: 200, height: 200,
            decoration: BoxDecoration(color: Colors.blueAccent.withOpacity(0.05), shape: BoxShape.circle),
          ),
        ),
        Positioned(
          bottom: 120,
          left: -60,
          child: Container(
            width: 300, height: 300,
            decoration: BoxDecoration(color: Colors.lightBlueAccent.withOpacity(0.08), shape: BoxShape.circle),
          ),
        ),
      ],
    );
  }

  Widget _buildDashboardCard(String title, IconData icon, Color color, VoidCallback onTap, {String? subTitle}) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.grey.withOpacity(0.1)),
        boxShadow: [
          BoxShadow(
            color: color.withOpacity(0.1),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: BorderRadius.circular(20),
          onTap: onTap,
          splashColor: color.withOpacity(0.1),
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center, // CĂN GIỮA
              children: [
                Container(
                  width: 50, height: 50,
                  decoration: BoxDecoration(color: color.withOpacity(0.1), shape: BoxShape.circle),
                  child: Icon(icon, size: 26, color: color),
                ),
                const SizedBox(height: 12),
                Text(title, 
                  textAlign: TextAlign.center,
                  style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold, color: Colors.black87)
                ),
                if (subTitle != null) ...[
                  const SizedBox(height: 4),
                  Text(subTitle, 
                    textAlign: TextAlign.center, maxLines: 1, overflow: TextOverflow.ellipsis,
                    style: TextStyle(fontSize: 12, color: Colors.grey[500], fontWeight: FontWeight.w500)
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildHeader(AsyncSnapshot<Map<String, dynamic>> snap) {
    String name = 'Sinh viên';
    String? avatarUrl;

    if (snap.hasData && snap.data!['success'] == true) {
      final res = snap.data!['data'] ?? {};
      final sv = res['sinhVien'] ?? {};
      final nd = res['nguoiDung'] ?? {};
      name = sv['tenSinhVien'] ?? nd['hoTen'] ?? 'Sinh viên';
      
      final rawAvatar = nd['anhDaiDien'] ?? nd['avatar'];
      if (rawAvatar != null && rawAvatar.toString().isNotEmpty && rawAvatar.toString().toLowerCase() != 'null') {
         String a = rawAvatar.toString();
         if (!a.startsWith('http')) {
            final base = ApiService.baseUrl.replaceAll(RegExp(r'/$'), '');
            a = base + (a.startsWith('/') ? a : '/$a');
         }
         avatarUrl = a;
      }
    }

    return Container(
      padding: EdgeInsets.only(top: MediaQuery.of(context).padding.top + 10, left: 24, right: 24, bottom: 24),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [Color(0xFF2563EB), Color(0xFF3B82F6)],
          begin: Alignment.bottomLeft, end: Alignment.topRight,
        ),
        borderRadius: const BorderRadius.only(bottomLeft: Radius.circular(30), bottomRight: Radius.circular(30)),
        boxShadow: [BoxShadow(color: const Color(0xFF2563EB).withOpacity(0.3), blurRadius: 20, offset: const Offset(0, 10))],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(2),
            decoration: BoxDecoration(border: Border.all(color: Colors.white.withOpacity(0.5), width: 2), shape: BoxShape.circle),
            child: CircleAvatar(
              radius: 26,
              backgroundColor: Colors.white,
              backgroundImage: avatarUrl != null ? NetworkImage(avatarUrl) : null,
              child: avatarUrl == null ? const Icon(Icons.person, color: Colors.grey) : null,
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Xin chào,', style: TextStyle(color: Colors.white.withOpacity(0.8), fontSize: 13)),
                Text(name, 
                  maxLines: 1, overflow: TextOverflow.ellipsis,
                  style: const TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold)
                ),
              ],
            ),
          ),
          IconButton(onPressed: (){}, icon: const Icon(Icons.notifications_none_rounded, color: Colors.white, size: 28))
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        _buildBackgroundDecoration(), // Lớp nền
        Column(
          children: [
            FutureBuilder<Map<String, dynamic>>(
              future: _futureInfo,
              builder: (context, snap) => _buildHeader(snap),
            ),
            Expanded(
              child: SingleChildScrollView(
                physics: const BouncingScrollPhysics(),
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text("Dịch vụ trực tuyến", style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Color(0xFF1E293B))),
                    const SizedBox(height: 16),
                    GridView.count(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      crossAxisCount: 2,
                      mainAxisSpacing: 16,
                      crossAxisSpacing: 16,
                      childAspectRatio: 1.1, // QUAN TRỌNG: Làm nút cân đối (hơi vuông)
                      children: [
                        _buildDashboardCard('Điểm danh', Icons.qr_code_scanner_rounded, const Color(0xFF0EA5E9), _handleQRScan, subTitle: 'Quét mã QR'),
                        _buildDashboardCard('Lịch học', Icons.calendar_month_rounded, const Color(0xFFF59E0B), 
                          () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ScheduleListPage())), subTitle: 'Xem TKB'),
                        _buildDashboardCard('Lịch sử', Icons.history_edu_rounded, const Color(0xFF8B5CF6), 
                          () => Navigator.push(context, MaterialPageRoute(builder: (_) => const AttendanceRecordsPage())), subTitle: 'Nhật ký'),
                        _buildDashboardCard('Hồ sơ', Icons.account_circle_rounded, const Color(0xFF10B981), 
                          () => Navigator.push(context, MaterialPageRoute(builder: (_) => const PersonalPage())), subTitle: 'Thông tin'),
                      ],
                    ),
                    const SizedBox(height: 80),
                  ],
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }
}