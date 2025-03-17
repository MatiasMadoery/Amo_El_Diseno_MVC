using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;
using static NuGet.Packaging.PackagingConstants;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

            var expense = await _context.Expenses.FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            // Envolver el Expense en un ExpenseDetailsViewModel
            var viewModel = new ExpenseDetailsViewModel
            {
                Expense = expense
            };

            return View(viewModel);
        }


        // GET: Expenses/Create
        public IActionResult Create()
        {
            var viewModel = new ExpenseDetailsViewModel();
            
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>());

            return View(viewModel);
        }


        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseDetailsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Si se ha cargado una imagen, redimensionarla y guardarla
                if (viewModel.Image != null && viewModel.Image.Length > 0)
                {
                    // Define la ruta donde se guardará la imagen
                    var fileName = Path.GetFileName(viewModel.Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/expenses", fileName);

                    // Usamos ImageSharp para redimensionar y comprimir la imagen
                    using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                    {
                        // Redimensionamos a un ancho máximo de 800px (la altura se ajusta proporcionalmente)
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(200, 0)
                        }));

                        // Configuramos el encoder JPEG con una calidad del 75%
                        var encoder = new JpegEncoder
                        {
                            Quality = 75
                        };

                        // Guardamos la imagen en el servidor
                        await image.SaveAsync(filePath, encoder);
                    }

                    // Asignamos la ruta de la imagen al pedido (asegúrate de que tu modelo Order tenga la propiedad ImagePath)
                    viewModel.Expense!.PaymentReceiptImagePath = "/img/expenses/" + fileName;
                }

                // Agregamos el pedido a la base de datos
                _context.Expenses.Add(viewModel.Expense!);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }    

            // Si hay errores, repuebla el dropdown
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>(), viewModel.Expense.AccountingAccount);
            return View(viewModel);
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

            // Envuelve el expense en un ExpenseDetailsViewModel
            var viewModel = new ExpenseDetailsViewModel
            {
                Expense = expense
            };

            // Poblar el dropdown de cuentas contables
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount)).Cast<AccountingAccount>(), expense.AccountingAccount);

            return View(viewModel);
        }



        // POST: Expenses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseDetailsViewModel viewModel)
        {
            if (id != viewModel.Expense.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Procesar la imagen si se subió un nuevo archivo
                    if (viewModel.Image != null && viewModel.Image.Length > 0)
                    {
                        // Construir la ruta completa a la carpeta donde se almacenarán las imágenes
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "expenses");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Generar un nombre de archivo único
                        var originalFileName = Path.GetFileName(viewModel.Image.FileName);
                        var fileName = $"{Guid.NewGuid()}_{originalFileName}";
                        var filePath = Path.Combine(folderPath, fileName);

                        // Depurar (opcional)
                        Console.WriteLine("Guardando imagen en: " + filePath);

                        // Usamos ImageSharp para redimensionar la imagen
                        using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                // Puedes ajustar el tamaño según lo necesites; aquí se establece un ancho máximo de 200px
                                Size = new Size(200, 0)
                            }));

                            // Define la calidad del encoder JPEG
                            var encoder = new JpegEncoder { Quality = 75 };
                            await image.SaveAsync(filePath, encoder);
                        }

                        // Asigna la ruta relativa al comprobante de pago en el modelo
                        viewModel.Expense.PaymentReceiptImagePath = "/img/expenses/" + fileName;
                        Console.WriteLine("Ruta asignada en modelo: " + viewModel.Expense.PaymentReceiptImagePath);
                    }

                    // Actualiza la entidad en la base de datos
                    _context.Update(viewModel.Expense);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(viewModel.Expense.Id))
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

            // Si el ModelState no es válido, repobla el dropdown de cuentas contables
            ViewBag.Accounts = new SelectList(Enum.GetValues(typeof(AccountingAccount))
                                                  .Cast<AccountingAccount>(), viewModel.Expense.AccountingAccount);
            return View(viewModel);
        }


        private bool ExpenseExists(int id) { return _context.Expenses.Any(e => e.Id == id); }

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
                // Si existe una ruta de imagen, construye la ruta absoluta
                if (!string.IsNullOrEmpty(expense.PaymentReceiptImagePath))
                {
                    // Se construye la ruta absoluta usando el directorio actual y la carpeta wwwroot:
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.PaymentReceiptImagePath.TrimStart('/'));

                    // Verifica que el archivo exista antes de eliminarlo
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
   
    }
}
