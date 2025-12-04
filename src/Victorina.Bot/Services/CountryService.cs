namespace Victorina.Bot.Services;

public static class CountryService
{
    public static readonly Dictionary<string, (string Flag, string Name)> Countries = new()
    {
        { "RU", ("🇷🇺", "Россия") },
        { "UA", ("🇺🇦", "Украина") },
        { "BY", ("🇧🇾", "Беларусь") },
        { "KZ", ("🇰🇿", "Казахстан") },
        { "UZ", ("🇺🇿", "Узбекистан") },
        { "AZ", ("🇦🇿", "Азербайджан") },
        { "GE", ("🇬🇪", "Грузия") },
        { "AM", ("🇦🇲", "Армения") },
        { "MD", ("🇲🇩", "Молдова") },
        { "KG", ("🇰🇬", "Кыргызстан") },
        { "TJ", ("🇹🇯", "Таджикистан") },
        { "TM", ("🇹🇲", "Туркменистан") },
        { "LV", ("🇱🇻", "Латвия") },
        { "LT", ("🇱🇹", "Литва") },
        { "EE", ("🇪🇪", "Эстония") },
        { "PL", ("🇵🇱", "Польша") },
        { "DE", ("🇩🇪", "Германия") },
        { "US", ("🇺🇸", "США") },
        { "GB", ("🇬🇧", "Великобритания") },
        { "IL", ("🇮🇱", "Израиль") },
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
            return "Не указана";

        return Countries.TryGetValue(countryCode.ToUpper(), out var country)
            ? country.Name
            : "Неизвестно";
    }

    public static string FormatPlayerName(string? firstName, string? lastName, string? username, string? countryCode)
    {
        var flag = GetFlag(countryCode);
        var nameParts = new List<string>();

        if (!string.IsNullOrEmpty(firstName))
            nameParts.Add(firstName);
        if (!string.IsNullOrEmpty(lastName))
            nameParts.Add(lastName);

        var fullName = nameParts.Count > 0 ? string.Join(" ", nameParts) : "Игрок";

        if (!string.IsNullOrEmpty(username))
            return $"{flag} {fullName} (@{username})";

        return $"{flag} {fullName}";
    }
}
