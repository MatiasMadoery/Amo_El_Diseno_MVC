using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Expense
    {
        public int Id { get; set; }
        [Display(Name = "Fecha")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        [Display(Name = "Cuenta")]
        public AccountingAccount AccountingAccount { get; set; } 
        [Display(Name = "Concepto")]
        public string? Concept { get; set; } 
        [Display(Name = "Importe")]
        public decimal? Amount { get; set; } 
        [Display(Name = "Receptor")]
        public string? Recipient { get; set; } 
        [Display(Name = "Comprobante de pago")]
        public string? PaymentReceiptImagePath { get; set; } 
    }

}
