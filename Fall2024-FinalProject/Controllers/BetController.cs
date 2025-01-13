using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_FinalProject.Data;
using Fall2024_FinalProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Principal;

namespace Fall2024_FinalProject.Controllers
{
    public class BetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<BetController> _logger;


        public BetController(ApplicationDbContext context, UserManager<User> userManager, ILogger<BetController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Bet
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Redirect to register page if no user is logged in
                return Redirect(Url.Page("/Account/Register", new { area = "Identity" }));
            }

            // Filter bets for the logged-in user
            var bets = await _context.Bets
                .Include(b => b.User)
                .Include(b => b.Fight)
                .Include(b => b.ChosenFighter)
                .Where(b => b.UserId == user.Id) // Filter bets by logged-in user's ID
                .ToListAsync();

            return View(bets);
        }

        // GET: Bet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets
                .Include(b => b.ChosenFighter)
                .Include(b => b.Fight)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bet == null)
            {
                return NotFound();
            }

            return View(bet);
        }

        // GET: Bet/Create
        public IActionResult Create()
        {
            // Populate the "Selection" (ChosenFighterId) dropdown
            ViewData["ChosenFighterId"] = new SelectList(_context.Fighters, "Id", "Name");

            // Retrieve unsettled fights and join with the Fighters table to get names
            var unsettledFights = _context.Fights
                .Where(f => f.Winner == null)
                .ToList();

            // Create a list of formatted fight events
            var fightEvents = unsettledFights.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = $"{_context.Fighters.FirstOrDefault(x => x.Id == f.Fighter1Id)?.Name} Vs " +
                       $"{_context.Fighters.FirstOrDefault(x => x.Id == f.Fighter2Id)?.Name}, " +
                       $"{f.Location}, {f.Date:MM-dd-yyyy}"
            }).ToList();

            ViewData["FightId"] = fightEvents;

            return View();
        }



        // POST: Bet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FightId,ChosenFighterId,BetAmount")] Bet bet)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { _logger.LogInformation("++++++++++USER IS NULL+++++++++++++++"); }
            else
            {
                _logger.LogInformation("---------------Username is " + user.UserName);
            }
            //var fight = await _context.Fights.FirstOrDefaultAsync(f => f.Id == bet.FightId);

            var fight = await _context.Fights.Include(f => f.Bets).FirstOrDefaultAsync(f => f.Id == bet.FightId);

            var chosenFighter = await _context.Fighters.FirstOrDefaultAsync(f => f.Id == bet.ChosenFighterId);

            bet.UserId = user.Id;
            bet.User = user;
            bet.Fight = fight;
            bet.ChosenFighter = chosenFighter;

            //Get odds for selected fighter
            if (bet.ChosenFighterId == fight.Fighter1Id)
            {
                bet.Odds = fight.Fighter1Odds;
            }
            else if (bet.ChosenFighterId == fight.Fighter2Id)
            {
                bet.Odds = fight.Fighter2Odds;
            }

            bet.IsWin = null; //Null indicates fight has not yet been decided

            //Check user balance and deduct
            if (user.Balance - bet.BetAmount >= 0)
            {
                user.Balance -= bet.BetAmount;
            }
            else
            {
                //Insufficient balance in user's account
                ModelState.AddModelError("BetAmount", "Insufficient balance. You have $" + user.Balance + " available.");
            }

            if (ModelState.IsValid)
            {
                //Add bet to list of bets within fight object
                fight.Bets.Add(bet);
                _logger.LogInformation("Adding bet of $" + bet.BetAmount + ". Fight now has " + fight.Bets.Count + " bets");
                _context.Add(bet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                _logger.LogInformation("======================================================");
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogError(error.ErrorMessage);
                    }
                }
            }
            ViewData["ChosenFighterId"] = new SelectList(_context.Fighters, "Id", "Name", bet.ChosenFighterId);
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", bet.FightId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", bet.UserId);
            return View(bet);
        }

        // GET: Bet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }
            ViewData["ChosenFighterId"] = new SelectList(_context.Fighters, "Id", "Name", bet.ChosenFighterId);
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", bet.FightId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", bet.UserId);
            return View(bet);
        }

        // POST: Bet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FightId,ChosenFighterId,BetAmount,Odds,IsWin")] Bet bet)
        {
            if (id != bet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BetExists(bet.Id))
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
            ViewData["ChosenFighterId"] = new SelectList(_context.Fighters, "Id", "Name", bet.ChosenFighterId);
            ViewData["FightId"] = new SelectList(_context.Fights, "Id", "Location", bet.FightId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Name", bet.UserId);
            return View(bet);
        }

        // GET: Bet/Delete/5
        // GET: Bet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bet = await _context.Bets
                .Include(b => b.ChosenFighter)
                .Include(b => b.Fight)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bet == null)
            {
                return NotFound();
            }

            // Check if the bet is undecided and redirect to Index with an error message
            if (bet.IsWin == null)
            {
                TempData["ErrorMessage"] = "You cannot delete a pending bet.";
                return RedirectToAction(nameof(Index));
            }

            return View(bet);
        }

        // POST: Bet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bet = await _context.Bets.FindAsync(id);
            if (bet == null)
            {
                return NotFound();
            }

            // Check if the bet is undecided
            if (bet.IsWin == null)
            {
                TempData["ErrorMessage"] = "You cannot delete a pending bet.";
                return RedirectToAction(nameof(Index));
            }

            // Bet is decided; proceed with deletion
            _context.Bets.Remove(bet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bet deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool BetExists(int id)
        {
            return _context.Bets.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<JsonResult> GetFightersByFightId(int fightId)
        {
            // Retrieve the fight, ensuring it exists
            var fight = await _context.Fights
                .FirstOrDefaultAsync(f => f.Id == fightId);

            if (fight == null)
            {
                return Json(new { success = false, message = "Fight not found" });
            }

            // Load fighters by their IDs
            var fighter1 = await _context.Fighters
                .FirstOrDefaultAsync(f => f.Id == fight.Fighter1Id);
            var fighter2 = await _context.Fighters
                .FirstOrDefaultAsync(f => f.Id == fight.Fighter2Id);

            if (fighter1 == null || fighter2 == null)
            {
                return Json(new { success = false, message = "One or more fighters not found" });
            }

            // Create a list of fighters to return
            var fighters = new[]
            {
        new { Id = fighter1.Id, Name = fighter1.Name, Odds = fight.Fighter1Odds },
        new { Id = fighter2.Id, Name = fighter2.Name, Odds = fight.Fighter2Odds }
    };

            return Json(fighters);
        }
    }
}
