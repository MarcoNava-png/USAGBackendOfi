using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Models.MultiTenant;

namespace WebApplication2.Data.DbContexts;


public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PlanLicencia> PlanesLicencia { get; set; }
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<SuperAdminTenant> SuperAdminTenants { get; set; }
    public DbSet<TenantAuditLog> AuditLogs { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Codigo).IsUnique();
            e.HasIndex(t => t.Subdominio).IsUnique();
            e.HasIndex(t => t.DatabaseName).IsUnique();
            e.HasIndex(t => t.Status);

            e.HasOne(t => t.PlanLicencia)
             .WithMany(p => p.Tenants)
             .HasForeignKey(t => t.IdPlanLicencia)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PlanLicencia>(e =>
        {
            e.HasIndex(p => p.Codigo).IsUnique();
        });

        builder.Entity<SuperAdmin>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
        });

        builder.Entity<SuperAdminTenant>(e =>
        {
            e.HasIndex(sat => new { sat.IdSuperAdmin, sat.IdTenant }).IsUnique();
        });

        builder.Entity<TenantAuditLog>(e =>
        {
            e.HasIndex(l => l.Timestamp);
            e.HasIndex(l => l.IdTenant);
            e.HasIndex(l => l.Accion);
        });

        builder.Entity<Notificacion>(e =>
        {
            e.HasIndex(n => n.FechaCreacion);
            e.HasIndex(n => n.Leida);
            e.HasIndex(n => n.Tipo);
            e.HasIndex(n => n.IdTenant);

            e.HasOne(n => n.Tenant)
             .WithMany()
             .HasForeignKey(n => n.IdTenant)
             .OnDelete(DeleteBehavior.SetNull);
        });

        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        builder.Entity<PlanLicencia>().HasData(
            new PlanLicencia
            {
                IdPlanLicencia = 1,
                Codigo = "BASIC",
                Nombre = "Básico",
                Descripcion = "Plan básico para escuelas pequeñas",
                PrecioMensual = 2500m,
                PrecioAnual = 25000m,
                MaxEstudiantes = 200,
                MaxUsuarios = 5,
                MaxCampus = 1,
                IncluyeSoporte = false,
                IncluyeReportes = false,
                IncluyeAPI = false,
                IncluyeFacturacion = false,
                Activo = true,
                Orden = 1
            },
            new PlanLicencia
            {
                IdPlanLicencia = 2,
                Codigo = "PRO",
                Nombre = "Profesional",
                Descripcion = "Plan profesional para escuelas medianas",
                PrecioMensual = 5000m,
                PrecioAnual = 50000m,
                MaxEstudiantes = 1000,
                MaxUsuarios = 20,
                MaxCampus = 3,
                IncluyeSoporte = true,
                IncluyeReportes = true,
                IncluyeAPI = false,
                IncluyeFacturacion = false,
                Activo = true,
                Orden = 2
            },
            new PlanLicencia
            {
                IdPlanLicencia = 3,
                Codigo = "ENTERPRISE",
                Nombre = "Enterprise",
                Descripcion = "Plan enterprise para universidades y grandes instituciones",
                PrecioMensual = 15000m,
                PrecioAnual = 150000m,
                MaxEstudiantes = 50000,
                MaxUsuarios = 100,
                MaxCampus = 10,
                IncluyeSoporte = true,
                IncluyeReportes = true,
                IncluyeAPI = true,
                IncluyeFacturacion = true,
                Activo = true,
                Orden = 3
            }
        );
    }
}
