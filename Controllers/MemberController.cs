using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Areas.Identity.Data;
using AnyoneForTennis.Models;

namespace AnyoneForTennis.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private readonly AnyoneForTennisContext _context;
        private readonly UserManager<AnyoneForTennisUser> _userManager;

        public MemberController(AnyoneForTennisContext context, UserManager<AnyoneForTennisUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View Schedules
        public async Task<IActionResult> MySchedules()
        {
            var userId = _userManager.GetUserId(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var schedules = await _context.MemberSchedules
                .Where(ms => ms.MemberId == member.MemberId)
                .Include(ms => ms.Schedule)
                .ThenInclude(s => s.Coach)
                .Select(ms => ms.Schedule!)
                .ToListAsync();

            return View("MySchedules", schedules);
        }


        // View Coach Profiles
        public async Task<IActionResult> Coaches()
        {
            var coaches = await _context.Coaches.ToListAsync();
            return View("Coaches", coaches);
        }



        // Enroll in Schedule
        [HttpGet]
        public async Task<IActionResult> Enroll()
        {
            var userId = _userManager.GetUserId(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var enrolledScheduleIds = await _context.MemberSchedules
                .Where(ms => ms.MemberId == member.MemberId)
                .Select(ms => ms.ScheduleId)
                .ToListAsync();

            var schedules = await _context.Schedules
                .Include(s => s.Coach)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var viewModel = new EnrollViewModel
            {
                Schedules = schedules,
                EnrolledScheduleIds = enrolledScheduleIds
            };

            return View("Enroll", viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> Enroll(int scheduleId)
        {
            var userId = _userManager.GetUserId(User);
            var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            var memberSchedule = new MemberSchedule
            {
                MemberId = member.MemberId,
                ScheduleId = scheduleId
            };

            _context.MemberSchedules.Add(memberSchedule);
            await _context.SaveChangesAsync();

            return RedirectToAction("MySchedules");
        }

        // View Enrolled Members
        public async Task<IActionResult> EnrolledMembers()
        {
            var userId = _userManager.GetUserId(User);
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == userId);
            if (coach == null)
            {
                return NotFound("Coach not found.");
            }

            var enrolledMembers = await _context.MemberSchedules
                .Where(ms => ms.Schedule.CoachId == coach.CoachId)
                .Select(ms => ms.Member)
                .ToListAsync();

            return View("EnrolledMembers", enrolledMembers);
        }

        // Get: Coach Profile
        public async Task<IActionResult> CoachProfile(int id)
        {
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.CoachId == id);
            if (coach == null)
            {
                return NotFound("Coach not found.");
            }

            return View("CoachProfile", coach);
        }

        // GET: Schedule Details
        public async Task<IActionResult> ScheduleDetails(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Coach)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule == null)
            {
                return NotFound("Schedule not found.");
            }

            return View("ScheduleDetails", schedule);
        }

    }
}
