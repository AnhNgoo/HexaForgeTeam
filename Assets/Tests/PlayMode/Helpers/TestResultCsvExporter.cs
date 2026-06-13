using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DuskBlade.Tests
{
    public static class TestResultCsvExporter
    {
        private const string ExportDirectory = "Assets/CSV";
        private const int MaxActualResultLength = 360;

        private static readonly string[] Header =
        {
            "Mã TC",
            "Tiêu đề testcase",
            "Kết quả mong đợi",
            "Kết quả thực tế",
            "Tình trạng thực thi",
            "Mức độ nghiêm trọng của lỗi",
            "Kiểu chạy (Thủ công / Tự động)",
            "Người kiểm thử",
            "Ngày bắt đầu",
            "Chi tiết bước kiểm thử",
            "Ghi chú"
        };

        public static string Export(string systemName, List<TestResultRecord> records)
        {
            Directory.CreateDirectory(ExportDirectory);

            string safeSystemName = SanitizeFileName(string.IsNullOrWhiteSpace(systemName) ? "TestResults" : systemName);
            string fileName = $"{safeSystemName}.csv";
            string path = Path.Combine(ExportDirectory, fileName).Replace("\\", "/");

            using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(ToCsvLine(Header));

                if (records != null)
                {
                    foreach (TestResultRecord record in records)
                    {
                        writer.WriteLine(ToCsvLine(ToColumns(record)));
                    }
                }
            }

            return path;
        }

        private static string[] ToColumns(TestResultRecord record)
        {
            if (record == null)
            {
                return new string[Header.Length];
            }

            return new[]
            {
                record.MaTC,
                record.TieuDeTestcase,
                record.KetQuaMongDoi,
                FormatActualResult(record.KetQuaThucTe),
                record.TinhTrangThucThi,
                record.MucDoNghiemTrongCuaLoi,
                record.KieuChay,
                record.NguoiKiemThu,
                record.NgayBatDau,
                record.ChiTietBuocKiemThu,
                record.GhiChu
            };
        }

        private static string ToCsvLine(IReadOnlyList<string> columns)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(Escape(columns[i]));
            }

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            bool needsQuotes = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
            string escaped = value.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{escaped}\"" : escaped;
        }

        private static string FormatActualResult(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            string formatted = value
                .Replace("\r\n", " | ")
                .Replace("\n", " | ")
                .Replace("\r", " | ")
                .Replace("\t", " ")
                .Trim();

            while (formatted.Contains("  ")) formatted = formatted.Replace("  ", " ");
            while (formatted.Contains("| |")) formatted = formatted.Replace("| |", "|");

            if (formatted.Length > MaxActualResultLength)
            {
                formatted = formatted.Substring(0, MaxActualResultLength) + "... (đã rút gọn, xem Unity Console để biết chi tiết)";
            }

            return formatted;
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}
