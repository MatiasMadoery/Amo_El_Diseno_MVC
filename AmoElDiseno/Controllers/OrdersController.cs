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
using static NuGet.Packaging.PackagingConstants;

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
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string searchString, int page = 1, int pageSize = 5)
        {
            IQueryable<Order> orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PaymentDeliveries);

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
                orders = orders.Where(o => o.Customer!.Name!.Contains(searchString) ||
                                           o.Customer!.LastName!.Contains(searchString));
            }

            // Ordenar en forma descendente para que el número de pedido más alto aparezca primero
            orders = orders.OrderByDescending(o => o.OrderNumber);

            // Obtener el total de órdenes
            int totalOrders = await orders.CountAsync();

            // Aplicar paginación
            var ordersPaged = await orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Crear el paginador (suponiendo que tienes una clase Pager<T> genérica similar a la que usaste para Customers)
            var pager = new Pager<Order>(ordersPaged, totalOrders, page, pageSize);

            // Para mantener el valor de búsqueda cuando se cambia de página
            ViewData["searchString"] = searchString;

            return View(pager);
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
            var orderNumberTracker = _context.OrderNumberTrackers.FirstOrDefault();
            int numeroPedido = 1;

            if (orderNumberTracker != null)
            {
                numeroPedido = orderNumberTracker.LastOrderNumber + 1;
            }

            string numeroPedidoStr = numeroPedido.ToString("D6");

            var viewModel = new OrderDetailsViewModel
            {
                Order = new Order
                {
                    OrderNumber = numeroPedidoStr,
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
                var orderNumberTracker = _context.OrderNumberTrackers.FirstOrDefault();
                int numeroPedido = 1;

                if (orderNumberTracker != null)
                {
                    numeroPedido = orderNumberTracker.LastOrderNumber + 1;
                }

                string numeroPedidoStr = numeroPedido.ToString("D6");

                viewModel.Order!.OrderNumber = numeroPedidoStr;

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
                    viewModel.Order!.ImagePath = "/img/ordersImages/" + fileName;
                }

                _context.Orders.Add(viewModel.Order!);
                await _context.SaveChangesAsync();

                if (orderNumberTracker == null)
                {
                    _context.OrderNumberTrackers.Add(new OrderNumberTracker { LastOrderNumber = numeroPedido });
                }
                else
                {
                    orderNumberTracker.LastOrderNumber = numeroPedido;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

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

        [HttpPost]
        public IActionResult AddPaymentDelivery(OrderDetailsViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.NewPaymentDelivery.OrderId = viewModel.Order!.Id;
                _context.PaymentDeliveries.Add(viewModel.NewPaymentDelivery);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = viewModel.Order.Id });
            }

            viewModel.Order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PaymentDeliveries)
                .FirstOrDefault(o => o.Id == viewModel.Order!.Id);

            return View("Details", viewModel);
        }
    }
}
