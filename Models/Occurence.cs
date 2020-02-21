using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotnetBelt.Models {
    public class Occurrence {
        [Key]
        public int OccurrenceId { get; set; }
        public User Creator { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [Display(Name = "Date & Time")]
        public DateTime OTime { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        [Display(Name = " ")]
        public string DurationUnit { get; set; }
        [Required]
        public string Description { get; set; }
        public List<Plan> Plans { get; set; }
    }
}