using AutoMapper;
using Mango.Services.Email.DbContexts;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.Email.Repository
{
    public class EmailRepository : IEmailRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContext;
        private IMapper _mapper;
        public EmailRepository(DbContextOptions<ApplicationDbContext> dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
           _mapper = mapper;
        }

        public async Task SendAndLogEmail(UpdatePaymentResultMessage message)
        {
            //implement an email sender

            //email log
            EmailLog log = new EmailLog
            {
                Email = message.Email,
                EmailSent= DateTime.UtcNow,
                Log= $"Order- {message.OrderId} has been created successfully."
            };

            await using var _db = new ApplicationDbContext(_dbContext);
            _db.EmailLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
