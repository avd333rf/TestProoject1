using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CitizenDatabase.Models
{
    public class SearchRequest : IValidatableObject
    {
        [MaxLength(256)]
        public string FullName { get; set; }

        [MaxLength(14)]
        public string Snils { get; set; }

        [MaxLength(12)]
        public string Inn { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DeathDate { get; set; }

        public int? PageSize { get; set; }

        public int? PageNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            FullName = FullName?.Trim();

            yield break;
        }
    }
}
