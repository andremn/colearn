using FinalProject.Model;

namespace FinalProject.Service.Recommenders
{
    internal abstract class PreferenceAttribute
    {
        public abstract float GetValueInStudentProfile(StudentDataTransfer student);
    }
}
