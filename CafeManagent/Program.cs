using CafeManagent.BackgroundWorkers;
using CafeManagent.dto.Configurations;
using CafeManagent.dto.response;
using CafeManagent.ErrorHandler;
using CafeManagent.Hubs;
using CafeManagent.Middlewares;
using CafeManagent.Models;
using CafeManagent.Services.Imp;
using CafeManagent.Services.Imp.AttendanceModule;
using CafeManagent.Services.Imp.AuthenticationModule;
using CafeManagent.Services.Imp.CustomerModule;
using CafeManagent.Services.Imp.OrderModule;
using CafeManagent.Services.Imp.ProductModule;
using CafeManagent.Services.Imp.RecipeModule;
using CafeManagent.Services.Imp.RequestModule;
using CafeManagent.Services.Imp.StaffModule;
using CafeManagent.Services.Imp.StaffWorkScheduleModule;
using CafeManagent.Services.Imp.TaskModule;
using CafeManagent.Services.Imp.WorkScheduleModule;
using CafeManagent.Services.Imp.WorkShiftModule;
using CafeManagent.Services.Interface;
using CafeManagent.Services.Interface.AttendanceModule;
using CafeManagent.Services.Interface.AuthenticationModule;
using CafeManagent.Services.Interface.CustomerModule;
using CafeManagent.Services.Interface.OrderModule;
using CafeManagent.Services.Interface.ProductModule;
using CafeManagent.Services.Interface.RecipeModule;
using CafeManagent.Services.Interface.RequestModuleDTO;
using CafeManagent.Services.Interface.StaffModule;
using CafeManagent.Services.Interface.StaffWorkScheduleModule;
using CafeManagent.Services.Interface.TaskModule;
using CafeManagent.Services.Interface.WorkScheduleModule;
using CafeManagent.Services.Interface.WorkShiftModule;
using CafeManagent.Ulties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeManagent.BackgroundWorkers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionHandler>();
});
//dùng signalR
builder.Services.AddSignalR();

// Kết nối database
builder.Services.AddDbContext<CafeManagementContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"));
});
//dẫn khi bị 403
builder.Services.AddAuthentication(options =>
{
    options.DefaultForbidScheme = "JwtMiddleware";
})
.AddCookie("JwtMiddleware", options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied"; 
});

builder.Services.AddSession();
//cái này để Hub gọi đc đến httpContext lấy session
builder.Services.AddHttpContextAccessor();
//DI
builder.Services.AddTransient<IStaffService, StaffService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAttendanceService, AttendanceService>();
builder.Services.AddTransient<IWorkScheduleService, WorkScheduleService>();
builder.Services.AddTransient<IRequestService, RequestService>();
builder.Services.AddTransient<IStaffProfileService, StaffProfileService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IWorkScheduleService, WorkScheduleService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IWorkShiftService, WorkShiftService>();
builder.Services.AddTransient<ICustomerProfileService, CustomerProfileService>();
builder.Services.AddTransient<IRecipeService, RecipeService>();
builder.Services.AddTransient<IStaffWorkScheduleService, StaffWorkScheduleService>();

builder.Services.AddTransient<IProductService, ProductService>(); 
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IVnPayService, VnPayService>();
builder.Services.AddSingleton<NotifyUlti>();
builder.Services.AddTransient<IStaffDirectoryService, StaffDirectoryService>();
//Background services
builder.Services.AddHostedService<WorkScheduleBackgroundWorker>();
builder.Services.AddHostedService<CleanTokenBackgroundWorker>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskReportService, TaskReportService>();

builder.Services.Configure<VnPayConfig>(
    builder.Configuration.GetSection("VNPay"));

builder.Services.AddRazorPages(o =>
{
    o.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
});
// workder service
builder.Services.AddHostedService<TaskStatusBackgroundWorker>();

var app = builder.Build();
app.UseGlobalExceptionMiddleware();
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

app.UseMiddleware<AuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

//bất cứ request nào cũng đi qua để thnog báo
app.UseMiddleware<NotifyMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ResponseHub>("/response");
app.MapHub<NotifyHub>("/notify");
app.MapHub<WorkShiftHub>("/workshifthub");

app.MapHub<OrderHub>("/orderHub");
app.MapHub<StaffHub>("/hubs/staff");
app.MapHub<TaskHub>("/taskhub");

app.Run();


