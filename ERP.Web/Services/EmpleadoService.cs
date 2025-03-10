using ERP.Web.Data;
using ERP.Web.Domain.Dto;
using ERP.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.Services;

public interface IEmpleadoService
{
    Task<List<EmpleadoDto>> Consultar(string filtro);
    Task<bool> Crear(EmpleadoDto request);
    Task<bool> Eliminar(int Id);
    Task<bool> Modificar(EmpleadoDto request);
}

public class EmpleadoService : IEmpleadoService
{
    private readonly AppDbContext _context;
    public EmpleadoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpleadoDto>> Consultar(string filtro)
    {
        var empleados = await
            _context.Empleados
            .Include(e => e.DatosPersonales)
            .Where(e => e.DatosPersonales.Nombre.Contains(filtro))
            .Select(
                e =>
                new EmpleadoDto()
                {
                    Id = e.Id,
                    PersonaId = e.PersonaId,
                    Sueldo = e.Sueldo,
                    DatosPersonales = new PersonaDto()
                    {
                        Id = e.DatosPersonales.Id,
                        Nombre = e.DatosPersonales.Nombre,
                        FechaDeNacimiento = e.DatosPersonales.FechaDeNacimiento
                    }
                }
            )
            .ToListAsync();
        return empleados;
    }

    public async Task<bool> Crear(EmpleadoDto request)
    {
        var empleado = Empleado.Create(
            request.DatosPersonales.Nombre,
            request.DatosPersonales.FechaDeNacimiento,
            request.Sueldo
        );
        _context.Empleados.Add(empleado);
        return (await _context.SaveChangesAsync()) > 0;
    }

    public async Task<bool> Modificar(EmpleadoDto request)
    {
        var empleado = await _context.Empleados
            .Include(e => e.DatosPersonales)
            .FirstOrDefaultAsync(e => e.Id == request.Id);
            
        empleado!.DatosPersonales.Nombre = request.DatosPersonales.Nombre;
        empleado!.DatosPersonales.FechaDeNacimiento = request.DatosPersonales.FechaDeNacimiento;
        empleado!.Sueldo = request.Sueldo;
        
        return (await _context.SaveChangesAsync()) > 0;
    }

    public async Task<bool> Eliminar(int Id)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == Id);

        _context.Empleados.Remove(empleado!);

        return (await _context.SaveChangesAsync()) > 0;
    }
}