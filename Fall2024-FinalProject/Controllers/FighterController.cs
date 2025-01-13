using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_FinalProject.Data;
using Fall2024_FinalProject.Models;
using Fall2024_FinalProject.Enums;
using Azure.AI.OpenAI;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Fall2024_FinalProject.Controllers
{
    public class FighterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAI.Chat.ChatClient _client;
        private readonly UserManager<User> _userManager;

        public FighterController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            var apiKey = configuration["AI_API_KEY"];
            var apiEndpoint = configuration["AI_API_ENDPOINT"];
            AzureOpenAIClient chat = new(new Uri(apiEndpoint), new System.ClientModel.ApiKeyCredential(apiKey));
            _client = chat.GetChatClient("gpt-35-turbo");
            _userManager = userManager;
        }

        // GET: Fighter
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fighters.ToListAsync());
        }

        // GET: Fighter/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Redirect to register page if no user is logged in
                return Redirect(Url.Page("/Account/Register", new { area = "Identity" }));
            }

            if (id == null)
            {
                return NotFound();
            }

            var fighter = await _context.Fighters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fighter == null)
            {
                return NotFound();
            }

            // generate AI about me
            var resultText = "";
            try
            {
                var name = fighter.Name;
                var style = fighter.FighterStyle.ToString();
                var weight = fighter.Weight;
                var health = fighter.Health;
                var strength = fighter.Strength;
                var agility = fighter.Agility;
                var intelligence = fighter.Intelligence;
                var completion = await _client.CompleteChatAsync($"You are a fighter named {name}. Your stats are weight {weight} pounds, health {health}/10, " +
                    $"strength {strength}/10, agility {agility}/10, and intelligence {intelligence}/10. Your fighting style is {style}. Generate two paragraphs" +
                    $"about yourself. The first paragraph should be a dramatic and interesting backstory, culminating in becoming a professional fighter." +
                    $"The second paragraph should be about yourself as a fighter, based on your stats. Be very descriptive and don't be afraid to make up some" +
                    $"random details to fill out the story/about yourself and make it entertaining. Use third person. Always use the full fighter name." +
                    $"Use descriptive adjectives rather than the number of the stat.");
                resultText = completion.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // make view model
            var viewModel = new FighterViewModel
            {
                Fighter = fighter,
                Backstory = resultText
            };

            return View(viewModel);

        }

        // GET: Fighter/Create
        [Authorize(Roles="Admin")]
        public IActionResult Create()
        {
            ViewBag.FightingStyles = new SelectList(Enum.GetValues(typeof(FightingStyle)));
            return View();
        }

        // POST: Fighter/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Health,Strength,Agility,Intelligence,Weight,FighterStyle")] Fighter fighter, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    fighter.FighterPhoto = memoryStream.ToArray();
                }
                else
                {
                    // Load the default image from a file or an embedded resource
                    var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "icons", "default.jpg");
                    fighter.FighterPhoto = await System.IO.File.ReadAllBytesAsync(defaultImagePath);
                }
                _context.Add(fighter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fighter);
        }

        // GET: Fighter/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fighter = await _context.Fighters.FindAsync(id);
            if (fighter == null)
            {
                return NotFound();
            }

            ViewBag.FightingStyles = new SelectList(Enum.GetValues(typeof(FightingStyle)));
            return View(fighter);
        }

        // POST: Fighter/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Health,Strength,Agility,Intelligence,Weight,FighterStyle")] Fighter fighter, IFormFile? photo)
        {
            if (id != fighter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingFighter = await _context.Fighters.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);

                    if (existingFighter == null)
                    {
                        return NotFound();
                    }
                    if (photo != null && photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await photo.CopyToAsync(memoryStream);
                        fighter.FighterPhoto = memoryStream.ToArray();
                    }
                    else
                    {
                        fighter.FighterPhoto = existingFighter.FighterPhoto;
                    }
                    _context.Update(fighter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FighterExists(fighter.Id))
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
            return View(fighter);
        }

        // GET: Fighter/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fighter = await _context.Fighters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fighter == null)
            {
                return NotFound();
            }

            return View(fighter);
        }

        // POST: Fighter/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fighter = await _context.Fighters.FindAsync(id);
            if (fighter != null)
            {
                // find all fights where the fighter is involved
                var fights = _context.Fights.Where(f => f.Fighter1Id == id || f.Fighter2Id == id);

                // remove the fights
                _context.Fights.RemoveRange(fights);

                // remove the fighter
                _context.Fighters.Remove(fighter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FighterExists(int id)
        {
            return _context.Fighters.Any(e => e.Id == id);
        }
    }
}
