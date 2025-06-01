using System.ComponentModel.DataAnnotations;

namespace Vjezba.Model
{
    public enum GameExcitement
    {
        [Display(Name = "Very Boring")]
        VeryBoring = 1,
        
        [Display(Name = "Boring")]
        Boring = 2,
        
        [Display(Name = "Average")]
        Average = 3,
        
        [Display(Name = "Interesting")]
        Interesting = 4,
        
        [Display(Name = "Exciting")]
        Exciting = 5,
        
        [Display(Name = "Very Exciting")]
        VeryExciting = 6
    }
}