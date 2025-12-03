import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:geolocator/geolocator.dart';
import '../services/api_service.dart';
import 'qr_scanner_page.dart';

// -----------------------------------------------------------------------------
// LOGIC VÀ HÀM HỖ TRỢ (GIỮ NGUYÊN)
// -----------------------------------------------------------------------------

// Màu sắc và Styles mới
const Color _headerColor = Color(0xFF3B82F6); // Xanh dương đậm cho header
const Color _pageBackgroundColor = Color(0xFFF1F5F9); // Màu nền rất nhạt (Slate 100)
const Color _decorativeShapeColor = Color(0xFFE0E7FF); // Xanh nhạt cho các hình trang trí

Future<void> _showCheckinDialog(BuildContext context, Map<String, dynamic> res) async {
  final data = res['data'] ?? res['Data'];
  if (data is! Map) return;
  final diemDanh = data['diemDanh'] as Map? ?? {};
  final trangThai = data['trangThaiDiemDanh'] as Map? ?? {};
  final sinhVien = data['sinhVien'] as Map? ?? {};
  final buoiHoc = data['buoiHoc'] as Map? ?? {};
  final lopHocPhan = data['lopHocPhan'] as Map? ?? {};

  String safe(dynamic v) => (v == null || v.toString().toLowerCase() == 'null') ? '' : v.toString();

  Color statusColor = Colors.blue;
  IconData statusIcon = Icons.info;
  final statusCode = safe(trangThai['codeTrangThai']);

  if (statusCode.contains('SUCCESS')) {
    statusColor = Colors.green;
    statusIcon = Icons.check_circle;
  } else if (statusCode.contains('LATE') || statusCode.contains('WARN')) {
    statusColor = Colors.orange;
    statusIcon = Icons.warning;
  } else if (statusCode.contains('FAIL') || statusCode.contains('ERROR')) {
    statusColor = Colors.red;
    statusIcon = Icons.cancel;
  }

  await showDialog(
    context: context,
    barrierDismissible: true,
    builder: (c) {
      return AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Thông tin điểm danh', style: TextStyle(fontWeight: FontWeight.bold)),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (safe(trangThai['tenTrangThai']).isNotEmpty)
                Container(
                  padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 12),
                  decoration: BoxDecoration(
                    color: statusColor.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    children: [
                      Icon(statusIcon, size: 20, color: statusColor),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          safe(trangThai['tenTrangThai']),
                          style: TextStyle(fontWeight: FontWeight.w700, color: statusColor),
                        ),
                      ),
                    ],
                  ),
                ),
              const SizedBox(height: 12),
              _kv('Mã điểm danh', safe(diemDanh['maDiemDanh'])),
              _kv('Thời gian quét', safe(diemDanh['thoiGianQuet'])),
              const Divider(height: 16),
              _kv('Mã SV', safe(sinhVien['maSinhVien'])),
              _kv('LHP', safe(lopHocPhan['maLopHocPhan'])),
              _kv('Tên LHP', safe(lopHocPhan['tenLopHocPhan'])),
              const Divider(height: 16),
              _kv('Ngày học', safe(buoiHoc['ngayHoc'])),
              _kv('Tiết bắt đầu', safe(buoiHoc['tietBatDau'])),
              _kv('Số tiết', safe(buoiHoc['soTiet'])),
              if (safe(trangThai['codeTrangThai']).isNotEmpty)
                _kv('Mã trạng thái', safe(trangThai['codeTrangThai'])),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(c), child: const Text('Đóng', style: TextStyle(fontWeight: FontWeight.bold))),
        ],
      );
    },
  );
}

Widget _kv(String k, String v) {
  if (v.isEmpty) return const SizedBox.shrink();
  return Padding(
    padding: const EdgeInsets.only(bottom: 6),
    child: Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 110,
          child: Text('$k:', style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.black54)),
        ),
        Expanded(child: Text(v, style: const TextStyle(color: Colors.black87))),
      ],
    ),
  );
}

bool _isCheckinSuccess(Map<String, dynamic> res) {
  final st = (res['status'] ?? res['Status'])?.toString();
  return st == '200' && (res['data'] ?? res['Data']) is Map;
}


// -----------------------------------------------------------------------------
// ATTENDANCE PAGE CLASS (GIỮ NGUYÊN LOGIC, CHỈ THAY ĐỔI BUILD METHOD)
// -----------------------------------------------------------------------------

class AttendancePage extends StatefulWidget {
  final DateTime? initialDate;
  const AttendancePage({super.key, this.initialDate});

