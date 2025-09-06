using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyLMS2.Models;
using MyLMS2.ViewModels;

namespace MyLMS2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UsersController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchEmail, string roleFilter)
        {
            var users = _userManager.Users.ToList();
            var viewModel = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                viewModel.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = string.Join(", ", roles),
                    LockoutEnd = user.LockoutEnd
                });
            }

            // Apply search
            if (!string.IsNullOrEmpty(searchEmail))
            {
                viewModel = viewModel
                    .Where(u => u.Email.Contains(searchEmail, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Apply filter
            if (!string.IsNullOrEmpty(roleFilter))
            {
                switch (roleFilter)
                {
                    case "Admin":
                    case "Instructor":
                    case "Student":
                        viewModel = viewModel.Where(u => u.Roles.Contains(roleFilter)).ToList();
                        break;
                    case "Disabled":
                        viewModel = viewModel.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now).ToList();
                        break;
                    case "Active":
                        viewModel = viewModel.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.Now).ToList();
                        break;
                }
            }

            // تمرير القيم للـ View
            ViewBag.SearchEmail = searchEmail;
            ViewBag.RoleFilter = roleFilter;

            return View(viewModel);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email
            };

            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // تحديث الإيميل
            user.Email = model.Email;
            user.UserName = model.Email;

            var emailResult = await _userManager.UpdateAsync(user);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // تحديث الباسورد لو المدخل مش فاضي
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var passwordValidator = new PasswordValidator<User>();
                var passwordResult = await passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError("", error.Description);
                    return View(model);
                }

                var hashedPassword = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
                user.PasswordHash = hashedPassword;
                await _userManager.UpdateAsync(user);
            }

            TempData["Message"] = "تم تعديل بيانات المستخدم بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Soft Delete: تعطيل الحساب
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);

            TempData["Message"] = "تم تعطيل الحساب بنجاح (Soft Delete)";
            return RedirectToAction(nameof(Index));
        }
    }
}
