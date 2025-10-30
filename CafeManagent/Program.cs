using CafeManagent.Hubs;
using CafeManagent.Models;
using CafeManagent.Services;
using CafeManagent.Services.Imp;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//dùng signalR
builder.Services.AddSignalR();

// Kết nối database
builder.Services.AddDbContext<CafeManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

builder.Services.AddSession();
//DI
builder.Services.AddTransient<IStaffService, StaffService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAttendanceService, AttendanceService>();
builder.Services.AddTransient<IRequestService, RequestService>();

builder.Services.AddTransient<IStaffProfileService, StaffProfileService>();
//builder.Services.AddSingleton<CafeManagementContext, CafeManagementContext>();


builder.Services.AddRazorPages(o =>
{
    o.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
});


var app = builder.Build();

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
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ResponseHub>("/response");
app.Run();