  @override
  State<AttendancePage> createState() => _AttendancePageState();
}

class _AttendancePageState extends State<AttendancePage> {
  DateTime _anchor = DateTime.now();
  bool _isLoading = false;
  List<Map<String, dynamic>> _items = [];

  @override
  void initState() {
    super.initState();
    _anchor = widget.initialDate ?? DateTime.now();
    _fetch();
  }

  String _fmtDate(DateTime d) => '${d.year.toString().padLeft(4,'0')}-${d.month.toString().padLeft(2,'0')}-${d.day.toString().padLeft(2,'0')}';

  Future<void> _fetch() async {
    setState(() => _isLoading = true);
    try {
      final prefs = await SharedPreferences.getInstance();
      final mssv = prefs.getString('username') ?? '';
      final filters = {
        'NgayHoc': _fmtDate(_anchor),
        if (mssv.isNotEmpty) 'MaSinhVien': mssv,
      };

      final res = await ApiService.fetchSchedule(
        page: 1,
        pageSize: 50,
        sortBy: 'NgayHoc',
        sortDir: 'ASC',
        filters: filters,
      );

      if (!mounted) return;

      if (res['success'] == true && res['items'] != null) {
        setState(() => _items = (res['items'] as List).cast<Map<String, dynamic>>());
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(res['message']?.toString() ?? 'Không thể tải lịch học.')),
        );
         setState(() => _items = []);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Lỗi tải dữ liệu: $e'), backgroundColor: Colors.red));
      }
      setState(() => _items = []);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  String _formatDisplayDate(String raw) {
    final parts = raw.split(RegExp(r'[-/]'));
    if (parts.length == 3 && parts[2].length == 4) {
      return '${parts[0].padLeft(2,'0')}/${parts[1].padLeft(2,'0')}/${parts[2]}';
    }
    return raw;
  }

