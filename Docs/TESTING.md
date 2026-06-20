# Quy trình kiểm thử (tối ưu)

## 1) Kiểm thử tích hợp (Integration testing)
- Các suite PlayMode hiện có (Player/Enemy) là dạng integration: dựng scene test (camera/light/ground) + spawn đối tượng + chạy vài frame để kiểm tra state/event/logic.
- Các test quan trọng đã gắn `Category("Integration")`.

## 2) Parallel testing (chạy song song) đa nền tảng PC/Mobile
Unity Test Runner không chạy đa nền tảng đồng thời trong 1 lần bấm; cách thực tế là chạy **nhiều job song song** (CI) — mỗi job chạy Unity CLI với `-buildTarget` khác nhau.

### Gợi ý ma trận nền tảng
- PC: `StandaloneWindows64`
- Mobile: `Android` (iOS cần runner macOS)

### Ví dụ lệnh Unity CLI (mỗi nền tảng 1 job)
Thay `UNITY_EXE` và `PROJECT_PATH` theo máy/CI của bạn.

- PlayMode P0 (ưu tiên chạy lỗi sớm):
```
"<UNITY_EXE>" -batchmode -nographics -projectPath "<PROJECT_PATH>" -runTests -testPlatform PlayMode -testCategory P0 -buildTarget StandaloneWindows64 -testResults "TestReports/results_P0_windows.xml" -logFile "TestReports/unity_windows_P0.log" -quit
```

- PlayMode Regression (chạy tất cả):
```
"<UNITY_EXE>" -batchmode -nographics -projectPath "<PROJECT_PATH>" -runTests -testPlatform PlayMode -buildTarget Android -testResults "TestReports/results_all_android.xml" -logFile "TestReports/unity_android_all.log" -quit
```

Gợi ý pipeline:
- Job 1: PlayMode P0 trên Windows
- Job 2: PlayMode all trên Windows
- Job 3: PlayMode P0/all trên Android
- (Job 4: iOS trên macOS)

Lưu ý:
- Chạy song song nhiều Unity instance trên 1 máy có thể bị giới hạn license/tài nguyên. CI matrix là cách an toàn nhất.

## 3) Test case prioritization (ưu tiên testcase)
Đã gắn category theo mức ưu tiên:
- `P0`: quan trọng nhất, chạy trước (Smoke/Critical)
- `P1`: quan trọng
- `P2`: phụ trợ/artifact

Chạy riêng P0:
```
-runTests -testCategory P0
```

Chạy theo nhóm tích hợp:
```
-runTests -testCategory Integration
```

## 4) Code coverage (độ bao phủ)
Cách phổ biến trong Unity là dùng package **Code Coverage**.

### CLI (nếu project đã cài Code Coverage package)
Ví dụ:
```
"<UNITY_EXE>" -batchmode -nographics -projectPath "<PROJECT_PATH>" -runTests -testPlatform PlayMode -testCategory P0 -enableCodeCoverage -coverageResultsPath "TestReports/Coverage" -coverageOptions "generateHtmlReport;generateAdditionalMetrics" -quit
```

Kết quả coverage thường nằm trong `TestReports/Coverage` (HTML/summary tùy options).

## 5) Báo cáo kiểm thử (CSV tiếng Việt)
Các test fixture đã gắn `[TestReport]` sẽ tự xuất CSV sau khi chạy.

- Output: thư mục `TestReports` (project root)
- File: `BaoCaoKiemThu_<Platform>_<Timestamp>.csv`
- Cột đúng format để dán Excel:
  - Mã TC
  - Tiêu đề testcase
  - Kết quả mong đợi
  - Kết quả thực tế
  - Kiểu chạy (Thủ công / Tự động)
  - Người kiểm thử
  - Ngày bắt đầu
  - Chi tiết bước kiểm thử
  - Ghi chú

Tuỳ biến người kiểm thử:
- Set biến môi trường `TESTER_NAME` trước khi chạy test (local/CI).
