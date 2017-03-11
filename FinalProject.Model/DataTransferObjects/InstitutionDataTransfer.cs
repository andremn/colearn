using System.Collections.Generic;

namespace FinalProject.Model
{
    public class InstitutionDataTransfer : IDataTransfer
    {
        public int Code { get; set; }

        public string FullName { get; set; }
        
        public int Id { get; set; }

        public IList<StudentDataTransfer> Moderators { get; set; }

        public IList<QuestionDataTransfer> Questions { get; set; }

        public string ShortName { get; set; }

        public IList<TagAcceptedDataTransfer> Tags { get; set; }
    }
}
