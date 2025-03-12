using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Display(Name = "Número de Pedido")]
        public string? OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [Display(Name = "Fecha")]
        public DateTime? Date { get; set; } = DateTime.Now;
        [Display(Name = "Fecha de Entrega")]
        public DateTime? DeliveryDate
        {
            get
            {
                return Date?.AddDays(30);
            }
            set
            {
            }
        }
        [Display(Name = "Detalles")]
        public string? Details { get; set; }
        [Display(Name = "Estado")]
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
        public string? ImagePath { get; set; }
    }
}
