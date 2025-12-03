import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'services/api_service.dart';

class LoginPage extends StatefulWidget {
	const LoginPage({super.key});

	@override
	State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
	// Đổi tên biến điều khiển từ email sang MSSV
	final TextEditingController _mssvCtl = TextEditingController();
	final TextEditingController _passCtl = TextEditingController();

	// Biến để quản lý trạng thái hiển thị mật khẩu
	bool _isPasswordVisible = false; 
  
	// Biến để quản lý trạng thái loading
	bool _isLoading = false;
  
	// Key để quản lý Form và validation
	final _formKey = GlobalKey<FormState>();

	@override
	void dispose() {
		_mssvCtl.dispose();
		_passCtl.dispose();
		super.dispose();
	}

	Future<void> _tryLogin() async {
		if (_formKey.currentState!.validate()) {
			setState(() {
				_isLoading = true;
			});

			final tenDangNhap = _mssvCtl.text.trim();
			final matKhau = _passCtl.text.trim();

			// Tránh lỗi use_build_context_synchronously: giữ tham chiếu trước khi await
			final messenger = ScaffoldMessenger.of(context);
			final navigator = Navigator.of(context);

			// Gọi API đăng nhập
			final result = await ApiService.login(tenDangNhap, matKhau);

			if (!mounted) return;
			setState(() {
				_isLoading = false;
			});

						if (result['success']) {
				// Đăng nhập thành công
				final data = result['data'];
        
				// Lưu token vào SharedPreferences (ưu tiên token top-level trả về từ ApiService)
				final prefs = await SharedPreferences.getInstance();
				final topAccess = result['accessToken'];
				final topRefresh = result['refreshToken'];
				final dataAccess = data['accessToken'] ?? data['access_token'] ?? data['token'] ?? data['jwt'] ?? (data['data']?['accessToken']);
				final dataRefresh = data['refreshToken'] ?? data['refresh_token'] ?? (data['data']?['refreshToken']);

				final accessToSave = topAccess ?? dataAccess;
				final refreshToSave = topRefresh ?? dataRefresh;

								if (accessToSave != null) {
					await prefs.setString('accessToken', accessToSave);
				} else {
										// Không có accessToken trong phản hồi
				}

								if (refreshToSave != null) {
					await prefs.setString('refreshToken', refreshToSave);
				} else {
										// Không có refreshToken trong phản hồi
				}

								// Lưu username để refresh token có thể gửi TenDangNhap theo yêu cầu API
								await prefs.setString('username', tenDangNhap);
        
				// Kiểm tra loại tài khoản
				// Kiểm tra loại tài khoản dựa trên phản hồi server (phanQuyen)
				String? roleCode;
				try {
					// server có thể trả phanQuyen ở nhiều vị trí: data['phanQuyen'] hoặc data['data']['phanQuyen']
					final p1 = data['phanQuyen'];
					final p2 = data['data'] is Map ? data['data']['phanQuyen'] : null;
					final pn = p1 ?? p2;
					if (pn is Map) {
						roleCode = pn['codeQuyen']?.toString();
					}
				} catch (_) {}

				if (roleCode != null) {
					final rc = roleCode.toLowerCase();
					if (rc.contains('sv') || rc.contains('sinh')) {
						messenger.showSnackBar(
							const SnackBar(content: Text('Đăng nhập thành công!'), backgroundColor: Colors.green),
						);
						navigator.pushReplacementNamed('/home');
					} else if (rc.contains('gv') || rc.contains('giang')) {
						messenger.showSnackBar(
							const SnackBar(content: Text('Đăng nhập giảng viên thành công!'), backgroundColor: Colors.green),
						);
						navigator.pushReplacementNamed('/home');
					} else if (rc.contains('qt') || rc.contains('quantri') || rc.contains('admin')) {
						messenger.showSnackBar(
							const SnackBar(content: Text('Chức năng dành cho quản trị viên đang được phát triển!'), backgroundColor: Colors.orange, duration: Duration(seconds: 3)),
						);
					} else {
						messenger.showSnackBar(const SnackBar(content: Text('Đăng nhập thành công!'), backgroundColor: Colors.green));
						navigator.pushReplacementNamed('/home');
					}
				} else {
					// Nếu server không trả phanQuyen, fallback về kiểm tra theo tiền tố username cũ
					if (tenDangNhap.startsWith('giangvien')) {
						messenger.showSnackBar(const SnackBar(content: Text('Đăng nhập giảng viên thành công!'), backgroundColor: Colors.green));
						navigator.pushReplacementNamed('/home');
					} else if (tenDangNhap.startsWith('sinhvien')) {
						messenger.showSnackBar(const SnackBar(content: Text('Đăng nhập thành công!'), backgroundColor: Colors.green));
						navigator.pushReplacementNamed('/home');
					} else if (tenDangNhap.startsWith('quantrivien')) {
						messenger.showSnackBar(const SnackBar(content: Text('Chức năng dành cho quản trị viên đang được phát triển!'), backgroundColor: Colors.orange, duration: Duration(seconds: 3)));
					} else {
						// Loại tài khoản không xác định
						messenger.showSnackBar(const SnackBar(content: Text('Loại tài khoản không hợp lệ!'), backgroundColor: Colors.red, duration: Duration(seconds: 3)));
					}
				}
			} else {
				// Đăng nhập thất bại - hiển thị thông báo từ server nếu có, kèm status/raw
				final msg = (result['message'] ?? 'Tài khoản hoặc mật khẩu không đúng!').toString();
				final hasStatusInMsg = msg.contains('(mã ') || msg.contains('code ');
				final status = !hasStatusInMsg && result['status'] != null ? ' (mã ${result['status']})' : '';
				final raw = (result['raw'] ?? '').toString();
				final endpoint = (result['endpoint'] ?? '').toString();
				String text = raw.isNotEmpty ? '$msg$status\n$raw' : '$msg$status';
				if ((result['status'] == 404 || (raw.contains('404'))) && endpoint.isNotEmpty) {
					text = '$text\nEndpoint: $endpoint';
				}
				messenger.showSnackBar(
					SnackBar(
						content: Text(text),
						backgroundColor: Colors.red,
						duration: const Duration(seconds: 4),
					),
				);
			}
		}

	}
	// Widget riêng cho Button Gradient
	Widget _buildLoginButton() {
		return Container(
			height: 50,
			decoration: BoxDecoration(
				borderRadius: BorderRadius.circular(12),
				gradient: const LinearGradient(
					colors: [Color.fromARGB(255, 68, 137, 255), Color.fromARGB(255, 27, 107, 255)], // Gradient màu xanh
					begin: Alignment.centerLeft,
					end: Alignment.centerRight,
				),
				boxShadow: [
					BoxShadow(
						color: Colors.blue.withValues(alpha: 0.5),
						spreadRadius: 2,
						blurRadius: 5,
						offset: const Offset(0, 3), 
					),
				],
			),
			child: Material(
				color: Colors.transparent,
				child: InkWell(
					onTap: _isLoading ? null : _tryLogin,
					borderRadius: BorderRadius.circular(12),
					child: Center(
						child: _isLoading
								? const SizedBox(
										height: 24,
										width: 24,
										child: CircularProgressIndicator(
											strokeWidth: 3,
											valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
										),
									)
								: const Text(
										"Đăng nhập",
										style: TextStyle(
											fontSize: 18, 
											fontWeight: FontWeight.bold, 
											color: Colors.white,
										),
									),
					),
				),
			),
		);
	}

