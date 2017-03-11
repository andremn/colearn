using System.Collections.Generic;

namespace FinalProject.Model
{
    public class PreferenceDataTransfer : IDataTransfer
    {
        public int Id { get; set; }

        public float MinSimilarity { get; set; }

        public float MinRating { get; set; }

        public StudentDataTransfer Student { get; set; }

        public IList<InstitutionDataTransfer> Institutions { get; set; }

        public IList<TagAcceptedDataTransfer> Tags { get; set; }

        public GradeDataTransfer Grade { get; set; }

        public uint MaxGradeOrder { get; set; }
    }
}