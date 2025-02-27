namespace AmoElDiseno.Models
{
    public class OrderDetailsViewModel
    {
        public Order? Order { get; set; }
        public PaymentDelivery NewPaymentDelivery { get; set; } = new PaymentDelivery();
    }

}
