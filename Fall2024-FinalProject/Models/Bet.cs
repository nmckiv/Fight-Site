using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fall2024_FinalProject.Models
{
    public class Bet
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }

        public int FightId { get; set; }
        public Fight? Fight { get; set; }

        // The fighter the user has chosen to bet on
        public int ChosenFighterId { get; set; }
        public Fighter? ChosenFighter { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Bet amount must be at least 1.")]
        public int BetAmount { get; set; }

        [Required]
        [Range(-5000, 5000, ErrorMessage = "Odds must be between -5000 and 5000.")]
        public int Odds { get; set; }

        public bool? IsWin { get; set; } 
    }
}
