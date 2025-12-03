import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../services/api_service.dart';

class ScheduleListPage extends StatefulWidget {
  const ScheduleListPage({super.key});

  @override
  State<ScheduleListPage> createState() => _ScheduleListPageState();
}

class _ScheduleListPageState extends State<ScheduleListPage> {
  final ScrollController _controller = ScrollController();
  final List<Map<String, dynamic>> _items = [];
  
  // State variables
  int _page = 1;
  int _totalPages = 1;
  bool _isLoading = false;
  String _view = 'day'; // day, week, month
  DateTime _anchor = DateTime.now();
  
  // Week selection logic
  int _selectedYear = DateTime.now().year;
  int _selectedWeek = 1;
  static const int _weekOffset = -1; 
  List<_WeekInfo> _weeksOfSelectedYear = [];

  // Theme Colors
  final Color _primaryColor = const Color(0xFF3B82F6); // Blue 500
  final Color _backgroundColor = const Color(0xFFF1F5F9); // Slate 100

  @override
  void initState() {
    super.initState();
    _initTimeData();
    _fetch();
    _controller.addListener(_scrollListener);
  }

  void _initTimeData() {
    _selectedYear = DateTime.now().year;
    final currentWeekIso = _isoWeekNumber(DateTime.now());
    _selectedWeek = currentWeekIso + _weekOffset;
    _weeksOfSelectedYear = _generateWeeks(_selectedYear);
    // Nếu view là week thì anchor phải khớp tuần
    _anchor = _findWeekByNumber(_selectedWeek)?.start ?? DateTime.now();
  }

  void _scrollListener() {
    if (_controller.position.pixels >= _controller.position.maxScrollExtent - 200 &&
        !_isLoading &&
        _page < _totalPages) {
      _fetch(nextPage: true);
    }
  }

  // --- Logic Helper ---

  _WeekInfo? _findWeekByNumber(int w) {
    return _weeksOfSelectedYear.firstWhere(
      (e) => e.number == w,
      orElse: () => _WeekInfo(number: w, start: _anchor, end: _anchor),
    );
  }

  List<_WeekInfo> _generateWeeks(int year) {
    final firstJan = DateTime(year, 1, 1);
    DateTime firstMonday = firstJan;
    while (firstMonday.weekday != DateTime.monday) {
      firstMonday = firstMonday.add(const Duration(days: 1));
    }
    final weeks = <_WeekInfo>[];
    DateTime cursor = firstMonday;
    while (cursor.year == year) {
      final start = cursor;
      final end = cursor.add(const Duration(days: 6));
      final iso = _isoWeekNumber(start);
      final displayNum = iso + _weekOffset;
      weeks.add(_WeekInfo(number: displayNum, start: start, end: end));
      cursor = cursor.add(const Duration(days: 7));
      if (cursor.year > year) break;
    }
    return weeks;
  }

  int _isoWeekNumber(DateTime date) {
    final thursday = date.add(Duration(days: 4 - date.weekday));
    final firstJan = DateTime(thursday.year, 1, 1);
    final diff = thursday.difference(firstJan).inDays;
    return 1 + (diff ~/ 7);
  }

  String _fmtDay(DateTime d) => '${d.day.toString().padLeft(2,'0')}/${d.month.toString().padLeft(2,'0')}';
  String _vnWeekday(int wd) {
    const map = {
      1: 'Thứ 2', 2: 'Thứ 3', 3: 'Thứ 4', 4: 'Thứ 5', 5: 'Thứ 6', 6: 'Thứ 7', 7: 'Chủ Nhật'
    };
    return map[wd] ?? 'CN';
  }

  // --- API Fetching ---

