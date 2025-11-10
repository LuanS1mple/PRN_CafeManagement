using CafeManagent.dto.attendance;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;

namespace CafeManagent.Services.Imp
{
    public class AttendanceService : IAttendanceService
    {
        private readonly CafeManagementContext _context;
        public AttendanceService(CafeManagementContext context)
        {
            _context = context;
        }

        public List<Attendance> GetAllAttance()
        {
            return _context.Attendances.Include(a => a.Staff).Include(a => a.Workshift).Include(a => a.Shift).ToList();  
        }

        public Attendance GetAttendance(DateOnly date, int shift, int staffId)
        {
            return _context.Attendances.FirstOrDefault(s => s.StaffId == staffId && s.Workdate.Equals(date) && s.ShiftId==shift);
        }

        public Attendance GetAttendanceCheckIn(int workshiftId, int staffId, DateOnly date, int shiftId)
        {
            return _context.Attendances
                .FirstOrDefault(ws =>
                    ws.StaffId == staffId &&
                    ws.WorkshiftId == workshiftId &&
                    ws.ShiftId == shiftId &&
                    ws.Workdate == date);
        }

        public void Update(Attendance attendance)
        {
            _context.Attendances.Update(attendance);
            _context.SaveChanges();
        }
        public List<Attendance> FilterAttendance(DateOnly? fromDate, DateOnly? toDate, string? keyword)
        {
            var query = _context.Attendances
                .Include(a => a.Staff)
                .Include(a => a.Workshift)
                .Include(a => a.Shift)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(a =>
                    (!string.IsNullOrEmpty(a.Staff.FullName) && a.Staff.FullName.ToLower().Contains(lowerKeyword)) ||
                    a.AttendanceId.ToString().Contains(lowerKeyword));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a =>
                    a.Workdate.HasValue && a.Workdate.Value >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(a =>
                    a.Workdate.HasValue && a.Workdate.Value <= toDate.Value);
            }

            return query
                .ToList();
        }

        public async Task<Attendance?> CheckInAsync(int workshiftId, int shiftId, int staffId, DateOnly date)
        {
            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.WorkshiftId == workshiftId);
            if (workShift == null)
                throw new Exception("Không tìm thấy ca làm.");

