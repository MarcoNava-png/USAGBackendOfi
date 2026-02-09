using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public BitacoraService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db; _mapper = mapper;
        }

        public async Task<long> AgregarAsync(BitacoraCreateDto dto, CancellationToken ct)
        {
            var entity = _mapper.Map<BitacoraRecibo>(dto);
            _db.BitacoraRecibo.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.IdBitacora;
        }

        public async Task<PagedResult<BitacoraDto>> GetBitacora(int page, int pageSize, string filter)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.BitacoraRecibo.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = filter.ToLower();
                query = query.Where(b =>
                    (b.Usuario != null && b.Usuario.ToLower().Contains(f)) ||
                    (b.TipoRecibo != null && b.TipoRecibo.ToLower().Contains(f)) ||
                    (b.Accion != null && b.Accion.ToLower().Contains(f)) ||
                    (b.Origen != null && b.Origen.ToLower().Contains(f)) ||
                    (b.Notas != null && b.Notas.ToLower().Contains(f)) ||
                    b.IdRecibo.ToString().Contains(filter) ||
                    b.IdBitacora.ToString().Contains(filter)
                );
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.FechaUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BitacoraDto
                {
                    IdBitacora = b.IdBitacora,
                    IdRecibo = b.IdRecibo,
                    TipoRecibo = b.TipoRecibo,
                    Usuario = b.Usuario,
                    FechaUtc = b.FechaUtc,
                    Accion = b.Accion,
                    Origen = b.Origen,
                    Notas = b.Notas
                })
                .ToListAsync();

            return new PagedResult<BitacoraDto>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
