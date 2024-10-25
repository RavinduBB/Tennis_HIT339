using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using AnyoneForTennis.Areas.Identity.Data;
using AnyoneForTennis.Models;

namespace AnyoneForTennis.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AnyoneForTennisUser> _signInManager;
        private readonly UserManager<AnyoneForTennisUser> _userManager;
        private readonly IUserStore<AnyoneForTennisUser> _userStore;
        private readonly IUserEmailStore<AnyoneForTennisUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly AnyoneForTennisContext _context;

        public RegisterModel(
            UserManager<AnyoneForTennisUser> userManager,
            IUserStore<AnyoneForTennisUser> userStore,
            SignInManager<AnyoneForTennisUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            AnyoneForTennisContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel(); // Initialize to avoid null

        public string? ReturnUrl { get; set; } // Allow null since it can be optional

        public IList<AuthenticationScheme>? ExternalLogins { get; set; } // Allow null for external logins

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty; // Initialize to avoid null

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty; // Initialize to avoid null

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty; // Initialize to avoid null

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; } = string.Empty; // Initialize to avoid null

            // Member-specific fields
            public string? MemberFirstName { get; set; }
            public string? MemberLastName { get; set; }

            // Coach-specific fields
            public string? CoachFirstName { get; set; }
            public string? CoachLastName { get; set; }
            public string? Biography { get; set; }
            public IFormFile? Photo { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null) // Allow null
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null) // Allow null
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Assign role to the user
                    var roleResult = await _userManager.AddToRoleAsync(user, Input.Role);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }

                    if (Input.Role == "Member")
                    {
                        var member = new Member
                        {
                            FirstName = Input.MemberFirstName,
                            LastName = Input.MemberLastName,
                            Email = Input.Email,
                            Active = true,
                            UserId = user.Id
                        };
                        _context.Members.Add(member);
                    }
                    else if (Input.Role == "Coach")
                    {
                        string? photoPath = null;
                        if (Input.Photo != null)
                        {
                            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "ProfileImages");
                            if (!Directory.Exists(imagesPath))
                            {
                                Directory.CreateDirectory(imagesPath);
                            }

                            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(Input.Photo.FileName)}";
                            var filePath = Path.Combine(imagesPath, uniqueFileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await Input.Photo.CopyToAsync(stream);
                            }
                            photoPath = $"/images/ProfileImages/{uniqueFileName}";
                        }

                        var coach = new Coach
                        {
                            FirstName = Input.CoachFirstName,
                            LastName = Input.CoachLastName,
                            Biography = Input.Biography,
                            Photo = photoPath,
                            UserId = user.Id
                        };
                        _context.Coaches.Add(coach);
                    }

                    await _context.SaveChangesAsync();

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        private AnyoneForTennisUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AnyoneForTennisUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AnyoneForTennisUser)}'. " +
                    $"Ensure that '{nameof(AnyoneForTennisUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AnyoneForTennisUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AnyoneForTennisUser>)_userStore;
        }
    }
}
