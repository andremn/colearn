using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface ICalendarDataAccess : IDataAccess<int, Calendar>
    {
        Task<Calendar> GetByStudentIdAsync(int studentId);
    }

    internal class CalendarDataAccess : ICalendarDataAccess
    {
        public async Task<long> CountAsync(IFilter<Calendar> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.CalendarFiles.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<Calendar> CreateAsync(Calendar item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Student != null)
                {
                    context.Students.Attach(item.Student);
                }

                var calendar = context.CalendarFiles.Add(item);

                await context.SaveChangesAsync();
                return calendar;
            }
        }

        public async Task<Calendar> DeleteAsync(Calendar item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<Calendar> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var calendar = await context.CalendarFiles
                    .Include(c => c.Student)
                    .SingleOrDefaultAsync(c => c.Id == id);

                calendar = context.CalendarFiles.Remove(calendar);
                await context.SaveChangesAsync();
                return calendar;
            }
        }

        public async Task<IList<Calendar>> GetAllAsync(IFilter<Calendar> filter = null)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.CalendarFiles
                    .Include(c => c.Student);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<Calendar> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return await context.CalendarFiles
                    .Include(c => c.Student)
                    .SingleOrDefaultAsync(c => c.Id == id);
            }
        }

        public async Task<Calendar> UpdateAsync(Calendar item)
        {
            using (var context = new DatabaseContext())
            {
                var calendar = await context.CalendarFiles
                    .Include(c => c.Student)
                    .SingleOrDefaultAsync(c => c.Id == item.Id);

                if (item.Student != null)
                {
                    item.Student = calendar.Student;
                }

                context.Entry(calendar).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return calendar;
            }
        }

        public async Task<Calendar> GetByStudentIdAsync(int studentId)
        {
            using (var context = new DatabaseContext())
            {
                return await context.CalendarFiles
                    .Include(c => c.Student)
                    .SingleOrDefaultAsync(c => c.Student.Id == studentId);
            }
        }
    }
}