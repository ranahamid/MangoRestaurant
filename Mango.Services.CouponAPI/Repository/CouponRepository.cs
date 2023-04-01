using AutoMapper;
using Mango.Services.CouponAPI.DbContexts;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Repository
{
    public class CouponRepository: ICouponRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public CouponRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<CouponDto> GetCouponByCode(string couponCode)
        {
            var couponData = await _db.Coupons.FirstOrDefaultAsync(x => x.CouponCode == couponCode);
            if (couponData != null)
            {
                return _mapper.Map<CouponDto>(couponData);
            }
            return new CouponDto();
        }
    }
}
