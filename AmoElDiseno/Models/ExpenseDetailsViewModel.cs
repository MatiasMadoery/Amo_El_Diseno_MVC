namespace AmoElDiseno.Models
{
    public class ExpenseDetailsViewModel
    {
        public Expense Expense { get; set; } = new Expense();
        public IFormFile? Image { get; set; }
    }
}

