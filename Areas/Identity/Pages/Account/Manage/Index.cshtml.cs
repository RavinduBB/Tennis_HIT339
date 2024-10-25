using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AnyoneForTennis.Areas.Identity.Data;

namespace AnyoneForTennis.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Member,Coach")]
    public class IndexModel(
        UserManager<AnyoneForTennisUser> userManager,
        //SignInManager<AnyoneForTennisUser> signInManager,
        AnyoneForTennisContext context) : PageModel
    {
        private readonly UserManager<AnyoneForTennisUser> _userManager = userManager;
        private readonly AnyoneForTennisContext _context = context;

        public string Username { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string? CoachName { get; set; }
        public string? MemberName { get; set; }
        public string? CoachPhoto { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            public string? CoachFirstName { get; set; }
            public string? CoachLastName { get; set; }
            public string? MemberFirstName { get; set; }
            public string? MemberLastName { get; set; }
            public string? Biography { get; set; }
            public string? Photo { get; set; }
            public IFormFile? PhotoUpload { get; set; }
        }

        private async Task LoadAsync(AnyoneForTennisUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user) ?? "DefaultUser"; // Provide a default if null
            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault() ?? "DefaultRole"; // Provide a default if null

            Username = userName;
            UserRole = userRole;

            Coach? coach = null;
            Member? member = null;

            if (userRole == "Coach")
            {
                coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (coach != null)
                {
                    CoachName = $"{coach.FirstName} {coach.LastName}";
                    CoachPhoto = coach.Photo;
                }
            }
            else if (userRole == "Member")
            {
                member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (member != null)
                {
                    MemberName = $"{member.FirstName} {member.LastName}";
                }
            }

            Input = new InputModel
            {
                CoachFirstName = coach?.FirstName,
                CoachLastName = coach?.LastName,
                MemberFirstName = member?.FirstName,
                MemberLastName = member?.LastName,
                Biography = coach?.Biography,
                Photo = coach?.Photo
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.PhotoUpload != null)
            {
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "ProfileImages");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(Input.PhotoUpload.FileName)}";
                var filePath = Path.Combine(imagesPath, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.PhotoUpload.CopyToAsync(stream);
                }

                var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (coach != null)
                {
                    coach.Photo = $"/images/ProfileImages/{uniqueFileName}";
                    _context.Coaches.Update(coach);
                    await _context.SaveChangesAsync();
                }
            }

            // Ensure UserRole is set before using it
            if (string.IsNullOrEmpty(UserRole))
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                UserRole = userRoles.FirstOrDefault() ?? "DefaultRole"; // Provide a default if null
            }

            if (UserRole == "Coach")
            {
                var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == user.Id);
                if (coach != null)
                {
                    coach.FirstName = Input.CoachFirstName ?? coach.FirstName; // Use existing value if null
                    coach.LastName = Input.CoachLastName ?? coach.LastName; // Use existing value if null
                    coach.Biography = Input.Biography ?? coach.Biography; // Use existing value if null
                    _context.Coaches.Update(coach);
                    await _context.SaveChangesAsync();
                }
            }
            else if (UserRole == "Member")
            {
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
                if (member != null)
                {
                    member.FirstName = Input.MemberFirstName ?? member.FirstName; // Use existing value if null
                    member.LastName = Input.MemberLastName ?? member.LastName; // Use existing value if null
                    _context.Members.Update(member);
                    await _context.SaveChangesAsync();
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
