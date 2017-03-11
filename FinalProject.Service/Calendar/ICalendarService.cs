using System;
using System.Threading.Tasks;
using FinalProject.Model;

namespace FinalProject.Service
{
    public interface ICalendarService : IService
    {
        Task<CalendarDataTransfer> CreateCalendarAsync(CalendarDataTransfer calendarDataTransfer);

        Task<CalendarDataTransfer> GetCalendarByStudentIdAsync(int id);

        Task<CalendarDataTransfer> SetMaxScheduleTimeForCalendarAsync(int calendarId, TimeSpan maxScheduleTime);
    }
}