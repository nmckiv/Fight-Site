using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_FinalProject.Data;
using Fall2024_FinalProject.Models;

namespace Fall2024_FinalProject.Controllers
{
    public class FighterFightController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FighterFightController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FighterFight
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FighterFight.Include(f => f.Fight).Include(f => f.Fighter);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FighterFight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fighterFight = await _context.FighterFight
                .Include(f => f.Fight)
                .Include(f => f.Fighter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fighterFight == null)
            {
                return NotFound();
            }

            return View(fighterFight);
        }

        // GET: FighterFight/Create
        public IActionResult Create()
        {
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location");
            ViewData["FighterId"] = new SelectList(_context.Fighters, "Id", "Name");
            return View();
        }

        // POST: FighterFight/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FighterId,FightId")] FighterFight fighterFight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fighterFight);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", fighterFight.FightId);
            ViewData["FighterId"] = new SelectList(_context.Fighters, "Id", "Name", fighterFight.FighterId);
            return View(fighterFight);
        }

        // GET: FighterFight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fighterFight = await _context.FighterFight.FindAsync(id);
            if (fighterFight == null)
            {
                return NotFound();
            }
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", fighterFight.FightId);
            ViewData["FighterId"] = new SelectList(_context.Fighters, "Id", "Name", fighterFight.FighterId);
            return View(fighterFight);
        }

        // POST: FighterFight/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FighterId,FightId")] FighterFight fighterFight)
        {
            if (id != fighterFight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fighterFight);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FighterFightExists(fighterFight.Id))
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
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", fighterFight.FightId);
            ViewData["FighterId"] = new SelectList(_context.Fighters, "Id", "Name", fighterFight.FighterId);
            return View(fighterFight);
        }

        // GET: FighterFight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fighterFight = await _context.FighterFight
                .Include(f => f.Fight)
                .Include(f => f.Fighter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fighterFight == null)
            {
                return NotFound();
            }

            return View(fighterFight);
        }

        // POST: FighterFight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fighterFight = await _context.FighterFight.FindAsync(id);
            if (fighterFight != null)
            {
                _context.FighterFight.Remove(fighterFight);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FighterFightExists(int id)
        {
            return _context.FighterFight.Any(e => e.Id == id);
        }
    }
}
