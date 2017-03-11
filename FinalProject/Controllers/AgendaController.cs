using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using FinalProject.Extensions;
using FinalProject.Helpers;
using FinalProject.Hubs;
using FinalProject.LocalResource;
using FinalProject.Model;
using FinalProject.Service;
using FinalProject.ViewModels;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Calendar = Ical.Net.Calendar;

namespace FinalProject.Controllers
{
    public class AgendaController : BaseController
    {
        private const string AvailableEventBackColor = "#4caf50";

        private const string BusyEventBackColor = "#2196f3";

        [Route("/Agenda/User/{id}")]
        public async Task<ActionResult> Index(int? id)
        {
            var student = await GetCurrentStudentAsync();
            var model = new AgendaViewModel();
            CalendarDataTransfer calendar;

            if (id.HasValue)
            {
                model.IsReadOnly = id.Value != student.Id;
                student = await GetService<IStudentService>().GetStudentByIdAsync(id.Value);
                model.StudentName = student.FirstName;
                model.StudentId = id.Value;
                calendar = await GetService<ICalendarService>()
                    .GetCalendarByStudentIdAsync(id.Value);
            }
            else
            {
                model.StudentId = student.Id;
                    calendar = await GetService<ICalendarService>()
                     .GetCalendarByStudentIdAsync(student.Id);
            }

            var maxScheduleTime = calendar == null || calendar.MaxScheduleTime == TimeSpan.Zero
                    ? TimeSpan.FromMinutes(30)
                    : calendar.MaxScheduleTime;

            model.MaxScheduleTime = (ulong)maxScheduleTime.TotalMilliseconds;
            
            return View(model);
        }

