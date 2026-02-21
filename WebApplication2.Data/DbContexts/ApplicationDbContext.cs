using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Models;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using WebApplication2.Core.Enums;

namespace WebApplication2.Data.DbContexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string, IdentityUserClaim<string>,
        IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApplicationDbContext> _logger;
 
        public ApplicationDbContext(IHttpContextAccessor httpContextAccessor, ILogger<ApplicationDbContext> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, ILogger<ApplicationDbContext> logger) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool forProvisioning) : base(options)
        {
            _httpContextAccessor = null!;
            _logger = null!;
        }

        public virtual DbSet<Estado> Estados { get; set; }
        public virtual DbSet<Municipio> Municipios { get; set; }
        public virtual DbSet<CodigoPostal> CodigosPostales { get; set; }
        public virtual DbSet<Aspirante> Aspirante { get; set; }
        public virtual DbSet<AspiranteConvenio> AspiranteConvenio { get; set; }
        public virtual DbSet<AspiranteEstatus> AspiranteEstatus { get; set; }
        public virtual DbSet<Campus> Campus { get; set; }
        public virtual DbSet<Convenio> Convenio { get; set; }
        public virtual DbSet<ConvenioAlcance> ConvenioAlcance { get; set; }
        public virtual DbSet<DiaSemana> DiaSemana { get; set; }
        public virtual DbSet<Direccion> Direccion { get; set; }
        public virtual DbSet<EstadoCivil> EstadoCivil { get; set; }
        public virtual DbSet<Estudiante> Estudiante { get; set; }
        public virtual DbSet<EstudiantePlan> EstudiantePlan { get; set; }
        public virtual DbSet<Genero> Genero { get; set; }
        public virtual DbSet<Grupo> Grupo { get; set; }
        public virtual DbSet<GrupoMateria> GrupoMateria { get; set; }
        public virtual DbSet<Horario> Horario { get; set; }
        public virtual DbSet<Inscripcion> Inscripcion { get; set; }
        public virtual DbSet<Materia> Materia { get; set; }
        public virtual DbSet<MateriaPlan> MateriaPlan { get; set; }
        public virtual DbSet<MedioContacto> MedioContacto { get; set; }
        public virtual DbSet<NivelEducativo> NivelEducativo { get; set; }
        public virtual DbSet<Periodicidad> Periodicidad { get; set; }
        public virtual DbSet<PeriodoAcademico> PeriodoAcademico { get; set; }
        public virtual DbSet<Persona> Persona { get; set; }
        public virtual DbSet<PlanEstudios> PlanEstudios { get; set; }
        public virtual DbSet<PlanModalidadDia> PlanModalidadDia { get; set; }
        public virtual DbSet<Profesor> Profesor { get; set; }
        public virtual DbSet<Turno> Turno { get; set; }
        public virtual DbSet<AspiranteBitacoraSeguimiento> AspiranteBitacoraSeguimiento { get; set; }
        public virtual DbSet<Parciales> Parciales { get; set; }
        public virtual DbSet<CalificacionParcial> CalificacionesParciales { get; set; }
        public virtual DbSet<CalificacionDetalle> CalificacionDetalle { get; set; }
        public virtual DbSet<Asistencia> Asistencia { get; set; }
        public virtual DbSet<PlanPago> PlanPago { get; set; }
        public virtual DbSet<PlanPagoDetalle> PlanPagoDetalle { get; set; }
        public virtual DbSet<ConceptoPago> ConceptoPago { get; set; }
        public virtual DbSet<ConceptoPrecio> ConceptoPrecio { get; set; }
        public virtual DbSet<LigaPago>  LigaPago { get; set; }
        public virtual DbSet<PagoAplicacion> PagoAplicacion { get; set; }
        public virtual DbSet<Pago> Pago { get; set; }
        public virtual DbSet<Recibo> Recibo { get; set; }   
        public virtual DbSet<ReciboDetalle> ReciboDetalle { get; set; }
        public virtual DbSet<PlanPagoAsignacion> PlanPagoAsignacion { get; set; }
        public virtual DbSet<RecargoPolitica> RecargoPolitica { get; set; }
        public virtual DbSet<BitacoraRecibo> BitacoraRecibo { get; set; }
        public virtual DbSet<MedioPago> MedioPago { get; set; }
        public virtual DbSet<DocumentoRequisito> DocumentoRequisito { get; set; }
        public virtual DbSet<AspiranteDocumento> AspiranteDocumento { get; set; }
        public virtual DbSet<Beca> Beca { get; set; }
        public virtual DbSet<BecaAsignacion> BecaAsignacion { get; set; }

        public virtual DbSet<PlantillaCobro> PlantillasCobro { get; set; }
        public virtual DbSet<PlantillaCobroDetalle> PlantillasCobroDetalles { get; set; }

        public virtual DbSet<CorteCaja> CorteCaja { get; set; }

        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }

        public virtual DbSet<TipoDocumentoEstudiante> TiposDocumentoEstudiante { get; set; }
        public virtual DbSet<SolicitudDocumento> SolicitudesDocumento { get; set; }

        public virtual DbSet<EstudianteGrupo> EstudianteGrupo { get; set; }

        public virtual DbSet<BitacoraAccion> BitacoraAcciones { get; set; }
        public virtual DbSet<NotificacionUsuario> NotificacionesUsuario { get; set; }

        public virtual DbSet<PlanDocumentoRequisito> PlanDocumentoRequisito { get; set; }

        public virtual DbSet<Modalidad> Modalidad { get; set; }
        public virtual DbSet<ModalidadPlan> ModalidadPlan { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BitacoraRecibo>().HasKey(x => x.IdBitacora);
            modelBuilder.Entity<Recibo>().HasKey(x => x.IdRecibo);
            modelBuilder.Entity<ReciboDetalle>().HasKey(x => x.IdReciboDetalle);
            modelBuilder.Entity<LigaPago>().HasKey(x => x.IdLigaPago);
            modelBuilder.Entity<Pago>().HasKey(x => x.IdPago);
            modelBuilder.Entity<PagoAplicacion>().HasKey(x => x.IdPagoAplicacion);
            modelBuilder.Entity<PlanPago>().HasKey(x => x.IdPlanPago);
            modelBuilder.Entity<PlanPago>()
                .HasOne(x => x.IdModalidadPlanNavigation)
                .WithMany(m => m.PlanPago)
                .HasForeignKey(x => x.IdModalidadPlan)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PlanPago_ModalidadPlan");
            modelBuilder.Entity<PlanPagoDetalle>().HasKey(x => x.IdPlanPagoDetalle);
            modelBuilder.Entity<PlanPagoAsignacion>().HasKey(x => x.IdPlanPagoAsignacion);
            modelBuilder.Entity<ConceptoPago>().HasKey(x => x.IdConceptoPago);
            modelBuilder.Entity<ConceptoPrecio>().HasKey(x => x.IdConceptoPrecio);
            modelBuilder.Entity<RecargoPolitica>().HasKey(x => x.IdRecargoPolitica);
            modelBuilder.Entity<MedioPago>().HasKey(x => x.IdMedioPago);
            modelBuilder.Entity<DocumentoRequisito>().HasKey(x => x.IdDocumentoRequisito);
            modelBuilder.Entity<AspiranteDocumento>().HasKey(x => x.IdAspiranteDocumento);
            modelBuilder.Entity<Beca>().HasKey(x => x.IdBeca);
            modelBuilder.Entity<BecaAsignacion>().HasKey(x => x.IdBecaAsignacion);
            modelBuilder.Entity<PlantillaCobro>().HasKey(x => x.IdPlantillaCobro);
            modelBuilder.Entity<PlantillaCobroDetalle>().HasKey(x => x.IdPlantillaDetalle);
            //modelBuilder.Entity<ConceptoPago>().Property(p => p.AplicaA).HasConversion<int>();
            //modelBuilder.Entity<ConceptoPago>().Property(p => p.Tipo).HasConversion<int>();


            modelBuilder.Entity<Municipio>()
                .HasOne(m => m.Estado)
                .WithMany(e => e.Municipios)
                .HasForeignKey(m => m.EstadoId)
                .IsRequired();

            modelBuilder.Entity<Municipio>()
                .HasIndex(m => m.EstadoId);

            modelBuilder.Entity<CodigoPostal>()
                .HasOne(cp => cp.Municipio)
                .WithMany(m => m.CodigosPostales)
                .HasForeignKey(cp => cp.MunicipioId)
                .IsRequired();

            modelBuilder.Entity<CodigoPostal>()
                .HasIndex(cp => cp.MunicipioId);

            modelBuilder.Entity<Aspirante>(entity =>
            {
                entity.HasKey(e => e.IdAspirante).HasName("PK__Aspirant__09EE6349C82C95C4");

                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");
                entity.Property(e => e.Observaciones).HasMaxLength(250);

                entity.Property(e => e.InstitucionProcedencia).HasMaxLength(200);
                entity.Property(e => e.NombreEmpresa).HasMaxLength(200);
                entity.Property(e => e.DomicilioEmpresa).HasMaxLength(300);
                entity.Property(e => e.PuestoEmpresa).HasMaxLength(100);
                entity.Property(e => e.QuienCubreGastos).HasMaxLength(200);

                entity.HasOne(d => d.IdAspiranteEstatusNavigation).WithMany(p => p.Aspirante)
                    .HasForeignKey(d => d.IdAspiranteEstatus)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Aspirante_Estatus");

                entity.HasOne(d => d.IdMedioContactoNavigation).WithMany(p => p.Aspirante)
                    .HasForeignKey(d => d.IdMedioContacto)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Aspirante_Medio");

                entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Aspirante)
                    .HasForeignKey(d => d.IdPersona)
                    .HasConstraintName("FK_Aspirante_Persona");

                entity.HasOne(d => d.IdPlanNavigation).WithMany(p => p.Aspirante)
                    .HasForeignKey(d => d.IdPlan)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Aspirante_Plan");

                entity.HasOne(d => d.IdModalidadNavigation).WithMany(p => p.Aspirante)
                    .HasForeignKey(d => d.IdModalidad)
                    .HasConstraintName("FK_Aspirante_Modalidad");

                entity.HasOne(d => d.IdPeriodoAcademicoNavigation).WithMany()
                    .HasForeignKey(d => d.IdPeriodoAcademico)
                    .HasConstraintName("FK_Aspirante_PeriodoAcademico");
            });

            modelBuilder.Entity<AspiranteConvenio>(entity =>
            {
                entity.HasKey(e => e.IdAspiranteConvenio).HasName("PK__Aspirant__F372F05FD82F39BA");

                entity.HasIndex(e => new { e.IdAspirante, e.IdConvenio }, "UQ_Aspirante_Convenio").IsUnique();

                entity.Property(e => e.Estatus)
                    .HasMaxLength(20)
                    .HasDefaultValue("Pendiente");
                entity.Property(e => e.Evidencia).HasMaxLength(200);
                entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("(sysutcdatetime())");

                entity.HasOne(d => d.IdAspiranteNavigation).WithMany(p => p.AspiranteConvenio)
                    .HasForeignKey(d => d.IdAspirante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AspConv_Aspirante");

                entity.HasOne(d => d.IdConvenioNavigation).WithMany(p => p.AspiranteConvenio)
                    .HasForeignKey(d => d.IdConvenio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AspConv_Convenio");
            });

            modelBuilder.Entity<AspiranteEstatus>(entity =>
            {
                entity.HasKey(e => e.IdAspiranteEstatus).HasName("PK__Aspirant__7B8DBE92EB8DEDF7");

                entity.HasIndex(e => e.DescEstatus, "UQ_AspiranteEstatus").IsUnique();

                entity.Property(e => e.DescEstatus).HasMaxLength(50);
            });

            modelBuilder.Entity<Campus>(entity =>
            {
                entity.HasKey(e => e.IdCampus).HasName("PK__Campus__DA49C2DE1E9DB12C");

                entity.HasIndex(e => e.ClaveCampus, "UQ_Campus_Clave").IsUnique();

                entity.HasIndex(e => e.Nombre, "UQ_Campus_Nombre").IsUnique();

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.ClaveCampus).HasMaxLength(20);
                entity.Property(e => e.Nombre).HasMaxLength(120);

                entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.Campus)
                    .HasForeignKey(d => d.IdDireccion)
                    .HasConstraintName("FK_Campus_Direccion");
            });

            modelBuilder.Entity<Convenio>(entity =>
            {
                entity.HasKey(e => e.IdConvenio).HasName("PK__Convenio__51CFFF2B890D4417");

                entity.HasIndex(e => e.ClaveConvenio, "UQ__Convenio__A6197EE9ADAF6A0B").IsUnique();

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.ClaveConvenio).HasMaxLength(30);
                entity.Property(e => e.DescuentoPct).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Monto).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.Nombre).HasMaxLength(120);
                entity.Property(e => e.TipoBeneficio).HasMaxLength(20);
            });

            modelBuilder.Entity<ConvenioAlcance>(entity =>
            {
                entity.HasKey(e => e.IdConvenioAlcance).HasName("PK__Convenio__2A4E02C0B88720E9");

                entity.HasOne(d => d.IdCampusNavigation).WithMany(p => p.ConvenioAlcance)
                    .HasForeignKey(d => d.IdCampus)
                    .HasConstraintName("FK_ConvAlc_Campus");

                entity.HasOne(d => d.IdConvenioNavigation).WithMany(p => p.ConvenioAlcance)
                    .HasForeignKey(d => d.IdConvenio)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ConvAlc_Convenio");

                entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany(p => p.ConvenioAlcance)
                    .HasForeignKey(d => d.IdPlanEstudios)
                    .HasConstraintName("FK_ConvAlc_Plan");
            });

            modelBuilder.Entity<DiaSemana>(entity =>
            {
                entity.HasKey(e => e.IdDiaSemana).HasName("PK__DiaSeman__7A209B4EBAF307F1");

                entity.HasIndex(e => e.Nombre, "UQ__DiaSeman__75E3EFCF34622C9D").IsUnique();

                entity.Property(e => e.Nombre).HasMaxLength(20);
            });

            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.HasKey(e => e.IdDireccion).HasName("PK__Direccio__1F8E0C76513A158C");

                entity.Property(e => e.Calle).HasMaxLength(100);
                entity.Property(e => e.NumeroExterior).HasMaxLength(10);
                entity.Property(e => e.NumeroInterior).HasMaxLength(10);
            });

            modelBuilder.Entity<EstadoCivil>(entity =>
            {
                entity.HasKey(e => e.IdEstadoCivil).HasName("PK__EstadoCi__889DE1B24D585C92");

                entity.HasIndex(e => e.DescEstadoCivil, "UQ_EstadoCivil").IsUnique();

                entity.Property(e => e.DescEstadoCivil).HasMaxLength(30);
            });

            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.HasKey(e => e.IdEstudiante).HasName("PK__Estudian__B5007C24138D11BB");

                entity.HasIndex(e => e.Matricula, "UQ__Estudian__0FB9FB4F890AE66D").IsUnique();

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.Email).HasMaxLength(120);
                entity.Property(e => e.FechaIngreso).HasDefaultValueSql("(CONVERT([date],getdate()))");
                entity.Property(e => e.Matricula).HasMaxLength(30);

                entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Estudiante)
                    .HasForeignKey(d => d.IdPersona)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Estudiante_Persona");

                entity.HasOne(d => d.IdPlanActualNavigation).WithMany(p => p.Estudiante)
                    .HasForeignKey(d => d.IdPlanActual)
                    .HasConstraintName("FK_Estudiante_Plan");
            });

            modelBuilder.Entity<EstudiantePlan>(entity =>
            {
                entity.HasKey(e => e.IdEstudiantePlan).HasName("PK__Estudian__1CF83B276A8D49B7");

                entity.HasIndex(e => new { e.IdEstudiante, e.IdPlanEstudios, e.FechaInicio }, "UQ_EstudiantePlan").IsUnique();

                entity.Property(e => e.FechaInicio).HasDefaultValueSql("(CONVERT([date],getdate()))");

                entity.HasOne(d => d.IdEstudianteNavigation).WithMany(p => p.EstudiantePlan)
                    .HasForeignKey(d => d.IdEstudiante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EstudiantePlan_Estudiante");

                entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany(p => p.EstudiantePlan)
                    .HasForeignKey(d => d.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EstudiantePlan_Plan");
            });

            modelBuilder.Entity<Genero>(entity =>
            {
                entity.HasKey(e => e.IdGenero).HasName("PK__Genero__0F8349880F3BC981");

                entity.HasIndex(e => e.DescGenero, "UQ_Genero").IsUnique();

                entity.Property(e => e.DescGenero).HasMaxLength(30);
            });

            modelBuilder.Entity<Grupo>(entity =>
            {
                entity.HasKey(e => e.IdGrupo).HasName("PK__Grupo__303F6FD92351A792");

                entity.HasIndex(e => new { e.IdPlanEstudios, e.IdPeriodoAcademico, e.NumeroCuatrimestre, e.NumeroGrupo, e.IdTurno }, "UQ_Grupo_Num").IsUnique();

                entity.Property(e => e.CapacidadMaxima).HasDefaultValue((short)40);

                entity.HasOne(d => d.IdPeriodoAcademicoNavigation).WithMany(p => p.Grupo)
                    .HasForeignKey(d => d.IdPeriodoAcademico)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Grupo_Periodo");

                entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany(p => p.Grupo)
                    .HasForeignKey(d => d.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Grupo_Plan");

                entity.HasOne(d => d.IdTurnoNavigation).WithMany(p => p.Grupo)
                    .HasForeignKey(d => d.IdTurno)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Grupo_Turno");
            });

            modelBuilder.Entity<GrupoMateria>(entity =>
            {
                entity.HasKey(e => e.IdGrupoMateria).HasName("PK__GrupoMat__9D026FCD2F0EA6B3");

                entity.HasIndex(e => new { e.IdGrupo, e.IdMateriaPlan }, "UQ_GrupoMateria").IsUnique();

                entity.Property(e => e.Aula).HasMaxLength(50);
                entity.Property(e => e.Cupo).HasDefaultValue((short)40);

                entity.HasOne(d => d.IdGrupoNavigation).WithMany(p => p.GrupoMateria)
                    .HasForeignKey(d => d.IdGrupo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GrupoMateria_Grupo");

                entity.HasOne(d => d.IdMateriaPlanNavigation).WithMany(p => p.GrupoMateria)
                    .HasForeignKey(d => d.IdMateriaPlan)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GrupoMateria_MatPlan");

                entity.HasOne(d => d.IdProfesorNavigation).WithMany(p => p.GrupoMateria)
                    .HasForeignKey(d => d.IdProfesor)
                    .HasConstraintName("FK_GrupoMateria_Profesor");
            });

            modelBuilder.Entity<Horario>(entity =>
            {
                entity.HasKey(e => e.IdHorario).HasName("PK__Horario__1539229BCC12B082");

                entity.HasIndex(e => new { e.IdGrupoMateria, e.IdDiaSemana, e.HoraInicio, e.HoraFin }, "UQ_Horario").IsUnique();

                entity.Property(e => e.Aula).HasMaxLength(50);
                entity.Property(e => e.HoraFin).HasPrecision(0);
                entity.Property(e => e.HoraInicio).HasPrecision(0);

                entity.HasOne(d => d.IdDiaSemanaNavigation).WithMany(p => p.Horario)
                    .HasForeignKey(d => d.IdDiaSemana)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Horario_Dia");

                entity.HasOne(d => d.IdGrupoMateriaNavigation).WithMany(p => p.Horario)
                    .HasForeignKey(d => d.IdGrupoMateria)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Horario_GrupoMat");
            });

            modelBuilder.Entity<Inscripcion>(entity =>
            {
                entity.HasKey(e => e.IdInscripcion).HasName("PK__Inscripc__A122F2BF81A0DA45");

                entity.HasIndex(e => new { e.IdEstudiante, e.IdGrupoMateria }, "UQ_Inscripcion").IsUnique();

                entity.Property(e => e.CalificacionFinal).HasColumnType("decimal(4, 1)");
                entity.Property(e => e.Estado)
                    .HasMaxLength(20)
                    .HasDefaultValue("Inscrito");
                entity.Property(e => e.FechaInscripcion).HasDefaultValueSql("(sysutcdatetime())");

                entity.HasOne(d => d.IdEstudianteNavigation).WithMany(p => p.Inscripcion)
                    .HasForeignKey(d => d.IdEstudiante)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inscripcion_Estudiante");

                entity.HasOne(d => d.IdGrupoMateriaNavigation).WithMany(p => p.Inscripcion)
                    .HasForeignKey(d => d.IdGrupoMateria)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inscripcion_GrupoMateria");
            });

            modelBuilder.Entity<Materia>(entity =>
            {
                entity.HasKey(e => e.IdMateria).HasName("PK__Materia__EC17467041102790");

                entity.HasIndex(e => e.Clave, "UQ__Materia__E8181E1169244C5A").IsUnique();

                entity.Property(e => e.Activa).HasDefaultValue(true);
                entity.Property(e => e.Clave).HasMaxLength(30);
                entity.Property(e => e.Creditos).HasColumnType("decimal(4, 1)");
                entity.Property(e => e.Nombre).HasMaxLength(150);
            });

            modelBuilder.Entity<MateriaPlan>(entity =>
            {
                entity.HasKey(e => e.IdMateriaPlan).HasName("PK__MateriaP__216FB17FE2CA7B4E");

                entity.HasIndex(e => new { e.IdPlanEstudios, e.IdMateria }, "UQ_MateriaPlan").IsUnique();

                entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.MateriaPlan)
                    .HasForeignKey(d => d.IdMateria)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MateriaPlan_Materia");

                entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany(p => p.MateriaPlan)
                    .HasForeignKey(d => d.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MateriaPlan_Plan");
            });

            modelBuilder.Entity<MedioContacto>(entity =>
            {
                entity.HasKey(e => e.IdMedioContacto).HasName("PK__MedioCon__3E86CE3C31C937DB");

                entity.Property(e => e.DescMedio).HasMaxLength(50);
            });

            modelBuilder.Entity<NivelEducativo>(entity =>
            {
                entity.HasKey(e => e.IdNivelEducativo).HasName("PK__NivelEdu__5035CA164D3A42BB");

                entity.HasIndex(e => e.DescNivelEducativo, "UQ_NivelEducativo").IsUnique();

                entity.Property(e => e.DescNivelEducativo).HasMaxLength(50);
            });

            modelBuilder.Entity<Periodicidad>(entity =>
            {
                entity.HasKey(e => e.IdPeriodicidad).HasName("PK__Periodic__DA476CCD8B84E741");

                entity.HasIndex(e => e.DescPeriodicidad, "UQ_Periodicidad").IsUnique();

                entity.Property(e => e.DescPeriodicidad).HasMaxLength(30);
            });

            modelBuilder.Entity<PeriodoAcademico>(entity =>
            {
                entity.HasKey(e => e.IdPeriodoAcademico).HasName("PK__PeriodoA__E57AB387D551DE0A");

                entity.HasIndex(e => e.Clave, "UQ__PeriodoA__E8181E117466779A").IsUnique();

                entity.Property(e => e.Clave).HasMaxLength(30);
                entity.Property(e => e.Nombre).HasMaxLength(100);

                entity.HasOne(d => d.IdPeriodicidadNavigation).WithMany(p => p.PeriodoAcademico)
                    .HasForeignKey(d => d.IdPeriodicidad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Periodo_Periodicidad");
            });

            modelBuilder.Entity<Persona>(entity =>
            {
                entity.HasKey(e => e.IdPersona).HasName("PK__Persona__2EC8D2AC48F4B00B");

                entity.HasIndex(e => e.Curp, "UQ_Persona_CURP").IsUnique();

                entity.HasIndex(e => e.Correo, "UQ_Persona_Email").IsUnique();

                entity.HasIndex(e => e.Rfc, "UQ_Persona_RFC").IsUnique();

                entity.Property(e => e.ApellidoMaterno).HasMaxLength(50);
                entity.Property(e => e.ApellidoPaterno).HasMaxLength(50);
                entity.Property(e => e.Celular).HasMaxLength(20);
                entity.Property(e => e.Correo).HasMaxLength(100);
                entity.Property(e => e.Curp).HasMaxLength(50);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Rfc).HasMaxLength(20);
                entity.Property(e => e.Telefono).HasMaxLength(20);

                // Contacto de emergencia
                entity.Property(e => e.NombreContactoEmergencia).HasMaxLength(150);
                entity.Property(e => e.TelefonoContactoEmergencia).HasMaxLength(20);
                entity.Property(e => e.ParentescoContactoEmergencia).HasMaxLength(50);

                entity.Property(e => e.Nacionalidad).HasMaxLength(100);

                entity.HasOne(d => d.IdDireccionNavigation).WithMany(p => p.Persona)
                    .HasForeignKey(d => d.IdDireccion)
                    .HasConstraintName("FK_Persona_Direccion");

                entity.HasOne(d => d.IdEstadoCivilNavigation).WithMany(p => p.Persona)
                    .HasForeignKey(d => d.IdEstadoCivil)
                    .HasConstraintName("FK_Persona_EstadoCivil");

                entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Persona)
                    .HasForeignKey(d => d.IdGenero)
                    .HasConstraintName("FK_Persona_Genero");

                entity.HasIndex(p => p.Nombre);
                entity.HasIndex(p => p.ApellidoPaterno);
                entity.HasIndex(p => p.ApellidoMaterno);
                entity.HasIndex(p => p.Curp);
            });

            modelBuilder.Entity<PlanEstudios>(entity =>
            {
                entity.HasKey(e => e.IdPlanEstudios).HasName("PK__PlanEstu__C60618471021EFD8");

                entity.HasIndex(e => new { e.ClavePlanEstudios, e.IdCampus }, "UQ_PlanEstudios_Campus").IsUnique();

                entity.Property(e => e.ClavePlanEstudios).HasMaxLength(100);
                entity.Property(e => e.DuracionMeses).HasDefaultValue(48);
                entity.Property(e => e.MinimaAprobatoriaFinal).HasDefaultValue(70);
                entity.Property(e => e.MinimaAprobatoriaParcial).HasDefaultValue(60);
                entity.Property(e => e.NombrePlanEstudios).HasMaxLength(100);
                entity.Property(e => e.PermiteAdelantar).HasDefaultValue(false);
                entity.Property(e => e.RVOE).HasMaxLength(50);
                entity.Property(e => e.Version).HasMaxLength(10);

                entity.HasOne(d => d.IdCampusNavigation).WithMany(p => p.PlanEstudios)
                    .HasForeignKey(d => d.IdCampus)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Plan_Campus");

                entity.HasOne(d => d.IdNivelEducativoNavigation).WithMany(p => p.PlanEstudios)
                    .HasForeignKey(d => d.IdNivelEducativo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Plan_NivelEducativo");

                entity.HasOne(d => d.IdPeriodicidadNavigation).WithMany(p => p.PlanEstudios)
                    .HasForeignKey(d => d.IdPeriodicidad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Plan_Periodicidad");
            });

            modelBuilder.Entity<Profesor>(entity =>
            {
                entity.HasKey(e => e.IdProfesor).HasName("PK__Profesor__C377C3A119E36880");

                entity.HasIndex(e => e.NoEmpleado, "UQ__Profesor__82F7575B30F93488").IsUnique();

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.EmailInstitucional).HasMaxLength(120);
                entity.Property(e => e.NoEmpleado).HasMaxLength(30);

                entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.Profesor)
                    .HasForeignKey(d => d.IdPersona)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Profesor_Persona");
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.HasKey(e => e.IdTurno).HasName("PK__Turno__C1ECF79ACE66190F");

                entity.HasIndex(e => e.Clave, "UQ__Turno__E8181E11A9929118").IsUnique();

                entity.Property(e => e.Clave).HasMaxLength(20);
                entity.Property(e => e.Nombre).HasMaxLength(50);
            });

            modelBuilder.Entity<Modalidad>(entity =>
            {
                entity.HasKey(e => e.IdModalidad).HasName("PK_Modalidad");

                entity.HasIndex(e => e.DescModalidad, "UQ_Modalidad").IsUnique();

                entity.Property(e => e.DescModalidad).HasMaxLength(50);
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<ModalidadPlan>(entity =>
            {
                entity.HasKey(e => e.IdModalidadPlan).HasName("PK_ModalidadPlan");

                entity.HasIndex(e => e.DescModalidadPlan, "UQ_ModalidadPlan").IsUnique();

                entity.Property(e => e.DescModalidadPlan).HasMaxLength(50);
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<PlanModalidadDia>(entity =>
            {
                entity.HasKey(e => e.IdPlanModalidadDia);

                entity.HasIndex(e => new { e.IdPlanEstudios, e.IdModalidad, e.IdDiaSemana }, "UQ_PlanModalidadDia").IsUnique();

                entity.HasOne(d => d.IdPlanEstudiosNavigation).WithMany()
                    .HasForeignKey(d => d.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanModalidadDia_Plan");

                entity.HasOne(d => d.IdModalidadNavigation).WithMany()
                    .HasForeignKey(d => d.IdModalidad)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanModalidadDia_Modalidad");

                entity.HasOne(d => d.IdDiaSemanaNavigation).WithMany()
                    .HasForeignKey(d => d.IdDiaSemana)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanModalidadDia_DiaSemana");
            });

            modelBuilder.Entity<CalificacionDetalle>(entity =>
            {
                entity.Property(p => p.MaxPuntos).HasPrecision(6, 2);
                entity.Property(p => p.PesoEvaluacion).HasPrecision(6, 2);
                entity.Property(p => p.Puntos).HasPrecision(5, 2);
            });

            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.HasKey(e => e.IdAsistencia);

                entity.Property(e => e.Observaciones).HasMaxLength(500);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(sysutcdatetime())");

                entity.HasOne(d => d.Inscripcion)
                    .WithMany()
                    .HasForeignKey(d => d.InscripcionId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Asistencia_Inscripcion");

                entity.HasOne(d => d.GrupoMateria)
                    .WithMany()
                    .HasForeignKey(d => d.GrupoMateriaId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Asistencia_GrupoMateria");

                entity.HasOne(d => d.ProfesorRegistro)
                    .WithMany()
                    .HasForeignKey(d => d.ProfesorRegistroId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Asistencia_Profesor");

                entity.HasIndex(e => new { e.InscripcionId, e.FechaSesion })
                    .HasDatabaseName("IX_Asistencia_Inscripcion_Fecha");

                entity.HasIndex(e => new { e.GrupoMateriaId, e.FechaSesion })
                    .HasDatabaseName("IX_Asistencia_GrupoMateria_Fecha");
            });

            modelBuilder.Entity<ConceptoPrecio>(e =>
            {
                e.Property(p => p.Importe).HasPrecision(12, 2);
            });

            modelBuilder.Entity<Pago>(e =>
            {
                e.Property(p => p.Monto).HasPrecision(12, 2);
            });

            modelBuilder.Entity<PagoAplicacion>(e =>
            {
                e.Property(p => p.MontoAplicado).HasPrecision(12, 2);

                e.HasOne(pa => pa.Pago)
                    .WithMany(p => p.Aplicaciones)
                    .HasForeignKey(pa => pa.IdPago)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(pa => pa.ReciboDetalle)
                    .WithMany(rd => rd.Aplicaciones)
                    .HasForeignKey(pa => pa.IdReciboDetalle)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlanPagoDetalle>(e =>
            {
                e.Property(p => p.Cantidad).HasPrecision(9, 2);
                e.Property(p => p.Importe).HasPrecision(12, 2);
            });

            modelBuilder.Entity<RecargoPolitica>(e =>
            {
                e.Property(p => p.TasaDiaria).HasPrecision(9, 6);
                e.Property(p => p.RecargoMinimo).HasPrecision(12, 2);
                e.Property(p => p.RecargoMaximo).HasPrecision(12, 2);
            });

            modelBuilder.Entity<Beca>(e =>
            {
                e.HasKey(x => x.IdBeca);
                e.Property(x => x.Clave).HasMaxLength(30).IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(500);
                e.Property(x => x.Tipo).HasMaxLength(20).HasDefaultValue("PORCENTAJE");
                e.Property(x => x.Valor).HasPrecision(12, 2);
                e.Property(x => x.TopeMensual).HasPrecision(12, 2);
                e.Property(x => x.Activo).HasDefaultValue(true);

                e.HasIndex(x => x.Clave).IsUnique();

                e.HasOne(b => b.ConceptoPago)
                    .WithMany()
                    .HasForeignKey(b => b.IdConceptoPago)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BecaAsignacion>(e =>
            {
                e.Property(p => p.Valor).HasPrecision(12, 2);
                e.Property(p => p.TopeMensual).HasPrecision(12, 2);

                e.HasOne(b => b.ConceptoPago)
                    .WithMany()
                    .HasForeignKey(b => b.IdConceptoPago)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.Estudiante)
                    .WithMany()
                    .HasForeignKey(b => b.IdEstudiante)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.Beca)
                    .WithMany(beca => beca.Asignaciones)
                    .HasForeignKey(b => b.IdBeca)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.PeriodoAcademico)
                    .WithMany()
                    .HasForeignKey(b => b.IdPeriodoAcademico)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Recibo>(e =>
            {
                e.Property(p => p.Subtotal).HasPrecision(12, 2);
                e.Property(p => p.Descuento).HasPrecision(12, 2);
                e.Property(p => p.Recargos).HasPrecision(12, 2);
                e.Property(p => p.Saldo).HasPrecision(12, 2);
                e.Property(p => p.Total)
                    .HasPrecision(12, 2)
                    .HasComputedColumnSql("ROUND([Subtotal]-[Descuento]+[Recargos],2)", stored: true);
            });

            modelBuilder.Entity<ConceptoPago>()
            .Property(p => p.AplicaA)
            .HasConversion<int>();

            modelBuilder.Entity<ReciboDetalle>(e =>
            {
                e.Property(p => p.Cantidad).HasPrecision(9, 2);
                e.Property(p => p.PrecioUnitario).HasPrecision(12, 2);
                e.Property(p => p.Importe)
                    .HasPrecision(12, 2)
                    .HasComputedColumnSql("ROUND([Cantidad]*[PrecioUnitario],2)", stored: true);

                e.HasOne(d => d.Recibo)
                    .WithMany(r => r.Detalles)
                    .HasForeignKey(d => d.IdRecibo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Recibo>(e =>
            {
                e.Property(x => x.Estatus).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<DocumentoRequisito>(e =>
            {
                e.Property(x => x.Clave).IsRequired().HasMaxLength(50);
                e.Property(x => x.Descripcion).IsRequired().HasMaxLength(200);
                e.HasIndex(x => x.Clave).IsUnique(); 
            });

            modelBuilder.Entity<AspiranteDocumento>(e =>
            {
                e.HasKey(x => x.IdAspiranteDocumento);

                e.HasOne(x => x.Aspirante)
                 .WithMany(a => a.Documentos)
                 .HasForeignKey(x => x.IdAspirante)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Requisito)
                 .WithMany(r => r.AspiranteDocumentos)
                 .HasForeignKey(x => x.IdDocumentoRequisito)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.IdAspirante, x.IdDocumentoRequisito }).IsUnique();

                e.Property(x => x.UrlArchivo).HasMaxLength(500);
                e.Property(x => x.Notas).HasMaxLength(500);
            });

            modelBuilder.Entity<CorteCaja>(e =>
            {
                e.HasKey(x => x.IdCorteCaja);
                e.Property(p => p.MontoInicial).HasPrecision(12, 2);
                e.Property(p => p.TotalEfectivo).HasPrecision(12, 2);
                e.Property(p => p.TotalTransferencia).HasPrecision(12, 2);
                e.Property(p => p.TotalTarjeta).HasPrecision(12, 2);
                e.Property(p => p.TotalGeneral).HasPrecision(12, 2);
            });

            modelBuilder.Entity<PlantillaCobro>(e =>
            {
                e.HasKey(x => x.IdPlantillaCobro);
                e.Property(x => x.NombrePlantilla).HasMaxLength(200).IsRequired();
                e.Property(x => x.CreadoPor).HasMaxLength(100).IsRequired();
                e.Property(x => x.ModificadoPor).HasMaxLength(100);
                e.Property(x => x.Version).HasDefaultValue(1);
                e.Property(x => x.EsActiva).HasDefaultValue(true);
                e.Property(x => x.EstrategiaEmision).HasDefaultValue(0);
                e.Property(x => x.NumeroRecibos).HasDefaultValue(4);
                e.Property(x => x.DiaVencimiento).HasDefaultValue(10);
                e.Property(x => x.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

                e.HasOne(x => x.IdPlanEstudiosNavigation)
                    .WithMany()
                    .HasForeignKey(x => x.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.IdModalidadNavigation)
                    .WithMany(m => m.PlantillaCobro)
                    .HasForeignKey(x => x.IdModalidad)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_PlantillaCobro_Modalidad");

                e.HasMany(x => x.Detalles)
                    .WithOne(d => d.IdPlantillaCobroNavigation)
                    .HasForeignKey(d => d.IdPlantillaCobro)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.IdPlanEstudios);
                e.HasIndex(x => x.EsActiva);
                e.HasIndex(x => new { x.IdPlanEstudios, x.NumeroCuatrimestre, x.EsActiva });
            });

            modelBuilder.Entity<PlantillaCobroDetalle>(e =>
            {
                e.HasKey(x => x.IdPlantillaDetalle);
                e.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
                e.Property(x => x.Cantidad).HasPrecision(9, 2).HasDefaultValue(1m);
                e.Property(x => x.PrecioUnitario).HasPrecision(12, 2);
                e.Property(x => x.Orden).HasDefaultValue(1);

                e.HasOne(x => x.IdConceptoPagoNavigation)
                    .WithMany()
                    .HasForeignKey(x => x.IdConceptoPago)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.IdPlantillaCobro);
                e.HasIndex(x => x.IdConceptoPago);
            });

            modelBuilder.Entity<Permission>(e =>
            {
                e.HasKey(x => x.IdPermission);
                e.Property(x => x.Code).HasMaxLength(100).IsRequired();
                e.Property(x => x.Name).HasMaxLength(200).IsRequired();
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.Module).HasMaxLength(100).IsRequired();
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => x.Module);
            });

            modelBuilder.Entity<RolePermission>(e =>
            {
                e.HasKey(x => x.IdRolePermission);
                e.Property(x => x.RoleId).HasMaxLength(450).IsRequired();
                e.Property(x => x.AssignedBy).HasMaxLength(450);
                e.Property(x => x.AssignedAt).HasDefaultValueSql("(sysutcdatetime())");
                e.Property(x => x.CanView).HasDefaultValue(true);
                e.Property(x => x.CanCreate).HasDefaultValue(false);
                e.Property(x => x.CanEdit).HasDefaultValue(false);
                e.Property(x => x.CanDelete).HasDefaultValue(false);

                e.HasOne(x => x.Role)
                    .WithMany()
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(x => x.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            });

            modelBuilder.Entity<TipoDocumentoEstudiante>(e =>
            {
                e.HasKey(x => x.IdTipoDocumento);
                e.Property(x => x.Clave).HasMaxLength(50).IsRequired();
                e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(500);
                e.Property(x => x.Precio).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                e.Property(x => x.DiasVigencia).HasDefaultValue(30);
                e.Property(x => x.RequierePago).HasDefaultValue(true);
                e.Property(x => x.Activo).HasDefaultValue(true);
                e.Property(x => x.Orden).HasDefaultValue(0);
                e.Property(x => x.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

                e.HasIndex(x => x.Clave).IsUnique();
            });

            modelBuilder.Entity<EstudianteGrupo>(e =>
            {
                e.HasKey(x => x.IdEstudianteGrupo);
                e.Property(x => x.Estado).HasMaxLength(30).HasDefaultValue("Inscrito");
                e.Property(x => x.Observaciones).HasMaxLength(500);
                e.Property(x => x.FechaInscripcion).HasDefaultValueSql("(sysutcdatetime())");

                e.HasIndex(x => new { x.IdEstudiante, x.IdGrupo }).IsUnique();

                e.HasOne(x => x.IdEstudianteNavigation)
                    .WithMany(est => est.EstudianteGrupo)
                    .HasForeignKey(x => x.IdEstudiante)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.IdGrupoNavigation)
                    .WithMany(g => g.EstudianteGrupo)
                    .HasForeignKey(x => x.IdGrupo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BitacoraAccion>(e =>
            {
                e.HasKey(x => x.IdBitacora);
                e.Property(x => x.UsuarioId).HasMaxLength(450).IsRequired();
                e.Property(x => x.NombreUsuario).HasMaxLength(200).IsRequired();
                e.Property(x => x.Accion).HasMaxLength(100).IsRequired();
                e.Property(x => x.Modulo).HasMaxLength(100).IsRequired();
                e.Property(x => x.Entidad).HasMaxLength(100).IsRequired();
                e.Property(x => x.EntidadId).HasMaxLength(100);
                e.Property(x => x.Descripcion).HasMaxLength(1000);
                e.Property(x => x.IpAddress).HasMaxLength(50);
                e.Property(x => x.FechaUtc).HasDefaultValueSql("(sysutcdatetime())");

                e.HasIndex(x => x.Modulo);
                e.HasIndex(x => x.UsuarioId);
                e.HasIndex(x => x.FechaUtc);
            });

            modelBuilder.Entity<NotificacionUsuario>(e =>
            {
                e.HasKey(x => x.IdNotificacion);
                e.Property(x => x.UsuarioDestinoId).HasMaxLength(450).IsRequired();
                e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
                e.Property(x => x.Mensaje).HasMaxLength(1000).IsRequired();
                e.Property(x => x.Tipo).HasMaxLength(20).IsRequired().HasDefaultValue("info");
                e.Property(x => x.Modulo).HasMaxLength(100);
                e.Property(x => x.UrlAccion).HasMaxLength(500);
                e.Property(x => x.Leida).HasDefaultValue(false);
                e.Property(x => x.FechaCreacion).HasDefaultValueSql("(sysutcdatetime())");

                e.HasIndex(x => x.UsuarioDestinoId);
                e.HasIndex(x => new { x.UsuarioDestinoId, x.Leida });
                e.HasIndex(x => x.FechaCreacion);
            });

            modelBuilder.Entity<PlanDocumentoRequisito>(e =>
            {
                e.HasKey(x => x.IdPlanDocumentoRequisito);

                e.HasOne(x => x.PlanEstudios)
                    .WithMany(p => p.DocumentosRequisito)
                    .HasForeignKey(x => x.IdPlanEstudios)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.DocumentoRequisito)
                    .WithMany(d => d.PlanDocumentosRequisito)
                    .HasForeignKey(x => x.IdDocumentoRequisito)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.IdPlanEstudios, x.IdDocumentoRequisito }).IsUnique();
            });

            modelBuilder.Entity<SolicitudDocumento>(e =>
            {
                e.HasKey(x => x.IdSolicitud);
                e.Property(x => x.FolioSolicitud).HasMaxLength(20).IsRequired();
                e.Property(x => x.Notas).HasMaxLength(500);
                e.Property(x => x.UsuarioSolicita).HasMaxLength(450);
                e.Property(x => x.UsuarioGenera).HasMaxLength(450);
                e.Property(x => x.FechaSolicitud).HasDefaultValueSql("(sysutcdatetime())");
                e.Property(x => x.Estatus).HasDefaultValue(EstatusSolicitudDocumento.PENDIENTE_PAGO);
                e.Property(x => x.Variante).HasDefaultValue(VarianteDocumento.COMPLETO);
                e.Property(x => x.VecesImpreso).HasDefaultValue(0);

                e.HasIndex(x => x.FolioSolicitud).IsUnique();
                e.HasIndex(x => x.CodigoVerificacion).IsUnique();
                e.HasIndex(x => x.IdEstudiante);
                e.HasIndex(x => x.Estatus);

                e.HasOne(x => x.Estudiante)
                    .WithMany()
                    .HasForeignKey(x => x.IdEstudiante)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TipoDocumento)
                    .WithMany(t => t.Solicitudes)
                    .HasForeignKey(x => x.IdTipoDocumento)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Recibo)
                    .WithMany()
                    .HasForeignKey(x => x.IdRecibo)
                    .OnDelete(DeleteBehavior.SetNull);
            });

        }

        public override int SaveChanges()
        {
            AuditEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AuditEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void AuditEntities()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
 
            var currentUserId = _httpContextAccessor?.HttpContext?.User?.FindFirst("userId")?.Value
                                ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                                ?? "System";
 
            try
            {
                _logger?.LogDebug("AuditEntities currentUserId='{userId}' HttpContextPresent={hasContext} Entries={entriesCount}",
                                  currentUserId,
                                  _httpContextAccessor?.HttpContext != null,
                                  entries?.Count() ?? 0);
            }
            catch
            {
            }
 
            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
 
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                }
 
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                }
            }
        }
    }
}
