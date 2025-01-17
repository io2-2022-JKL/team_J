﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VaccinationSystem.Models
{
    public class OpeningHours
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public TimeSpan From { get; set; }
        [Required]
        public TimeSpan To { get; set; }
        [Required]
        public WeekDay WeekDay { get; set; }
        [ForeignKey("VaccinationCenter")]
        public virtual Guid VaccinationCenterId { get; set; }
        public virtual VaccinationCenter VaccinationCenter { get; set; }
    }
}
