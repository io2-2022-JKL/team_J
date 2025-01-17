﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VaccinationSystem.Models
{
    public class TimeSlot
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime From { get; set; }
        [Required]
        public DateTime To { get; set; }
        [ForeignKey("Doctor")]
        public virtual Guid? DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; }
        [Required]
        public bool IsFree { get; set; }
        [Required]
        public bool Active { get; set; }
    }
}
