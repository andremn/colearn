using System.Collections.Generic;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal class PreferenceModelBuilder : IPreferenceModelBuilder
    {
        public PreferenceModelBuilder()
        {
        }

        public IList<PreferenceAttribute> GetPreferenceAttributes(PreferenceDataTransfer preferences)
        {
            if (preferences == null)
            {
                return null;
            }

            var preferenceAttributes = new List<PreferenceAttribute>
            {
                new TagAttribute(preferences.Tags ?? new TagAcceptedDataTransfer[0]),
                new AvgRatingAttribute(preferences.MinRating),
                new InstitutionAttribute(preferences.Institutions),
                new GradeAttribute(preferences.Grade)
            };

            return preferenceAttributes;
        }
    }
}
