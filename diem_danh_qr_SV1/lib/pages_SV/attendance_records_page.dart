import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../services/api_service.dart';

class AttendanceRecordsPage extends StatefulWidget {
  const AttendanceRecordsPage({super.key});

  @override
  State<AttendanceRecordsPage> createState() => _AttendanceRecordsPageState();
}

class _AttendanceRecordsPageState extends State<AttendanceRecordsPage> {
  late Future<Map<String, dynamic>> _futureRecords;

  // Màu sắc và Styles
  static const Color _primaryColor = Color(0xFF3B82F6); // Blue
  static const Color _backgroundColor = Color(0xFFF1F5F9); // Light Gray

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final prefs = await SharedPreferences.getInstance();
    final username = prefs.getString('username') ?? '';
    
    // Tải trang đầu tiên (hoặc trang hiện tại nếu có logic phân trang phức tạp hơn)
    final future = ApiService.fetchAttendanceRecords(
      page: 1,
      pageSize: 50, // Lấy nhiều hơn để giảm tải API call
      maSinhVien: username.isNotEmpty ? username : null,
      sortBy: 'ThoiGianQuet',
      sortDir: 'DESC',
    );

    setState(() {
      _futureRecords = future;
    });
    // Đợi future hoàn thành để RefreshIndicator hoạt động đúng
    await future;
  }

  // Hàm Helper: Trả về màu sắc dựa trên mã trạng thái
  Color _statusColor(String codeTrangThai) {
    final code = codeTrangThai.toUpperCase();
    if (code.contains('PRESENT') || code.contains('DA_DIEM_DANH')) return Colors.green.shade600;
    if (code.contains('FRAUD') || code.contains('KHONG_HOP_LE')) return Colors.red.shade600;
    if (code.contains('ABSENT') || code.contains('VANG_MAT')) return Colors.orange.shade700;
    return Colors.blueGrey;
  }
  
  // Hàm Helper: Xây dựng huy hiệu trạng thái
  Widget _buildStatusBadge(String status, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: color.withOpacity(.15),
        borderRadius: BorderRadius.circular(25),
        border: Border.all(color: color, width: 1.5),
      ),
      child: Text(
        status, // Giữ nguyên chữ in hoa/thường từ API
        style: TextStyle(color: color, fontSize: 13, fontWeight: FontWeight.w700),
      ),
    );
  }

  // Hàm Helper: Xây dựng hàng chi tiết
  Widget _buildDetailRow(IconData icon, String label, String value, {Color? valueColor}) {
    if (value.isEmpty || value == '0' || value == '---') return const SizedBox.shrink();
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 18, color: _primaryColor.withOpacity(0.7)),
          const SizedBox(width: 8),
          Text('$label: ', style: TextStyle(color: Colors.grey.shade600, fontSize: 14)),
          Expanded(
            child: Text(
              value, 
              style: TextStyle(
                color: valueColor ?? Colors.black87, 
                fontSize: 14, 
                fontWeight: FontWeight.w500
              )
            ),
          ),
        ],
      ),
    );
  }

  // Hàm Helper: Xây dựng Card cho mỗi bản ghi
  Widget _buildAttendanceCard(Map item) {
    String safe(dynamic v) => (v == null || v.toString().toLowerCase() == 'null' || v.toString().isEmpty) ? '---' : v.toString();

    final diemDanh = item['diemDanh'] is Map ? item['diemDanh'] as Map : {};
    final trangThaiDiemDanh = item['trangThaiDiemDanh'] is Map ? item['trangThaiDiemDanh'] as Map : {};
    final buoiHoc = item['buoiHoc'] is Map ? item['buoiHoc'] as Map : {};
    final lopHocPhan = item['lopHocPhan'] is Map ? item['lopHocPhan'] as Map : {};

    final tenLHP = safe(lopHocPhan['tenLopHocPhan']);
    final maLHP = safe(lopHocPhan['maLopHocPhan']);
    final tenTrangThai = safe(trangThaiDiemDanh['tenTrangThai']);
    final codeTrangThai = safe(trangThaiDiemDanh['codeTrangThai']);
    final thoiGianQuet = safe(diemDanh['thoiGianQuet']);
    final ngayHoc = safe(buoiHoc['ngayHoc']);
    final tietBD = safe(buoiHoc['tietBatDau']);
    final soTiet = safe(buoiHoc['soTiet']);
    final maDiemDanh = safe(diemDanh['maDiemDanh']);

    final statusColor = _statusColor(codeTrangThai);
    
    // Logic parse time (Tách giờ quét)
    String displayTime = '---';
    if (thoiGianQuet != '---') {
      try {
        final dt = DateTime.parse(thoiGianQuet).toLocal();
        displayTime = '${dt.hour.toString().padLeft(2, '0')}:${dt.minute.toString().padLeft(2, '0')}:${dt.second.toString().padLeft(2, '0')}';
      } catch (_) {
        displayTime = thoiGianQuet.split(' ')[1]; // Fallback to just time part
      }
    }

    return Card(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      elevation: 4,
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 1. Header (Subject Name & Status)
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Text(
                    tenLHP,
                    style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: Color(0xFF1E293B)),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                const SizedBox(width: 10),
                _buildStatusBadge(tenTrangThai, statusColor),
              ],
            ),
            const SizedBox(height: 12),
            const Divider(color: Color(0xFFF1F5F9), height: 1),
            const SizedBox(height: 12),
            
            // 2. Details (Date, Time, Tiết học)
            _buildDetailRow(Icons.calendar_month, 'Ngày học', ngayHoc),
            _buildDetailRow(Icons.access_time_filled, 'Thời gian quét', displayTime),
            _buildDetailRow(Icons.watch_later_outlined, 'Tiết học', '$tietBD (${soTiet} tiết)'),
            
            const SizedBox(height: 8),

            // 3. Metadata (LHP Code, Mã Điểm Danh)
            _buildDetailRow(Icons.bookmark_border, 'Mã LHP', maLHP, valueColor: Colors.blueGrey),
            _buildDetailRow(Icons.qr_code_2, 'Mã Điểm danh', maDiemDanh, valueColor: Colors.blueGrey),
          ],
        ),
      ),
    );
  }

  // Hàm Helper: Xây dựng trạng thái rỗng
  Widget _buildEmptyState() {
     return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.history, size: 72, color: Colors.grey.shade300),
          const SizedBox(height: 12),
          Text('Chưa có bản ghi điểm danh', style: TextStyle(fontSize: 18, color: Colors.grey.shade500, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          Text('Khi bạn điểm danh thành công, bản ghi sẽ xuất hiện ở đây.', textAlign: TextAlign.center, style: TextStyle(color: Colors.grey.shade400)),
          const SizedBox(height: 18),
          ElevatedButton.icon(
            icon: const Icon(Icons.refresh), 
            label: const Text('Làm mới'), 
            onPressed: _load,
            style: ElevatedButton.styleFrom(backgroundColor: _primaryColor, foregroundColor: Colors.white),
          ),
        ],
      ),
    );
  }
  
  // Hàm Helper: Xây dựng trạng thái lỗi
  Widget _buildErrorState(String message) {
     return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 72, color: Colors.redAccent),
          const SizedBox(height: 16),
          Text(
            message,
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: Colors.black87),
          ),
          const SizedBox(height: 20),
          ElevatedButton.icon(
            onPressed: _load,
            icon: const Icon(Icons.refresh),
            label: const Text('Thử lại'),
            style: ElevatedButton.styleFrom(backgroundColor: _primaryColor, foregroundColor: Colors.white),
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
        title: const Text(
          'Lịch Sử Điểm Danh', 
          style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)
        ),
        backgroundColor: _primaryColor,
        elevation: 0,
        centerTitle: true,
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        color: _primaryColor,
        child: FutureBuilder<Map<String, dynamic>>(
          future: _futureRecords,
          builder: (context, snap) {
            // 1. Loading State
            if (snap.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator(color: _primaryColor));
            }
            
            // 2. Error/API Failure State
            if (snap.hasError) return _buildErrorState('Lỗi kết nối: ${snap.error}');

            final res = snap.data ?? {};
            final statusStr = (res['status'] ?? res['Status'])?.toString();
            final isOk = statusStr == '200' || res['success'] == true;
            
            if (!isOk) {
              return _buildErrorState((res['message'] ?? res['Message'] ?? 'Không thể tải lịch sử').toString());
            }

            final data = res['data'] is Map ? res['data'] as Map : {};
            final List items = data['items'] is List ? data['items'] as List : [];
            final totalRecords = data['totalRecords']?.toString() ?? '0';
            final totalPages = data['totalPages']?.toString() ?? '0';

            // 3. Empty State
            if (items.isEmpty) {
              return _buildEmptyState();
            }

            // 4. Success State (List View)
            return Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 12, 16, 6),
                  child: Row(
                    children: [
                      Icon(Icons.list_alt, color: _primaryColor),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'Tổng: $totalRecords ${totalPages != '0' ? '• Trang 1 / $totalPages' : ''}',
                          style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.black87),
                        ),
                      ),
                      // Không cần hiển thị pageSize nếu API luôn trả về 50
                      // const Text('Hiển thị 50', style: TextStyle(color: Colors.grey)),
                    ],
                  ),
                ),
                Expanded(
                  child: ListView.builder(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
                    itemCount: items.length,
                    itemBuilder: (context, i) {
                      final it = items[i];
                      if (it is! Map) return const SizedBox.shrink();
                      return _buildAttendanceCard(it.cast<String, dynamic>());
                    },
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}