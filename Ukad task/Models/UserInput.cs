using System.ComponentModel.DataAnnotations;

namespace Ukad_task.Models
{
    public class UserInput
    {
        [Required]
        [Url(ErrorMessage = "URL is not valid")]
        [Display(Name = "URL")]
        public string URL { get; set; }

        [Required]
        [Range(0.1, 100, ErrorMessage = "Incorect number. Please enter from 0.1 to 100.0")]
        [Display(Name = "Timeout")]
        public float Timeout { get; set; }

        [Display(Name = "Include inner sitemaps")]
        public bool IncludeInnerSitemaps { get; set; }
    }
}