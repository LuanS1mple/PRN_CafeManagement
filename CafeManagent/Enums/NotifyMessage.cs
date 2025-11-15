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
        public static readonly NotifyMessage CA_CUA_NGAY_CU = new("Không thể thao tác với ca làm ở ngày đã qua.");
        public static readonly NotifyMessage NHAN_VIEN_TRONG = new("Tên nhân viên không được để trống.");
        public static readonly NotifyMessage KHONG_TIM_THAY_NHAN_VIEN = new("Không tìm thấy nhân viên.");
        public static readonly NotifyMessage KHONG_THAY_CA_LAM = new("Không tìm thấy loại ca làm việc.");
        public static readonly NotifyMessage TRUNG_CA = new("Nhân viên đã có ca này trong ngày đã chọn (trùng ca).");
        public static readonly NotifyMessage THEM_CA_LAM_OK = new("Thêm ca làm thành công!");
        public static readonly NotifyMessage THEM_CA_THAT_BAI = new("Thêm ca làm thất bại.");
        public static readonly NotifyMessage XOA_CA_LAM_OK = new("Đã xóa ca làm thành công!");
        public static readonly NotifyMessage XOA_CA_THAT_BAI = new("Xóa ca làm thất bại.");
        public static readonly NotifyMessage CAP_NHAT_CA_OK = new("Cập nhật ca làm thành công!");
        public static readonly NotifyMessage CAP_NHAT_CA_THAT_BAI = new("Cập nhật ca làm thất bại.");

        // ================= MAIL =================
        public static readonly NotifyMessage MAIL_THANH_CONG = new("Gửi mail thành công!");
        public static readonly NotifyMessage MAIL_THAT_BAI = new("Gửi mail thất bại.");
        
        
        //CustomerProfile
        public static readonly NotifyMessage THEM_KHACH_HANG_THANH_CONG = new("Thêm khách hàng thành công.");
        public static readonly NotifyMessage KHACH_HANG_DA_TON_TAI = new("Khách hàng đã tồn tại.");
        public static readonly NotifyMessage XOA_KHACH_HANG_THANH_CONG = new("Xóa khách hàng thành công.");
        public static readonly NotifyMessage KHACH_HANG_KHONG_TON_TAI = new("Khách hàng không tồn tại.");
        public static readonly NotifyMessage SUA_KHACH_HANG_THANH_CONG = new("Sửa khách hàng thành công.");
        public static readonly NotifyMessage SUA_KHACH_HANG_THAT_BAI = new("Sửa khách hàng thất bại.");
    }
}
