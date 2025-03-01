using System.Globalization;

namespace UsersApp.Resourses
{
    public class DateService
    {
        public static Dictionary<int, string> GetLocalizedDays()
        {
            var culture = CultureInfo.CurrentCulture;
            return Enumerable.Range(0, 7)
                .ToDictionary(i => i, i => culture.DateTimeFormat.GetDayName((DayOfWeek)i));
        }
    }
}
