import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

class QRScannerPage extends StatefulWidget {
  const QRScannerPage({super.key});

  @override
  State<QRScannerPage> createState() => _QRScannerPageState();
}

class _QRScannerPageState extends State<QRScannerPage> {
  // Màu sắc và Styles
  static const Color _primaryColor = Color(0xFF3B82F6); // Blue

  final MobileScannerController _controller = MobileScannerController(
    detectionSpeed: DetectionSpeed.noDuplicates,
    facing: CameraFacing.back,
    torchEnabled: false,
  );

  bool _isHandling = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _handleDetect(BarcodeCapture capture) {
    if (_isHandling) return;
    final barcodes = capture.barcodes;
    if (barcodes.isEmpty) return;

    final String? code = barcodes.first.rawValue;
    if (code == null || code.isEmpty) return;

    setState(() => _isHandling = true);

    // In kết quả quét (Giữ nguyên logic gốc)
    print("====================================");
    print("ĐÃ QUÉT ĐƯỢC: $code");
    print("====================================");

    // Dùng Future.delayed để đảm bảo Navigation không bị gọi quá nhanh
    // và cho phép MobileScanner ổn định trước khi pop.
    Future.delayed(const Duration(milliseconds: 300), () {
      if (mounted) {
        Navigator.of(context).pop(code); // Trả token về màn hình gọi
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    // Kích thước của khung quét
    final scanArea = (MediaQuery.of(context).size.width < 400 || MediaQuery.of(context).size.height < 400) ? 250.0 : 300.0;

    return Scaffold(
      backgroundColor: Colors.black, // Nền đen cho trải nghiệm quét tốt hơn
      appBar: AppBar(
        title: const Text('Quét Mã QR', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        backgroundColor: Colors.transparent, // AppBar trong suốt
        elevation: 0,
        iconTheme: const IconThemeData(color: Colors.white), // Icon màu trắng
        actions: [
          // Nút Đổi Camera
          IconButton(
            tooltip: 'Đổi camera',
            icon: ValueListenableBuilder<MobileScannerState>(
              valueListenable: _controller,
              builder: (context, state, _) {
                final facing = state.cameraDirection;
                if (facing == CameraFacing.front) {
                  return const Icon(Icons.camera_front, color: Colors.white);
                }
                return const Icon(Icons.camera_rear, color: Colors.white);
              },
            ),
            onPressed: () => _controller.switchCamera(),
          ),
          // Nút Bật/Tắt Flash
          IconButton(
            tooltip: 'Bật/Tắt đèn flash',
            icon: ValueListenableBuilder<MobileScannerState>(
              valueListenable: _controller,
              builder: (context, state, _) {
                final torch = state.torchState;
                if (torch == TorchState.on) {
                  return const Icon(Icons.flash_on, color: Colors.yellowAccent);
                }
                return const Icon(Icons.flash_off, color: Colors.white);
              },
            ),
            onPressed: () => _controller.toggleTorch(),
          ),
        ],
      ),
      extendBodyBehindAppBar: true, // Mở rộng body ra sau AppBar
      body: Stack(
        fit: StackFit.expand,
        children: [
          // Camera View
          MobileScanner(
            controller: _controller,
            onDetect: _handleDetect,
          ),

          // Vùng Overlay (Tạo khung quét)
          Center(
            child: Container(
              width: scanArea,
              height: scanArea,
              decoration: BoxDecoration(
                border: Border.all(color: Colors.white38, width: 2),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Stack(
                children: [
                  // Bốn góc nổi bật
                  _buildCorner(Alignment.topLeft),
                  _buildCorner(Alignment.topRight),
                  _buildCorner(Alignment.bottomLeft),
                  _buildCorner(Alignment.bottomRight),
                ],
              ),
            ),
          ),
          
          // Văn bản hướng dẫn ở phía trên
          Positioned(
            top: MediaQuery.of(context).size.height / 2 - scanArea / 2 - 60,
            left: 0,
            right: 0,
            child: const Padding(
              padding: EdgeInsets.symmetric(horizontal: 40.0),
              child: Text(
                'Đặt mã QR của buổi học vào khung để điểm danh',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.white, fontSize: 16, shadows: [Shadow(color: Colors.black54, blurRadius: 4)]),
              ),
            ),
          ),

          // Văn bản thông báo ở phía dưới
          Align(
            alignment: Alignment.bottomCenter,
            child: Container(
              margin: const EdgeInsets.only(bottom: 50, left: 24, right: 24),
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
              decoration: BoxDecoration(
                color: _primaryColor,
                borderRadius: BorderRadius.circular(25),
                boxShadow: const [BoxShadow(color: Colors.black38, blurRadius: 10)]
              ),
              child: const Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.info_outline, color: Colors.white, size: 20),
                  SizedBox(width: 8),
                  Text(
                    'Đang chờ quét...',
                    style: TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
  
  // Widget Helper: Xây dựng các góc của khung quét
  Widget _buildCorner(Alignment alignment) {
    const double cornerSize = 30;
    const double cornerThickness = 4;
    return Align(
      alignment: alignment,
      child: Container(
        width: cornerSize,
        height: cornerSize,
        decoration: BoxDecoration(
          border: Border(
            top: alignment.y < 0 ? BorderSide(color: _primaryColor, width: cornerThickness) : BorderSide.none,
            bottom: alignment.y > 0 ? BorderSide(color: _primaryColor, width: cornerThickness) : BorderSide.none,
            left: alignment.x < 0 ? BorderSide(color: _primaryColor, width: cornerThickness) : BorderSide.none,
            right: alignment.x > 0 ? BorderSide(color: _primaryColor, width: cornerThickness) : BorderSide.none,
          ),
        ),
      ),
    );
  }
}