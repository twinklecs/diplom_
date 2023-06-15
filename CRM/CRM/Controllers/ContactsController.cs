using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    public class ContactsController : Controller
    {
        private readonly CRMContext _context;

        public ContactsController(CRMContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index(string filter)
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            ViewBag.Id = user.Id;

            List<User> usersList = _context.User.ToList();
            string[] users = new string[usersList[usersList.Count - 1].Id + 1];
            var j = 1;
            foreach (var item in usersList)
            {
                j = item.Id;
                users[j] = item.Login;
            }
            ViewBag.data2 = users;
            var qry = _context.Contact.AsNoTracking().OrderBy(p => p.Id).AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                qry = qry.Where(p => p.FIO.Contains(filter));
            }
            var model = await qry.ToListAsync();
            ViewBag.filter = filter;
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }
            List<Contact> contactList = _context.Contact.ToList();
            string[] contacts = new string[contactList[contactList.Count - 1].Id + 1];
            var i = 1;
            foreach (var item in contactList)
            {
                i = item.Id;
                contacts[i] = item.FIO;
            }
            ViewBag.data = contacts;
            List<User> usersList = _context.User.ToList();
            string[] users = new string[usersList[usersList.Count - 1].Id + 1];
            var j = 1;
            foreach (var item in usersList)
            {
                j = item.Id;
                users[j] = item.Login;
            }
            ViewBag.data2 = users;
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            ViewBag.userId = user.Id;
            return View(contact);
        }

        [Authorize]
        public async Task<IActionResult> CreateAsync(int? com)
        {
            ViewBag.id = com;
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            List<User> companiesList = _context.User.ToList();
            ViewBag.data = companiesList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact.FindAsync(id);
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            if (contact == null)
            {
                return NotFound();
            }
            List<Contact> contactList = _context.Contact.ToList();
            ViewBag.data = contactList;
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .FirstOrDefaultAsync(m => m.Id == id);
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            if (contact == null)
            {
                return NotFound();
            }
            List<Contact> contactList = _context.Contact.ToList();
            string[] contacts = new string[contactList[contactList.Count - 1].Id + 1];
            var i = 1;
            foreach (var item in contactList)
            {
                i = item.Id;
                contacts[i] = item.FIO;
            }
            ViewBag.data = contacts;
            List<User> usersList = _context.User.ToList();
            string[] users = new string[usersList[usersList.Count - 1].Id + 1];
            var j = 1;
            foreach (var item in usersList)
            {
                j = item.Id;
                users[j] = item.Login;
            }
            ViewBag.data2 = users;
            ViewBag.userId = user.Id;
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contact.FindAsync(id);
            contact.IsDeleted = 1;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contact.Any(e => e.Id == id);
        }
    }
}
