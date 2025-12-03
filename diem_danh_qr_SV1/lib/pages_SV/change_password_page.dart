import 'package:flutter/material.dart';
import '../services/api_service.dart';
import 'package:shared_preferences/shared_preferences.dart';

class ChangePasswordPage extends StatefulWidget {
  const ChangePasswordPage({super.key});

  @override
  State<ChangePasswordPage> createState() => _ChangePasswordPageState();
}

class _ChangePasswordPageState extends State<ChangePasswordPage> {
  // Màu sắc và Styles
  static const Color _primaryColor = Color(0xFF3B82F6); // Blue
  static const Color _backgroundColor = Color(0xFFF1F5F9); // Light Gray/Off-white

  final _formKey = GlobalKey<FormState>(); 
  final _oldPasswordController = TextEditingController();
  final _newPasswordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  bool _isOldPasswordVisible = false;
  bool _isNewPasswordVisible = false;
  bool _isConfirmPasswordVisible = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _logToken();
  }

  Future<void> _logToken() async {
    final prefs = await SharedPreferences.getInstance();
    final accessToken = prefs.getString('accessToken');
    if (accessToken == null && mounted) {
      // Chỉ hiển thị Snackbar nếu chưa ở trạng thái đang tải
      if (!_isLoading) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Không tìm thấy token, bạn cần đăng nhập lại!'), backgroundColor: Colors.red),
        );
      }
    }
  }

  @override
  void dispose() {
    _oldPasswordController.dispose();
    _newPasswordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _changePassword() async {
    if (_formKey.currentState!.validate()) {
      setState(() {
        _isLoading = true;
      });
      final oldPass = _oldPasswordController.text;
      final newPass = _newPasswordController.text;
      
      // Hiển thị dialog loading (thay thế cho việc dùng showDialog)
      final navigator = Navigator.of(context);
      final messenger = ScaffoldMessenger.of(context);

      final result = await ApiService.changePassword(oldPass, newPass);

      if (!mounted) return;
      
      setState(() {
        _isLoading = false;
      });

      // Show SnackBar
      if (result['success']) {
        messenger.showSnackBar(
          SnackBar(content: Text(result['message'] ?? 'Đổi mật khẩu thành công!'), backgroundColor: Colors.green),
        );
        // Tùy chọn: Xóa nội dung các trường sau khi thành công
        _oldPasswordController.clear();
        _newPasswordController.clear();
        _confirmPasswordController.clear();
      } else {
        final msg = (result['message'] ?? '').toLowerCase();
        String displayMessage = result['message'] ?? 'Đổi mật khẩu thất bại!';
        
        if (msg.contains('mật khẩu cũ') || msg.contains('matkhaucu') || msg.contains('sai')) {
          displayMessage = 'Sai mật khẩu cũ!';
        }
        
        messenger.showSnackBar(
          SnackBar(content: Text(displayMessage), backgroundColor: Colors.red),
        );
      }
    }
  }

  // Widget xây dựng TextField cho mật khẩu
  Widget _buildPasswordField({
    required TextEditingController controller,
    required String label,
    required bool isVisible,
    required VoidCallback toggleVisibility,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: !isVisible, 
      keyboardType: TextInputType.visiblePassword,
      decoration: InputDecoration(
        labelText: label,
        labelStyle: TextStyle(color: Colors.grey.shade700),
        filled: true,
        fillColor: Colors.white,
        border: OutlineInputBorder(
          borderRadius: const BorderRadius.all(Radius.circular(15)),
          borderSide: BorderSide.none,
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: const BorderRadius.all(Radius.circular(15)),
          borderSide: BorderSide(color: Colors.grey.shade300, width: 1),
        ),
        focusedBorder: const OutlineInputBorder(
          borderRadius: BorderRadius.all(Radius.circular(15)),
          borderSide: BorderSide(color: _primaryColor, width: 2),
        ),
        prefixIcon: const Icon(Icons.lock_outline, color: _primaryColor),
        suffixIcon: IconButton(
          icon: Icon(isVisible ? Icons.visibility : Icons.visibility_off, color: Colors.grey),
          onPressed: toggleVisibility,
        ),
        contentPadding: const EdgeInsets.symmetric(vertical: 18, horizontal: 15),
      ),
      validator: validator,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _backgroundColor,
      appBar: AppBar(
        title: const Text(
          'Đổi Mật Khẩu',
          style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)
        ),
        backgroundColor: _primaryColor,
        elevation: 0,
        centerTitle: true,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            // Icon lớn và thông báo
            const Icon(
              Icons.security_update_good_rounded, 
              size: 80, 
              color: _primaryColor,
            ),
            const SizedBox(height: 16),
            const Text(
              'Thiết lập mật khẩu mới',
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.black87),
            ),
            const SizedBox(height: 8),
            Text(
              'Mật khẩu mới phải khác mật khẩu cũ và có ít nhất 6 ký tự.',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 14, color: Colors.grey.shade600),
            ),
            const SizedBox(height: 32),
            
            // Form
            Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  // 1. Mật khẩu cũ
                  _buildPasswordField(
                    controller: _oldPasswordController,
                    label: 'Mật khẩu cũ',
                    isVisible: _isOldPasswordVisible,
                    toggleVisibility: () {
                      setState(() {
                        _isOldPasswordVisible = !_isOldPasswordVisible;
                      });
                    },
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng nhập mật khẩu cũ';
                      }
                      return null;
                    },
                  ),

                  const SizedBox(height: 16.0),

                  // 2. Mật khẩu mới
                  _buildPasswordField(
                    controller: _newPasswordController,
                    label: 'Mật khẩu mới',
                    isVisible: _isNewPasswordVisible,
                    toggleVisibility: () {
                      setState(() {
                        _isNewPasswordVisible = !_isNewPasswordVisible;
                      });
                    },
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng nhập mật khẩu mới';
                      }
                      if (value.length < 6) {
                        return 'Mật khẩu phải có ít nhất 6 ký tự';
                      }
                       if (value == _oldPasswordController.text) {
                        return 'Mật khẩu mới phải khác mật khẩu cũ';
                      }
                      return null;
                    },
                  ),

                  const SizedBox(height: 16.0),

                  // 3. Xác nhận mật khẩu mới
                  _buildPasswordField(
                    controller: _confirmPasswordController,
                    label: 'Xác nhận mật khẩu mới',
                    isVisible: _isConfirmPasswordVisible,
                    toggleVisibility: () {
                      setState(() {
                        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
                      });
                    },
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Vui lòng xác nhận mật khẩu';
                      }
                      if (value != _newPasswordController.text) {
                        return 'Mật khẩu xác nhận không khớp';
                      }
                      return null;
                    },
                  ),

                  const SizedBox(height: 32.0),

                  // Nút Đổi mật khẩu
                  ElevatedButton(
                    onPressed: _isLoading ? null : _changePassword,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: _primaryColor,
                      foregroundColor: Colors.white,
                      minimumSize: const Size(double.infinity, 56),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                      elevation: 5,
                    ),
                    child: _isLoading
                        ? const SizedBox(
                            width: 24,
                            height: 24,
                            child: CircularProgressIndicator(
                              color: Colors.white,
                              strokeWidth: 3,
                            ),
                          )
                        : const Text(
                            'Đổi Mật Khẩu', 
                            style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)
                          ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}