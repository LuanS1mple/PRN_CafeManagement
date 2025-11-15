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

        //
        public static readonly NotifyMessage SUA_SAN_PHAM_THANH_CONG = new("Cập nhật sản phẩm thành công");
        public static readonly NotifyMessage DU_LIEU_KHONG_HOP_LE = new("Dữ kiệu nhập không hợp lệ");
        public static readonly NotifyMessage THEM_SAN_PHAM_THANH_CONG = new("Thêm sản phẩm thành công");
        public static readonly NotifyMessage SAN_PHAM_DA_TON_TAI = new("Sản phẩm đã tồn tại");
        public static readonly NotifyMessage XOA_SAN_PHAM_THANH_CONG = new("Xóa sản phẩm thành công");
        public static readonly NotifyMessage SAN_PHAM_KHONG_TON_TAI = new("Sản phầm không tồn tại");

        // ================= WORKSHIFT =================
        public static readonly NotifyMessage INVALID_WORKSHIFT_DATE = new("Không thể thao tác với ca làm ở ngày đã qua.");
        public static readonly NotifyMessage EMPTY_EMPLOYEE_NAME = new("Tên nhân viên không được để trống.");
        public static readonly NotifyMessage EMPLOYEE_NOT_FOUND = new("Không tìm thấy nhân viên.");
        public static readonly NotifyMessage SHIFT_TYPE_NOT_FOUND = new("Không tìm thấy loại ca làm việc.");
        public static readonly NotifyMessage SHIFT_DUPLICATED = new("Nhân viên đã có ca này trong ngày đã chọn (trùng ca).");
        public static readonly NotifyMessage ADD_WORKSHIFT_SUCCESS = new("Thêm ca làm thành công!");
        public static readonly NotifyMessage ADD_WORKSHIFT_FAILED = new("Thêm ca làm thất bại.");
        public static readonly NotifyMessage WORKSHIFT_NOT_FOUND = new("Không tìm thấy ca làm.");
        public static readonly NotifyMessage DELETE_WORKSHIFT_SUCCESS = new("Đã xóa ca làm thành công!");
        public static readonly NotifyMessage DELETE_WORKSHIFT_FAILED = new("Xóa ca làm thất bại.");
        public static readonly NotifyMessage UPDATE_WORKSHIFT_SUCCESS = new("Cập nhật ca làm thành công!");
        public static readonly NotifyMessage UPDATE_WORKSHIFT_FAILED = new("Cập nhật ca làm thất bại.");
        public static readonly NotifyMessage SHIFT_CONFLICT = new("Nhân viên này đã có ca tương tự trong ngày đã chọn.");

        // ================= MAIL =================
        public static readonly NotifyMessage SEND_MAIL_SUCCESS = new("Gửi mail thành công!");
        public static readonly NotifyMessage SEND_MAIL_FAILED = new("Gửi mail thất bại.");
        
        
        //CustomerProfile
        public static readonly NotifyMessage THEM_KHACH_HANG_THANH_CONG = new("Thêm khách hàng thành công.");
        public static readonly NotifyMessage KHACH_HANG_DA_TON_TAI = new("Khách hàng đã tồn tại.");
        public static readonly NotifyMessage XOA_KHACH_HANG_THANH_CONG = new("Xóa khách hàng thành công.");
        public static readonly NotifyMessage KHACH_HANG_KHONG_TON_TAI = new("Khách hàng không tồn tại.");
        public static readonly NotifyMessage SUA_KHACH_HANG_THANH_CONG = new("Sửa khách hàng thành công.");
        public static readonly NotifyMessage SUA_KHACH_HANG_THAT_BAI = new("Sửa khách hàng thất bại.");
    }
}
