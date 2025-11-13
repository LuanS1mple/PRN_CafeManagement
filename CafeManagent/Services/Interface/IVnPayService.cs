using CafeManagent.dto.response;

namespace CafeManagent.Services.Interface
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(string orderId, decimal amount, HttpContext context);
        VnPayResponse ProcessVnPayReturn(IQueryCollection collections);
    }
}
