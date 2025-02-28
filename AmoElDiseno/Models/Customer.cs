using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string? Name { get; set; }
        [Display(Name = "Apellido")]
        public string? LastName { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }
        [Display(Name = "Provincia")]
        public string? Province { get; set; }
        [Display(Name = "Ciudad")]
        public string? City { get; set; }
        [Display(Name = "Dirección")]
        public string? Address { get; set; }


        [Display(Name = "Cliente")]
        public string? FullName => $"{Name} {LastName}";


        //Relation with Order
        public ICollection<Order>? Orders { get; set; }  
    }
}
