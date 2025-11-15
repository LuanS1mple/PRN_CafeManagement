namespace CafeManagent.dto.request.CustomerModuleDTO
{
    public class UpdateCustomerProfileDTO
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int LoyaltyPoint { get; set; }
    }
}
