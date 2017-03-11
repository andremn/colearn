using FinalProject.Model;
using FinalProject.Shared.Expressions;

namespace FinalProject.Common
{
    public class TypeMapperConfiguration
    {
        public static void Configure()
        {
            TypeMapper.AddMap<AnswerDataTransfer, Answer>();
            TypeMapper.AddMap<AnswerRatingDataTransfer, AnswerRatingDataTransfer>();
            TypeMapper.AddMap<CalendarDataTransfer, Calendar>();
            TypeMapper.AddMap<InstitutionDataTransfer, Institution>();
            TypeMapper.AddMap<InstitutionRequestDataTransfer, InstitutionRequest>();
            TypeMapper.AddMap<PreferenceDataTransfer, Preference>();
            TypeMapper.AddMap<QuestionDataTransfer, Question>();
            TypeMapper.AddMap<StudentDataTransfer, Student>();
            TypeMapper.AddMap<TagAcceptedDataTransfer, Tag>();
            TypeMapper.AddMap<TagRequestDataTransfer, TagRequest>();
            TypeMapper.AddMap<TextAnswerDataTransfer, Answer>();
            TypeMapper.AddMap<VideoAnswerDataTransfer, Answer>();
        }
    }
}