        public async Task<ActionResult> GetEvents(GetEventsViewModel model)
        {
            var calendarWrapper = model.StudentId.HasValue
                ? await GetCalendarForStudentAsync(model.StudentId.Value)
                : await GetCalendarForCurrentStudentAsync();

            var calendar = calendarWrapper.Calendar;
            var occurences = calendar.GetOccurrences(model.Start, model.End);
            var jsonOccurrences = occurences.Select(ParseOccurrenceToJson);

            return Json(jsonOccurrences, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> New(CreateEventViewModel model)
        {
            var calendarWrapper = await GetCalendarForCurrentStudentAsync();
            var calendar = calendarWrapper.Calendar;
            var startDateTime = DateTimeHelper.FromUnixTimeStampMilliseconds(model.Start);
            var endDateTime = !model.End.HasValue
                ? startDateTime.AddHours(24)
                : DateTimeHelper.FromUnixTimeStampMilliseconds(model.End.Value);

            var calendarEvent = new Event
            {
                Start = new CalDateTime(startDateTime),
                End = new CalDateTime(endDateTime),
                IsAllDay = !model.End.HasValue,
                Organizer = new Organizer(calendarWrapper.Student.Email)
            };

            if (model.RecurrenceMode != RecurrenceMode.None)
            {
                try
                {
                    calendarEvent.RecurrenceRules =
                        GetRecurrencePatterns(model.RecurrenceMode, model.RecurrenceDay);
                }
                catch (InvalidOperationException ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
                }
            }

            MergeAdjacentEvents(calendar.Events, calendarEvent);
            calendar.Events.Add(calendarEvent);
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
            return Json("OK");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(EventViewModel eventData, bool deleteAll)
        {
            var student = await GetCurrentStudentAsync();
            var calendarWrapper = await GetCalendarForCurrentStudentAsync();
            var calendar = calendarWrapper.Calendar;
            var calendarEvent = calendar.Events.Single(e => e.Uid == eventData.Id);

            if (calendarEvent == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (calendarEvent.RecurrenceRules.Count > 0 && !deleteAll)
            {
                var start = eventData.Start.UtcDateTime;
                var period = new Period(new CalDateTime(start));

                if (calendarEvent.ExceptionDates.Count == 0)
                {
                    calendarEvent.ExceptionDates.Add(new PeriodList());
                }

                calendarEvent.ExceptionDates[0].Add(period);
            }
            else
            {
                calendar.Events.Remove(calendarEvent);
            }

            // Todo: Add available event and merge it with MergeAdjacentEvents(calendar.Events, calendarEvent);
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);

            var organizerEmail = calendarEvent.Organizer.Value.ToString().Replace("mailto:", string.Empty);

            if (organizerEmail == student.Email)
            {
                return Json("OK");
            }

            student = await GetService<IStudentService>().GetStudentByEmailAsync(organizerEmail);
            calendarWrapper = await GetCalendarForStudentAsync(student.Id);
            calendar = calendarWrapper.Calendar;
            calendarEvent = calendar.Events.Single(e => e.Uid == calendarEvent.Uid);
            calendarEvent.Attendees.Clear();
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
            return Json("OK");
        }

        [HttpPost]
        public async Task<ActionResult> Edit(CreateEventViewModel eventData, bool editAll)
        {
            var calendarWrapper = await GetCalendarForCurrentStudentAsync();
            var calendar = calendarWrapper.Calendar;
            var calendarEvent = calendar.Events.SingleOrDefault(e => e.Uid == eventData.Id);

            if (calendarEvent == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var startDateTime = DateTimeHelper.FromUnixTimeStampMilliseconds(eventData.Start);
            var endDateTime = !eventData.End.HasValue
                ? startDateTime.AddHours(24)
                : DateTimeHelper.FromUnixTimeStampMilliseconds(eventData.End.Value);

            if (calendarEvent.RecurrenceRules.Count > 0 && !editAll)
            {
                var start = startDateTime;
                var period = new Period(new CalDateTime(start));

                if (calendarEvent.ExceptionDates.Count == 0)
                {
                    calendarEvent.ExceptionDates.Add(new PeriodList());
                }

                calendarEvent.ExceptionDates[0].Add(period);
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
                calendarEvent = new Event();
                calendar.Events.Add(calendarEvent);
            }
            else
            {
                if (eventData.RecurrenceMode != RecurrenceMode.None)
                {
                    try
                    {
                        calendarEvent.RecurrenceRules =
                            GetRecurrencePatterns(eventData.RecurrenceMode,
                                eventData.RecurrenceDay);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
                    }
                }
            }
            
            calendarEvent.Start = new CalDateTime(startDateTime);
            calendarEvent.End = new CalDateTime(endDateTime);
            calendarEvent.IsAllDay = !eventData.End.HasValue;

            MergeAdjacentEvents(calendar.Events, calendarEvent);
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
            return Json("OK");
        }

        [HttpPost]
        public async Task<ActionResult> Event(string id)
        {
            var calendarWrapper = await GetCalendarForCurrentStudentAsync();
            var calendar = calendarWrapper.Calendar;
            var calendarEvent = calendar.Events.Single(e => e.Uid == id);

            if (calendarEvent == null)
            {
                return Json(new { });
            }

            var recurrenceMode = RecurrenceMode.None;
            var recurrenceDay = (DayOfWeek?)null;

            if (calendarEvent.RecurrenceRules.Count > 0)
            {
                recurrenceMode = (RecurrenceMode)calendarEvent.RecurrenceRules[0].Frequency;

                if (recurrenceMode != RecurrenceMode.None &&
                    recurrenceMode != RecurrenceMode.Daily)
                {
                    recurrenceDay = calendarEvent.RecurrenceRules[0].ByDay[0].DayOfWeek;
                }
            }

            var jsonEvent = new
            {
                id,
                start = new DateTimeOffset(calendarEvent.Start.AsUtc).ToString("o"),
                end = new DateTimeOffset(calendarEvent.Start.AsUtc).ToString("o"),
                isAllDay = calendarEvent.IsAllDay,
                recurrenceMode,
                recurrenceDay
            };

            return Json(jsonEvent);
        }

        [HttpPost]
        public async Task<ActionResult> Join(JoinEventViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var student = await GetCurrentStudentAsync();
            var calendarWrapper = await GetCalendarForStudentAsync(model.StudentId);
            var calendar = calendarWrapper.Calendar;
            var calendarEvent = calendar.Events.SingleOrDefault(e => e.Uid == model.Id);

            if (calendarEvent == null)
            {
                return new HttpNotFoundResult();
            }

            var attendee = new Attendee
            {
                Members = new List<string> { student.Email }
            };

            var occurrences = calendar.GetOccurrences(model.Start.UtcDateTime, model.End.UtcDateTime);
            var occurrence = occurrences.SingleOrDefault(o => ((IEvent)o.Source).Uid == calendarEvent.Uid);

            if (occurrence == null)
            {
                return HttpNotFound();
            }

            var periodStart = new CalDateTime(model.Start.UtcDateTime) { HasTime = true };
            var periodEnd = new CalDateTime(model.End.UtcDateTime) { HasTime = true };
            var organizer = calendarEvent.Organizer;

            if (calendarEvent.RecurrenceRules.Count > 0)
            {
                if (calendarEvent.ExceptionDates.Count == 0)
                {
                    calendarEvent.ExceptionDates.Add(new PeriodList());
                }

                var exceptionDate = calendarEvent.Start.AsUtc.SetDate(model.Start.UtcDateTime);

                calendarEvent.ExceptionDates[0].Add(new CalDateTime(exceptionDate));
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            }
            else
            {
                calendar.Events.Remove(calendarEvent);
            }

            if (!occurrence.Period.StartTime.Equals(periodStart) || !occurrence.Period.EndTime.Equals(periodEnd))
            {
                SplitAndMergeEvent(calendarEvent, occurrence, periodStart, periodEnd, calendar);
            }

            calendarEvent = new Event
            {
                Description = $"{model.Title} ({student.FirstName} {student.LastName})",
                Start = periodStart,
                End = periodEnd,
                Organizer = organizer,
                Attendees = new List<IAttendee> { attendee }
            };

            calendar.Events.Add(calendarEvent);
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);

            calendarWrapper = await GetCalendarForCurrentStudentAsync();
            calendar = calendarWrapper.Calendar;
            calendar.Events.Add(calendarEvent);
            await calendar.SaveAsync(calendarWrapper.CalendarFilePath);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<ActionResult> Leave(string id)
        {
            var calendarWrapper = await GetCalendarForCurrentStudentAsync();
            var calendar = calendarWrapper.Calendar;
            var calendarEvent = calendar.Events.SingleOrDefault(e => e.Uid == id);

            if (calendarEvent == null)
            {
                return new HttpNotFoundResult();
            }
            
            var organizerEmail = calendarEvent.Organizer.Value.ToString().Replace("mailto:", string.Empty);

            if (organizerEmail == calendarWrapper.Student.Email)
            {
                calendarEvent.Attendees.Clear();
                calendarEvent.Description = null;

                AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);

                var attendee = calendarEvent.Attendees.FirstOrDefault();

                if (attendee == null)
                {
                    return Json("OK");
                }

                var attendeeEmail = attendee.Value.ToString().Replace("mailto:", string.Empty);
                var student = await GetService<IStudentService>().GetStudentByEmailAsync(attendeeEmail);

                calendarWrapper = await GetCalendarForStudentAsync(student.Id);
                calendar = calendarWrapper.Calendar;
                calendarEvent = calendar.Events.Single(e => e.Uid == id);
                calendar.Events.Remove(calendarEvent);
                AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            }
            else
            {
                calendar.Events.Remove(calendarEvent);
                AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);

                var organizer = await GetService<IStudentService>().GetStudentByEmailAsync(organizerEmail);

                calendarWrapper = await GetCalendarForStudentAsync(organizer.Id);
                calendar = calendarWrapper.Calendar;
                calendarEvent = calendar.Events.SingleOrDefault(e => e.Uid == id);

                if (calendarEvent == null)
                {
                    return Json("OK");
                }

                calendarEvent.Attendees.Clear();
                calendarEvent.Description = null;
                AgendaHub.NotifyUserCalendarChanged(calendarWrapper.Student.Id);
                await calendar.SaveAsync(calendarWrapper.CalendarFilePath);
            }

            return Json("OK");
        }

        public async Task<ActionResult> HasEvent(int studentId, long? timeStamp)
        {
            var student = await GetCurrentStudentAsync();
            var calendarWrapper = await GetCalendarForStudentAsync(studentId);
            var calendar = calendarWrapper.Calendar;
            var dateTime = timeStamp.HasValue
                ? DateTimeHelper.FromUnixTimeStampMilliseconds(timeStamp.Value)
                : DateTime.UtcNow;

            var calDateTime = new CalDateTime(dateTime);
            var occurrences = calendar.GetOccurrences(dateTime);
            var events = (from occurrence in occurrences
                          where occurrence.Period.Contains(calDateTime)
                          select occurrence.Source).OfType<Event>();

            var hasEvent =
                events.Any(calEvent =>
                    calEvent.Attendees.Any(a =>
                        a.Members.Any(m =>
                            m.Equals(
                                student.Email,
                                StringComparison.OrdinalIgnoreCase))));

            return Json(hasEvent);
        }

        [HttpPost]
        public async Task<ActionResult> SetMaxScheduleTime(string maxScheduleTime)
        {
            var time = XmlConvert.ToTimeSpan(maxScheduleTime);
            var calendarService = GetService<ICalendarService>();
            var student = await GetCurrentStudentAsync();
            // Todo: move this to the calendar service
            var calendar = await calendarService.GetCalendarByStudentIdAsync(student.Id) ??
                await CreateCalendarForStudentAsync(student);

            await calendarService.SetMaxScheduleTimeForCalendarAsync(
                calendar.Id,
                time);

            return Json("OK");
        }

        private class CalendarFileWrapper
        {
            public CalendarFileWrapper(ICalendar calendar, StudentDataTransfer student, string calendarFilePath)
            {
                Calendar = calendar;
                Student = student;
                CalendarFilePath = calendarFilePath;
            }

            public ICalendar Calendar { get; }

            public StudentDataTransfer Student { get; }

            public string CalendarFilePath { get; }
        }

        #region Helpers

        private void SplitAndMergeEvent(IEvent calendarEvent, Occurrence occurrence, IDateTime periodStart, IDateTime periodEnd,
            ICalendar calendar)
        {
            var organizer = calendarEvent.Organizer;
            var occurrenceStartTime = occurrence.Period.StartTime;
            var occurrenceEndTime = occurrence.Period.EndTime;

            if (periodStart.GreaterThan(occurrenceStartTime) && periodEnd.Equals(occurrenceEndTime))
            {
                calendarEvent = new Event
                {
                    Name = calendarEvent.Name,
                    Start = occurrenceStartTime,
                    End = periodStart,
                    Organizer = organizer
                };

                MergeAdjacentEvents(calendar.Events, calendarEvent);
                calendar.Events.Add(calendarEvent);
            }
            else if (periodEnd.LessThan(occurrenceEndTime) && periodStart.Equals(occurrenceStartTime))
            {
                calendarEvent = new Event
                {
                    Name = calendarEvent.Name,
                    Start = periodEnd,
                    End = occurrenceEndTime,
                    Organizer = organizer
                };

                MergeAdjacentEvents(calendar.Events, calendarEvent);
                calendar.Events.Add(calendarEvent);
            }
            else
            {
                var end = occurrenceEndTime;

                calendarEvent = new Event
                {
                    Name = calendarEvent.Name,
                    Start = occurrenceStartTime,
                    End = periodStart,
                    Organizer = organizer
                };

                MergeAdjacentEvents(calendar.Events, calendarEvent);
                calendar.Events.Add(calendarEvent);

                calendarEvent = new Event
                {
                    Name = calendarEvent.Name,
                    Start = periodEnd,
                    End = end,
                    Organizer = organizer
                };

                MergeAdjacentEvents(calendar.Events, calendarEvent);
                calendar.Events.Add(calendarEvent);
            }
        }

        private void MergeAdjacentEvents(ICollection<IEvent> events, IEvent targetEvent)
        {
            var availableEvents = events
                .Where(e => e.Attendees.Count == 0)
                .ToList();

            foreach (var @event in availableEvents)
            {
                if (@event.Start.AsUtc.Equals(targetEvent.End.AsUtc))
                {
                    targetEvent.End = @event.End;
                    events.Remove(@event);
                }
                else if (@event.End.AsUtc.Equals(targetEvent.Start.AsUtc))
                {
                    targetEvent.Start = @event.Start;
                    events.Remove(@event);
                }
            }
        }

        private IList<IRecurrencePattern> GetRecurrencePatterns(RecurrenceMode mode, DayOfWeek? recurrenceDay)
        {
            var recurrencePattern = new RecurrencePattern((FrequencyType)mode, 1);

            if (mode == RecurrenceMode.Daily)
            {
                return new List<IRecurrencePattern>
                {
                    recurrencePattern
                };
            }

            if (!recurrenceDay.HasValue)
            {
                throw new InvalidOperationException($"Recurrence mode '{mode}' must have a recurrence day");
            }

            recurrencePattern.ByDay = new List<IWeekDay>
            {
                new WeekDay(recurrenceDay.Value)
            };

            return new List<IRecurrencePattern>
            {
                recurrencePattern
            };
        }

        private async Task<CalendarFileWrapper> GetCalendarForCurrentStudentAsync()
        {
            var student = await GetCurrentStudentAsync();

            return await GetCalendarForStudentAsync(student);
        }

        private async Task<CalendarFileWrapper> GetCalendarForStudentAsync(int studentId)
        {
            var student = await GetService<IStudentService>().GetStudentByIdAsync(studentId);

            return await GetCalendarForStudentAsync(student);
        }

        private async Task<CalendarFileWrapper> GetCalendarForStudentAsync(StudentDataTransfer student)
        {
            var calendarDataTransfer = await GetService<ICalendarService>().GetCalendarByStudentIdAsync(student.Id) ??
                               await CreateCalendarForStudentAsync(student);
            var calendars = Calendar.LoadFromFile(calendarDataTransfer.FilePath);

            return new CalendarFileWrapper(
                calendars.First(),
                student,
                calendarDataTransfer.FilePath);
        }

        private static async Task<CalendarDataTransfer> CreateCalendarForStudentAsync(StudentDataTransfer student)
        {
            var calendarFolderPath = FileSystemHelper.GetAbsolutePath($@"App_Data/Calendars/{student.Email}");
            var calendarFilePath = $@"{calendarFolderPath}/calendar.ics";

            FileSystemHelper.CreateFolder(calendarFolderPath);

            var calendarDataTransfer = new CalendarDataTransfer
            {
                FilePath = calendarFilePath,
                Student = student
            };

            var calendar = new Calendar();

            await calendar.SaveAsync(calendarFilePath);
            return await GetService<ICalendarService>().CreateCalendarAsync(calendarDataTransfer);
        }

        private static object ParseOccurrenceToJson(Occurrence occurrence)
        {
            var calendarEvent = occurrence.Source as Event;

            if (calendarEvent == null)
            {
                throw new ArgumentException(
                    $"'{nameof(occurrence)}.{nameof(occurrence.Source)}' must be of type '{typeof(Event)}'");
            }

            var occurrenceStart = occurrence.Period.StartTime.AsUtc;
            var occurrenceEnd = occurrenceStart.Add(occurrence.Period.Duration);
            
            var isBusy = calendarEvent.Attendees.Any() && calendarEvent.Attendees.First().Members.Any();
            var color = isBusy ? BusyEventBackColor : AvailableEventBackColor;
            var title = isBusy ? calendarEvent.Description : Resource.AvailableEventTitle;

            return new
            {
                id = calendarEvent.Uid,
                start = occurrenceStart,
                end = occurrenceEnd,
                isAllDay = calendarEvent.IsAllDay,
                title,
                color,
                isBusy
            };
        }

        #endregion
    }
}