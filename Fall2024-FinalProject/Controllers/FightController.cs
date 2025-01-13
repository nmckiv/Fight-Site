using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fall2024_FinalProject.Data;
using Fall2024_FinalProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Numerics;
using Microsoft.AspNetCore.Identity;

namespace Fall2024_FinalProject.Controllers
{
    public class FightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<BetController> _logger;
        private readonly UserManager<User> _userManager;

        public FightController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration, ILogger<BetController> logger)
        {
            _context = context;
            _config = configuration;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var fights = await _context.Fights
                .ToListAsync();

            var fightAggregates = fights.Select(fight => new FightAggregate
            {
                Fight = fight,
                Fighter1 = _context.Fighters.FirstOrDefault(f => f.Id == fight.Fighter1Id),
                Fighter2 = _context.Fighters.FirstOrDefault(f => f.Id == fight.Fighter2Id)
            }).OrderBy(f => f.Fight.Winner).ToList();

            return View(fightAggregates);
        }


        // GET: Fight/Details/5
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

            var fight = await _context.Fights.FirstOrDefaultAsync(m => m.Id == id);
            if (fight == null)
            {
                return NotFound();
            }

            var fighter1 = await _context.Fighters.FirstOrDefaultAsync(m => m.Id == fight.Fighter1Id);
            var fighter2 = await _context.Fighters.FirstOrDefaultAsync(m => m.Id == fight.Fighter2Id);

            if (fighter1 == null || fighter2 == null)
            {
                return NotFound();
            }

            var fightAggregate = new FightAggregate
            {
                Fight = fight,
                Fighter1 = fighter1,
                Fighter2 = fighter2
            };

            return View(fightAggregate);
        }

        // GET: Fight/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Fighter1Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name");
            ViewData["Fighter2Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name");
            var mapApiKey = _config["MAP_API_KEY"]; ViewBag.MapApiKey = mapApiKey;

            return View();
        }

        // POST: Fight/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Longitude,Latitude,Location,Fighter1Id,Fighter2Id,Fighter1Odds,Fighter2Odds")] Fight fight)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fight);
                await _context.SaveChangesAsync();

                var fighterFight1 = new FighterFight { FighterId = fight.Fighter1Id, FightId = fight.Id };
                var fighterFight2 = new FighterFight { FighterId = fight.Fighter2Id, FightId = fight.Id };

                _context.FighterFights.Add(fighterFight1);
                _context.FighterFights.Add(fighterFight2);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["Fighter1Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter1Id);
            ViewData["Fighter2Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter2Id);
            return View(fight);
        }

        // GET: Fight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fight = await _context.Fights.FindAsync(id);
            if (fight == null)
            {
                return NotFound();
            }

            ViewData["Fighter1Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter1Id);
            ViewData["Fighter2Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter2Id);
            
            var mapApiKey = _config["MAP_API_KEY"]; ViewBag.MapApiKey = mapApiKey;
            return View(fight);
        }

        // POST: Fight/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Longitude,Latitude,Location,Fighter1Id,Fighter2Id,Fighter1Odds,Fighter2Odds")] Fight fight)
        {
            if (id != fight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingFight = await _context.Fights
                        .FirstOrDefaultAsync(f => f.Id == id);

                    if (existingFight == null)
                    {
                        return NotFound();
                    }

                    // Update the fight details
                    existingFight.Date = fight.Date;
                    existingFight.Location = fight.Location;
                    existingFight.Latitude = fight.Latitude;
                    existingFight.Longitude = fight.Longitude;
                    existingFight.Fighter1Id = fight.Fighter1Id;
                    existingFight.Fighter2Id = fight.Fighter2Id;
                    existingFight.Fighter1Odds = fight.Fighter1Odds;
                    existingFight.Fighter2Odds = fight.Fighter2Odds;

                    // Save the fight changes
                    _context.Update(existingFight);
                    await _context.SaveChangesAsync();

                    // Handle FighterFight relationships
                    var existingFighterFights = await _context.FighterFights
                        .Where(ff => ff.FightId == id)
                        .ToListAsync();

                    // Remove old relationships if fighters changed
                    if (!existingFighterFights.Any(ff => ff.FighterId == fight.Fighter1Id))
                    {
                        var oldFighterFight = existingFighterFights.FirstOrDefault(ff => ff.FighterId != fight.Fighter1Id);
                        if (oldFighterFight != null)
                        {
                            _context.FighterFights.Remove(oldFighterFight);
                        }

                        var newFighterFight = new FighterFight { FighterId = fight.Fighter1Id, FightId = fight.Id };
                        _context.FighterFights.Add(newFighterFight);
                    }

                    if (!existingFighterFights.Any(ff => ff.FighterId == fight.Fighter2Id))
                    {
                        var oldFighterFight = existingFighterFights.FirstOrDefault(ff => ff.FighterId != fight.Fighter2Id);
                        if (oldFighterFight != null)
                        {
                            _context.FighterFights.Remove(oldFighterFight);
                        }

                        var newFighterFight = new FighterFight { FighterId = fight.Fighter2Id, FightId = fight.Id };
                        _context.FighterFights.Add(newFighterFight);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FightExists(fight.Id))
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

            ViewData["Fighter1Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter1Id);
            ViewData["Fighter2Id"] = new SelectList(await _context.Fighters.ToListAsync(), "Id", "Name", fight.Fighter2Id);
           

            return View(fight);
        }

        // GET: Fight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fight = await _context.Fights.FirstOrDefaultAsync(m => m.Id == id);
            if (fight == null)
            {
                return NotFound();
            }

            return View(fight);
        }

        // POST: Fight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fight = await _context.Fights.FindAsync(id);
            if (fight != null)
            {
                // remove associated FighterFight records
                var fighterFights = await _context.FighterFights.Where(ff => ff.FightId == fight.Id).ToListAsync();
                _context.FighterFights.RemoveRange(fighterFights);

                // remove associated Bet records
                var bets = await _context.Bets.Where(b => b.FightId == fight.Id).ToListAsync();
                _context.Bets.RemoveRange(bets);

                // remove the fight itself
                _context.Fights.Remove(fight);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FightExists(int id)
        {
            return _context.Fights.Any(e => e.Id == id);
        }

        [HttpGet("api/Fight/GetLocationName")]
        public async Task<IActionResult> GetLocationName(double latitude, double longitude)
        {
            var apiKey = "8nPFfxiJUCDHzbhmodSiqoZYSC7vOAkCM1IVhELQColpj9kHuyL0JQQJ99ALACYeBjFZ25JCAAAgAZMP4QE3";
            var reverseGeocodeUrl = $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={latitude},{longitude}&subscription-key={apiKey}";

            using (var httpClient = new HttpClient()) { 
                var response = await httpClient.GetStringAsync(reverseGeocodeUrl); 
                var locationData = JsonConvert.DeserializeObject<dynamic>(response);
                var city = locationData?.addresses[0]?.address?.municipality;
                var state = locationData?.addresses[0]?.address?.countrySubdivision;
                var country = locationData?.addresses[0]?.address?.countryCode;

                return Ok($"{city}, {state}, {country}"); 
            }
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> SimulateFight(int id)
        {
            var fight = await _context.Fights.Include(f => f.Bets).FirstOrDefaultAsync(f => f.Id == id);

            if (fight == null)
            {
                return NotFound();
            }

            // Fetch the fighters separately using their IDs
            var fighter1 = await _context.Fighters.FirstOrDefaultAsync(f => f.Id == fight.Fighter1Id);
            var fighter2 = await _context.Fighters.FirstOrDefaultAsync(f => f.Id == fight.Fighter2Id);

            if (fighter1 == null || fighter2 == null)
            {
                return NotFound("Fighter not found.");
            }

            Debug.WriteLine("Chat Call\n\n");

            //Hard coding winner for betting dev
            var simulationResult = await CallSimulateFightApi(fight, fighter1, fighter2);

            Debug.WriteLine("Chat Call funct returned \n\n");

            fight.Winner = simulationResult.Winner;
            fight.Summary = simulationResult.Summary;
            fight.RoundsFought = simulationResult.RoundsFought;
            _context.Update(fight);

            // payout bets here
            foreach(Bet bet in fight.Bets)
            {
                bet.IsWin = bet.ChosenFighter.Name == fight.Winner;
                if (bet.IsWin.Value)
                {
                    bet.IsWin = true;
                    var user = await _context.Users.FirstOrDefaultAsync(f => f.Id == bet.UserId);
                    if (bet.Odds > 0)
                    {
                        // Positive odds: Payout = BetAmount * (Odds / 100) + BetAmount
                        user.Balance += (bet.BetAmount * bet.Odds / 100) + bet.BetAmount;
                    }
                    else
                    {
                        // Negative odds: Payout = BetAmount * (100 / -Odds) + BetAmount
                        user.Balance += (bet.BetAmount * 100 / -bet.Odds) + bet.BetAmount;
                    }
                }

                else
                {
                    bet.IsWin = false;
                }
            }

            await _context.SaveChangesAsync();

            // Redirect to the details view with the updated fight information
            return RedirectToAction(nameof(Details), new { id = fight.Id });
        }


        private async Task<FightSimulationResult> CallSimulateFightApi(Fight fight, Fighter fighter1, Fighter fighter2)
        {
            var apiKey = new System.ClientModel.ApiKeyCredential(_config["AI_API_KEY"] ?? throw new Exception("AI_API_KEY does not exist in the current Configuration"));
            var apiEndpoint = new Uri(_config["AI_API_ENDPOINT"] ?? throw new Exception("AI_API_ENDPOINT does not exist in the current Configuration"));
            AzureOpenAIClient client = new(apiEndpoint, apiKey);
            ChatClient chat = client.GetChatClient("gpt-35-turbo");
            Debug.WriteLine("Chat client aquried \n\n");
            FightSimulationResult simulationResult;

            try
            {
                ChatCompletion completion = await chat.CompleteChatAsync($@"
        Simulate a fight between two fighters and return the result in JSON format. Summary should be five detailed sentances:
        {{
            ""summary"": ""Fight summary"",
            ""roundsFought"": int,
            ""winner"": ""Fighter name""
        }}. 
        Fighter 1: {fighter1.Serialize()}, Fighter 2: {fighter2.Serialize()}, Location: {fight.Location}");

                Debug.WriteLine(completion.Content[0].Text);
                simulationResult = JsonConvert.DeserializeObject<FightSimulationResult>(completion.Content[0].Text);

            }
            catch (Exception ex)
            {
                // Log the error for debugging purposes
                Console.Error.WriteLine($"AI data error: {ex.Message}");

                // Return a default result in case of error
                simulationResult = new FightSimulationResult
                {
                    Summary = "Error: Unable to simulate the fight due to an unexpected response format.",
                    RoundsFought = 0,
                    Winner = "Unknown"
                };
            }
            Debug.WriteLine("Chat Call Deserialized \n\n");

            return simulationResult;
        }



    }
}
   
