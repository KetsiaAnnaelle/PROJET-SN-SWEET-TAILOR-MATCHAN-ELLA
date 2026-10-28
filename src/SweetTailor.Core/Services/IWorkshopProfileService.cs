using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface IWorkshopProfileService
{
    Task<WorkshopProfile> GetProfileAsync();
    Task<WorkshopProfile> UpdateProfileAsync(WorkshopProfile profile);
    Task<WorkshopStats> GetStatsAsync();
}
