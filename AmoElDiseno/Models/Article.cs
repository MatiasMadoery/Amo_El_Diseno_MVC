using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Article
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string? Name { get; set; }
        [Display(Name = "Precio")]
        public decimal? Price { get; set; }
    }
}
