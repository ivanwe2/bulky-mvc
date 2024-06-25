using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repositories.Abstractions;
using Bulky.Models.Models;
using Bulky.Utility.OrderUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repositories
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public void Update(OrderHeader orderHeader)
        {
            _dbSet.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _dbSet.FirstOrDefault(x => x.Id == id);

            if (orderFromDb != null) 
            {
                orderFromDb.OrderStatus = orderStatus;

                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessiodId, string paymentIntentId)
        {
            var orderFromDb = _dbSet.FirstOrDefault(x => x.Id == id);

            if (!string.IsNullOrEmpty(sessiodId))
            {
                orderFromDb.SessionId = sessiodId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
