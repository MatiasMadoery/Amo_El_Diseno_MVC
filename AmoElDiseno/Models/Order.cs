namespace AmoElDiseno.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? Date { get; set; }
        public string? Details { get; set; }        
        public OrderStatus Status { get; set; }
        public bool? IsPaid { get; set; }
        public decimal? Total { get; set; }

        //Relation with Customer
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }



    }
}
