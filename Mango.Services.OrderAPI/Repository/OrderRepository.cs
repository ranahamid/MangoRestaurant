using AutoMapper;
using Mango.Services.OrderAPI.DbContexts;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContext;
        private IMapper _mapper;
        public OrderRepository(DbContextOptions<ApplicationDbContext> dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
           _mapper = mapper;
        }
        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            await using var _db = new ApplicationDbContext(_dbContext);
            _db.OrderHeaders.Add(orderHeader);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UpdateOrderPaymentStatus(int orderHeaderId, bool paid)
        {
            await using var _db = new ApplicationDbContext(_dbContext);
            var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == orderHeaderId);
            if (orderHeader != null)
            {
                orderHeader.PaymentStatus = paid;
                await _db.SaveChangesAsync();
            }
        }
    }
}
