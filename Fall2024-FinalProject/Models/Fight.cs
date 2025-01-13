using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fall2024_FinalProject.Models
{
	public class Fight
	{

		[Key]
		public int Id { get; set; }

        [Required(ErrorMessage = "Please select a Date and Time.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Map Error: Longitude")]
        public float Longitude { get; set; }
        [Required(ErrorMessage = "Map Error: Latitude")]
        public float Latitude { get; set; }

        [Required(ErrorMessage = "Please Select a Location")]
        public string Location { get; set; }
        //[Required]
        //public string State { get; set; }

        [Required]
        [DifferentFighters]
        public int Fighter1Id { get; set; }

        [Required]
        [DifferentFighters]
        public int Fighter2Id { get; set; }

        [Required]
        [ValidOdds]
        public int Fighter1Odds { get; set; }

        [Required]
        [ValidOdds]
        public int Fighter2Odds { get; set; }

        public List<Bet> Bets { get; set; } = new List<Bet>();

        public string? Summary { get; set; }
        public int? RoundsFought { get; set; }
        public string? Winner { get; set; }

    }
    public class DifferentFighters : ValidationAttribute 
    { 
        protected override ValidationResult IsValid(object value, ValidationContext validationContext) 
        { 
            var fight = (Fight)validationContext.ObjectInstance;
            if (fight.Fighter1Id == fight.Fighter2Id) 
            { 
                return new ValidationResult("A fighter cannot fight themselves."); 
            } 
            return ValidationResult.Success; 
        }
    }
    public class ValidOddsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int odds)
            {
                // Check if odds are in the valid ranges
                if ((odds >= -5000 && odds <= -100) || (odds >= 100 && odds <= 5000))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Odds must be between -5000 and -100, or between 100 and 5000.");
        }
    }

}