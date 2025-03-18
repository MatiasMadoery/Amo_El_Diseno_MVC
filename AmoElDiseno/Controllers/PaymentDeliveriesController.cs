using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AmoElDiseno.Models;

namespace AmoElDiseno.Controllers
{
    public class PaymentDeliveriesController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentDeliveriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PaymentDeliveries
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PaymentDeliveries.Include(p => p.Order);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PaymentDeliveries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentDelivery = await _context.PaymentDeliveries
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentDelivery == null)
            {
                return NotFound();
            }

            return View(paymentDelivery);
        }

        // GET: PaymentDeliveries/Create
        public IActionResult Create()
        {
            var model = new PaymentDelivery
            {
                Date = DateTime.Now
            };

            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View(model);
        }

        // POST: PaymentDeliveries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,Date,Amount")] PaymentDelivery paymentDelivery)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentDelivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentDelivery.OrderId);
            return View(paymentDelivery);
        }

        // GET: PaymentDeliveries/Edit/5
        public async Task<IActionResult> Edit(int? id, int? orderId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentDelivery = await _context.PaymentDeliveries.FindAsync(id);
            if (paymentDelivery == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = orderId; // Pasar OrderId a la vista
            ViewData["OrderIdSelectList"] = new SelectList(_context.Orders, "Id", "Id", paymentDelivery.OrderId);
            return View(paymentDelivery);
        }

        // POST: PaymentDeliveries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,Date,Amount")] PaymentDelivery paymentDelivery)
        {
            if (id != paymentDelivery.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPaymentDelivery = await _context.PaymentDeliveries.FindAsync(id);
                    if (existingPaymentDelivery == null)
                    {
                        return NotFound();
                    }

                    existingPaymentDelivery.Date = paymentDelivery.Date;
                    existingPaymentDelivery.Amount = paymentDelivery.Amount;

                    _context.Update(existingPaymentDelivery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentDeliveryExists(paymentDelivery.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Orders", new { id = paymentDelivery.OrderId });
            }

            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentDelivery.OrderId);
            return View(paymentDelivery);
        }


        // GET: PaymentDeliveries/Delete/5
        public async Task<IActionResult> Delete(int? id, int? orderId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymentDelivery = await _context.PaymentDeliveries
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (paymentDelivery == null)
            {
                return NotFound();
            }

            ViewData["OrderId"] = orderId; 

            return View(paymentDelivery);
        }

        // POST: PaymentDeliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? orderId)
        {
            var paymentDelivery = await _context.PaymentDeliveries.FindAsync(id);
            if (paymentDelivery != null)
            {
                _context.PaymentDeliveries.Remove(paymentDelivery);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("Details", "Orders", new {id = orderId});
        }

        private bool PaymentDeliveryExists(int id)
        {
            return _context.PaymentDeliveries.Any(e => e.Id == id);
        }
    }
}
