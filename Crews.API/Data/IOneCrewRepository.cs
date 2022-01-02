using System.Collections.Generic;
using System.Threading.Tasks;
using Crews.API.Data.Entities;

namespace Crews.API.Data;

public interface IOneCrewRepository
{
    void Reset();

    Task<IEnumerable<Crew>> GetCrewsAsync(bool includeTrainings = false);

    Task<Crew> GetCrewByIdAsync(int id, bool includeTrainings = false);

    Task<IEnumerable<Training>> GetTrainingsByCrewIdAsync(int crewId);

    Task<Training> GetTrainingByCrewIdAsync(int crewId, int id);

    void AddOrUpdate(object entity);

    void Delete(object entity);

    Task<int> SaveChangesAsync();
}