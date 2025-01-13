using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Fall2024_FinalProject.Models
{
	public class User : IdentityUser
	{
		public string Name { get; set; }

		public int Balance { get; set; } = 1000;
        
		[Required]
        public string? Role { get; set; }

        public byte[]? ProfilePhoto { get; set; }
 
        public List<Bet> Bets { get; set; } = new List<Bet>();

    }

}

