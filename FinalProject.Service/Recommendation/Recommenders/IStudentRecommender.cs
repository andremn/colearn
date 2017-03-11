using System.Collections.Generic;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal interface IStudentRecommender
    {
        IList<RecommendedStudentDataTransfer> GetRecommenderStudentsForStudentPreferences(
            PreferenceDataTransfer studentPreferences);
    }
}
