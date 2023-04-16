using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Mapping;
using Mango.Services.ShoppingCartAPI.RabbitMqSender;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShoppingCartContext") ??
                         throw new InvalidOperationException("Connection string 'ShoppingCartContext' not found.")));

#region mapper
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ICartReporsitory, CartReporsitory>();

builder.Services.AddScoped<IMessageBus, AzureServiceBusMessageBus>();
builder.Services.AddScoped<ICouponReposity, CouponReposity>();
builder.Services.AddSingleton<IRabbitMqCartMessageSender, RabbitMqCartMessageSender>();
#endregion

builder.Services.AddControllers();
builder.Services.AddHttpClient<ICouponReposity, CouponReposity>(x=> x.BaseAddress= new Uri(builder.Configuration["ServiceUrls:CouponAPI"]));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", x =>
{
    x.Authority = "https://localhost:44357/";
    x.Audience = "ShoppingCartAPI";
    x.RequireHttpsMetadata = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        //ValidIssuer = Configuration["Jwt:Issuer"],
        //ValidAudience = Configuration["Jwt:Audience"],
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "mango");
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mango.Services.ShoppingCartAPI", Version = "v1" });
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Enter 'Bearer' [space] and your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type= ReferenceType.SecurityScheme,
                    Id="Bearer"
                },
                Scheme="oauth2",
                Name="Bearer",
                In= ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
