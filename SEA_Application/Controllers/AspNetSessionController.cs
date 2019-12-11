using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using System.Globalization;

namespace SEA_Application.Controllers
{
    public class AspNetSessionController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetSession
        public ActionResult Index()
        {
            return View(db.AspNetSessions.ToList());
        }

        public ActionResult Indexs()
        {
            ViewBag.Error = "Session Started successfully";
            return View("Index", db.AspNetSessions.ToList());
        }

        // GET: AspNetSession/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }

        // GET: AspNetSession/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetSession/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SessionName,SessionStartDate,SessionEndDate,Status")] AspNetSession aspNetSession)
        {

            var TransactionObj = db.Database.BeginTransaction();
            if (db.AspNetSessions.Where(x => x.Status == "Active").Count() == 0 || aspNetSession.Status == "InActive" )
            {
                
                    if (ModelState.IsValid)
                    {

                        aspNetSession.SessionEndDate = Convert.ToDateTime(Request.Form["SessionEndDate"]);
                        aspNetSession.SessionStartDate = Convert.ToDateTime(Request.Form["SessionStartDate"]);
                        db.AspNetSessions.Add(aspNetSession);
                        db.SaveChanges();
                    }
                    TransactionObj.Commit();
                    ////////////////////////////////////////////////////////Term Add/////////////////////////////////////////////////////////////////
                    int length = 3;
                    for (int i = 0; i < length; i++)
                    {
                        AspNetTerm aspnetTerm = new AspNetTerm();
                        aspnetTerm.SessionID = aspNetSession.Id;
                        aspnetTerm.TermName = "Term " + (i + 1);
                        aspnetTerm.TermStartDate = DateTime.Now;
                        aspnetTerm.TermEndDate = DateTime.Now;
                        aspnetTerm.Status = "InActive";
                        db.AspNetTerms.Add(aspnetTerm);
                        db.SaveChanges();
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    var UserNameLog = User.Identity.Name;
                    AspNetUser a = db.AspNetUsers.First(x => x.UserName == UserNameLog);
                    string UserIDLog = a.Id;
                    var logMessage = "New Session Added, SessionName: " + aspNetSession.SessionName + ", SessionStartDate: " + aspNetSession.SessionStartDate + ", SessionEndDate: " + aspNetSession.SessionEndDate + ", Status: " + aspNetSession.Status;

                    var LogControllerObj = new AspNetLogsController();
                    LogControllerObj.CreateLogSave(logMessage, UserIDLog);
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


                    return RedirectToAction("Indexs");
                
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        // GET: AspNetSession/Edit/5

            public ActionResult CheckStatus(string status)
            {
            string status1 = "error";
            if (status == "Active")
            {
                if(db.AspNetSessions.Where(x => x.Status == "Active").Count() == 0)
                {
                    status1 = "success";
                }
            
            }
              return  Content(status1);
            }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }

        // POST: AspNetSession/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SessionName,SessionStartDate,SessionEndDate,Status")] AspNetSession aspNetSession)
        {
            if (ModelState.IsValid)
            {
                if (db.AspNetSessions.Where(x => x.Status == "Active").Count() == 0 || aspNetSession.Status == "InActive")
                {
                    db.Entry(aspNetSession).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(aspNetSession);
        }

        // GET: AspNetSession/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            if (aspNetSession == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSession);
        }
        public ActionResult DeleteSession(int id)
        {
            var session = db.AspNetSessions.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            try
            {
                db.AspNetSessions.Remove(session);
                db.SaveChanges();
                return RedirectToAction("Index");

            }
            catch
            {
                ViewBag.Error = "This session has terms in it. It can't be deleted";
                return View("Details", session);
            }
        }

        // POST: AspNetSession/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetSession aspNetSession = db.AspNetSessions.Find(id);
            db.AspNetSessions.Remove(aspNetSession);
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