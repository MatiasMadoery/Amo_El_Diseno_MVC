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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            return View();
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
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentDelivery.OrderId);
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
                    _context.Update(paymentDelivery);
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", paymentDelivery.OrderId);
            return View(paymentDelivery);
        }

        // GET: PaymentDeliveries/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: PaymentDeliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentDelivery = await _context.PaymentDeliveries.FindAsync(id);
            if (paymentDelivery != null)
            {
                _context.PaymentDeliveries.Remove(paymentDelivery);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentDeliveryExists(int id)
        {
            return _context.PaymentDeliveries.Any(e => e.Id == id);
        }
    }
}
