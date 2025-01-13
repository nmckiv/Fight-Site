using System;
using System.ComponentModel.DataAnnotations;
using Fall2024_FinalProject.Enums;
using Newtonsoft.Json;

namespace Fall2024_FinalProject.Models
{
	public class Fighter
	{
        [Key]
		public int Id { get; set; }

		[Required]
		public required string Name { get; set; }

		[Required]
        [Range(1, 10,ErrorMessage = "Health must be an integer between 1 and 10.")]
		public required int Health { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Strength must be an integer between 1 and 10.")]
        public required int Strength { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Agility must be an integer between 1 and 10.")]
        public required int Agility { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Intelligence must be an integer between 1 and 10.")]
        public required int Intelligence { get; set; }

        [Required(ErrorMessage = "Please select a fighting style.")]
        [Display(Name = "Fighting Style")]
        public required FightingStyle FighterStyle { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Weight in pounds must be positive. Max weight is 1000 lbs.")]
        public required int Weight { get; set; }

        public byte[]? FighterPhoto { get; set; }

        public string Serialize() { 
            return JsonConvert.SerializeObject(new { Name, Health, Strength, Agility, Intelligence, FighterStyle, Weight }); 
        }

    }
}