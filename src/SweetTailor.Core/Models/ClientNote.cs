namespace SweetTailor.Core.Models;

public class ClientNote
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string DateFormatted => Date.ToString("d MMM yyyy", new System.Globalization.CultureInfo("fr-FR"));
    public string Content { get; set; } = string.Empty;
    public bool IsImportant { get; set; }
}
