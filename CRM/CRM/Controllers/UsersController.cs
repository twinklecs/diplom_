using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    public class UsersController : Controller
    {
        private readonly CRMContext _context;

        public UsersController(CRMContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 1, string sortExpression = "Id")
        {
            var qry = _context.User.AsNoTracking().OrderBy(p => p.Id).Where(p => p.IsDeleted == 0);
            var model = await PagingList.CreateAsync(qry, 6, page, sortExpression, "Id");
            string[] roles = { "Admin", "Moderator", "User" };
            ViewBag.roles = roles;
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
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
            string[] roles = { "Admin", "Moderator", "User" };
            ViewBag.roles = roles;
            return View(user);
        }

        [Authorize(Roles = "Admin, Moderator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                var user1 = await _context.User.AsNoTracking().FirstOrDefaultAsync(m => m.Login == user.Login);
                if (user1 == null)
                {
                    user.Password = HashPassword(user.Password);
                    user.IsDeleted = 0;
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Логин принят");
                    return View(user);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Логин принят");
                return View(user);
            }
        }

        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
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

            try
            {
                var user1 = await _context.User.AsNoTracking().FirstOrDefaultAsync(m => m.Login == user.Login);
                if (user1 == null || id == user1.Id)
                {

                    if (ModelState.IsValid)
                    {
                        ViewBag.Message = null;
                        if (adminnumber > 2 || user.RoleId == 1)
                        {
                            if (!IsMD5(user.Password))
                            {
                                user.Password = HashPassword(user.Password);
                            }
                            _context.Update(user);
                            await _context.SaveChangesAsync();
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Message = String.Format("Вы не можете понизить в должности больше администраторов! Их должно быть как минимум 2!");
                            return View(user);
                        }
                    }
                    return View(user);
                }
                else
                {
                    ModelState.AddModelError("", "Логин принят");
                    return View(user);
                }
            }
            catch (Exception)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (id != user.Id)
                        {
                            return NotFound();
                        }
                        if (!IsMD5(user.Password))
                        {
                            user.Password = HashPassword(user.Password);
                        }
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        return View(user);
                    }
                    else
                    {
                        return View(user);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Неверные данные");
                    return View(user);
                }
            }


        }

        [Authorize(Roles = "Admin")]
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
            string[] roles = { "Admin", "Moderator", "User" };
            ViewBag.roles = roles;
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
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.Message = String.Format("Вы не можете удалить больше администраторов! Их должно быть как минимум 2!");
                return View(user);
            }
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
        public static bool IsMD5(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return false;
            }

            return Regex.IsMatch(input, "^[0-9a-fA-F]{32}$", RegexOptions.Compiled);
        }
    }
}
