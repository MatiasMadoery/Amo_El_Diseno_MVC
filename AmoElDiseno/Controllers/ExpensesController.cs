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
            IQueryable<Expense> expensesQuery = _context.Expenses;

            if (!string.IsNullOrEmpty(searchString))
            {
                expensesQuery = expensesQuery.Where(e =>
                    e.Recipient!.Contains(searchString) ||
                    e.Concept!.Contains(searchString));
            }

            if (searchAccount.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.AccountingAccount == searchAccount);
            }

            expensesQuery = expensesQuery.OrderByDescending(e => e.TransactionDate);

            int totalExpenses = await expensesQuery.CountAsync();

            var expensesPaged = await expensesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pager = new Pager<Expense>(expensesPaged, totalExpenses, page, pageSize);

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
        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseDetailsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Procesar archivo subido si existe
                if (viewModel.Image != null && viewModel.Image.Length > 0)
                {
                    var fileName = Path.GetFileName(viewModel.Image.FileName);
                    var extension = Path.GetExtension(fileName).ToLower();

                    // Verificamos si es PDF o imagen
                    if (extension == ".pdf" || viewModel.Image.ContentType == "application/pdf")
                    {
                        // Ruta destino para archivos PDF
                        var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdf/expenses");
                        if (!Directory.Exists(pdfDirectory))
                        {
                            Directory.CreateDirectory(pdfDirectory);
                        }
                        var pdfPath = Path.Combine(pdfDirectory, fileName);

                        using (var stream = new FileStream(pdfPath, FileMode.Create))
                        {
                            await viewModel.Image.CopyToAsync(stream);
                        }

                        // Guarda la ruta del PDF en la propiedad adecuada del modelo.
                        // Asumiremos que en tu entidad Expense agregaste la propiedad PaymentReceiptPDFPath.
                        viewModel.Expense!.PaymentReceiptPDFPath = "/pdf/expenses/" + fileName;
                    }
                    else
                    {
                        // Procesar imagen con ImageSharp (lo que ya haces)
                        var imgDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/expenses");
                        if (!Directory.Exists(imgDirectory))
                        {
                            Directory.CreateDirectory(imgDirectory);
                        }
                        var imagePath = Path.Combine(imgDirectory, fileName);

                        using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(200, 0)
                            }));

                            var encoder = new JpegEncoder { Quality = 75 };
                            await image.SaveAsync(imagePath, encoder);
                        }

                        viewModel.Expense!.PaymentReceiptImagePath = "/img/expenses/" + fileName;
                    }
                }

                // Agregamos el gasto a la base de datos
                _context.Expenses.Add(viewModel.Expense!);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, repoblar el dropdown de cuentas
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
                    // Procesar el archivo subido (imagen o PDF)
                    if (viewModel.Image != null && viewModel.Image.Length > 0)
                    {
                        // Obtener el nombre y extensión del archivo
                        var originalFileName = Path.GetFileName(viewModel.Image.FileName);
                        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

                        // Si es PDF
                        if (extension == ".pdf" || viewModel.Image.ContentType == "application/pdf")
                        {
                            var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "expenses");
                            if (!Directory.Exists(pdfDirectory))
                            {
                                Directory.CreateDirectory(pdfDirectory);
                            }

                            // Generar un nombre único para el archivo PDF
                            var fileName = $"{Guid.NewGuid()}_{originalFileName}";
                            var pdfPath = Path.Combine(pdfDirectory, fileName);

                            using (var stream = new FileStream(pdfPath, FileMode.Create))
                            {
                                await viewModel.Image.CopyToAsync(stream);
                            }
                            // Asigna la ruta del PDF en la propiedad correspondiente.
                            viewModel.Expense.PaymentReceiptPDFPath = "/pdf/expenses/" + fileName;
                            // También puedes limpiar la propiedad de imagen si lo consideras necesario:
                            viewModel.Expense.PaymentReceiptImagePath = null;
                        }
                        else
                        {
                            // Procesar la imagen con ImageSharp
                            var imgDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "expenses");
                            if (!Directory.Exists(imgDirectory))
                            {
                                Directory.CreateDirectory(imgDirectory);
                            }

                            var fileName = $"{Guid.NewGuid()}_{originalFileName}";
                            var filePath = Path.Combine(imgDirectory, fileName);

                            Console.WriteLine("Guardando imagen en: " + filePath);

                            using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                            {
                                image.Mutate(x => x.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Max,
                                    Size = new Size(200, 0) // Redimensiona manteniendo la proporción
                                }));

                                var encoder = new JpegEncoder { Quality = 75 };
                                await image.SaveAsync(filePath, encoder);
                            }

                            viewModel.Expense.PaymentReceiptImagePath = "/img/expenses/" + fileName;
                            Console.WriteLine("Ruta asignada en modelo: " + viewModel.Expense.PaymentReceiptImagePath);
                            // Si se subió una imagen, podrías querer limpiar la propiedad PDF
                            viewModel.Expense.PaymentReceiptPDFPath = null;
                        }
                    }

                    // Actualizamos la entidad en la BD
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

            // Si el ModelState no es válido, repoblar el dropdown de cuentas contables
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
        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                // Si existe una ruta de imagen, eliminarla
                if (!string.IsNullOrEmpty(expense.PaymentReceiptImagePath))
                {
                    // Construir la ruta absoluta usando el directorio actual y la carpeta wwwroot:
                    var fullPathImage = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.PaymentReceiptImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPathImage))
                    {
                        System.IO.File.Delete(fullPathImage);
                    }
                }

                // Si existe una ruta de PDF, eliminarla
                if (!string.IsNullOrEmpty(expense.PaymentReceiptPDFPath))
                {
                    var fullPathPdf = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.PaymentReceiptPDFPath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPathPdf))
                    {
                        System.IO.File.Delete(fullPathPdf);
                    }
                }

                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
