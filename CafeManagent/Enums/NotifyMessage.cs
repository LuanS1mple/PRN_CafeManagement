namespace CafeManagent.Enums
{
    public class NotifyMessage
    {
        public string Message { get; }

        private NotifyMessage(string message)
        {
            Message = message;
        }
        //thêm vào đây message
        public static readonly NotifyMessage HAVE_REQUEST = new("Bạn có request mới cần xử lí");
        public static readonly NotifyMessage PHAN_HOI_THANH_CONG = new("Phản hổi thành công");
        public static readonly NotifyMessage TAO_REQUEST_THANH_CONG = new("Tạo request thành công");
        public static readonly NotifyMessage TAO_REQUEST_THAT_BAI = new("Tạo request thất bại");
        public static readonly NotifyMessage GET_REQUEST_THAT_BAI = new("Lấy dữ liệt thất bại");
        //Thông báo ca làm
        public static readonly NotifyMessage THONG_BAO_CA_LAM_TRC_1_DAY = new("Bạn có ca làm vào ngày mai");
        public static readonly NotifyMessage THONG_BAO_CA_LAM_TRC_30_MIN = new("Ca làm của bạn ngày hôm nay sắp bắt đầu");
        public static readonly NotifyMessage THONG_BAO_CA_LAM = new("Ca làm của bạn đã bắt đầu");
        public static readonly NotifyMessage UPDATE_STATUS_ORDER = new("Cập nhật nhật trạng thái thành công ");
        public static readonly NotifyMessage CREATE_ORDER = new("Đơn hàng đã tạo mới thành công");

        //Attendance
        public static readonly NotifyMessage PHAN_HOI_THIEU_THONG_TIN_CA_LAM = new("Thiếu thông tin nhân viên hoặc ca làm việc!");
        public static readonly NotifyMessage PHAN_HOI_THIEU_THONG_TIN_NHAN_VIEN = new("Không tìm thấy ca làm của nhân viên trong ngày!");
        public static readonly NotifyMessage CHECK_IN_THANH_CONG = new("Đã check in thành công cho nhân viên ");
        public static readonly NotifyMessage CHECK_OUT_THANH_CONG = new("Đã check out thành công cho nhân viên ");
    }
}
