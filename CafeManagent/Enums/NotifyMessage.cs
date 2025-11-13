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
    }
}