  Future<bool> _ensureLocationPermission() async {
    bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('⚠️ Bật GPS trước khi điểm danh')),
      );
      return false;
    }
    LocationPermission perm = await Geolocator.checkPermission();
    if (perm == LocationPermission.denied) {
      perm = await Geolocator.requestPermission();
    }
    if (perm == LocationPermission.deniedForever) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('❌ Quyền vị trí bị từ chối vĩnh viễn. Mở Settings để cấp lại.')),
      );
      await Geolocator.openAppSettings();
      return false;
    }
    if (perm == LocationPermission.denied) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Bạn đã từ chối quyền vị trí')),
      );
      return false;
    }
    return true;
  }

  Future<void> _onCheckIn(Map<String, dynamic> item) async {
    if (!await _ensureLocationPermission()) return;
    
    ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Đang chờ quét QR...'), duration: Duration(seconds: 1)),
    );

    final code = await Navigator.push(
      context,
      MaterialPageRoute(builder: (_) => const QRScannerPage()),
    );
    if (code == null || !mounted) return;
    
    try {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Đang lấy vị trí và gửi yêu cầu điểm danh...'), duration: Duration(seconds: 2)),
      );

      final pos = await Geolocator.getCurrentPosition(desiredAccuracy: LocationAccuracy.high, timeLimit: const Duration(seconds: 10));
      final res = await ApiService.attendanceCheckin(code, pos.latitude, pos.longitude);
      if (!mounted) return;
      
      ScaffoldMessenger.of(context).removeCurrentSnackBar();

      if (_isCheckinSuccess(res)) {
        await _showCheckinDialog(context, res);
      } else {
        final msg = (res['message'] ?? res['Message'] ?? 'Điểm danh thất bại').toString();
        await _showFailureDialog(context, msg);
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).removeCurrentSnackBar();
      await _showFailureDialog(context, 'Lỗi: $e');
    } finally {
        _fetch();
    }
  }

  // Wrap _onCheckIn to prevent uncaught exceptions from crashing the UI
  Future<void> _safeOnCheckIn(Map<String, dynamic> item) async {
    try {
      await _onCheckIn(item);
    } catch (e, st) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Lỗi khi điểm danh: $e')));
      }
    }
  }

  Future<void> _showFailureDialog(BuildContext context, String message) async {
    await showDialog(
      context: context,
      builder: (c) => AlertDialog(
        title: const Text('Điểm danh thất bại', style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold)),
        content: Text(message),
        actions: [
          TextButton(onPressed: () => Navigator.pop(c), child: const Text('Đóng')),
        ],
      ),
    );
  }


  String _findInMap(Map src, List<String> keys, [int depth = 3]) {
    if (depth < 0) return '';
    for (final k in keys) {
      if (src.containsKey(k) && src[k] != null && src[k].toString().trim().isNotEmpty) return src[k].toString();
      final low = k.toLowerCase();
      if (src.containsKey(low) && src[low] != null && src[low].toString().trim().isNotEmpty) return src[low].toString();
    }
    for (final v in src.values) {
      if (v is Map) {
        final out = _findInMap(v, keys, depth - 1);
        if (out.isNotEmpty) return out;
      }
    }
    return '';
  }

  String _findLecturer(Map src) {
    final nameKeys = ['HoTen', 'hoTen', 'TenGiangVien', 'tenGiangVien', 'Ten', 'ten', 'name', 'ho_ten'];
    final candidate = _findInMap(src, nameKeys);
    if (candidate.isNotEmpty) return candidate;
    for (final k in ['GiangVien', 'giangVien', 'NguoiDung', 'nguoiDung']) {
      if (src.containsKey(k) && src[k] is Map) {
        final inner = _findInMap(src[k] as Map, nameKeys);
        if (inner.isNotEmpty) return inner;
      }
    }
    return 'Chưa xác định';
  }

  Color _getColorForItem(String seed) {
    final colors = [
      const Color(0xFF3B82F6), // Blue
      const Color(0xFF10B981), // Emerald
      const Color(0xFFF59E0B), // Amber
      const Color(0xFFEF4444), // Red
      const Color(0xFF8B5CF6), // Violet
    ];
    if (seed.isEmpty) return colors[0];
    final sum = seed.runes.fold<int>(0, (p, r) => p + r);
    return colors[sum % colors.length];
  }

  // -----------------------------------------------------------------------------
  // GIAO DIỆN (ĐÃ CHỈNH SỬA)
  // -----------------------------------------------------------------------------
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _pageBackgroundColor, // Áp dụng màu nền nhạt
      appBar: AppBar(
        title: const Text('Điểm Danh', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
        centerTitle: true,
        backgroundColor: _headerColor, // Áp dụng màu xanh đậm cho header
        elevation: 0,
      ),
      body: Stack(
        children: [
          // 1. Decorative Background Shapes (mô phỏng Home Page)
          Positioned(
            top: -150,
            left: -150,
            child: Container(
              width: 300,
              height: 300,
              decoration: BoxDecoration(
                color: _decorativeShapeColor.withOpacity(0.6),
                borderRadius: BorderRadius.circular(150),
              ),
            ),
          ),
          Positioned(
            bottom: -200,
            right: -100,
            child: Container(
              width: 350,
              height: 350,
              decoration: BoxDecoration(
                color: _decorativeShapeColor.withOpacity(0.4),
                borderRadius: BorderRadius.circular(175),
              ),
            ),
          ),

          // 2. Main Content
          Column(
            children: [
              // Phần chọn ngày (Date Selector) - Giữ nguyên logic và style trắng
              _buildDateSelector(), 
              
              Expanded(
                child: _isLoading
                    ? const Center(child: CircularProgressIndicator(color: _headerColor))
                    : _items.isEmpty
                        ? Center(
                            child: Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(Icons.calendar_today_outlined, size: 60, color: Colors.grey[300]),
                                const SizedBox(height: 10),
                                const Text('Không có buổi học nào trong ngày', style: TextStyle(color: Colors.grey)),
                              ],
                            ),
                          )
                        : ListView.builder(
                            padding: const EdgeInsets.only(top: 8, bottom: 16),
                            itemCount: _items.length,
                            itemBuilder: (context, index) {
                              final item = _items[index];
                              return _buildScheduleCard(item);
                            },
                          ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildDateSelector() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: const BoxDecoration(
        color: Colors.white, // Vẫn giữ nền trắng cho thanh chọn ngày
        border: Border(bottom: BorderSide(color: Color.fromARGB(255, 230, 230, 230), width: 1)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          IconButton(
            icon: const Icon(Icons.arrow_back_ios, size: 20),
            color: _headerColor,
            onPressed: () {
              setState(() => _anchor = _anchor.subtract(const Duration(days: 1)));
              _fetch();
            },
          ),
          InkWell(
            onTap: () async {
               final selectedDate = await showDatePicker(
                context: context,
                initialDate: _anchor,
                firstDate: DateTime.now().subtract(const Duration(days: 365)),
                lastDate: DateTime.now().add(const Duration(days: 365)),
                locale: const Locale('vi', 'VN'),
                builder: (context, child) {
                  return Theme(
                    data: ThemeData.light().copyWith(
                      colorScheme: const ColorScheme.light(
                        primary: _headerColor, 
                      ),
                    ),
                    child: child!,
                  );
                },
              );
              if (selectedDate != null && selectedDate != _anchor) {
                setState(() => _anchor = selectedDate);
                _fetch();
              }
            },
            child: Row(
              children: [
                const Icon(Icons.event, color: _headerColor),
                const SizedBox(width: 8),
                Text(
                  '${_anchor.day.toString().padLeft(2, '0')}/${_anchor.month.toString().padLeft(2, '0')}/${_anchor.year}',
                  style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 18, color: Colors.black87),
                ),
              ],
            ),
          ),
          IconButton(
            icon: const Icon(Icons.arrow_forward_ios, size: 20),
            color: _headerColor,
            onPressed: () {
              setState(() => _anchor = _anchor.add(const Duration(days: 1)));
              _fetch();
            },
          ),
        ],
      ),
    );
  }

  Widget _buildScheduleCard(Map<String, dynamic> item) {
    final tenMon = _findInMap(item, [
      'TenMonHoc','tenMonHoc','TenLop','TenLopHocPhan','TenMon','TenMonHocFull'
    ]);
    final maLHP = _findInMap(item, [
      'MaLopHocPhan','maLopHocPhan'
    ]);
    final phong = _findInMap(item, [
      'TenPhong','tenPhong','Phong','TenPhongHoc','PhongHoc'
    ]);
    final gv = _findLecturer(item);
    final tietBD = _findInMap(item, ['TietBatDau', 'tietBatDau']);
    final soTiet = _findInMap(item, ['SoTiet', 'soTiet']);
    // Tính tiết kết thúc để hiển thị (tietBD + soTiet - 1)
    final tietKT = int.tryParse(tietBD) != null && int.tryParse(soTiet) != null ? (int.parse(tietBD) + int.parse(soTiet) - 1).toString() : 'N/A';


    final colorSeed = (maLHP.isNotEmpty ? maLHP : tenMon).toString();

    return Padding(
  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
  child: Card(
    elevation: 6,
    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
    child: InkWell(
    borderRadius: BorderRadius.circular(15),
    onTap: () => _safeOnCheckIn(item),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Accent bar
              Container(
                width: 8,
                margin: const EdgeInsets.symmetric(vertical: 6),
                decoration: BoxDecoration(
                  color: _getColorForItem(colorSeed),
                  borderRadius: BorderRadius.circular(8),
                  boxShadow: [
                    BoxShadow(
                      color: _getColorForItem(colorSeed).withOpacity(0.22),
                      blurRadius: 8,
                      offset: const Offset(0, 3),
                    )
                  ],
                ),
              ),

              const SizedBox(width: 12),

              // Nội dung chi tiết
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      tenMon,
                      style: const TextStyle(
                        fontSize: 17,
                        fontWeight: FontWeight.w700,
                        color: _headerColor,
                      ),
                    ),
                    const SizedBox(height: 6),
                    _buildInfoRow(Icons.class_outlined, 'LHP:', maLHP),
                    _buildInfoRow(Icons.schedule, 'Tiết:', '$tietBD - $tietKT ($soTiet tiết)'),
                    _buildInfoRow(Icons.location_on_outlined, 'Phòng:', phong),
                    _buildInfoRow(Icons.person_outline, 'GV:', gv),
                  ],
                ),
              ),

              // Nút điểm danh
              Align(
                alignment: Alignment.center,
                child: Column(
                  children: [
                    IconButton(
                      onPressed: () => _safeOnCheckIn(item),
                      icon: const Icon(Icons.qr_code_scanner, size: 30, color: Colors.green),
                      tooltip: 'Điểm danh',
                    ),
                    const Text(
                      'Scan',
                      style: TextStyle(
                        fontSize: 12,
                        color: Colors.green,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    ),
  ),
);

  }

  Widget _buildInfoRow(IconData icon, String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 16, color: Colors.grey[600]),
          const SizedBox(width: 6),
          Text(
            '$label ',
            style: const TextStyle(fontSize: 13, color: Colors.black54, fontWeight: FontWeight.w500),
          ),
          Expanded(
            child: Text(
              value,
              style: const TextStyle(fontSize: 13, color: Colors.black87),
            ),
          ),
        ],
      ),
    );
  }
}