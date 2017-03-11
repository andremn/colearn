using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Extensions;
using FinalProject.DataAccess.Factory;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IPreferenceDataAccess : IDataAccess<int, Preference>
    {
        Task<Preference> GetByStudentIdOrDefaultAsync(int id);

        Task<Preference> GetDefaultAsync();
    }

    internal class PreferenceDataAccess : DataAccess<int, Preference>, IPreferenceDataAccess
    {
        public PreferenceDataAccess(IContextFactory factory)
            : base(factory)
        {
        }

        public override async Task<Preference> CreateAsync(Preference item)
        {
            if (item.Student != null)
            {
                Context.Students.Attach(item.Student);
            }

            if (item.Grade != null)
            {
                Context.Grades.Attach(item.Grade);
            }

            if (item.Institutions != null)
            {
                foreach (var institution in item.Institutions)
                {
                    Context.Institutions.Attach(institution);
                }
            }

            return await base.CreateAsync(item);
        }

        public override async Task<Preference> DeleteByIdAsync(int id)
        {
            var preference = await Context.Preferences
                    .Include(p => p.Student.InstructorTags)
                    .Include(p => p.Institutions)
                    .Include(p => p.Grade)
                    .SingleOrDefaultAsync(p => p.Id == id);

            return await DeleteAsync(preference);
        }

        public override async Task<IList<Preference>> GetAllAsync(IFilter<Preference> filter = null)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Preferences
                    .Include(p => p.Student.InstructorTags)
                    .Include(p => p.Grade)
                    .Include(p => p.Institutions);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public override async Task<Preference> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return await context.Preferences
                    .Include(p => p.Student.InstructorTags)
                    .Include(p => p.Institutions)
                    .Include(p => p.Grade)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Id == id);
            }
        }

        public override async Task<Preference> UpdateAsync(Preference item)
        {
            var preference = await Context.Preferences
                .Include(p => p.Institutions)
                .Include(p => p.Grade)
                .SingleAsync(p => p.Id == item.Id);

            if (item.Institutions != null)
            {
                var newInstitutions = item.Institutions
                    .Except(preference.Institutions, i => i.Id)
                    .ToList();

                var removedInstitutions = preference.Institutions
                    .Except(item.Institutions, i => i.Id)
                    .ToList();

                foreach (var institution in removedInstitutions)
                {
                    Context.Institutions.Attach(institution);
                    preference.Institutions.Remove(institution);
                }

                foreach (var institution in newInstitutions)
                {
                    Context.Institutions.Attach(institution);
                    preference.Institutions.Add(institution);
                }
            }

            if (item.Student != null)
            {
                Context.Students.Attach(item.Student);
            }

            if (item.Grade == null)
            {
                preference.Grade = null;
            }
            else if (preference.Grade == null ||
                item.Grade.Id != preference.Grade.Id)
            {
                Context.Grades.Attach(item.Grade);
                preference.Grade = item.Grade;
            }

            Context.Entry(preference).CurrentValues.SetValues(item);

            return await base.UpdateAsync(preference);
        }

        public async Task<Preference> GetByStudentIdOrDefaultAsync(int id)
        {
            var preference = await Context.Preferences
                    .Include(p => p.Student.InstructorTags)
                    .Include(p => p.Institutions)
                    .Include(p => p.Grade)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Student.Id == id);

            if (preference != null)
            {
                return preference;
            }

            preference = await GetDefaultAsync();
            preference.Student = await Context.Students
                .Include(s => s.InstructorTags)
                .AsNoTracking()
                .SingleAsync(s => s.Id == id);

            return preference;
        }

        public async Task<Preference> GetDefaultAsync()
        {
            return await Context.Preferences
                .Include(p => p.Student.InstructorTags)
                .Include(p => p.Institutions)
                .Include(p => p.Grade)
                .AsNoTracking()
                .SingleAsync(p => p.IsDefault);
        }
    }
}