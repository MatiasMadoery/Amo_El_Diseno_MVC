namespace AmoElDiseno.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime? Date { get; set; }
        public string? Details { get; set; }        
        public OrderStatus Status { get; set; }        
        public decimal? Total { get; set; }
        public ICollection<PaymentDelivery>? PaymentDeliveries { get; set; }
        public decimal? PendingBalance
        {
            get
            {
                decimal totalPaid = PaymentDeliveries?.Sum(pd => pd.Amount) ?? 0;
                return (Total ?? 0) - totalPaid;
            }
        }


    }
}
