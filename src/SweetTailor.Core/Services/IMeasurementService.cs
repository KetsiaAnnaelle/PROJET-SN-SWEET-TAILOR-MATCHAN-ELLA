using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public interface IMeasurementService
{
    Task<List<MeasurementItem>> GetMeasurementsForClientAsync(int clientId);
    Task<bool> SaveMeasurementsAsync(int clientId, List<MeasurementItem> measurements);
}

public class MeasurementService : IMeasurementService
{
    private readonly IClientService _clientService;

    public MeasurementService(IClientService clientService)
    {
        _clientService = clientService;
    }

    public async Task<List<MeasurementItem>> GetMeasurementsForClientAsync(int clientId)
    {
        var client = await _clientService.GetClientByIdAsync(clientId);
        return client?.Measurements ?? throw new InvalidOperationException("Client introuvable.");
    }

    public async Task<bool> SaveMeasurementsAsync(int clientId, List<MeasurementItem> measurements)
    {
        return await _clientService.UpdateMeasurementsAsync(clientId, measurements);
    }
}
