namespace AmoElDiseno.Models
{
    public class BalanceViewModel
    {
        //Lista de ingresos y egresos
        public IEnumerable<PaymentDelivery> Incomes { get; set; } = new List<PaymentDelivery>();
        public IEnumerable<Expense> Expenses { get; set; } = new List<Expense>();

        //Total y saldo neto
        public decimal TotalIncome {  get; set; }
        public decimal TotalExpense { get; set;}
        public decimal NetBalance => TotalIncome - TotalExpense;

        //Parametros de filtrado
        public PeriodType SelectedPeriodType { get; set; }
        public int SelectedYear { get; set; }
        //Perido mensual
        public int? SelectedMonth { get; set; }
        //Periodo semestral
        public int? SelectedSemester { get; set; }

    }
}
