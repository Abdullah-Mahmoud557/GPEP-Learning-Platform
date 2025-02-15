using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Userstories.Data;
using Userstories.Models;

public class PersonalizationProfilesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PersonalizationProfilesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var profiles = await _context.PersonalizationProfiles.ToListAsync();
        return View(profiles);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int learnerId, int profileId)
    {
        var profile = await _context.PersonalizationProfiles
            .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

        if (profile == null)
        {
            return NotFound();
        }

        return View(profile);
    }
    // [HttpPost]
    // public async Task<IActionResult> Edit(int learnerId, int profileId, PersonalizationProfiles model)
    // {
    //     if (learnerId != model.LearnerID || profileId != model.ProfileID)
    //     {
    //         return BadRequest();
    //     }
    //
    //     if (ModelState.IsValid)
    //     {
    //         try
    //         {
    //             var result = await _context.Database.ExecuteSqlRawAsync(
    //                 "EXEC ProfileUpdate @learnerID = {0}, @ProfileID = {1}, @PreferedContentType = {2}, @emotional_state = {3}, @PersonalityType = {4}",
    //                 model.LearnerID, model.ProfileID, model.PreferredContentType, model.EmotionalState, model.PersonalityType);
    //             await _context.SaveChangesAsync();
    //             if (result == 0)
    //             {
    //                 return NotFound("LearnerID or ProfileID does not exist.");
    //             }
    //
    //             return RedirectToAction(nameof(Index));
    //         }
    //         catch (Exception ex)
    //         {
    //             ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
    //         }
    //     }
    //
    //     return View(model);
    // }
    
    [HttpPost]
    public IActionResult Edit(int learnerId, int profileId, PersonalizationProfiles model)
    {
        if (learnerId != model.LearnerID || profileId != model.ProfileID)
        {
            return BadRequest();
        }

        var profile = _context.PersonalizationProfiles
            .FirstOrDefault(p => p.LearnerID == learnerId && p.ProfileID == profileId);
        if (profile == null)
        {
            return NotFound();
        }

        profile.PreferredContentType = model.PreferredContentType;
        profile.EmotionalState = model.EmotionalState;
        profile.PersonalityType = model.PersonalityType;
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
    
    
    
    [HttpPost]
    public async Task<IActionResult> Delete(int learnerId, int profileId)
    {
        var profile = await _context.PersonalizationProfiles
            .FirstOrDefaultAsync(p => p.LearnerID == learnerId && p.ProfileID == profileId);

        if (profile == null)
        {
            return NotFound();
        }

        _context.PersonalizationProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
