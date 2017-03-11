using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.Model
{
    public enum InstitutionRequestStatus
    {
        Approved = 0,

        Declined = 1,

        Pending = 2
    }

    public class InstitutionRequest : IModel
    {
        public DateTime CreatedTime { get; set; }

        [Key]
        public int Id { get; set; }

        public int InstitutionCode { get; set; }

        public string InstitutionFullName { get; set; }

        public string InstitutionShortName { get; set; }

        public string OwnerEmail { get; set; }

        public InstitutionRequestStatus Status { get; set; }
    }
}