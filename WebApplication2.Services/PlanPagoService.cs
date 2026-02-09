using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlanPagoService : IPlanPagoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public PlanPagoService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        public async Task<int> CrearPlanAsync(CrearPlanPagoDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<PlanPago>(dto);
            _db.PlanPago.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.IdPlanPago;
        }

        public async Task<int> AgregarDetalleAsync(int idPlanPago, CrearPlanDetalleDto dto, CancellationToken ct)
        {
            var existe = await _db.PlanPago.AnyAsync(x => x.IdPlanPago == idPlanPago, ct);
            if (!existe) throw new InvalidOperationException("PlanPago no existe");

            var entity = _mapper.Map<PlanPagoDetalle>(dto);
            entity.IdPlanPago = idPlanPago;
            _db.PlanPagoDetalle.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.IdPlanPagoDetalle;
        }

        public async Task<long> AsignarPlanAsync(AsignarPlanDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<PlanPagoAsignacion>(dto);
            _db.PlanPagoAsignacion.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.IdPlanPagoAsignacion;
        }
    }
}
