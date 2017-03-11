using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FinalProject.DataAccess.Filters;
using FinalProject.Model;
using FinalProject.Shared.Extensions;

namespace FinalProject.DataAccess
{
    public interface ITagDataAccess : IDataAccess<int, Tag>
    {
        Task<IList<Tag>> GetAllForInstitutionAsync(int institutionId);

        Task<Tag> GetTagByNameForInstitutionAsync(string text, int institutionId);
    }

    internal class TagDataAccess : ITagDataAccess
    {
        public async Task<long> CountAsync(IFilter<Tag> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Tags.AsQueryable();

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.CountAsync();
            }
        }

        public async Task<Tag> CreateAsync(Tag item)
        {
            using (var context = new DatabaseContext())
            {
                if (item.Institution != null)
                {
                    context.Institutions.Attach(item.Institution);
                }

                var tag = context.Tags.Add(item);

                await context.SaveChangesAsync();
                return tag;
            }
        }

        public async Task<Tag> DeleteAsync(Tag item)
        {
            return await DeleteByIdAsync(item.Id);
        }

        public async Task<Tag> DeleteByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                var tag =
                    await
                        context.Tags.Include(t => t.Parents)
                            .Include(t => t.Children)
                            .Include(t => t.Institution)
                            .Include(t => t.Questions)
                            .SingleOrDefaultAsync(institution => institution.Id == id);

                tag = context.Tags.Remove(tag);
                await context.SaveChangesAsync();
                return tag;
            }
        }

        public async Task<IList<Tag>> GetAllAsync(IFilter<Tag> filter)
        {
            using (var context = new DatabaseContext())
            {
                var queryable = context.Tags.Include(tag => tag.Parents)
                    .Include(tag => tag.Children)
                    .Include(t => t.Institution)
                    .Include(t => t.Questions);

                queryable = filter?.Apply(queryable) ?? queryable;

                return await queryable.ToListAsync();
            }
        }

        public async Task<IList<Tag>> GetAllForInstitutionAsync(int institutionId)
        {
            using (var context = new DatabaseContext())
            {
                var institution =
                    await context.Institutions.Include(i => i.Tags).SingleOrDefaultAsync(i => i.Id == institutionId);

                return institution?.Tags ?? new List<Tag>();
            }
        }

        public async Task<Tag> GetByIdAsync(int id)
        {
            using (var context = new DatabaseContext())
            {
                return
                    await
                        context.Tags.Include(tag => tag.Parents)
                            .Include(tag => tag.Children)
                            .Include(t => t.Institution)
                            .Include(t => t.Questions)
                            .SingleOrDefaultAsync(tag => tag.Id == id);
            }
        }

        public async Task<Tag> GetTagByNameForInstitutionAsync(string text, int institutionId)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            using (var context = new DatabaseContext())
            {
                var tags = await context.Tags
                    .Include(tag => tag.Parents)
                    .Include(tag => tag.Children)
                    .Include(t => t.Institution)
                    .Include(t => t.Questions)
                    .Where(t => t.Institution.Id == institutionId)
                    .ToListAsync();

                return tags
                    .SingleOrDefault(t => t.Text.RemoveDiacritics()
                        .Equals(text.RemoveDiacritics(), 
                            StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<Tag> UpdateAsync(Tag item)
        {
            using (var context = new DatabaseContext())
            {
                var tag =
                    await
                        context.Tags.Include(t => t.Parents)
                            .Include(t => t.Children)
                            .Include(t => t.Institution)
                            .SingleOrDefaultAsync(i => i.Id == item.Id);

                if (tag == null)
                {
                    return null;
                }

                tag.Children.Clear();

                foreach (var child in item.Children)
                {
                    var existingChild = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == child.Id);

                    if (existingChild != null)
                    {
                        context.Entry(existingChild).CurrentValues.SetValues(child);
                    }
                    else
                    {
                        existingChild = new Tag
                        {
                            Text = child.Text,
                            Parents = child.Parents,
                            Children = child.Children
                        };

                        existingChild = context.Tags.Add(existingChild);
                    }

                    tag.Children.Add(existingChild);
                }

                tag.Parents.Clear();

                foreach (var parent in item.Parents)
                {
                    var existingParent = context.Tags.Include(t => t.Children).SingleOrDefault(t => t.Id == parent.Id);

                    if (existingParent != null)
                    {
                        context.Entry(existingParent).CurrentValues.SetValues(parent);
                    }
                    else
                    {
                        existingParent = new Tag
                        {
                            Text = parent.Text,
                            Children = parent.Children,
                            Parents = parent.Parents
                        };

                        existingParent = context.Tags.Add(existingParent);
                    }

                    tag.Parents.Add(existingParent);
                }

                context.Entry(tag).CurrentValues.SetValues(item);
                await context.SaveChangesAsync();
                return tag;
            }
        }
        
        internal class TagEqualityComparer : IEqualityComparer<Tag>
        {
            public bool Equals(Tag x, Tag y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode(Tag obj)
            {
                return obj?.Id ?? base.GetHashCode();
            }
        }
    }
}