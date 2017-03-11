using System;
using System.IO;
using System.Threading.Tasks;
using FinalProject.Helpers;
using Ical.Net.Interfaces;
using Ical.Net.Serialization;
using Ical.Net.Serialization.iCalendar.Serializers;

namespace FinalProject.Extensions
{
    public static class CalendarExtensions
    {
        public static async Task SaveAsync(this ICalendar calendar, string filePath)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException(nameof(calendar));
            }

            File.WriteAllText(filePath, string.Empty);

            var serializer = new CalendarSerializer(SerializationContext.Default);

            using (var fileStream = FileSystemHelper.CreateFileForWrite(filePath))
            {
                var content = serializer.SerializeToString(calendar);

                using (var writer = new StreamWriter(fileStream))
                {
                    await writer.WriteAsync(content);
                }
            }
        }
    }
}