using System;
using System.Threading.Tasks;
using FinalProject.DataAccess;
using FinalProject.DataAccess.UnitOfWork;
using FinalProject.Model;

namespace FinalProject.Service
{
    public class CalendarService : ICalendarService
    {
        private readonly ICalendarDataAccess _calendarDataAccess;
        private readonly IUnitOfWork _unitOfWork;

        public CalendarService(ICalendarDataAccess calendarDataAccess, IUnitOfWork unitOfWork)
        {
            if (calendarDataAccess == null)
            {
                throw new ArgumentNullException(nameof(calendarDataAccess));
            }

            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            _calendarDataAccess = calendarDataAccess;
            _unitOfWork = unitOfWork;
        }

        public async Task<CalendarDataTransfer> CreateCalendarAsync(CalendarDataTransfer calendarDataTransfer)
        {
            var calendar = calendarDataTransfer.ToCalendar();

            calendar = await _calendarDataAccess.CreateAsync(calendar);

            await _unitOfWork.CommitAsync();
            return calendar?.ToCalendarDataTransfer();
        }

        public async Task<CalendarDataTransfer> GetCalendarByStudentIdAsync(int id)
        {
            var calendar = await _calendarDataAccess.GetByStudentIdAsync(id);
            
            return calendar?.ToCalendarDataTransfer();
        }

        public async Task<CalendarDataTransfer> SetMaxScheduleTimeForCalendarAsync(int calendarId, TimeSpan maxScheduleTime)
        {
            var calendar = await _calendarDataAccess.GetByIdAsync(calendarId);

            calendar.MaxScheduleTime = maxScheduleTime;
            calendar = await _calendarDataAccess.UpdateAsync(calendar);

            await _unitOfWork.CommitAsync();
            return calendar?.ToCalendarDataTransfer();
        }
    }
}