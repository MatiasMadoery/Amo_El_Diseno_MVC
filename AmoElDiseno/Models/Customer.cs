using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Telefono")]
        public string? Phone { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }


        [Display(Name = "Cliente")]
        public string? FullName => $"{Name} {LastName}";


        //Relation with Order
        public ICollection<Order> Orders { get; set; }  
    }
}