            var existing = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StaffId == staffId &&
                a.WorkshiftId == workshiftId &&
                a.ShiftId == shiftId &&
                a.Workdate == date);
            if (existing != null)
                throw new Exception("Nhân viên này đã Check-In cho ca này rồi!");

            var attendance = new Attendance
            {
                StaffId = staffId,
                WorkshiftId = workshiftId,
                ShiftId = shiftId,
                Workdate = date,
                CheckIn = TimeOnly.FromDateTime(DateTime.Now),
                Status = 1
            };

            var lateMinutes = (attendance.CheckIn.Value.ToTimeSpan() - workShift.StartTime.Value.ToTimeSpan()).TotalMinutes;
            if (lateMinutes <= 0)
                attendance.Note = "Đi đúng giờ";
            else if (lateMinutes < 60)
                attendance.Note = $"Đi trễ {lateMinutes:F0} phút";
            else
            {
                var hours = (int)(lateMinutes / 60);
                var mins = (int)(lateMinutes % 60);
                attendance.Note = $"Đi trễ {hours} giờ {mins} phút";
            }

            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance?> CheckOutAsync(int workshiftId, int shiftId, int staffId, DateOnly date)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a =>
                a.StaffId == staffId &&
                a.WorkshiftId == workshiftId &&
                a.ShiftId == shiftId &&
                a.Workdate == date);

            if (attendance == null)
                throw new Exception("Nhân viên chưa Check-In trong ca này.");
            if (attendance.CheckOut != null)
                throw new Exception("Nhân viên này đã Check-Out rồi!");

            var workShift = await _context.WorkShifts.FirstOrDefaultAsync(ws => ws.WorkshiftId == workshiftId);
            if (workShift == null)
                throw new Exception("Không tìm thấy thông tin ca làm.");

            attendance.CheckOut = TimeOnly.FromDateTime(DateTime.Now);

            var totalMinutes = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan()).TotalMinutes;
            var expectedMinutes = (workShift.EndTime.Value.ToTimeSpan() - workShift.StartTime.Value.ToTimeSpan()).TotalMinutes;

            if (totalMinutes < expectedMinutes * 0.5)
                attendance.Note += " (Rời sớm)";
            else
                attendance.Note += " (Hoàn thành ca làm)";

            attendance.TotalHour = (decimal)(totalMinutes / 60);
            attendance.Status = 2;

            _context.Attendances.Update(attendance);
            await _context.SaveChangesAsync();

            return attendance;
        }


        public async Task<Attendance?> GetAttendanceWithShiftAsync(int workshiftId, int staffId, DateOnly date, int shiftId)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Staff)
                .Include(ws => ws.Workshift)
                .Include(ws => ws.Manager)
                .FirstOrDefaultAsync(ws =>
                    ws.StaffId == staffId &&
                    ws.WorkshiftId == workshiftId &&
                    ws.ShiftId == shiftId &&
                    ws.Date == date);

            if (schedule == null) return null;

            return new Attendance
            {
                StaffId = schedule.StaffId,
                ShiftId = schedule.ShiftId,
                WorkshiftId = schedule.WorkshiftId,
                Workdate = schedule.Date,
                Staff = schedule.Staff,
                Workshift = schedule.Workshift
            };
        }

        public List<Attendance> GetAttendanceByMonth(int? staffId, int? month, int? year)
        {
            if (staffId == null) return new List<Attendance>();
            if (!month.HasValue || !year.HasValue) return new List<Attendance>();

            var startDate = new DateOnly(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return _context.Attendances
                .Include(a => a.Staff)
                .Include(a => a.Shift)
                .Where(a => a.StaffId == staffId
                            && a.Workdate >= startDate
                            && a.Workdate <= endDate)
                .ToList();
        }

        public async Task<List<MonthlyReport>> GetMonthlyReportAsync(int? staffId, int month, int year)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var staffQuery = _context.Staff.AsQueryable();
            if (staffId.HasValue)
            {
                staffQuery = staffQuery.Where(s => s.StaffId == staffId.Value);
            }
            var staffs = await staffQuery.ToListAsync();

     
            var attendance = await _context.Attendances
                .Include(a => a.Staff)
                .Where(a => a.Workdate >= startDate && a.Workdate <= endDate)
                .ToListAsync();

            var requests = await _context.Requests
                .Include(r => r.Staff)
                .Where(r => r.ReportDate >= startDate.ToDateTime(new TimeOnly(0, 0))
                         && r.ReportDate <= endDate.ToDateTime(new TimeOnly(23, 59))
                         && r.ReportType.Contains("Nghỉ phép"))
                .ToListAsync();

   
            var reports = staffs.Select(s =>
            {
                var staffAttendances = attendance.Where(a => a.StaffId == s.StaffId).ToList();
                var validWorkdays = staffAttendances.Where(a => (a.TotalHour ?? 0) >= 4).ToList();

                return new MonthlyReport
                {
                    StaffId = s.StaffId,
                    StaffName = s.FullName,
                    Month = month,
                    Year = year,
                    workingDays = validWorkdays.Count,
                    TotalHours = validWorkdays.Sum(a => a.TotalHour ?? 0),
                    LateCount = staffAttendances.Count(a => a.Note != null && a.Note.ToLower().Contains("trễ")),
                    LeaveEarlyCount = staffAttendances.Count(a => a.Note != null && a.Note.ToLower().Contains("rời sớm")),
                    LeaveDays = requests.Count(r => r.StaffId == s.StaffId)
                };
            })
 
            .Where(r => r.workingDays > 0)
            .ToList();

            return reports;
        }




        public async Task<byte[]> ExportMonthlyReportToExcelAsync(List<MonthlyReport> monthlyReportList)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Báo cáo tháng");

                // --- Header ---
                ws.Cell(1, 1).Value = "Mã NV";
                ws.Cell(1, 2).Value = "Tên nhân viên";
                ws.Cell(1, 3).Value = "Tháng";
                ws.Cell(1, 4).Value = "Năm";
                ws.Cell(1, 5).Value = "Số ngày công";
                ws.Cell(1, 6).Value = "Tổng giờ làm";
                ws.Cell(1, 7).Value = "Số ca đi trễ";
                ws.Cell(1, 8).Value = "Số ca rời sớm";
                ws.Cell(1, 9).Value = "Số ngày nghỉ phép";

                int row = 2;
                foreach (var r in monthlyReportList)
                {
                    ws.Cell(row, 1).Value = r.StaffId;
                    ws.Cell(row, 2).Value = r.StaffName;
                    ws.Cell(row, 3).Value = r.Month;
                    ws.Cell(row, 4).Value = r.Year;
                    ws.Cell(row, 5).Value = r.workingDays;
                    ws.Cell(row, 6).Value = r.TotalHours;
                    ws.Cell(row, 7).Value = r.LateCount;
                    ws.Cell(row, 8).Value = r.LeaveEarlyCount;
                    ws.Cell(row, 9).Value = r.LeaveDays;
                    row++;
                }
                ws.Cell(row, 2).Value = "TỔNG CỘNG";
                ws.Cell(row, 2).Style.Font.Bold = true;

                ws.Cell(row, 5).Value = monthlyReportList.Sum(r => r.workingDays);
                ws.Cell(row, 6).Value = monthlyReportList.Sum(r => r.TotalHours);
                ws.Cell(row, 7).Value = monthlyReportList.Sum(r => r.LateCount);
                ws.Cell(row, 8).Value = monthlyReportList.Sum(r => r.LeaveEarlyCount);
                ws.Cell(row, 9).Value = monthlyReportList.Sum(r => r.LeaveDays);
                ws.Range(row, 2, row, 9).Style.Font.Bold = true;
                ws.Range(row, 2, row, 9).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Range(row, 2, row, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return await System.Threading.Tasks.Task.FromResult(stream.ToArray());
                }
            }
        }

    }
}

