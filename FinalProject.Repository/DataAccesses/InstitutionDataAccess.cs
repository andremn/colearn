using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;

namespace FinalProject.DataAccess
{
    public interface IInstitutionDataAccess : IDataAccess<int, Institution>
    {
        Task<Institution> GetByCodeAsync(int code);
    }

    internal class InstitutionDataAccess : IInstitutionDataAccess
    {
        public async Task<long> CountAsync(IFilter<Institution> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Institutions.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<Institution> CreateAsync(Institution item)
        {
            using (var context = new DatabaseContext())
            {
                var institution = context.Institutions.Add(item);

                await context.SaveChangesAsync();
                return institution;
            }
        }

        public async Task<Institution> DeleteAsync(Institution item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<Institution> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var university = await context.Institutions.Include(i => i.Tags).SingleOrDefaultAsync(i => i.Id == id);

                university = context.Institutions.Remove(university);
                await context.SaveChangesAsync();
                return university;
            }
        }

        public async Task<IList<Institution>> GetAllAsync(IFilter<Institution> filter = null)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Institutions
                    .Include(i => i.Moderators)
                    .Include(i => i.Tags)
                    .Include(i => i.Questions);

                queryable = filter?.Apply(queryable) ?? queryable;
                return await queryable.ToListAsync();
            }
        }

        public async Task<Institution> GetByCodeAsync(int code)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.Institutions.Where(institution => institution.Code == code)
                            .Include(i => i.Tags)
                            .SingleOrDefaultAsync();
            }
        }

        public async Task<Institution> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return await context.Institutions.Include(i => i.Tags).SingleOrDefaultAsync(i => i.Id == id);
            }
        }

        public async Task<Institution> UpdateAsync(Institution item)
        {
            using (var context = new DatabaseContext())
            {
                var institution = await context.Institutions
                    .Include(i => i.Tags)
                    .SingleOrDefaultAsync(i => i.Id == item.Id);

                if (institution == null)
                {
                    return null;
                }

                institution.Tags.Clear();

                foreach (var tag in item.Tags)
                {
                    var existingTag = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == tag.Id);

                    if (existingTag != null)
                    {
                        context.Entry(existingTag).CurrentValues.SetValues(tag);
                    }
                    else
                    {
                        existingTag = new Tag { Text = tag.Text, Children = tag.Children };

                        existingTag = context.Tags.Add(existingTag);
                    }

                    institution.Tags.Add(existingTag);
                }

                context.Entry(institution).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return institution;
            }
        }
    }
}