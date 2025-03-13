using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;
using static NuGet.Packaging.PackagingConstants;

namespace AmoElDiseno.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index(string searchString, AccountingAccount? searchAccount, int page = 1, int pageSize = 5)
        {
            // Inicia la consulta con la entidad Expenses
            IQueryable<Expense> expensesQuery = _context.Expenses;

            // Filtra si se proporcionó un término de búsqueda para Recipient o Concept
            if (!string.IsNullOrEmpty(searchString))
            {
                expensesQuery = expensesQuery.Where(e =>
                    e.Recipient!.Contains(searchString) ||
                    e.Concept!.Contains(searchString));
            }

            // Filtra si se proporcionó una cuenta contable
            if (searchAccount.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.AccountingAccount == searchAccount);
            }

            // Ordena de forma descendente por TransactionDate (puedes ajustar esto si prefieres otro orden)
            expensesQuery = expensesQuery.OrderByDescending(e => e.TransactionDate);

            // Obtiene el total de registros para la paginación
            int totalExpenses = await expensesQuery.CountAsync();

            // Aplica la paginación
            var expensesPaged = await expensesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Crea el objeto Pager<Expense>
            var pager = new Pager<Expense>(expensesPaged, totalExpenses, page, pageSize);

            // Mantiene el término de búsqueda y la cuenta contable en la vista
            ViewData["searchString"] = searchString;
            ViewData["searchAccount"] = searchAccount;

            return View(pager);
        }


        // GET: Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            var expense = new Expense();
            
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>());

            return View(expense);
        }


        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense, IFormFile PaymentReceiptImage)
        {
            if (ModelState.IsValid)
            {
                // Si se ha enviado una imagen, guardarla en disco
                if (PaymentReceiptImage != null && PaymentReceiptImage.Length > 0)
                {
                    // Obtén el nombre de archivo y construye la ruta
                    var fileName = Path.GetFileName(PaymentReceiptImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "expenses", fileName);

                    // Guarda el archivo en la ruta especificada
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await PaymentReceiptImage.CopyToAsync(stream);
                    }

                    // Asigna la ruta relativa a la propiedad PaymentReceiptImagePath
                    expense.PaymentReceiptImagePath = $"/img/expenses/{fileName}";
                }

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si no es válido, repuebla la lista de cuentas contables para la vista de error
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>(), expense.AccountingAccount);
            return View(expense);
        }


        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            // Rellenamos el ViewBag para el dropdown utilizando el enum AccountingAccount.
            ViewBag.Accounts = new SelectList(
                Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>(),
                expense.AccountingAccount);

            return View(expense);
        }


        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense, IFormFile PaymentReceiptImage)
        {
            if (id != expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Si se ha subido un nuevo archivo, actualizamos el comprobante de pago
                    if (PaymentReceiptImage != null && PaymentReceiptImage.Length > 0)
                    {
                        var fileName = Path.GetFileName(PaymentReceiptImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "expenses", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await PaymentReceiptImage.CopyToAsync(stream);
                        }
                        expense.PaymentReceiptImagePath = "/img/expenses/" + fileName;
                    }

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount))
                                                  .Cast<AccountingAccount>(), expense.AccountingAccount);
            return View(expense);
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }


        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
   
    }
}
