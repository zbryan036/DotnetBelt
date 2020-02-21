using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using DotnetBelt.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetBelt.Controllers {
    public class HomeController : Controller {
        public BeltContext context;
        public HomeController(BeltContext Context) {
            context = Context;
        }
        public IActionResult Index() {
            if(!LoggedIn()) return View("Login");
            int LUID = (int)HttpContext.Session.GetInt32("LUID");
            ViewBag.Occurrences = context.Occurrences.Include(o => o.Plans)
            .Include(o => o.Creator).Where(o => o.OTime > DateTime.Now).OrderByDescending(o => o.OTime);
            ViewBag.ActiveUser = context.Users.Include(u => u.Plans).FirstOrDefault(u => u.UserId == LUID);
            return View();
        }
        [HttpGet("/newoccurrence")]
        public ViewResult NewOccurrence() {
            if(!LoggedIn()) return View("Login");
            return View();
        }
        public IActionResult CreateOccurrence(Occurrence O) {
            if(!LoggedIn()) return View("Login");
            if (ModelState.IsValid) {
                if (O.OTime - DateTime.Now < new TimeSpan(0)) {
                    ModelState.AddModelError("OTime", "Event must occur in the future");
                } else {
                    O.Plans = new List<Plan>();
                    O.Creator = context.Users.FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("LUID"));
                    context.Add(O);
                    context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View("NewOccurrence");
        }
        [HttpGet("/occurrences/{id}")]
        public IActionResult OneOccurrence(int id) {
            if(!LoggedIn()) return View("Login");
            ViewBag.Occurrence = context.Occurrences.Include(o => o.Creator)
            .Include(o => o.Plans).ThenInclude(p => p.U)
            .FirstOrDefault(o => o.OccurrenceId == id);
            ViewBag.ActiveUser = context.Users.Include(u => u.Plans)
            .ThenInclude(p => p.O)
            .FirstOrDefault(u => u.UserId == (int)HttpContext.Session.GetInt32("LUID"));
            return View();
        }
        [HttpGet("/{id}/join")]
        public IActionResult Join(int id) {
            if(!LoggedIn()) return View("Login");
            int LUID = (int)HttpContext.Session.GetInt32("LUID");
            if(context.Plans.Any(p => p.UserId == LUID && p.OccurrenceId == id)) return RedirectToAction("Index");
            Plan P = new Plan();
            P.UserId = LUID;
            P.OccurrenceId = id;
            P.U = context.Users.FirstOrDefault(u => u.UserId == LUID);
            P.O = context.Occurrences.FirstOrDefault(o => o.OccurrenceId == P.OccurrenceId);
            context.Add(P);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet("/{id}/leave")]
        public IActionResult Leave(int id) {
            if(!LoggedIn()) return View("Login");
            int LUID = (int)HttpContext.Session.GetInt32("LUID");
            if(!context.Plans.Any(p => p.UserId == LUID && p.OccurrenceId == id)) return RedirectToAction("Index");
            Plan P = context.Plans.Where(r => r.OccurrenceId == id).FirstOrDefault(r => r.UserId == LUID);
            context.Remove(P);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet("{id}/delete")]
        public IActionResult Delete(int id) {
            if(!LoggedIn()) return View("Login");
            Occurrence O = context.Occurrences.FirstOrDefault(o => o.OccurrenceId == id);
            if(O.GetType() == typeof(User) && HttpContext.Session.GetInt32("LUID") == O.Creator.UserId) {
                return RedirectToAction("Index");
            }
            context.Remove(O);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Login(LogUser loggingUser) {
            PasswordHasher<LogUser> LHasher = new PasswordHasher<LogUser>();
            if (ModelState.IsValid) {
                User match = context.Users.FirstOrDefault(u => u.Email == loggingUser.LEmail);
                if(match == null) {
                    ModelState.AddModelError("LEmail", "We don't have a user with that email address");
                } else if(LHasher.VerifyHashedPassword(loggingUser, match.Password, loggingUser.LPassword) == 0) {
                    ModelState.AddModelError("LPassword", "Email Address and Password do not match");
                } else {
                    HttpContext.Session.SetInt32("LUID", match.UserId);
                    return RedirectToAction("Index");
                }
            }
            return View();
        }
        public IActionResult Register(User newUser) {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            if(ModelState.IsValid) {
                foreach(User u in context.Users) {
                    if(u.Email == newUser.Email) {
                        ModelState.AddModelError("Email", "That email address is already in use");
                        return View("Login");
                    }
                }
                if(PCheck(newUser, "abcdefghijklmnopqrstuvwxyz")) {
                    ModelState.AddModelError("Password", "Password must contain at least one letter");
                    return View("Login");
                } else if(PCheck(newUser, "1234567890")) {
                    ModelState.AddModelError("Password", "Password must contain at least one number");
                    return View("Login");
                } else if(PCheck(newUser, "`~!@#$%^&*()_-+={[}]|\\:;\"'<,>.?/")) {
                    ModelState.AddModelError("Password", "Password must contain at least one special character(!@#, etc.)");
                    return View("Login");
                }
                newUser.Plans = new List<Plan>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                context.Add(newUser);
                context.SaveChanges();
                HttpContext.Session.SetInt32("LUID", newUser.UserId);
                return RedirectToAction("Index");
            }
            return View("Login");
        }
        [HttpGet("/logout")]
        public ViewResult LogOut() {
            HttpContext.Session.SetInt32("LUID", -1);
            return View("Login");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet("/cleardata")]
        public RedirectToActionResult ClearData() {
            foreach(Occurrence x in context.Occurrences) context.Remove(x);
            foreach(User x in context.Users) context.Remove(x);
            context.SaveChanges();
            System.Console.WriteLine("###############################################################\nDatabase Cleared\n\n");
            return RedirectToAction("Index");
        }
        public bool LoggedIn() {
            int? LUID = HttpContext.Session.GetInt32("LUID");
            if(LUID == null || LUID == -1) return false;
            return true;
        }
        public bool PCheck(User u, string s) {
            System.Console.WriteLine("#################################################################");
            System.Console.WriteLine("Checking Password with " + s);
            string pass = u.Password.ToLower();
            foreach(char c in s) {
                if(pass.Contains(c)) return false;
            }
            System.Console.WriteLine("Invalid");
            return true;
        }
    }
}
