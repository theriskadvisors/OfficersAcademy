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
    public class AspNetTimeTableController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);
        // GET: AspNetTimeTable
        public ActionResult Index()
        {
            var aspNetTimeTables = db.AspNetTimeTables.Include(a => a.AspNetRoom).Include(a => a.AspNetSubject).Include(a => a.AspNetTimeslot);
            return View(aspNetTimeTables.ToList());
        }

        // GET: AspNetTimeTable/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTimeTable aspNetTimeTable = db.AspNetTimeTables.Find(id);
            if (aspNetTimeTable == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTimeTable);
        }

        // GET: AspNetTimeTable/Create
        public ActionResult Create()
        {

            try
            {
                var sessionid = db.AspNetSessions.Where(x => x.Id == SessionID && x.Status == "Active").FirstOrDefault().SessionName;
                ViewBag.RoomID = new SelectList(db.AspNetRooms, "Id", "Name");
                ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => x.AspNetClass.ClassName == sessionid), "Id", "SubjectName");
                ViewBag.SlotID = new SelectList(db.AspNetTimeslots, "Id", "Name");
            }
            catch(Exception ex)
            {
                ViewBag.RoomID = new SelectList("");
                ViewBag.SubjectID = new SelectList("");
                ViewBag.SlotID = new SelectList("");
                return View();
            }
            return View();
        }

        // POST: AspNetTimeTable/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RoomID,SlotID,SubjectID,TeacherID,Timestamp,Day")] AspNetTimeTable aspNetTimeTable)
        {
            if (ModelState.IsValid)
            {
                db.AspNetTimeTables.Add(aspNetTimeTable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RoomID = new SelectList(db.AspNetRooms, "Id", "Name", aspNetTimeTable.RoomID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTimeTable.SubjectID);
            ViewBag.SlotID = new SelectList(db.AspNetTimeslots, "Id", "Name", aspNetTimeTable.SlotID);
            return View(aspNetTimeTable);
        }

        // GET: AspNetTimeTable/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTimeTable aspNetTimeTable = db.AspNetTimeTables.Find(id);
            if (aspNetTimeTable == null)
            {
                return HttpNotFound();
            }
            ViewBag.RoomID = new SelectList(db.AspNetRooms, "Id", "Name", aspNetTimeTable.RoomID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTimeTable.SubjectID);
            ViewBag.SlotID = new SelectList(db.AspNetTimeslots, "Id", "Name", aspNetTimeTable.SlotID);
            return View(aspNetTimeTable);
        }

        // POST: AspNetTimeTable/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RoomID,SlotID,SubjectID,TeacherID,Timestamp,Day")] AspNetTimeTable aspNetTimeTable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetTimeTable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RoomID = new SelectList(db.AspNetRooms, "Id", "Name", aspNetTimeTable.RoomID);
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetTimeTable.SubjectID);
            ViewBag.SlotID = new SelectList(db.AspNetTimeslots, "Id", "Name", aspNetTimeTable.SlotID);
            return View(aspNetTimeTable);
        }

        // GET: AspNetTimeTable/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetTimeTable aspNetTimeTable = db.AspNetTimeTables.Find(id);
            if (aspNetTimeTable == null)
            {
                return HttpNotFound();
            }
            return View(aspNetTimeTable);
        }

        // POST: AspNetTimeTable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetTimeTable aspNetTimeTable = db.AspNetTimeTables.Find(id);
            db.AspNetTimeTables.Remove(aspNetTimeTable);
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
