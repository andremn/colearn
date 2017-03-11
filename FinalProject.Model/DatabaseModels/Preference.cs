using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public class Preference : IModel
    {
        [Key]
        public int Id { get; set; }

        public float MinSimilarity { get; set; }

        public float MinRating { get; set; }

        public virtual Student Student { get; set; }

        public virtual IList<Institution> Institutions { get; set; }

        public virtual Grade Grade { get; set; }

        public short GradeYear { get; set; }

        public bool IsDefault { get; set; }
    }
}