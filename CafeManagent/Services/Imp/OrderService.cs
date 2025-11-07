using System;
using System.Collections.Generic;
using System.Linq;
using CafeManagent.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Services
{
    public class OrderService : IOrderService
    {
        private readonly CafeManagementContext _db;
        public OrderService(CafeManagementContext db)
        {
            _db = db;
        }

        public List<Order> GetAll()
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetByStatuses(params int[] statuses)
        {
            return _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderItems)
                .Where(o => o.Status != null && statuses.Contains(o.Status.Value))
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetById(int id)
        {
            return _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public Order Add(Order order)
        {
            order.OrderDate = DateTime.Now;
            order.Status = 0; // waiting
            _db.Orders.Add(order);
            _db.SaveChanges();
            return order;
        }

        public bool Cancel(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = -1;
            _db.SaveChanges();
            return true;
        }

        public bool SetPreparing(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 1;
            _db.SaveChanges();
            return true;
        }

        public bool SetReady(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 2;
            _db.SaveChanges();
            return true;
        }

        public bool ConfirmDelivered(int id)
        {
            var o = _db.Orders.Find(id);
            if (o == null) return false;
            o.Status = 3;
            _db.SaveChanges();
            return true;
        }
    }
}
