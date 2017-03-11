using System.Collections.Generic;
using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal interface IPreferenceModelBuilder
    {
        IList<PreferenceAttribute> GetPreferenceAttributes(PreferenceDataTransfer preferences);
    }
}