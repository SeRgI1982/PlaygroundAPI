using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crews.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crews.API.Data;

public class OneCrewRepository : IOneCrewRepository
{
    private readonly OneCrewContext _context;

    public OneCrewRepository(OneCrewContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    public void Reset()
    {
        _context.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e => e.State = EntityState.Detached);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    public async Task<IEnumerable<Crew>> GetCrewsAsync(bool includeTrainings = false)
    {
        IQueryable<Crew> query = _context.Crews;

        if (includeTrainings)
        {
            query = query.Include(c => c.Trainings);
        }

        var crews = await query.ToArrayAsync();
        return crews;
    }

    public async Task<Crew> GetCrewByIdAsync(int id, bool includeTrainings = false)
    {
        IQueryable<Crew> query = _context.Crews;

        if (includeTrainings)
        {
            query = query.Include(c => c.Trainings);
        }

        query = query.Where(c => c.Id == id);
        var crew = await query.SingleOrDefaultAsync();
        return crew;
    }

    public async Task<IEnumerable<Training>> GetTrainingsByCrewIdAsync(int crewId)
    {
        IQueryable<Training> query = _context.Trainings;
        query = query.Where(t => t.Crew.Id == crewId);
        return await query.ToArrayAsync();
    }

    public async Task<Training> GetTrainingByCrewIdAsync(int crewId, int id)
    {
        IQueryable<Training> query = _context.Trainings;
        query = query.Where(t => t.Crew.Id == crewId && t.Id == id);
        return await query.SingleOrDefaultAsync();
    }

    public void AddOrUpdate(object entity)
    {
        var state = _context.Entry(entity).State;

        switch (state)
        {
            case EntityState.Detached:
                _context.Add(entity);
                break;
            case EntityState.Modified:
                _context.Update(entity);
                break;
            case EntityState.Added:
            case EntityState.Deleted:
            case EntityState.Unchanged:
                // do nothing
                break;
        }
    }

    public void Delete(object entity)
    {
        _context.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}