	@override
	Widget build(BuildContext context) {
		return Scaffold(
			backgroundColor: Colors.white, // Nền trắng theo hình
			body: SafeArea(
				child: Center(
					child: SingleChildScrollView(
						padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 40),
						child: Form(
							key: _formKey,
							child: Column(
								crossAxisAlignment: CrossAxisAlignment.stretch,
								children: [
									// 1. Logo Trường
									Center(
										child: Image.asset(
											'asset/logo_huit.jpg',
											height: 100,
											errorBuilder: (context, error, stackTrace) =>
												const Icon(Icons.school, size: 100, color: Colors.blue), // Fallback Icon
										),
									),
                  
									const SizedBox(height: 30),

									// 2. Tiêu đề chính
									const Text(
										"HỆ THỐNG QUẢN LÝ ĐIỂM DANH QR",
										textAlign: TextAlign.center,
										style: TextStyle(
											fontSize: 22,
											fontWeight: FontWeight.bold,
											color: Color.fromARGB(255, 33, 107, 183), // Màu xanh đậm
										),
									),
									const SizedBox(height: 8),

									// 3. Tiêu đề phụ
									const Text(
										"Nhanh chóng - Chính xác - Hiện đại",
										textAlign: TextAlign.center,
										style: TextStyle(
											fontSize: 16,
											color: Colors.red, // Màu đỏ
										),
									),
									const SizedBox(height: 30),
                  
									// 4. Tiêu đề Đăng nhập
									const Text(
										"Đăng nhập tài khoản",
										textAlign: TextAlign.center,
										style: TextStyle(
											fontSize: 18,
											fontWeight: FontWeight.w600,
											color: Colors.blue, 
										),
									),
									const SizedBox(height: 24),

									// 5. Nhập Tên đăng nhập
									TextFormField(
										controller: _mssvCtl,
										keyboardType: TextInputType.text,
										validator: (value) {
											if (value == null || value.isEmpty) {
												return 'Vui lòng nhập tên đăng nhập';
											}
											return null;
										},
										decoration: InputDecoration(
											labelText: "Tên đăng nhập",
											hintText: "",
											filled: true,
											fillColor: Colors.white,
											border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.grey)),
											focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.blue, width: 2)),
											contentPadding: const EdgeInsets.symmetric(vertical: 16, horizontal: 16)
										),
									),
									const SizedBox(height: 16),

									// 6. Nhập Mật khẩu
									TextFormField(
										controller: _passCtl,
										obscureText: !_isPasswordVisible,
										validator: (value) {
											if (value == null || value.isEmpty) {
												return 'Vui lòng nhập Mật khẩu';
											}
											return null;
										},
										decoration: InputDecoration(
											labelText: "Nhập mật khẩu",
											filled: true,
											fillColor: Colors.white,
											border: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.grey)),
											focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12), borderSide: const BorderSide(color: Colors.blue, width: 2)),
											contentPadding: const EdgeInsets.symmetric(vertical: 16, horizontal: 16),
											// Icon toggle ẩn/hiện mật khẩu
											suffixIcon: IconButton(
												icon: Icon(
													_isPasswordVisible ? Icons.visibility : Icons.visibility_off,
													color: Colors.grey,
												),
												onPressed: () {
													setState(() {
														_isPasswordVisible = !_isPasswordVisible;
													});
												},
											),
										),
									),
                  
									const SizedBox(height: 40),

									// 7. Button Đăng nhập (với Gradient)
									_buildLoginButton(),
                  
									const SizedBox(height: 20),

									// (Đã bỏ link đăng ký theo yêu cầu)
								],
							),
						),
					),
				),
			),
		);
	}
}