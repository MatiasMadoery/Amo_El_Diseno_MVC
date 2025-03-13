using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AmoElDiseno.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string searchString)
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PaymentDeliveries)
                .AsQueryable();

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.Date >= startDate);
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.Date <= endDate);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.Customer.Name.Contains(searchString) ||
                                           o.Customer.LastName.Contains(searchString));
            }

            return View(await orders.ToListAsync());
        }


        // GET: Orders/Details/5
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PaymentDeliveries)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailsViewModel
            {
                Order = order
            };

            return View(viewModel);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            var viewModel = new OrderDetailsViewModel
            {
                Order = new Order
                {
                    Status = OrderStatus.Presupuestado,
                    Date = DateTime.Now
                }
            };

            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name");
            return View(viewModel);
        }  


        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderDetailsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Si se ha cargado una imagen, redimensionarla y guardarla
                if (viewModel.Image != null && viewModel.Image.Length > 0)
                {
                    // Define la ruta donde se guardará la imagen, por ejemplo en wwwroot/img/ordersImages
                    var fileName = Path.GetFileName(viewModel.Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/ordersImages", fileName);

                    // Usamos ImageSharp para redimensionar y comprimir la imagen
                    using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                    {
                        // Redimensionamos a un ancho máximo de 800px (la altura se ajusta proporcionalmente)
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(100, 0)
                        }));

                        // Configuramos el encoder JPEG con una calidad del 75%
                        var encoder = new JpegEncoder
                        {
                            Quality = 65
                        };

                        // Guardamos la imagen en el servidor
                        await image.SaveAsync(filePath, encoder);
                    }

                    // Asignamos la ruta de la imagen al pedido (asegúrate de que tu modelo Order tenga la propiedad ImagePath)
                    viewModel.Order!.ImagePath = "/img/ordersImages/" + fileName;
                }

                // Agregamos el pedido a la base de datos
                _context.Orders.Add(viewModel.Order!);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, volvemos a popular el ViewBag para la selección de clientes
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name");
            return View(viewModel);
        }
        
        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailsViewModel
            {
                Order = order
            };

            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", order.CustomerId);

            return View(viewModel);
        }


        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderDetailsViewModel viewModel)
        {
            if (id != viewModel.Order!.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (viewModel.Image != null && viewModel.Image.Length > 0)
                    {
                        var fileName = Path.GetFileName(viewModel.Image.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/ordersImages", fileName);

                        using (var image = await Image.LoadAsync(viewModel.Image.OpenReadStream()))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(100, 0)
                            }));

                            var encoder = new JpegEncoder
                            {
                                Quality = 65
                            };
                            await image.SaveAsync(filePath, encoder);
                        }

                        viewModel.Order.ImagePath = "/img/ordersImages/" + fileName;
                    }

                    _context.Update(viewModel.Order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "No se puede guardar el pedido porque otro usuario ya ha actualizado los datos.");
                }
                catch (Exception ex)
                {
                    // Manejamos cualquier otra excepción no esperada.
                    return StatusCode(StatusCodes.Status500InternalServerError, "Ha ocurrido un error al guardar el pedido: " + ex.Message);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", viewModel.Order.CustomerId);
            return View(viewModel);
        }



        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                // Si existe una ruta de imagen, construye la ruta absoluta
                if (!string.IsNullOrEmpty(order.ImagePath))
                {
                    // La propiedad ImagePath es relativa, normalmente por ejemplo "/img/ordersImages/imagen.jpg"
                    // Se construye la ruta absoluta usando el directorio actual y la carpeta wwwroot:
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", order.ImagePath.TrimStart('/'));

                    // Verifica que el archivo exista antes de eliminarlo
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
