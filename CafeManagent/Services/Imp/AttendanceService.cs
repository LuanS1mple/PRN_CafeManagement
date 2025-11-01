using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

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
                .OrderByDescending(a => a.Workdate)
                .ToList();
        }

        public Attendance CheckIn(int staffId, int shiftId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

  
            var shift = _context.WorkShifts.FirstOrDefault(s => s.WorkshiftId == shiftId);
            if (shift == null)
                throw new Exception("Không tìm thấy ca làm.");

            
            var attendance = GetAttendanceWithShift(shiftId, staffId, today);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    ShiftId = shiftId,
                    StaffId = staffId,
                    Workdate = today,
                    CheckIn = TimeOnly.FromDateTime(DateTime.Now),
                    Status = 1
                };
                var lateMinutes = (attendance.CheckIn.Value.ToTimeSpan() - shift.StartTime.Value.ToTimeSpan()).TotalMinutes;

                if (lateMinutes <= 0)
                    attendance.Note = "Đi đúng giờ";
                else if (lateMinutes < 60)
                    attendance.Note = $"Đi trễ {lateMinutes:F0} phút";
                else
                {
                    var hours = (int)(lateMinutes / 60);
                    var minutes = (int)(lateMinutes % 60);
                    attendance.Note = $"Đi trễ {hours} giờ {minutes} phút";
                }

                _context.Attendances.Add(attendance);
            }
            else
            {
                throw new Exception("Nhân viên này đã check-in cho ca này rồi!");
            }

            _context.SaveChanges();
            return attendance;
        }

        public Attendance CheckOut(int staffId, int shiftId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var shift = _context.WorkShifts.FirstOrDefault(s => s.WorkshiftId == shiftId);
            if (shift == null)
                throw new Exception("Không tìm thấy ca làm.");
            var attendance = GetAttendanceWithShift(shiftId, staffId, today);
            if (attendance == null)
                throw new Exception("Nhân viên chưa check-in trong ca này.");
            if (attendance.CheckOut != null)
                throw new Exception("Nhân viên này đã check-out rồi!");
            attendance.CheckOut = TimeOnly.FromDateTime(DateTime.Now);
            var duration = (attendance.CheckOut.Value.ToTimeSpan() - attendance.CheckIn.Value.ToTimeSpan());
            var workMinutes = duration.TotalMinutes;
            if (workMinutes < (shift.EndTime.Value.ToTimeSpan() - shift.StartTime.Value.ToTimeSpan()).TotalMinutes * 0.5)
                attendance.Note = attendance.Note + " (Rời sớm)";
            else
                attendance.Note = attendance.Note + " (Hoàn thành ca làm)";
            double totalhour = duration.TotalHours;
            attendance.TotalHour = (decimal)totalhour;
            _context.Attendances.Update(attendance);
            _context.SaveChanges();

            return attendance;
        }

        public Attendance GetAttendanceWithShift(int shiftId, int staffId, DateOnly date)
        {
            return _context.Attendances
                .Include(a => a.Shift)
                .Include(a => a.Workshift)
                .Include(a => a.Staff)
                .FirstOrDefault(a => a.ShiftId == shiftId && a.StaffId == staffId && a.Workdate == date);
        }

    }
}
