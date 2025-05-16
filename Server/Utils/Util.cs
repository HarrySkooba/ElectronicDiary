namespace Server.Utils
{
    public static class Util
    {
        public static string GetValueOrDefault(this string value, string defaultValue = "Не указано")
            => !string.IsNullOrEmpty(value) ? value : defaultValue;

        public static string GetValueOrDefaultDate(this DateOnly? value, string format = "dd.MM.yyyy")
            => value.HasValue ? value.Value.ToString(format) : "Не указано";
    }
}
