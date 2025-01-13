
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Fall2024_FinalProject.Models

{
    public class FighterFight
    {
        [Key]
        public int Id { get; set; }

        public int FighterId { get; set; }
        public Fighter? Fighter { get; set; }

        public int FightId { get; set; }
        public Fight? Fight { get; set; }
    }
}
