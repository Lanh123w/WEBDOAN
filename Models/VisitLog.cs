using System;
using System.ComponentModel.DataAnnotations;

namespace WEBDOAN.Models
{
    public class VisitLog
    {
        public int Id { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        public string VisitorIP { get; set; }
    }
}
