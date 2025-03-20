using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class PaymentDelivery
    {      
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        [Display(Name = "Fecha")]
        public DateTime? Date { get; set; } = DateTime.Now;
        [Display(Name = "Monto")]
        public decimal? Amount { get; set; } = 0;

    }
}
