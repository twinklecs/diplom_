using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    public class AccountsController : Controller
    {
        private readonly CRMContext _context;

        public AccountsController(CRMContext context)
        {
            _context = context;
        }

        const string UserName = "_Name";
        const string SessionAge = "_Age";

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> DetailsAsync()
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View(user);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        private async Task<bool> ValidateLoginAsync(string userName, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == userName);
            try
            {
                if (password == user.Password && user.IsDeleted == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> ReturnRole(string userName)
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == userName);
            var role = await _context.Role.FindAsync(Convert.ToInt32(user.RoleId));
            return role.Name;
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                model.Password = HashPassword(model.Password);

                if (await ValidateLoginAsync(model.UserName, model.Password))
                {

                    HttpContext.Session.SetString(UserName, model.UserName);
                    string role = await ReturnRole(model.UserName);
                    var claims = new List<Claim>
                {
                    new Claim("user", model.UserName),
                    new Claim("role", role)
                };

                    await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Неверный логин или пароль");
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }

        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                var user1 = await _context.User.FirstOrDefaultAsync(m => m.Login == user.Login);
                if (user1.Id == 0)
                {
                    if (ModelState.IsValid)
                    {
                        user.Password = HashPassword(user.Password);
                        _context.Add(user);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Login");
                    }
                    return View(user);
                }
                else
                {
                    ModelState.AddModelError("", "Вход в систему принят");
                    return View(user);
                }
            }
            catch (Exception)
            {
                try
                {
                    user.Password = HashPassword(user.Password);
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Login");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Неверные данные");
                    return View(user);
                }

            }
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            var adminnumber = await _context.User.CountAsync(m => m.RoleId == 1);
            ViewBag.Message = null;

            try
            {
                int x = await LoginUnChangedAsync(id, user.Login);
                if (x < 3)
                {
                    if (ModelState.IsValid)
                    {
                        if (adminnumber > 2 || user.RoleId == 1)
                        {
                            if (!IsMD5(user.Password))
                            {
                                user.Password = HashPassword(user.Password);
                            }
                            _context.Update(user);
                            await _context.SaveChangesAsync();
                            if (x == 1 || user.RoleId != Convert.ToInt32(User.FindFirst("role").Value))
                            {
                                await HttpContext.SignOutAsync();
                                string role = await ReturnRole(user.Login);
                                var claims = new List<Claim>
                            {
                                new Claim("user", user.Login),
                                new Claim("role", role)
                            };

                                await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));
                            }
                            return RedirectToAction("Details");
                        }
                        else
                        {
                            ViewBag.Message = String.Format("Вы не можете удалить больше администраторов! Их должно быть как минимум 2!");
                            return View(user);
                        }
                    }
                    return View(user);
                }
                else
                {
                    ModelState.AddModelError("", "Вход в систему принят");
                    return View(user);
                }
            }
            catch (Exception)
            {
                try
                {
                    if (!IsMD5(user.Password))
                    {
                        user.Password = HashPassword(user.Password);
                    }
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    await HttpContext.SignOutAsync();
                    string role = await ReturnRole(user.Login);
                    var claims = new List<Claim>
                        {
                            new Claim("user", user.Login),
                            new Claim("role", role)
                        };

                    await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies", "user", "role")));

                    return RedirectToAction("Details");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Неверные данные");
                    return View(user);
                }

            }
        }
        public IActionResult AccessDenied(string returnUrl = null)
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        public string HashPassword(string password)
        {
            byte[] hashedPassword;
            using (MD5 md5 = MD5.Create())
            {
                hashedPassword = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashedPassword.Length; i++)
            {
                sb.Append(hashedPassword[i].ToString("x2"));
            }

            return Convert.ToString(sb);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            var adminnumber = await _context.User.CountAsync(m => m.RoleId == 1);
            
            ViewBag.Message = null;
            if (adminnumber > 2 || user.RoleId != 1)
            {
                user.IsDeleted = 1;
                await _context.SaveChangesAsync();
                await HttpContext.SignOutAsync();
                return Redirect("/");
            }
            else
            {
                ViewBag.Message = String.Format("Вы не можете удалить больше администраторов! Их должно быть как минимум 2!");
                return View(user);
            }

        }
        public static bool IsMD5(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return false;
            }

            return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
        }

        public async Task<int> LoginUnChangedAsync(int id, string login)
        {
            if (login == User.FindFirst("user").Value)
            {
                return 1;
            }
            else
            {
                var user1 = await _context.User.FirstOrDefaultAsync(m => m.Login == login);
                if (user1 == null)
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }
        }
    }
}
