// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MyLMS2.Models;

namespace MyLMS2.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<User> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        // عشان نوصل الـ ReturnUrl للـ View
        [BindProperty]
        public string ReturnUrl { get; set; }

        public void OnGet()
        {
            // نجيب الصفحة اللي جه منها
            ReturnUrl = HttpContext.Request.Headers["Referer"].ToString();

            // fallback للهوم لو الـ Referer فاضي
            if (string.IsNullOrEmpty(ReturnUrl))
            {
                ReturnUrl = Url.Page("/", new { area = "" });
            }
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // لو مفيش returnUrl نرجع للهوم
            return RedirectToPage("/Index");
        }
    }
}