  Future<void> _fetch({bool nextPage = false}) async {
    if (_isLoading) return;
    setState(() => _isLoading = true);

    if (!nextPage) {
      _page = 1;
      _items.clear();
    }

    try {
      final prefs = await SharedPreferences.getInstance();
      final mssv = prefs.getString('username') ?? '';
      final filters = _queryParamsForView();
      if (mssv.isNotEmpty) filters['MaSinhVien'] = mssv;

      final res = await ApiService.fetchSchedule(
        page: _page,
        pageSize: 20,
        sortBy: 'NgayHoc',
        sortDir: 'ASC',
        filters: filters,
      );

      if (!mounted) return;

      if (res['success'] == true) {
        final List<Map<String,dynamic>> newItems = (res['items'] as List).cast<Map<String,dynamic>>();
        setState(() {
          _items.addAll(newItems);
          _totalPages = int.tryParse(res['totalPages']?.toString() ?? '1') ?? 1;
          if (newItems.isNotEmpty) _page++;
        });
      } else {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(res['message'] ?? 'Lỗi tải lịch')));
      }
    } catch (e) {
      if(mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Lỗi kết nối: $e')));
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Map<String, String> _queryParamsForView() {
    if (_view == 'day') {
      final d = _anchor;
      final fmt = '${d.year}-${d.month.toString().padLeft(2,'0')}-${d.day.toString().padLeft(2,'0')}';
      return {'NgayHoc': fmt};
    } else if (_view == 'week') {
      return {
        'Tuan': _selectedWeek.toString(),
        'Thang': _anchor.month.toString(),
        'Nam': _selectedYear.toString(),
      };
    } else {
      return {'Thang': _anchor.month.toString(), 'Nam': _anchor.year.toString()};
    }
  }

  // --- Pickers ---

  Future<void> _pickDay() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _anchor,
      firstDate: DateTime(2020),
      lastDate: DateTime(2030),
      builder: (context, child) {
        return Theme(
          data: Theme.of(context).copyWith(
            colorScheme: ColorScheme.light(primary: _primaryColor),
          ),
          child: child!,
        );
      },
    );
    if (picked != null) {
      setState(() {
        _anchor = picked;
        _refreshData();
      });
    }
  }

  void _openYearPicker() async {
    final years = [_selectedYear - 1, _selectedYear, _selectedYear + 1];
    final picked = await showModalBottomSheet<int>(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (c) => Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const SizedBox(height: 10),
          Container(width: 40, height: 4, decoration: BoxDecoration(color: Colors.grey[300], borderRadius: BorderRadius.circular(2))),
          const Padding(padding: EdgeInsets.all(16), child: Text('Chọn năm học', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold))),
          ...years.map((y) => ListTile(
            title: Text('Năm $y', style: const TextStyle(fontSize: 16)),
            trailing: y == _selectedYear ? Icon(Icons.check_circle, color: _primaryColor) : null,
            onTap: () => Navigator.pop(c, y),
          )),
          const SizedBox(height: 20),
        ],
      ),
    );
    if (picked != null && picked != _selectedYear) {
      setState(() {
        _selectedYear = picked;
        _weeksOfSelectedYear = _generateWeeks(_selectedYear);
        _selectedWeek = _isoWeekNumber(DateTime.now()) + _weekOffset; // Reset về tuần hiện tại của năm mới
        _anchor = _findWeekByNumber(_selectedWeek)?.start ?? DateTime(_selectedYear, 1, 1);
        _refreshData();
      });
    }
  }

  void _openWeekPicker() async {
    final picked = await showModalBottomSheet<int>(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (c) => Container(
        constraints: BoxConstraints(maxHeight: MediaQuery.of(context).size.height * 0.6),
        child: Column(
          children: [
            const SizedBox(height: 10),
            Container(width: 40, height: 4, decoration: BoxDecoration(color: Colors.grey[300], borderRadius: BorderRadius.circular(2))),
            const Padding(padding: EdgeInsets.all(16), child: Text('Chọn tuần học', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold))),
            Expanded(
              child: ListView.separated(
                itemCount: _weeksOfSelectedYear.length,
                separatorBuilder: (context, index) => const Divider(height: 1),
                itemBuilder: (context, index) {
                  final w = _weeksOfSelectedYear[index];
                  final isSelected = w.number == _selectedWeek;
                  return ListTile(
                    tileColor: isSelected ? _primaryColor.withOpacity(0.05) : null,
                    title: Text('Tuần ${w.number}', style: TextStyle(fontWeight: isSelected ? FontWeight.bold : FontWeight.normal, color: isSelected ? _primaryColor : Colors.black87)),
                    subtitle: Text('${_fmtDay(w.start)} - ${_fmtDay(w.end)}'),
                    trailing: isSelected ? Icon(Icons.check, color: _primaryColor) : null,
                    onTap: () => Navigator.pop(c, w.number),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
    if (picked != null && picked != _selectedWeek) {
      final info = _findWeekByNumber(picked);
      setState(() {
        _selectedWeek = picked;
        if (info != null) _anchor = info.start;
        _refreshData();
      });
    }
  }

  void _openMonthPicker() async {
    // Reuse logic cũ nhưng làm gọn UI
    // (Giữ logic cũ của bạn vì nó khá custom, chỉ style lại button kích hoạt)
    int tmpYear = _anchor.year;
    final picked = await showModalBottomSheet<DateTime>(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (c) {
        return StatefulBuilder(
          builder: (c, setStateSheet) {
            return Container(
              padding: const EdgeInsets.all(16),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      IconButton(icon: const Icon(Icons.chevron_left), onPressed: () => setStateSheet(() => tmpYear--)),
                      Text('Năm $tmpYear', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                      IconButton(icon: const Icon(Icons.chevron_right), onPressed: () => setStateSheet(() => tmpYear++)),
                    ],
                  ),
                  const SizedBox(height: 10),
                  Wrap(
                    spacing: 12, runSpacing: 12,
                    children: List.generate(12, (i) {
                      final m = i + 1;
                      final isSelected = m == _anchor.month && tmpYear == _anchor.year;
                      return InkWell(
                        onTap: () => Navigator.pop(c, DateTime(tmpYear, m, 1)),
                        borderRadius: BorderRadius.circular(12),
                        child: Container(
                          width: (MediaQuery.of(context).size.width - 60) / 4,
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          decoration: BoxDecoration(
                            color: isSelected ? _primaryColor : Colors.white,
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: isSelected ? _primaryColor : Colors.grey.shade300),
                            boxShadow: isSelected ? [BoxShadow(color: _primaryColor.withOpacity(0.3), blurRadius: 4, offset: const Offset(0,2))] : [],
                          ),
                          alignment: Alignment.center,
                          child: Text('T$m', style: TextStyle(color: isSelected ? Colors.white : Colors.black87, fontWeight: FontWeight.bold)),
                        ),
                      );
                    }),
                  ),
                  const SizedBox(height: 10),
                ],
              ),
            );
          },
        );
      },
    );
    if (picked != null) {
      setState(() {
        _anchor = picked;
        _refreshData();
      });
    }
  }

  void _refreshData() {
    _page = 1;
    _items.clear();
    _fetch();
  }

  // --- UI Building ---

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _backgroundColor,
      appBar: AppBar(
        title: const Text('Lịch học', style: TextStyle(fontWeight: FontWeight.bold, color: Colors.white)),
        backgroundColor: _primaryColor,
        elevation: 0,
        centerTitle: true,
      ),
      body: Column(
        children: [
          _buildTopControlBar(),
          Expanded(
            child: RefreshIndicator(
              onRefresh: () async => _refreshData(),
              color: _primaryColor,
              child: _items.isEmpty && !_isLoading
                  ? _buildEmptyState()
                  : (_view == 'day' ? _buildFlatList() : _buildGroupedList()),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTopControlBar() {
    return Container(
      padding: const EdgeInsets.fromLTRB(16, 16, 16, 20),
      decoration: BoxDecoration(
        color: _primaryColor,
        borderRadius: const BorderRadius.vertical(bottom: Radius.circular(24)),
        boxShadow: [BoxShadow(color: _primaryColor.withOpacity(0.3), blurRadius: 10, offset: const Offset(0, 5))],
      ),
      child: Column(
        children: [
          // Segmented Control
          Container(
            padding: const EdgeInsets.all(4),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.2),
              borderRadius: BorderRadius.circular(25),
            ),
            child: Row(
              children: [
                _buildSegmentBtn('Ngày', 'day'),
                _buildSegmentBtn('Tuần', 'week'),
                _buildSegmentBtn('Tháng', 'month'),
              ],
            ),
          ),
          const SizedBox(height: 16),
          // Filter Button
          _buildFilterButton(),
        ],
      ),
    );
  }

  Widget _buildSegmentBtn(String label, String value) {
    final isActive = _view == value;
    return Expanded(
      child: GestureDetector(
        onTap: () {
          if (!isActive) {
            setState(() {
              _view = value;
              // Logic reset anchor khi chuyển view nếu cần
              if (value == 'week') {
                 // Đảm bảo anchor nằm trong tuần đang chọn
                 _anchor = _findWeekByNumber(_selectedWeek)?.start ?? _anchor;
              }
            });
            _refreshData();
          }
        },
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(vertical: 8),
          decoration: BoxDecoration(
            color: isActive ? Colors.white : Colors.transparent,
            borderRadius: BorderRadius.circular(20),
            boxShadow: isActive ? [BoxShadow(color: Colors.black.withOpacity(0.1), blurRadius: 4, offset: const Offset(0, 2))] : [],
          ),
          alignment: Alignment.center,
          child: Text(
            label,
            style: TextStyle(
              fontWeight: FontWeight.w600,
              color: isActive ? _primaryColor : Colors.white.withOpacity(0.9),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildFilterButton() {
    String text = '';
    VoidCallback onTap = () {};

    if (_view == 'day') {
      text = '${_vnWeekday(_anchor.weekday)}, ngày ${_anchor.day} tháng ${_anchor.month}, ${_anchor.year}';
      onTap = _pickDay;
    } else if (_view == 'week') {
      text = 'Tuần $_selectedWeek - Năm $_selectedYear';
      // Logic đặc biệt: Bấm vào thì hiện dialog chọn Năm trước, rồi chọn Tuần sau?
      // Để đơn giản ta hiện bottom sheet chọn Năm, trong đó có nút chuyển sang chọn Tuần hoặc ngược lại
      // Ở đây mình tách ra 2 vùng bấm hoặc 1 vùng chung. Mình làm 1 vùng chung mở action sheet.
      return Row(
        children: [
          Expanded(
            child: _glassButton(
              icon: Icons.calendar_view_week,
              text: 'Tuần $_selectedWeek',
              onTap: _openWeekPicker,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: _glassButton(
              icon: Icons.calendar_today,
              text: 'Năm $_selectedYear',
              onTap: _openYearPicker,
            ),
          ),
        ],
      );
    } else {
      text = 'Tháng ${_anchor.month} / ${_anchor.year}';
      onTap = _openMonthPicker;
    }

    return _glassButton(icon: Icons.event, text: text, onTap: onTap, isFullWidth: true);
  }

  Widget _glassButton({required IconData icon, required String text, required VoidCallback onTap, bool isFullWidth = false}) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: isFullWidth ? double.infinity : null,
        padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(icon, size: 18, color: _primaryColor),
            const SizedBox(width: 8),
            Text(text, style: TextStyle(color: _primaryColor, fontWeight: FontWeight.bold, fontSize: 15)),
            const SizedBox(width: 4),
            Icon(Icons.arrow_drop_down, color: _primaryColor),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyState() {
    final bottomInset = MediaQuery.of(context).padding.bottom + kBottomNavigationBarHeight;
    return Padding(
      padding: EdgeInsets.only(bottom: bottomInset),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.event_busy, size: 80, color: Colors.grey[300]),
            const SizedBox(height: 16),
            Text('Không có lịch học nào', style: TextStyle(fontSize: 18, color: Colors.grey[500], fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text('Thử chọn thời gian khác xem sao', style: TextStyle(color: Colors.grey[400])),
          ],
        ),
      ),
    );
  }

  // --- List Builders ---

  Widget _buildFlatList() {
    final bottomInset = MediaQuery.of(context).padding.bottom + kBottomNavigationBarHeight;
    return ListView.builder(
      controller: _controller,
      padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottomInset),
      itemCount: _items.length + (_isLoading ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= _items.length) return const Center(child: Padding(padding: EdgeInsets.all(16), child: CircularProgressIndicator()));
        return _buildTimelineCard(_items[index], showDate: false);
      },
    );
  }

  Widget _buildGroupedList() {
    final groups = _buildDayGroups();
    final bottomInset = MediaQuery.of(context).padding.bottom + kBottomNavigationBarHeight;
    return ListView.builder(
      controller: _controller,
      padding: EdgeInsets.fromLTRB(16, 16, 16, 16 + bottomInset),
      itemCount: groups.length + (_isLoading ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= groups.length) return const Center(child: Padding(padding: EdgeInsets.all(16), child: CircularProgressIndicator()));
        final g = groups[index];
        return Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.only(left: 4, bottom: 8, top: 8),
              child: Row(
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(color: _primaryColor.withOpacity(0.1), borderRadius: BorderRadius.circular(8)),
                    child: Text('${g.date.day}/${g.date.month}', style: TextStyle(color: _primaryColor, fontWeight: FontWeight.bold)),
                  ),
                  const SizedBox(width: 8),
                  Text(_vnWeekday(g.date.weekday), style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Colors.black87)),
                ],
              ),
            ),
            ...g.items.map((it) => _buildTimelineCard(it, showDate: false)),
          ],
        );
      },
    );
  }

  // Helper gom nhóm
  List<_DayGroup> _buildDayGroups() {
    final Map<DateTime,List<Map<String,dynamic>>> bucket = {};
    for (final it in _items) {
      final buoi = it['buoiHoc'] ?? {};
      final raw = buoi['ngayHoc']?.toString() ?? '';
      final d = _parseNgayHoc(raw) ?? DateTime.now();
      final key = DateTime(d.year, d.month, d.day);
      bucket.putIfAbsent(key, () => []).add(it);
    }
    final keys = bucket.keys.toList()..sort();
    return keys.map((d) => _DayGroup(date: d, items: bucket[d]!)).toList();
  }

  DateTime? _parseNgayHoc(String raw) {
    try {
      if (raw.contains('-')) {
        final p = raw.split('-');
        if (p[0].length == 4) return DateTime(int.parse(p[0]), int.parse(p[1]), int.parse(p[2])); // yyyy-MM-dd
        return DateTime(int.parse(p[2]), int.parse(p[1]), int.parse(p[0])); // dd-MM-yyyy
      }
      if (raw.contains('/')) {
        final p = raw.split('/');
        return DateTime(int.parse(p[2]), int.parse(p[1]), int.parse(p[0]));
      }
    } catch (_) {}
    return null;
  }

  // --- CARD DESIGN (Timeline Style) ---

  Widget _buildTimelineCard(Map<String, dynamic> item, {bool showDate = false}) {
    final monHoc = item['monHoc'] ?? {};
    final buoi = item['buoiHoc'] ?? {};
    final phong = item['phongHoc'] ?? {};
    final gvInfo = item['giangVienInfo'] ?? {};
    final lopHp = item['lopHocPhan'] ?? {};

    final tenMon = monHoc['tenMonHoc'] ?? lopHp['tenLopHocPhan'] ?? 'Chưa cập nhật';
    final maLHP = lopHp != null
      ? (lopHp['maLopHocPhan'] ?? lopHp['MaLopHocPhan'] ?? lopHp['maLopHocPhan'] ?? '')
      : '';
    final colorSeed = (maLHP.toString().trim().isNotEmpty) ? maLHP.toString() : tenMon.toString();
    final tietBD = int.tryParse(buoi['tietBatDau']?.toString() ?? '0') ?? 0;
    final soTiet = int.tryParse(buoi['soTiet']?.toString() ?? '0') ?? 0;
    final tietKT = tietBD + soTiet - 1;
    final tenPhong = phong['tenPhong'] ?? '---';
    final giangVien = gvInfo['hoTen'] ?? '---';

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      child: IntrinsicHeight(
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Cột thời gian (Timeline Left)
            SizedBox(
              width: 50,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.start,
                children: [
                  Text(tietBD.toString(), style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.black87)),
                  Text('Tiết', style: TextStyle(fontSize: 12, color: Colors.grey[500])),
                  Container(width: 1, height: 20, color: Colors.grey[300], margin: const EdgeInsets.symmetric(vertical: 4)),
                  Text(tietKT > 0 ? tietKT.toString() : '?', style: TextStyle(fontSize: 14, color: Colors.grey[600], fontWeight: FontWeight.w600)),
                ],
              ),
            ),
            // Card nội dung
            Expanded(
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.05), blurRadius: 10, offset: const Offset(0, 4))],
                ),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(16),
                  child: Stack(
                    children: [
                      // Vạch màu trang trí bên trái
                      Positioned(left: 0, top: 0, bottom: 0, child: Container(width: 6, color: _getRandomColor(colorSeed))),
                      Padding(
                        padding: const EdgeInsets.fromLTRB(20, 16, 16, 16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(tenMon.toString(), style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF1E293B))),
                            const SizedBox(height: 8),
                            const Divider(height: 1, color: Color(0xFFF1F5F9)),
                            const SizedBox(height: 8),
                            Row(
                              children: [
                                Icon(Icons.location_on_outlined, size: 16, color: Colors.grey[500]),
                                const SizedBox(width: 4),
                                Expanded(child: Text('Phòng: $tenPhong', style: TextStyle(color: Colors.grey[700], fontSize: 13, fontWeight: FontWeight.w500))),
                              ],
                            ),
                            const SizedBox(height: 4),
                            Row(
                              children: [
                                Icon(Icons.person_outline, size: 16, color: Colors.grey[500]),
                                const SizedBox(width: 4),
                                Expanded(child: Text('GV: $giangVien', style: TextStyle(color: Colors.grey[700], fontSize: 13))),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Color _getRandomColor(String seed) {
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
}

class _WeekInfo {
  final int number;
  final DateTime start;
  final DateTime end;
  _WeekInfo({required this.number, required this.start, required this.end});
}

class _DayGroup {
  final DateTime date;
  final List<Map<String,dynamic>> items;
  _DayGroup({required this.date, required this.items});
}