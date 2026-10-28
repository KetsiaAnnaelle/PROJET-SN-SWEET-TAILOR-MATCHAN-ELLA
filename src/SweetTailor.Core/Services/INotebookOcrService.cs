namespace SweetTailor.Core.Services;

public interface INotebookOcrService
{
    Task<string> ReadTextAsync(string imagePath, CancellationToken cancellationToken = default);
}
