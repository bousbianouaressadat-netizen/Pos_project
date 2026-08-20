using System.Text;
using ERP.API.Application.Interfaces;
using ERP.API.Application.Services;
using ERP.API.Infrastructure.Data;
using ERP.API.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- Database ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- JWT Authentication ----
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ---- Authorization بالصلاحيات (Permission-based، وليس Role-based فقط) ----
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder();
builder.Services.AddAuthorization(options =>
{
    // نُنشئ Policy ديناميكيًا لكل صلاحية عبر RequirePermissionAttribute + Handler أعلاه
});
builder.Services.AddOptions();
builder.Services.Configure<AuthorizationOptions>(options => { });

// تسجيل Policies للصلاحيات المعروفة (تُقرأ أيضًا ديناميكيًا عبر الـ Handler)
builder.Services.PostConfigure<AuthorizationOptions>(options =>
{
    var knownPermissions = new[]
    {
        "CanSell", "CanDiscount", "CanChangePrice", "CanDeleteSale", "CanReturn",
        "CanViewCost", "CanViewProfit", "CanModifyStock", "CanCloseCash",
        "CanManageUsers", "CanViewReports"
    };
    // ملاحظة: القائمة أعلاه بالفعل تحتوي CanCloseCash — لا حاجة لتكرارها.

    foreach (var perm in knownPermissions)
    {
        options.AddPolicy($"Permission:{perm}", policy =>
            policy.Requirements.Add(new PermissionRequirement(perm)));
    }
});

// ---- Dependency Injection ----
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICashSessionService, CashSessionService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IPrintingService, PrintingService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    // ⚠️ AllowAnyOrigin مؤقت لبيئة التطوير فقط (Codespaces يعطي رابط فرعي مختلف كل جلسة).
    // قبل النشر الفعلي عند العميل، استبدلها بـ WithOrigins(رابط محدد) للأمان.
    options.AddPolicy("AllowLocalFrontend", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---- تطبيق Migrations + بيانات أولية عند بدء التشغيل ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DataSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
