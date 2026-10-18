namespace SweetTailor.Core.Models;

public class CatalogModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string SvgAsset { get; set; } = "gown";
    public string SvgFillColor { get; set; } = "#26306B";
    public string GradientClass { get; set; } = "g-rose";
    public bool IsFavorite { get; set; }
    public double CardHeight { get; set; } = 180;
    public List<string> RequiredMeasurements { get; set; } = new();
    public List<int> SimilarModelIds { get; set; } = new();
    public string ImagePath { get; set; } = string.Empty;
}
