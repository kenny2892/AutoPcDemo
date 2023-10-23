using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoPcDemoWebsite.Data;
using AutoPcDemoWebsite.Models;

namespace AutoPcDemoWebsite.Controllers
{
    public class TestDatasController : Controller
    {
        private readonly TestingDataContext _context;

        public TestDatasController(TestingDataContext context)
        {
            _context = context;
        }

        // GET: TestDatas
        public IActionResult Index()
        {
              return View();
        }

        // Load Table Content
        public async Task<IActionResult> LoadTable(string searchBy)
        {
            if(searchBy is null)
            {
                searchBy = "";
            }

            // For the sake of this being a test project, just search through the id
            IQueryable<TestData> results = _context.TestDatas.Where(testData => testData.ID.ToLower().Contains(searchBy.ToLower()));

            ViewData["SearchBy"] = searchBy;
            return PartialView("_DataTableContent", await results.ToListAsync());
        }

        // GET: TestDatas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TestDatas == null)
            {
                return NotFound();
            }

            var testData = await _context.TestDatas
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testData == null)
            {
                return NotFound();
            }

            return View(testData);
        }

        // GET: TestDatas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TestDatas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CategoryOne,CategoryTwo,Date,Enabled")] TestData testData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(testData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(testData);
        }

        // GET: TestDatas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.TestDatas == null)
            {
                return NotFound();
            }

            var testData = await _context.TestDatas.FindAsync(id);
            if (testData == null)
            {
                return NotFound();
            }
            return View(testData);
        }

        // POST: TestDatas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,CategoryOne,CategoryTwo,Date,Enabled")] TestData testData)
        {
            if (id != testData.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestDataExists(testData.ID))
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
            return View(testData);
        }

        // GET: TestDatas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.TestDatas == null)
            {
                return NotFound();
            }

            var testData = await _context.TestDatas
                .FirstOrDefaultAsync(m => m.ID == id);
            if (testData == null)
            {
                return NotFound();
            }

            return View(testData);
        }

        // POST: TestDatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.TestDatas == null)
            {
                return Problem("Entity set 'TestingDataContext.TestDatas'  is null.");
            }
            var testData = await _context.TestDatas.FindAsync(id);
            if (testData != null)
            {
                _context.TestDatas.Remove(testData);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestDataExists(string id)
        {
          return _context.TestDatas.Any(e => e.ID == id);
        }
    }
}
