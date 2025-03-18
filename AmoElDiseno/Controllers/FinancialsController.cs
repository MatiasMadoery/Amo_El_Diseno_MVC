using AmoElDiseno.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmoElDiseno.Controllers
{
    public class FinancialsController : Controller
    {
        private readonly AppDbContext _context;
        
        public FinancialsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Balances");
        }


        // GET: Financials/Balances
        public async Task<IActionResult> Balances(PeriodType periodType = PeriodType.Mensual, int year = 2025, int? month = null, int? semester = null)
        {
            // Determinar el rango de fecha en función del periodo elegido
            DateTime startDate, endDate;

            switch (periodType)
            {
                case PeriodType.Mensual:
                    if (!month.HasValue)
                        month = DateTime.Now.Month;
                    startDate = new DateTime(year, month.Value, 1);
                    endDate = startDate.AddMonths(1).AddTicks(-1);
                    break;
                case PeriodType.Semestral:
                    if (!semester.HasValue)
                        semester = (DateTime.Now.Month <= 6) ? 1 : 2;
                    if (semester == 1)
                    {
                        startDate = new DateTime(year, 1, 1);
                        endDate = new DateTime(year, 6, 30, 23, 59, 59);
                    }
                    else // segundo semestre
                    {
                        startDate = new DateTime(year, 7, 1);
                        endDate = new DateTime(year, 12, 31, 23, 59, 59);
                    }
                    break;
                case PeriodType.Anual:
                default:
                    startDate = new DateTime(year, 1, 1);
                    endDate = new DateTime(year, 12, 31, 23, 59, 59);
                    break;
            }

            // Consultar "ingresos": supondremos que PaymentDelivery representa ingresos (el monto pagado en cada entrega)
            var incomes = await _context.PaymentDeliveries
                              .Where(pd => pd.Date >= startDate && pd.Date <= endDate)
                              .Include(pd => pd.Order)
                              .ToListAsync();

            // Consultar "egresos"
            var expenses = await _context.Expenses
                              .Where(e => e.TransactionDate >= startDate && e.TransactionDate <= endDate)
                              .ToListAsync();

            var totalIncome = incomes.Sum(i => i.Amount ?? 0);
            var totalExpense = expenses.Sum(e => e.Amount ?? 0);

            var viewModel = new BalanceViewModel
            {
                Incomes = incomes,
                Expenses = expenses,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                SelectedPeriodType = periodType,
                SelectedYear = year,
                SelectedMonth = month,
                SelectedSemester = semester
            };

            return View("~/Views/Balances/Index.cshtml", viewModel);

        }
    }
}
