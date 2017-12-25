﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FineUploader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskImpossible.Data;
using TaskImpossible.Models;
using TaskImpossible.Services;

namespace TaskImpossible.Controllers
{
    public class TasksController : Controller
    {
        private readonly ImpossibleContext _context;

        private IHostingEnvironment _environment;

        public TasksController(ImpossibleContext context, IHostingEnvironment env)
        {
            _context = context;
            _environment = env;
        }

        // GET: Tasks
        public async Task<IActionResult> Index(string s, string so, string cf, int? page)
        {
            IQueryable<iTask> tasks;
            ViewData["cs"] = so;

            ViewData["NameSortParm"] = string.IsNullOrEmpty(so) ? "name_desc" : "";
            ViewData["DateSortParm"] = so == "Date" ? "date_desc" : "Date";

            if (s != null)
                page = 1;
            else
                s = cf;

            ViewData["cf"] = s;
            tasks = !string.IsNullOrEmpty(s)
                ? _context.Tasks.Where(t => t.Title.Contains(s) || t.Description.Contains(s) || s == null)
                : _context.Tasks;
            switch (so)
            {
                case "distance":
                    tasks = tasks.OrderByDescending(t => t.Location);
                    break;
                case "name_desc":
                    tasks = tasks.OrderByDescending(t => t.Title);
                    break;
                case "Date":
                    tasks = tasks.OrderBy(t => t.CreationDate);
                    break;
                case "date_desc":
                    tasks = tasks.OrderByDescending(t => t.CreationDate);
                    break;
                default:
                    tasks = tasks.OrderBy(t => t.Title);
                    break;
            }

            var pageSize = 12;
            return View(await PagedList.PaginatedList<iTask>.CreateAsync(tasks.AsNoTracking(), page ?? 1, pageSize));
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var iTask = await _context.Tasks
                .SingleOrDefaultAsync(m => m.Id == id);
            if (iTask == null)
                return NotFound();

            return View(iTask);
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,Owner,Title,Description,Category,Type,Location,Lat,Lon,Radius,NegotiationMarker,Status,CreationDate,StartDate,EndDate,Renumeration,PaymentTerms,Currency,CoverPhoto,DatePretext")]
            iTask iTask)
        {
            if (ModelState.IsValid)
            {
                _context.Add(iTask);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iTask);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var iTask = await _context.Tasks.SingleOrDefaultAsync(m => m.Id == id);
            if (iTask == null)
                return NotFound();
            return View(iTask);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind(
                "Id,Owner,Title,Description,Category,Type,Location,Lat,Lon,Radius,NegotiationMarker,Status,CreationDate,StartDate,EndDate,Renumeration,PaymentTerms,Currency,CoverPhoto,DatePretext")]
            iTask iTask)
        {
            if (id != iTask.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!iTaskExists(iTask.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(iTask);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var iTask = await _context.Tasks
                .SingleOrDefaultAsync(m => m.Id == id);
            if (iTask == null)
                return NotFound();

            return View(iTask);
        }

        // POST: Tasks/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var iTask = await _context.Tasks.SingleOrDefaultAsync(m => m.Id == id);
            _context.Tasks.Remove(iTask);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool iTaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }


        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "Uploads");
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            return RedirectToAction("Index");
        }
    }
}