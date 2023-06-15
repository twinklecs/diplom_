using CRM.Data;
using CRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Controllers
{
    public class NotesController : Controller
    {
        private readonly CRMContext _context;

        public NotesController(CRMContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            ViewBag.userId = user.Id;
            List<Contact> contactList = _context.Contact.ToList();
            string[] contacts = new string[contactList[contactList.Count - 1].Id + 1];
            var i = 1;
            foreach (var item in contactList)
            {
                i = item.Id;
                contacts[i] = Convert.ToString(item.FIO);
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
            return View(await _context.Note.ToListAsync());
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Note
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
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
            return View(note);
        }

        [Authorize]
        public async Task<IActionResult> CreateAsync(int? com)
        {
            ViewBag.id = com;
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            var contact = await _context.Contact.FindAsync(com);
            if (contact != null)
            {
                ViewBag.name = contact.FIO;
            }
            ViewBag.user = user.Id;
            List<Contact> contactList = _context.Contact.ToList();
            ViewBag.data = contactList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                note.IsDeleted = 0;
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Note.FindAsync(id);
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            if (note == null)
            {
                return NotFound();
            }
            List<Contact> contactList = _context.Contact.ToList();
            ViewBag.data = contactList;
            if (user.Id != note.UserId)
            {
                return NotFound();
            }
            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id))
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
            return View(note);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _context.User.FirstOrDefaultAsync(m => m.Login == User.FindFirst("user").Value);
            var note = await _context.Note
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }
            if (user.Id != note.UserId)
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
            return View(note);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Note.FindAsync(id);
            note.IsDeleted = 1;
            //_context.Note.Remove(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id)
        {
            return _context.Note.Any(e => e.Id == id);
        }
    }
}
