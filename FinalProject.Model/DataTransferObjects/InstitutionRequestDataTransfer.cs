using System;

namespace FinalProject.Model
{
    public class InstitutionRequestDataTransfer : IDataTransfer
    {
        public DateTime CreatedTime { get; set; }
        
        public int Id { get; set; }

        public int InstitutionCode { get; set; }

        public string InstitutionFullName { get; set; }

        public string InstitutionShortName { get; set; }

        public string OwnerEmail { get; set; }

        public InstitutionRequestStatus Status { get; set; }
    }
}
