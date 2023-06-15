using CRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    public class RolesController : Controller
    {
        private readonly CRMContext _context;

        public RolesController(CRMContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Role.ToListAsync());
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.Role
                .FirstOrDefaultAsync(m => m.Id == id);
            var user = await _context.User.Where(m => m.RoleId == role.Id).ToListAsync();
            var loggedUser = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            ViewBag.userId = loggedUser.Id;
            if (role == null)
            {
                return NotFound();
            }
            ViewBag.Message = role.Name + "s";
            return View(user);
        }

    }
}
