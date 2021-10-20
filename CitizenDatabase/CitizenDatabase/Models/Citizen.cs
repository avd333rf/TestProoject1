using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CitizenDatabase.Models
{
    [Index(nameof(FullName))]
    [Index(nameof(Snils), IsUnique = true)]
    [Index(nameof(Inn), IsUnique = true)]
    [Index(nameof(BirthDate))]
    [Index(nameof(DeathDate))]
    public class Citizen : IValidatableObject
    {
        public Int64 Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string FullName { get; set; }

        [MaxLength(14), MinLength(14)]
        public string Snils { get; set; }

        [MaxLength(12), MinLength(12)]
        public string Inn { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DeathDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            FullName = FullName.Trim();

            //TODO SNILS validation
            //ХХХ-ХХХ-ХХХ YY

            //TODO INN validation
            //XXXXXXXXXXXX

            yield break;
        }
    }
}
