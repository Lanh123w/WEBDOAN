using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication3.Data;
using WEBDOAN.Models;

namespace WebApplication3.Controllers;


public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOnlineUserService _onlineUserService;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IOnlineUserService onlineUserService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _onlineUserService = onlineUserService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            // ✅ Ghi lại thời gian đăng nhập
            var activity = new UserActivity
            {
                UserId = user.Id,
                LastLoginTime = DateTime.Now
            };
            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();

            // ✅ Đánh dấu người dùng đang hoạt động
            _onlineUserService.UserActive(user.Id);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Sai thông tin đăng nhập");
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Gán role "USER" cho người dùng mới
            await _userManager.AddToRoleAsync(user, "USER");

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // ✅ Lấy người dùng hiện tại
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
           


            // ✅ Đánh dấu người dùng không còn hoạt động
            _onlineUserService.UserInactive(user.Id);
        }

        // ✅ Đăng xuất khỏi hệ thống
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }



    [HttpPost]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi từ nhà cung cấp: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return RedirectToAction(nameof(Login));

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
        {
            // ✅ Lấy thông tin người dùng đã đăng nhập
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            // ✅ Ghi lại thời gian đăng nhập
            var activity = new UserActivity
            {
                UserId = user.Id,
                LastLoginTime = DateTime.Now
            };
            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();

            // ✅ Đánh dấu người dùng đang hoạt động
            _onlineUserService.UserActive(user.Id);

            var redirectUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("Index", "Home") : returnUrl;
            return LocalRedirect(redirectUrl ?? "/Home/Index");
        }

        // Nếu người dùng chưa có tài khoản, tạo mới
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email != null)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "USER");
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);

                // ✅ Ghi lại thời gian đăng nhập cho tài khoản mới
                var activity = new UserActivity
                {
                    UserId = user.Id,
                    LastLoginTime = DateTime.Now
                };
                _context.UserActivities.Add(activity);
                await _context.SaveChangesAsync();

                // ✅ Đánh dấu người dùng đang hoạt động
                _onlineUserService.UserActive(user.Id);

                var redirectUrl = string.IsNullOrEmpty(returnUrl) ? Url.Action("Index", "Home") : returnUrl;
                return LocalRedirect(redirectUrl ?? "/Home/Index");
            }
        }

        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
