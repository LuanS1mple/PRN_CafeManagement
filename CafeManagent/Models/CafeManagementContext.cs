using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CafeManagent.Models;

public partial class CafeManagementContext : DbContext
{
    public CafeManagementContext()
    {
    }

    public CafeManagementContext(DbContextOptions<CafeManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<WorkSchedule> WorkSchedules { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=CafeManagement; Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__20D6A96860B72F62");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.CheckIn).HasColumnName("check_in");
            entity.Property(e => e.CheckOut).HasColumnName("check_out");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalHour)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("total_hour");
            entity.Property(e => e.Workdate).HasColumnName("workdate");
            entity.Property(e => e.WorkshiftId).HasColumnName("workshift_id");

            entity.HasOne(d => d.Shift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Attendanc__shift__571DF1D5");

            entity.HasOne(d => d.Staff).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Attendanc__staff__5812160E");

            entity.HasOne(d => d.Workshift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.WorkshiftId)
                .HasConstraintName("FK__Attendanc__works__59063A47");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__F8D664236D44B39B");

            entity.ToTable("Contract");

            entity.HasIndex(e => e.StaffId, "UQ__Contract__1963DD9DCFD1BF01").IsUnique();

            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.BaseSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("base_salary");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.Position)
                .HasMaxLength(50)
                .HasColumnName("position");
            entity.Property(e => e.SignedDate)
                .HasColumnType("datetime")
                .HasColumnName("signed_date");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Staff).WithOne(p => p.Contract)
                .HasForeignKey<Contract>(d => d.StaffId)
                .HasConstraintName("FK__Contract__staff___59FA5E80");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__CD65CB856CC2B647");

            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.FullName)
                .HasMaxLength(20)
                .HasColumnName("full_name");
            entity.Property(e => e.LoyaltyPoint).HasColumnName("loyaltyPoint");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IngredientId).HasName("PK__Ingredie__B0E453CFBEB2569E");

            entity.ToTable("Ingredient");

            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.CostPerUnit).HasColumnName("cost_per_unit");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IngredientName)
                .HasMaxLength(50)
                .HasColumnName("ingredient_name");
            entity.Property(e => e.QuantityInStock).HasColumnName("quantity_in_stock");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Order__465962291C0FB701");

            entity.ToTable("Order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Discount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("order_price");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.Vat).HasColumnName("VAT");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Order__customer___5AEE82B9");

            entity.HasOne(d => d.Staff).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Order__staff_id__5BE2A6F2");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__OrderIte__52020FDD2E94F2A4");

            entity.ToTable("OrderItem");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(30)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__OrderItem__order__5CD6CB2B");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PK__Payroll__D99FC944980238A1");

            entity.ToTable("Payroll");

            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.Bonus)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("bonus");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.OvertimeHour)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("overtime_hour");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.Penalty)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("penalty");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalHour)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_hour");
            entity.Property(e => e.TotalSalary)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_salary");

            entity.HasOne(d => d.Manager).WithMany(p => p.PayrollManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Payroll__manager__5DCAEF64");

            entity.HasOne(d => d.Staff).WithMany(p => p.PayrollStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Payroll__staff_i__5EBF139D");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__47027DF560BF3BB2");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(255)
                .HasColumnName("product_image");
            entity.Property(e => e.ProductName)
                .HasMaxLength(20)
                .HasColumnName("product_name");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId).HasName("PK__Recipe__3571ED9BE8F047AE");

            entity.ToTable("Recipe");

            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.IngredientId).HasColumnName("ingredient_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.QuantityNeeded).HasColumnName("quantity_needed");

            entity.HasOne(d => d.Ingredient).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.IngredientId)
                .HasConstraintName("FK__Recipe__ingredie__5FB337D6");

            entity.HasOne(d => d.Product).WithMany(p => p.Recipes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__Recipe__product___60A75C0F");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3213E83F50A5E996");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExpireTime)
                .HasColumnType("datetime")
                .HasColumnName("expire_time");
            entity.Property(e => e.IsEnable).HasColumnName("is_enable");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");

            entity.HasOne(d => d.Staff).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__RefreshTo__staff__619B8048");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__Request__779B7C58335135E8");

            entity.ToTable("Request");

            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Detail)
                .HasMaxLength(255)
                .HasColumnName("detail");
            entity.Property(e => e.ReportDate)
                .HasColumnType("datetime")
                .HasColumnName("report_date");
            entity.Property(e => e.ReportType)
                .HasMaxLength(20)
                .HasColumnName("report_type");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.ResolvedDate)
                .HasColumnType("datetime")
                .HasColumnName("resolved_date");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.HasOne(d => d.ResolvedByNavigation).WithMany(p => p.RequestResolvedByNavigations)
                .HasForeignKey(d => d.ResolvedBy)
                .HasConstraintName("FK__Request__resolve__628FA481");

            entity.HasOne(d => d.Staff).WithMany(p => p.RequestStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Request__staff_i__6383C8BA");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__760965CC091DC99F");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(20)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__1963DD9CD415746D");

            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Address)
                .HasMaxLength(20)
                .HasColumnName("address");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreateAt)
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.Email)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(20)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Img)
                .HasMaxLength(50)
                .HasColumnName("img");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .HasColumnName("user_name");

            entity.HasOne(d => d.Role).WithMany(p => p.Staff)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Staff__role_id__6477ECF3");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Task__0492148D9C89C0D1");

            entity.ToTable("Task");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssignTime)
                .HasColumnType("datetime")
                .HasColumnName("assign_time");
            entity.Property(e => e.DueTime)
                .HasColumnType("datetime")
                .HasColumnName("due_time");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TasktypeId).HasColumnName("tasktype_id");

            entity.HasOne(d => d.Manager).WithMany(p => p.TaskManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Task__manager_id__656C112C");

            entity.HasOne(d => d.Staff).WithMany(p => p.TaskStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__Task__staff_id__66603565");

            entity.HasOne(d => d.Tasktype).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.TasktypeId)
                .HasConstraintName("FK__Task__tasktype_i__6754599E");
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.TasktypeId).HasName("PK__TaskType__E7E8CD439D93950A");

            entity.ToTable("TaskType");

            entity.Property(e => e.TasktypeId).HasColumnName("tasktype_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.TaskName)
                .HasMaxLength(100)
                .HasColumnName("task_name");

            entity.HasOne(d => d.Role).WithMany(p => p.TaskTypes)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__TaskType__role_i__68487DD7");
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__WorkSche__7B267220E2AC3F67");

            entity.ToTable("WorkSchedule");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(50)
                .HasColumnName("shift_name");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.WorkshiftId).HasColumnName("workshift_id");

            entity.HasOne(d => d.Manager).WithMany(p => p.WorkScheduleManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__WorkSched__manag__693CA210");

            entity.HasOne(d => d.Staff).WithMany(p => p.WorkScheduleStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__WorkSched__staff__6A30C649");

            entity.HasOne(d => d.Workshift).WithMany(p => p.WorkSchedules)
                .HasForeignKey(d => d.WorkshiftId)
                .HasConstraintName("FK__WorkSched__works__6B24EA82");
        });

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.WorkshiftId).HasName("PK__WorkShif__2E54FD2084470B7C");

            entity.ToTable("WorkShift");

            entity.Property(e => e.WorkshiftId).HasColumnName("workshift_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(50)
                .HasColumnName("shift_name");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
