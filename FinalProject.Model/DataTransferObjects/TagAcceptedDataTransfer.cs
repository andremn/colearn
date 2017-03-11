using System.Collections.Generic;

namespace FinalProject.Model
{
    public class TagAcceptedDataTransfer : TagDataTransfer
    {
        public IList<TagAcceptedDataTransfer> Children { get; set; }

        public InstitutionDataTransfer Institution { get; set; }

        public IList<StudentDataTransfer> Instructors { get; set; }

        public IList<TagAcceptedDataTransfer> Parents { get; set; }

        public IList<QuestionDataTransfer> Questions { get; set; }
    }
}
