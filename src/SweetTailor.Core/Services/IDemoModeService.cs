namespace SweetTailor.Core.Services;

public enum DemoListState
{
    Normal,
    Empty,
    Error,
    Slow
}

public interface IDemoModeService
{
    DemoListState CurrentState { get; set; }
    Task SimulateDelayAsync();
}

public class DemoModeService : IDemoModeService
{
    public DemoListState CurrentState { get; set; } = DemoListState.Normal;

    public async Task SimulateDelayAsync()
    {
        int delay = CurrentState switch
        {
            DemoListState.Slow => 5000,
            _ => Random.Shared.Next(800, 1200)
        };
        await Task.Delay(delay);

        if (CurrentState == DemoListState.Error)
        {
            throw new HttpRequestException("Connexion impossible (NET-01)");
        }
    }
}
