using System.Collections.Generic;
using CafeManagent.Models;

namespace CafeManagent.Services
{
    public interface IOrderService
    {
        List<Order> GetAll(); // toàn bộ (dùng cho history)
        List<Order> GetByStatuses(params int[] statuses);
        Order? GetById(int id);
        Order Add(Order order);
        bool Cancel(int id);
        bool SetPreparing(int id);
        bool SetReady(int id);
        bool ConfirmDelivered(int id);
    }
}
