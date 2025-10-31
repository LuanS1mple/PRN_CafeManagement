namespace CafeManagent.Enums
{
    public class NotifyMessage
    {
        public string Message { get; }

        private NotifyMessage(string message)
        {
            Message = message;
        }

        public static readonly NotifyMessage HAVE_REQUEST = new("Bạn có request mới cần xử lí");

    }
}
