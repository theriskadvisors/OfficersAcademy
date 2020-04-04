using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspnetSubjectTopicsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetSubjectTopics
        public ActionResult Index()
        {
            var aspnetSubjectTopics = db.AspnetSubjectTopics.Include(a => a.AspNetSubject);
            return View(aspnetSubjectTopics.ToList());
        }

        // GET: AspnetSubjectTopics/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetSubjectTopic aspnetSubjectTopic = db.AspnetSubjectTopics.Find(id);
            if (aspnetSubjectTopic == null)
            {
                return HttpNotFound();
            }
            return View(aspnetSubjectTopic);
        }

        // GET: AspnetSubjectTopics/Create
        public ActionResult Create()
        {
            ViewBag.SubjectId = new SelectList(db.AspNetSubjects, "Id", "SubjectName");

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
         
            return View();
        }

        // POST: AspnetSubjectTopics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,StartDate,EndDate,SubjectId,FAQ")] AspnetSubjectTopic aspnetSubjectTopic)
        {
            if (ModelState.IsValid)
            {
                db.AspnetSubjectTopics.Add(aspnetSubjectTopic);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SubjectId = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspnetSubjectTopic.SubjectId);
            return View(aspnetSubjectTopic);
        }


        public JsonResult StudentByClass(int id)
        {
            var result1 = db.AspNetSubjects.Where(x => x.ClassID == id).ToList();
            var obj = JsonConvert.SerializeObject(result1);



            return Json(obj, JsonRequestBehavior.AllowGet);

        }


        // GET: AspnetSubjectTopics/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetSubjectTopic aspnetSubjectTopic = db.AspnetSubjectTopics.Find(id);
            if (aspnetSubjectTopic == null)
            {
                return HttpNotFound();
            }

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectId = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspnetSubjectTopic.SubjectId);
            return View(aspnetSubjectTopic);
        }

        // POST: AspnetSubjectTopics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,StartDate,EndDate,SubjectId,FAQ")] AspnetSubjectTopic aspnetSubjectTopic)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspnetSubjectTopic).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubjectId = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspnetSubjectTopic.SubjectId);
            return View(aspnetSubjectTopic);
        }

        // GET: AspnetSubjectTopics/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetSubjectTopic aspnetSubjectTopic = db.AspnetSubjectTopics.Find(id);
            if (aspnetSubjectTopic == null)
            {
                return HttpNotFound();
            }
            return View(aspnetSubjectTopic);
        }

        // POST: AspnetSubjectTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetSubjectTopic aspnetSubjectTopic = db.AspnetSubjectTopics.Find(id);
            db.AspnetSubjectTopics.Remove(aspnetSubjectTopic);
            db.SaveChanges();
            return RedirectToAction("Index");
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
