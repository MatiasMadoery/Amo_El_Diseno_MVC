using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class OrderDetailsViewModel
    {
        public Order? Order { get; set; }
        public PaymentDelivery? NewPaymentDelivery { get; set; } = new PaymentDelivery();
        public IFormFile? Image { get; set; }
        public int CustomerId { get; set; }
        [Display(Name = "Fecha de Entrega")]
        public DateTime? DeliveryDate { get; set; }
    }

}
