namespace SweetTailor.Core.Models;

public enum MeasurementCategory
{
    HautDuCorps,
    Bas,
    Longueurs
}

public class MeasurementItem
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = "cm";
    public MeasurementCategory Category { get; set; } = MeasurementCategory.HautDuCorps;
    public bool IsHighlighted { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public string FormattedValue => $"{Value:0.#}";
    public string DisplayString => $"{FormattedValue} {Unit}";
}
