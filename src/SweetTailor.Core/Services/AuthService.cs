using System.Text.RegularExpressions;

namespace SweetTailor.Core.Services;

public record AuthResult(bool IsSuccess, string? ErrorMessage = null, string? Token = null);

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string identifier, string password);
    Task<AuthResult> RegisterAsync(string fullName, string phone, string email, string password, string confirmPassword, bool acceptedTerms);
    Task<AuthResult> SendPasswordResetCodeAsync(string identifier);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);

    public Task<AuthResult> LoginAsync(string identifier, string password)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Task.FromResult(new AuthResult(false, "Veuillez entrer votre e-mail ou téléphone."));

        if (string.IsNullOrWhiteSpace(password))
            return Task.FromResult(new AuthResult(false, "Veuillez entrer votre mot de passe."));

        // Demo user verification
        return Task.FromResult(new AuthResult(true, null, "token-sweet-tailor-user"));
    }

    public Task<AuthResult> RegisterAsync(string fullName, string phone, string email, string password, string confirmPassword, bool acceptedTerms)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Task.FromResult(new AuthResult(false, "Le nom complet est obligatoire."));

        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 9)
            return Task.FromResult(new AuthResult(false, "Numéro de téléphone invalide."));

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
            return Task.FromResult(new AuthResult(false, "Adresse e-mail invalide. Exemple : nom@mail.com"));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || !DigitRegex.IsMatch(password))
            return Task.FromResult(new AuthResult(false, "Le mot de passe doit comporter au moins 8 caractères, dont 1 chiffre."));

        if (password != confirmPassword)
            return Task.FromResult(new AuthResult(false, "Les mots de passe ne correspondent pas."));

        if (!acceptedTerms)
            return Task.FromResult(new AuthResult(false, "Vous devez accepter les conditions d'utilisation."));

        return Task.FromResult(new AuthResult(true, null, "token-registered-user"));
    }

    public Task<AuthResult> SendPasswordResetCodeAsync(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return Task.FromResult(new AuthResult(false, "Veuillez renseigner votre e-mail ou téléphone."));

        return Task.FromResult(new AuthResult(true));
    }

    public Task LogoutAsync() => Task.CompletedTask;
}
