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
    private readonly IDemoModeService _demoMode;

    public MeasurementService(IClientService clientService, IDemoModeService demoMode)
    {
        _clientService = clientService;
        _demoMode = demoMode;
    }

    public async Task<List<MeasurementItem>> GetMeasurementsForClientAsync(int clientId)
    {
        await _demoMode.SimulateDelayAsync();
        var client = await _clientService.GetClientByIdAsync(clientId);
        return client?.Measurements ?? new List<MeasurementItem>();
    }

    public async Task<bool> SaveMeasurementsAsync(int clientId, List<MeasurementItem> measurements)
    {
        await Task.Delay(1000);
        var client = await _clientService.GetClientByIdAsync(clientId);
        if (client != null)
        {
            client.Measurements = measurements;
            return true;
        }
        return false;
    }
}
