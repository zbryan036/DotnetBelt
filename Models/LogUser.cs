using System.ComponentModel.DataAnnotations;

namespace DotnetBelt.Models {
    public class LogUser {
        [Required]
        [Display(Name="Email")]
        [DataType(DataType.EmailAddress)]
        public string LEmail { get; set; }
        [Required]
        [Display(Name="Password")]
        [DataType(DataType.Password)]
        public string LPassword { get; set; }
    }
}