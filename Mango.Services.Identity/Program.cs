using Microsoft.EntityFrameworkCore;
using Mango.Services.Identity.DbContexts;
using Mango.Services.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Mango.Services.Identity;
using Mango.Services.Identity.Initializer;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductContext") ??
                         throw new InvalidOperationException("Connection string 'ProductContext' not found.")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
    AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

var identityBuilder = builder.Services.AddIdentityServer
    (
    x =>
        {
            x.Events.RaiseErrorEvents = true;
            x.Events.RaiseInformationEvents = true;
            x.Events.RaiseFailureEvents = true;
            x.Events.RaiseSuccessEvents = true;
            x.EmitStaticAudienceClaim = true;
             
        }
    )
    .AddInMemoryIdentityResources(SD.IdentityResources)
    .AddInMemoryApiScopes(SD.ApiScopes)
    .AddInMemoryClients(SD.Clients)
    .AddAspNetIdentity<ApplicationUser>();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

identityBuilder.AddDeveloperSigningCredential();


// Add services to the container.
builder.Services.AddControllersWithViews();
var app = builder.Build();


var serviceProvider = builder.Services.BuildServiceProvider();
var dbInitializer = serviceProvider.GetService<IDbInitializer>();
dbInitializer.Initialize();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
