namespace AmoElDiseno.Models
{
    public class PaymentDelivery
    {      
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public decimal? Amount { get; set; }        

    }
}
