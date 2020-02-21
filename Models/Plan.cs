using System.ComponentModel.DataAnnotations;

namespace DotnetBelt.Models {
    public class Plan {
        [Key]
        public int PlanId { get; set; }
        public int OccurrenceId { get; set; }
        public int UserId { get; set; }
        public Occurrence O { get; set; }
        public User U { get; set; }
    }
}