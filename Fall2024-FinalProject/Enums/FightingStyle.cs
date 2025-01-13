using System;
using System.ComponentModel.DataAnnotations;

namespace Fall2024_FinalProject.Enums
{
    public enum FightingStyle
    {

        [Display(Name = "Capoiera")]
        Capoiera,

        [Display(Name = "Ranged Projectile")]
        RangedProjectile,

        [Display(Name = "Melee")]
        Melee,

        [Display(Name = "Pyrotechnics")]
        Pyrotechnics,

        [Display(Name = "Wizardry")]
        Wizardry

    }
}
