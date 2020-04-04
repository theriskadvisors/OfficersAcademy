using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspnetQuizsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetQuizs
        public ActionResult Index()
        {
            return View(db.AspnetQuizs.ToList());
        }

        // GET: AspnetQuizs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuiz);
        }

        // GET: AspnetQuizs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspnetQuizs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Start_Date,Due_Date,Created_By,CreationDate")] AspnetQuiz aspnetQuiz)
        {
            if (ModelState.IsValid)
            {
                db.AspnetQuizs.Add(aspnetQuiz);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspnetQuiz);
        }

        // GET: AspnetQuizs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuiz);
        }

        // POST: AspnetQuizs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Start_Date,Due_Date,Created_By,CreationDate")] AspnetQuiz aspnetQuiz)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspnetQuiz).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspnetQuiz);
        }

        // GET: AspnetQuizs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuiz);
        }

        // POST: AspnetQuizs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            db.AspnetQuizs.Remove(aspnetQuiz);
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
