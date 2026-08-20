using ERP.API.Domain.Entities;
using ERP.API.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Infrastructure.Data;

public static class DataSeeder
{
    private static readonly (string Code, string AR, string FR, string Category)[] DefaultPermissions =
    {
        ("CanSell", "يمكنه البيع", "Peut vendre", "Sales"),
        ("CanDiscount", "يمكنه الخصم", "Peut appliquer une remise", "Sales"),
        ("CanChangePrice", "يمكنه تغيير السعر", "Peut modifier le prix", "Sales"),
        ("CanDeleteSale", "يمكنه حذف بيع", "Peut supprimer une vente", "Sales"),
        ("CanReturn", "يمكنه الاسترجاع", "Peut effectuer un retour", "Sales"),
        ("CanViewCost", "يمكنه رؤية التكلفة", "Peut voir le coût", "Reports"),
        ("CanViewProfit", "يمكنه رؤية الربح", "Peut voir le profit", "Reports"),
        ("CanModifyStock", "يمكنه تعديل المخزون", "Peut modifier le stock", "Stock"),
        ("CanCloseCash", "يمكنه إغلاق الصندوق", "Peut clôturer la caisse", "Cash"),
        ("CanManageUsers", "يمكنه إدارة المستخدمين", "Peut gérer les utilisateurs", "Users"),
        ("CanViewReports", "يمكنه رؤية التقارير", "Peut voir les rapports", "Reports"),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        // 1) الصلاحيات الثابتة
        foreach (var p in DefaultPermissions)
        {
            if (!await db.Permissions.AnyAsync(x => x.Code == p.Code))
            {
                db.Permissions.Add(new Permission
                {
                    Code = p.Code,
                    DescriptionAR = p.AR,
                    DescriptionFR = p.FR,
                    Category = p.Category
                });
            }
        }
        await db.SaveChangesAsync();

        // 2) مؤسسة تجريبية أولى (إن لم توجد) + دور Administrator + مستخدم Admin افتراضي
        if (!await db.Companies.AnyAsync())
        {
            var company = new Company { Name = "المؤسسة الافتراضية", ActivityType = "General" };
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            var allPermissions = await db.Permissions.ToListAsync();

            var adminRole = new Role { CompanyID = company.CompanyID, Name = "Administrator", IsSystemRole = true };
            foreach (var perm in allPermissions)
                adminRole.RolePermissions.Add(new RolePermission { PermissionID = perm.PermissionID, Role = adminRole });

            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();

            var adminUser = new User
            {
                CompanyID = company.CompanyID,
                Username = "admin",
                FullName = "مدير النظام",
                PasswordHash = PasswordHasher.Hash("ChangeMe123!"), // ⚠️ غيّرها فورًا بعد أول تشغيل
                IsActive = true
            };
            adminUser.UserRoles.Add(new UserRole { RoleID = adminRole.RoleID, User = adminUser });

            db.Users.Add(adminUser);
            await db.SaveChangesAsync();

            db.Warehouses.Add(new Warehouse { CompanyID = company.CompanyID, Name = "المستودع الرئيسي", IsDefault = true });
            await db.SaveChangesAsync();
        }
    }
}
