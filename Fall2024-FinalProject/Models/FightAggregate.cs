using System.ComponentModel.DataAnnotations;

namespace Fall2024_FinalProject.Models
{
    public class FightAggregate
    {
        [Required]
        public Fight Fight { get; set; }
        [Required]
        public Fighter Fighter1 { get; set; }
        [Required]
        public Fighter Fighter2 { get; set; }

    }
}
