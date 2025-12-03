import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'student_info_page.dart'; 
import 'change_password_page.dart'; 

class PersonalPage extends StatefulWidget {
  const PersonalPage({super.key});

  @override
  State<PersonalPage> createState() => _PersonalPageState();
}

class _PersonalPageState extends State<PersonalPage> {
  late Future<Map<String, dynamic>> _futureInfo;

  // Màu sắc và Styles chung cho giao diện mới
  static const Color _primaryColor = Color(0xFF3B82F6); // Blue
  static const Color _lightBlue = Color(0xFF60A5FA); // Light Blue
  static const Color _dangerColor = Color(0xFFEF4444); // Red
  static const Color _backgroundColor = Color(0xFFF1F5F9); // Light Gray/Off-white

  @override
  void initState() {
    super.initState();
    _futureInfo = ApiService.fetchUserInfo();
  }
  
  // Hàm xây dựng menu item được tái sử dụng
  Widget _buildMenuItem({
    required IconData icon,
    required String title,
    required Color color,
    VoidCallback? onTap,
    Widget? trailing,
  }) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(15),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 8.0, horizontal: 8.0),
          child: ListTile(
            leading: Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: color.withOpacity(0.15),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Icon(icon, color: color, size: 24),
            ),
            title: Text(
              title,
              style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.black87),
            ),
            trailing: trailing ?? const Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey),
          ),
        ),
      ),
    );
  }

  // Hàm xây dựng SwitchListTile cho mục Thông báo
  Widget _buildSwitchTile({
    required IconData icon,
    required String title,
    required bool value,
    required ValueChanged<bool> onChanged,
    required Color color,
  }) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 4.0, horizontal: 8.0),
        child: SwitchListTile(
          value: value,
          onChanged: onChanged,
          activeColor: color,
          secondary: Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: color.withOpacity(0.15),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, color: color, size: 24),
          ),
          title: Text(
            title,
            style: const TextStyle(fontWeight: FontWeight.w600, color: Colors.black87),
          ),
        ),
      ),
    );
  }

  void _showPolicyDialog() async {
    bool agreed = false;
    await showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (c) {
        return StatefulBuilder(
          builder: (c2, setState2) {
            return Padding(
              padding: EdgeInsets.only(bottom: MediaQuery.of(c2).viewInsets.bottom),
              child: DraggableScrollableSheet(
                expand: false,
                initialChildSize: 0.8,
                minChildSize: 0.4,
                maxChildSize: 0.95,
                builder: (_, controller) {
                  return Column(
                    children: [
                      const SizedBox(height: 12),
                      Container(width: 40, height: 4, decoration: BoxDecoration(color: Colors.grey[300], borderRadius: BorderRadius.circular(2))),
                      const SizedBox(height: 12),
                      Expanded(
                        child: SingleChildScrollView(
                          controller: controller,
                          padding: const EdgeInsets.symmetric(horizontal: 16),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              const Text('Điều khoản & Chính sách', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                              const SizedBox(height: 12),
                              const Text('1. Mục đích sử dụng', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Ứng dụng dùng để điểm danh bằng mã QR, xem lịch học và lịch sử điểm danh.'),
                              const SizedBox(height: 12),
                              const Text('2. Tài khoản', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Sinh viên phải dùng tài khoản được cấp.'),
                              const Text('Giữ bí mật mật khẩu; mọi hoạt động từ tài khoản là do sinh viên chịu trách nhiệm.'),
                              const SizedBox(height: 12),
                              const Text('3. Quy định điểm danh', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Chỉ điểm danh khi có mặt tại lớp và trong thời gian cho phép.'),
                              const Text('Không chụp, chia sẻ hoặc sử dụng mã QR của người khác.'),
                              const Text('Nghiêm cấm chỉnh sửa, làm giả mã QR.'),
                              const SizedBox(height: 12),
                              const Text('4. Thu thập & bảo mật dữ liệu', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Hệ thống lưu thông tin cá nhân, lịch học, lịch sử điểm danh.'),
                              const Text('Dữ liệu được bảo mật, không chia sẻ cho bên thứ ba trừ yêu cầu của nhà trường hoặc pháp luật.'),
                              const SizedBox(height: 12),
                              const Text('5. Quyền lợi của sinh viên', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Xem, cập nhật thông tin cá nhân.'),
                              const Text('Yêu cầu hỗ trợ khi gặp lỗi ứng dụng.'),
                              const SizedBox(height: 12),
                              const Text('6. Thay đổi điều khoản', style: TextStyle(fontWeight: FontWeight.w600)),
                              const SizedBox(height: 6),
                              const Text('Ứng dụng có thể cập nhật điều khoản, sẽ thông báo khi có thay đổi.'),
                              const SizedBox(height: 18),
                            ],
                          ),
                        ),
                      ),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
                        child: Column(
                          children: [
                            CheckboxListTile(
                              value: agreed,
                              onChanged: (v) => setState2(() => agreed = v ?? false),
                              title: const Text('Tôi đồng ý điều khoản và chính sách'),
                              controlAffinity: ListTileControlAffinity.leading,
                            ),
                            Row(
                              children: [
                                Expanded(
                                  child: OutlinedButton(
                                    onPressed: () => Navigator.pop(c2),
                                    child: const Text('Hủy'),
                                  ),
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: ElevatedButton(
                                    onPressed: agreed ? () {
                                      Navigator.pop(c2);
                                    } : null,
                                    child: const Text('Đồng ý'),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ],
                  );
                },
              ),
            );
          },
        );
      },
    );
  }


  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _backgroundColor,
      body: FutureBuilder<Map<String, dynamic>>(
        future: _futureInfo,
        builder: (context, snap) {
          Widget header;
          
          // --- Logic Header ---
          
          String fullName = 'Sinh viên';
          String mssv = '---';
          String? avatarUrl;
          bool hasError = snap.hasError || snap.data == null || snap.data!['success'] != true;

          if (snap.connectionState == ConnectionState.done && !hasError) {
            final res = snap.data!['data'] ?? {};
            final Map<String, dynamic> nguoiDung = res['nguoiDung'] is Map ? Map<String, dynamic>.from(res['nguoiDung']) : {};
            final Map<String, dynamic> sinhVien = res['sinhVien'] is Map ? Map<String, dynamic>.from(res['sinhVien']) : {};

            fullName = sinhVien['tenSinhVien'] ?? sinhVien['TenSinhVien'] ?? nguoiDung['hoTen'] ?? nguoiDung['HoTen'] ?? fullName;
            mssv = sinhVien['maSinhVien'] ?? sinhVien['MaSinhVien'] ?? nguoiDung['tenDangNhap'] ?? mssv;
            final rawAvatar = nguoiDung['anhDaiDien'] ?? nguoiDung['anhdaidien'] ?? nguoiDung['avatar'];
            
            if (rawAvatar is String && rawAvatar.isNotEmpty && rawAvatar.toLowerCase() != 'null') {
              var a = rawAvatar;
              if (!a.toLowerCase().startsWith('http')) {
                final base = ApiService.baseUrl.replaceAll(RegExp(r'/$'), '');
                a = base + (a.startsWith('/') ? a : '/$a');
              }
              avatarUrl = a;
            }
          }
          
          // Xây dựng Header
          header = Container(
            width: double.infinity,
            padding: const EdgeInsets.only(top: 60, bottom: 32),
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [_lightBlue, _primaryColor],
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
              ),
              // Tăng độ cong của góc dưới
              borderRadius: BorderRadius.only(bottomLeft: Radius.circular(30), bottomRight: Radius.circular(30)),
              boxShadow: [BoxShadow(color: Colors.black26, blurRadius: 10, offset: Offset(0, 4))],
            ),
            child: Column(
              children: [
                Stack(
                  children: [
                    // Avatar (hoặc Placeholder)
                    ClipOval(
                      child: (avatarUrl != null && !hasError)
                          ? Image.network(
                              avatarUrl.toString(), 
                              width: 100, height: 100, 
                              fit: BoxFit.cover, 
                              errorBuilder: (c, e, s) => Container(width: 100, height: 100, color: Colors.grey.shade300, child: const Icon(Icons.person, size: 56, color: Colors.white)),
                            )
                          : Container(width: 100, height: 100, decoration: BoxDecoration(color: Colors.grey.shade300, shape: BoxShape.circle), child: const Icon(Icons.person, size: 56, color: Colors.white)),
                    ),
                    // Loading indicator
                    if (snap.connectionState == ConnectionState.waiting)
                      Positioned.fill(
                        child: Container(
                          decoration: BoxDecoration(color: Colors.black.withOpacity(0.3), shape: BoxShape.circle),
                          child: const Center(child: SizedBox(width: 30, height: 30, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 3))),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 16),
                Text(
                  fullName, 
                  style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.bold),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 4),
                Text(
                  'MSSV: $mssv', 
                  style: const TextStyle(color: Colors.white70, fontSize: 14),
                ),
              ],
            ),
          );

          // --- Phần nội dung chính (Menu & Footer) ---
          return Column(
            children: [
              header,
              const SizedBox(height: 18),
              Expanded(
                child: ListView(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  children: [
                    // Nhóm 1: Thông tin & Bảo mật
                    _buildMenuItem(
                      icon: Icons.badge,
                      title: 'Thông tin sinh viên',
                      color: _primaryColor,
                      onTap: () {
                        Navigator.push(context, MaterialPageRoute(builder: (context) => const StudentInfoPage()));
                      },
                    ),
                    _buildMenuItem(
                      icon: Icons.lock_open_rounded,
                      title: 'Đổi mật khẩu',
                      color: _primaryColor,
                      onTap: () {
                        Navigator.push(context, MaterialPageRoute(builder: (context) => const ChangePasswordPage()));
                      },
                    ),
                    const SizedBox(height: 16),

                    // Nhóm 2: Chính sách & Góp ý
                    _buildMenuItem(
                      icon: Icons.policy,
                      title: 'Điều khoản & chính sách',
                      color: Colors.orange,
                      onTap: () { _showPolicyDialog(); },
                    ),
                    _buildMenuItem(
                      icon: Icons.feedback,
                      title: 'Góp ý ứng dụng',
                      color: Colors.teal,
                      onTap: () { /* Add feedback navigation */ },
                    ),
                    const SizedBox(height: 16),

                    // Nhóm 3: Cài đặt (Thông báo)
                    _buildSwitchTile(
                      icon: Icons.notifications_active,
                      title: 'Thông báo',
                      color: Colors.pinkAccent,
                      value: true, // Giữ nguyên giá trị mặc định true
                      onChanged: (v) { /* Add notification toggle logic */ },
                    ),

                    const SizedBox(height: 24),
                    
                    // Nút Đăng xuất
                    ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _dangerColor,
                        foregroundColor: Colors.white,
                        minimumSize: const Size(double.infinity, 52),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                        elevation: 5,
                      ),
                      onPressed: () {
                        // Giữ nguyên logic cũ
                        Navigator.pushReplacementNamed(context, '/login');
                      },
                      child: const Text('Đăng xuất', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    ),
                    
                    const SizedBox(height: 24),
                    
                    // Thông tin phiên bản
                    const Center(child: Text('Phiên bản 1.0.0', style: TextStyle(color: Colors.grey, fontSize: 12))),
                    const SizedBox(height: 18),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}