﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LexiconLMS.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Web.Hosting;
using System.Net.Mime;

namespace LexiconLMS.Controllers
{
    public class DocumentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Documents
        public ActionResult Index()
        {

            var documents = db.Documents.Include(d => d.Activity).Include(d => d.Course).Include(d => d.Module);
            return View(documents.ToList());
        }

        public ActionResult DocumentFilterRoot(int courseid)
        {

            IQueryable<Document> document = db.Documents.Where(z => z.CourseId == courseid && z.ModuleId == null && z.ActivityId == null);
            ViewBag.courseid = courseid;
            ViewBag.coursename = db.Courses.Where(v => v.CourseID == courseid).Select(x => x.Name).SingleOrDefault().ToString();
            TempData["courseid"] = courseid;
            TempData["modulid"] = null;
            TempData["activityid"] = null;
            return View("Index", document.ToList());
        }

        public ActionResult DocumentFilter(int? courseid, int? modulid, int? activityid)
        {
            if (courseid != null && modulid == null && activityid == null)
            {
                IQueryable<Document> document = db.Documents.Where(x => x.CourseId == courseid);
                ViewBag.courseid = courseid;
                ViewBag.coursename = db.Courses.Where(v => v.CourseID == courseid).Select(x => x.Name).SingleOrDefault().ToString();
                TempData["courseid"] = courseid;
                return View("Index", document.ToList());
            }
            else if (courseid != null && modulid != null && activityid == null)
            {
                if (courseid == null)
                {
                    courseid = Convert.ToInt32(TempData["courseid"]);
                }
                IQueryable<Document> document = db.Documents.Where(z => z.CourseId == courseid && z.ModuleId == modulid);
                ViewBag.courseid = courseid;
                ViewBag.modulid = modulid;
                ViewBag.coursename = db.Courses.Where(v => v.CourseID == courseid).Select(x => x.Name).SingleOrDefault().ToString();
                ViewBag.modulname = db.Modules.Where(v => v.ModuleID == modulid).Select(x => x.Name).SingleOrDefault().ToString();
                TempData["courseid"] = courseid;
                TempData["modulid"] = modulid;
                return View("Index", document.ToList());
            }
            else
            {
                if (courseid == null)
                {
                    courseid = Convert.ToInt32(TempData["courseid"]);
                }
                IQueryable<Document> document = db.Documents.Where(z => z.CourseId == courseid && z.ModuleId == modulid && z.ActivityId == activityid);
                ViewBag.courseid = courseid;
                ViewBag.modulid = modulid;
                ViewBag.activityid = activityid;
                ViewBag.activity = activityid; // For show in view
                ViewBag.coursename = db.Courses.Where(v => v.CourseID == courseid).Select(x => x.Name).SingleOrDefault().ToString();
                ViewBag.modulname = db.Modules.Where(v => v.ModuleID == modulid).Select(x => x.Name).SingleOrDefault().ToString();
                ViewBag.activityname = db.Activities.Where(v => v.ActivityId == activityid).Select(x => x.Name).SingleOrDefault().ToString();

                TempData["courseid"] = courseid;
                TempData["modulid"] = modulid;
                TempData["activityid"] = activityid;

                return View("Index", document.ToList());

            }
        }

        [HttpPost]
        public ActionResult Upload([Bind(Include = "DocumentId")] Document document, HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    document.UserId = User.Identity.GetUserId();
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Files"), fileName);
                    file.SaveAs(path);
                    document.DeadlineDate = DateTime.Now;
                    document.TimeStamp = DateTime.Now;
                    document.FileName = fileName;
                    document.FilePath = path;
                    document.CourseId = Convert.ToInt32(TempData["courseid"]);
                    if (TempData["modulid"] != null)
                    {
                        document.ModuleId = Convert.ToInt32(TempData["modulid"]);
                    }
                    if (TempData["activityid"] != null)
                    {
                        document.ActivityId = Convert.ToInt32(TempData["activityid"]);
                    }

                    db.Documents.Add(document);
                    db.SaveChanges();
                    if (TempData["courseid"] != null && TempData["modulid"] == null && TempData["activityid"] == null)
                    {
                        return RedirectToAction("DocumentFilterRoot", new { courseid = TempData["courseid"] });
                    }
                    return RedirectToAction("DocumentFilter", new { courseid = TempData["courseid"], modulid = TempData["modulid"], activityid = TempData["activityid"] });
                }
                ViewBag.Message = "Upload successful";
                return RedirectToAction("DocumentFilter", new { courseid = TempData["courseid"], modulid = TempData["modulid"], activityid = TempData["activityid"] });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Message = "Upload failed";
                if (TempData["courseid"] != null && TempData["modulid"] == null && TempData["activityid"] == null)
                {
                    return RedirectToAction("DocumentFilterRoot", new { courseid = TempData["courseid"] });
                }
                return RedirectToAction("DocumentFilter", new { courseid = TempData["courseid"], modulid = TempData["modulid"], activityid = TempData["activityid"] });
                //  return RedirectToAction("Index");
            }

        }

        public FilePathResult GetFileFromDisk(int documenid)
        {
            string fileName = db.Documents.Where(z => z.DocumentId == documenid).Select(x => x.FileName).SingleOrDefault();
            return File("~/Files/" + fileName, MediaTypeNames.Text.Plain, fileName);
        }



        // GET: Documents/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // GET: Documents/Create
        public ActionResult Create()
        {
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Name");
            ViewBag.CourseId = new SelectList(db.Courses, "CourseID", "Name");
            ViewBag.ModuleId = new SelectList(db.Modules, "ModuleID", "Name");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DocumentId,FileName,FilePath,Description,TimeStamp,DeadlineDate,UserId,CourseId,ModuleId,ActivityId")] Document document)
        {
            if (ModelState.IsValid)
            {
                db.Documents.Add(document);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Name", document.ActivityId);
            ViewBag.CourseId = new SelectList(db.Courses, "CourseID", "Name", document.CourseId);
            ViewBag.ModuleId = new SelectList(db.Modules, "ModuleID", "Name", document.ModuleId);
            return View(document);
        }

        // GET: Documents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Name", document.ActivityId);
            ViewBag.CourseId = new SelectList(db.Courses, "CourseID", "Name", document.CourseId);
            ViewBag.ModuleId = new SelectList(db.Modules, "ModuleID", "Name", document.ModuleId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DocumentId,FileName,FilePath,Description,TimeStamp,DeadlineDate,UserId,CourseId,ModuleId,ActivityId")] Document document)
        {
            if (ModelState.IsValid)
            {
                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ActivityId = new SelectList(db.Activities, "ActivityId", "Name", document.ActivityId);
            ViewBag.CourseId = new SelectList(db.Courses, "CourseID", "Name", document.CourseId);
            ViewBag.ModuleId = new SelectList(db.Modules, "ModuleID", "Name", document.ModuleId);
            return View(document);
        }

        // GET: Documents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Document document = db.Documents.Find(id);
            db.Documents.Remove(document);
            db.SaveChanges();
            return RedirectToAction("DocumentFilter", new { courseid = TempData["courseid"], modulid = TempData["modulid"], activityid = TempData["activityid"] });
            //return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}