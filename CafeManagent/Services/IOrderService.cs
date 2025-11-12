using CafeManagent.dto.Order;
using CafeManagent.Models;
using System.Collections.Generic;

namespace CafeManagent.Services
{
    public interface IOrderService
    {
        List<Order> GetAll();
        List<Order> GetByStatuses(params int[] statuses);
        Order? GetById(int id);
        Order Add(Order order);
        bool Cancel(int id);
        bool SetPreparing(int id);
        bool SetReady(int id);
        bool ConfirmDelivered(int id);
        bool SetRefunded(int id);
        Order CreateOrderFromDraft(OrderDraftDto draft, int? staffId, decimal grandTotal, int? customerId);
    }
}
