using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Migrations;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Models.dtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CouponReposity : ICouponReposity
    {
        private readonly HttpClient _client; 
        
        private IMapper _mapper;

        public CouponReposity(HttpClient client, IMapper mapper)
        {
            _client = client;
           _mapper = mapper;
        }

        public async Task<CouponDto> GetCoupon(string couponName)
        {
            var response = await _client.GetAsync($"/api/CouponAPI/{couponName}");
            var apiContent= await response.Content.ReadAsStringAsync();
            var resp= JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if(resp!=null && resp.IsSuccess)
            {
                var result = Convert.ToString(resp.Result);
                return JsonConvert.DeserializeObject<CouponDto>(result);
            }
            return new CouponDto();
        }
    }
}
