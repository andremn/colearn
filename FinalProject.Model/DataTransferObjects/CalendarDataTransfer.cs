using System;

namespace FinalProject.Model
{
    public class CalendarDataTransfer : IDataTransfer
    {
        public int Id { get; set; }

        public string FilePath { get; set; }

        public StudentDataTransfer Student { get; set; }

        public TimeSpan MaxScheduleTime { get; set; }
    }
}
