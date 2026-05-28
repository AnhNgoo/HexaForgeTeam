# TestReports (CSV)

CSV report chỉ được tạo **sau khi bạn chạy test có gắn `[TestReport]`** trong Unity Test Runner.

## Vị trí tạo report
- Thư mục: `d:\Unity\HexaForgeTeam\TestReports\`

## Các file CSV cố định (dễ import Excel)
- `Enemy.csv`
- `Player.csv`
- `Gold.csv`
- `All.csv` (tổng hợp)

Các file này sẽ **được ghi thêm dòng (append) qua nhiều lần chạy** và giữ nguyên tên (không tạo file theo timestamp).

## Vì sao bạn chưa thấy file CSV?
- Nếu chưa chạy test (hoặc chạy test không có `[TestReport]`) thì file CSV sẽ chưa được tạo.

## Cách tạo nhanh
1. Mở Unity.
2. `Window > General > Test Runner`.
3. Chọn một test fixture có `[TestReport]` (ví dụ: `GoldTest`, `GoldManagerTest`, `EnemyInitializationTests`, ...).
4. Nhấn **Run**.
5. Quay lại thư mục này để thấy các file `Enemy.csv/Player.csv/Gold.csv/All.csv`.

## Tham chiếu code
- `GetReportPath()`/`GetGroupReportPath()`/`GetReportDirectory()` nằm ở: `Assets/Tests/TestCommon/CsvTestReportWriter.cs`
- Hook ghi CSV nằm ở: `Assets/Tests/TestCommon/TestReportAttribute.cs`
