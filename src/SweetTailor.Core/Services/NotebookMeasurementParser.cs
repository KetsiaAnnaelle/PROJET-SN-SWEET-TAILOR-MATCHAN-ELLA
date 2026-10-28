using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using SweetTailor.Core.Models;

namespace SweetTailor.Core.Services;

public sealed record NotebookImportResult(
    string FullName,
    string PhoneNumber,
    ClientCategory? Category,
    IReadOnlyList<MeasurementItem> Measurements);

public static partial class NotebookMeasurementParser
{
    private sealed record MeasurementDefinition(
        string Name,
        MeasurementCategory Category,
        string[] Aliases);

    private static readonly MeasurementDefinition[] Definitions =
    [
        new("Tour de cou", MeasurementCategory.HautDuCorps, ["tour de cou", "cou"]),
        new("Carrure dos", MeasurementCategory.HautDuCorps, ["carrure dos", "carrure"]),
        new("Longueur d'épaule", MeasurementCategory.HautDuCorps, ["longueur d epaule", "epaule"]),
        new("Tour de poitrine", MeasurementCategory.HautDuCorps, ["tour de poitrine", "poitrine"]),
        new("Hauteur de poitrine", MeasurementCategory.HautDuCorps, ["hauteur de poitrine", "poitrine hauteur"]),
        new("Tour de taille", MeasurementCategory.HautDuCorps, ["tour de taille", "taille"]),
        new("Longueur de manche", MeasurementCategory.HautDuCorps, ["longueur de manche", "manche"]),
        new("Tour de bras", MeasurementCategory.HautDuCorps, ["tour de bras", "bras"]),
        new("Tour de hanches", MeasurementCategory.Bas, ["tour de hanches", "hanches", "bassin"]),
        new("Tour de cuisse", MeasurementCategory.Bas, ["tour de cuisse", "cuisse"]),
        new("Entrejambe", MeasurementCategory.Bas, ["entrejambe", "entre jambe"]),
        new("Longueur robe", MeasurementCategory.Longueurs, ["longueur robe", "robe"]),
        new("Longueur jupe", MeasurementCategory.Longueurs, ["longueur jupe", "jupe"]),
        new("Longueur pantalon", MeasurementCategory.Longueurs, ["longueur pantalon", "pantalon"]),
        new("Hauteur totale", MeasurementCategory.Longueurs, ["hauteur totale", "taille totale"])
    ];

    private static readonly Regex NamePattern = NameRegex();
    private static readonly Regex PhonePattern = PhoneRegex();
    private static readonly Regex NumberPattern = NumberRegex();

    public static NotebookImportResult Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var fullName = ReadField(lines, ["nom complet", "nom", "cliente", "client"]);
        var phone = ReadField(lines, ["telephone", "tel", "phone"]);
        if (string.IsNullOrWhiteSpace(phone))
        {
            var phoneMatch = PhonePattern.Match(text);
            if (phoneMatch.Success && phoneMatch.Value.Count(char.IsDigit) is >= 9 and <= 15)
            {
                phone = phoneMatch.Value.Trim();
            }
        }

        var category = ParseCategory(text);
        var measurements = new Dictionary<string, MeasurementItem>(StringComparer.Ordinal);

        foreach (var line in lines)
        {
            var normalizedLine = Normalize(line);
            var definition = Definitions
                .SelectMany(item => item.Aliases.Select(alias => (Definition: item, Alias: Normalize(alias))))
                .Where(item => normalizedLine.Contains(item.Alias, StringComparison.Ordinal))
                .OrderByDescending(item => item.Alias.Length)
                .Select(item => item.Definition)
                .FirstOrDefault();

            if (definition is null)
            {
                continue;
            }

            var numberMatches = NumberPattern.Matches(line);
            if (numberMatches.Count == 0)
            {
                continue;
            }

            var rawValue = numberMatches[^1].Value.Replace(',', '.');
            if (!double.TryParse(rawValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) ||
                !double.IsFinite(value) || value is <= 0 or > 500)
            {
                continue;
            }

            measurements[definition.Name] = new MeasurementItem
            {
                Name = definition.Name,
                Value = value,
                Unit = "cm",
                Category = definition.Category,
                IsHighlighted = definition.Name == "Tour de taille"
            };
        }

        return new NotebookImportResult(fullName, phone, category, measurements.Values.ToList());
    }

    private static string ReadField(IEnumerable<string> lines, IReadOnlyCollection<string> labels)
    {
        foreach (var line in lines)
        {
            var match = NamePattern.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var label = Normalize(match.Groups["label"].Value);
            if (labels.Contains(label, StringComparer.Ordinal))
            {
                return match.Groups["value"].Value.Trim();
            }
        }

        return string.Empty;
    }

    private static ClientCategory? ParseCategory(string text)
    {
        var normalized = Normalize(text);
        if (Regex.IsMatch(normalized, @"\benfant\b|\benfants\b"))
        {
            return ClientCategory.Enfant;
        }
        if (Regex.IsMatch(normalized, @"\bhomme\b|\bmasculin\b"))
        {
            return ClientCategory.Homme;
        }
        if (Regex.IsMatch(normalized, @"\bfemme\b|\bfeminin\b"))
        {
            return ClientCategory.Femme;
        }

        return null;
    }

    private static string Normalize(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
            }
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    [GeneratedRegex(@"^\s*(?<label>nom complet|nom|cliente|client|telephone|tel|phone)\s*[:\-]\s*(?<value>.+?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"(?:\+?\d[\d\s().-]{7,}\d)", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"(?<!\d)\d{1,3}(?:[.,]\d{1,2})?(?!\d)", RegexOptions.CultureInvariant)]
    private static partial Regex NumberRegex();
}
