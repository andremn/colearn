using System;

namespace FinalProject.ViewModels
{
    public enum RecurrenceMode
    {
        None = 0,
        Daily = 4,
        Weekly = 5,
        Monthly = 6
    }

    public class AgendaViewModel
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }

        public bool IsReadOnly { get; set; }

        public ulong MaxScheduleTime { get; set; }
    }

    public class EventViewModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset? End { get; set; }

        public DayOfWeek? RecurrenceDay { get; set; }

        public RecurrenceMode RecurrenceMode { get; set; }
    }

    public class GetEventsViewModel
    {
        public int? StudentId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }

    public class CreateEventViewModel
    {
        public string Id { get; set; }
        
        public long Start { get; set; }

        public long? End { get; set; }

        public DayOfWeek? RecurrenceDay { get; set; }

        public RecurrenceMode RecurrenceMode { get; set; }
    }

    public class JoinEventViewModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTimeOffset Start { get; set; }

        public DateTimeOffset End { get; set; }

        public int StudentId { get; set; }
    }
}