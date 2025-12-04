namespace Victorina.Bot.Services;

public static class CountryService
{
    // Only 6 supported countries
    public static readonly Dictionary<string, (string Flag, string Name)> Countries = new()
    {
        { "RU", ("🇷🇺", "Russia") },
        { "IN", ("🇮🇳", "India") },
        { "BR", ("🇧🇷", "Brazil") },
        { "IR", ("🇮🇷", "Iran") },
        { "DE", ("🇩🇪", "Germany") },
        { "UZ", ("🇺🇿", "Uzbekistan") },
    };

    public static string GetFlag(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
            return "🌍";

        return Countries.TryGetValue(countryCode.ToUpper(), out var country)
            ? country.Flag
            : "🌍";
    }

    public static string GetCountryName(string? countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
            return "Not set";

        return Countries.TryGetValue(countryCode.ToUpper(), out var country)
            ? country.Name
            : "Unknown";
    }

    public static string FormatPlayerName(string? firstName, string? lastName, string? username, string? countryCode)
    {
        var flag = GetFlag(countryCode);
        var nameParts = new List<string>();

        if (!string.IsNullOrEmpty(firstName))
            nameParts.Add(firstName);
        if (!string.IsNullOrEmpty(lastName))
            nameParts.Add(lastName);

        var fullName = nameParts.Count > 0 ? string.Join(" ", nameParts) : "Player";

        if (!string.IsNullOrEmpty(username))
            return $"{flag} {fullName} (@{username})";

        return $"{flag} {fullName}";
    }
}
