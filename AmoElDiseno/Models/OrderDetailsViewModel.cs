using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class OrderDetailsViewModel
    {
        public Order? Order { get; set; }
        public PaymentDelivery NewPaymentDelivery { get; set; } = new PaymentDelivery();
        public IFormFile? Image { get; set; }
        [Display(Name = "Cliente")]
        public int CustomerId { get; set; }
    }

}
