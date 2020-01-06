using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspNetNotesController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspNetNotes
        public ActionResult Index()
        {
            var aspNetNotes = db.AspNetNotes.Include(a => a.AspNetSubject);
            return View(aspNetNotes.ToList());
        }

        public ActionResult ApproveOrders(int OrderId)
        {

            AspNetOrder OrderToModify = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault();
            OrderToModify.Status = "Approved";


            db.Entry(OrderToModify).State = EntityState.Modified;
            db.SaveChanges(); ;
            
            
           List< AspNetNotesOrder> NotesOrderToModify =  db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).ToList();

           foreach(var NotesOrder in NotesOrderToModify)
            {
                NotesOrder.Status = "Approved";


                db.Entry(NotesOrder).State = EntityState.Modified;

                db.SaveChanges();

            }




            return Json("", JsonRequestBehavior.AllowGet);
        }


        // GET: AspNetNotes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            if (aspNetNote == null)
            {
                return HttpNotFound();
            }
            return View(aspNetNote);
        }

        public ActionResult RecentOrders()
        {
            //  var CurrentUserId = User.Identity.GetUserId();

            //   int StudentId = db.AspNetStudents.Where(x => x.StudentID == CurrentUserId).FirstOrDefault().Id;

            //var result = from Notes in db.AspNetNotes
            //             join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID

            //             select new
            //             {
            //                 Id = OrderNotes.Id,
            //                 Title = Notes.Title,
            //                 Discription = Notes.Description,
            //                 CourseType = Notes.CourseType,
            //                 Price = Notes.Price,
            //                 Quantity = OrderNotes.Quantity,
            //                 Status = OrderNotes.Status,

            //             };

            var result = db.StudentOrderDetails().ToList();





            return Json(result, JsonRequestBehavior.AllowGet);
        }



        public ActionResult RecentOrdersDetails(int OrderId)
        {

            var result = from Notes in db.AspNetNotes
                         join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID
                         where OrderNotes.OrderId == OrderId
                         select new
                         {
                             Id = OrderNotes.Id,
                             Title = Notes.Title,
                             Discription = Notes.Description,
                             CourseType = Notes.CourseType,
                             Price = Notes.Price,
                             Quantity = OrderNotes.Quantity,
                             Status = OrderNotes.Status,
                             Total = Notes.Price * OrderNotes.Quantity,

                         };




            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // GET: AspNetNotes/Create
        public ActionResult Create()
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName");
            return View();
        }

        // POST: AspNetNotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,SubjectID,CourseType,Price,CreationDate")] AspNetNote aspNetNote)
        {

            aspNetNote.CourseType = Request.Form["CourseType"];
            aspNetNote.CreationDate = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.AspNetNotes.Add(aspNetNote);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }

        // GET: AspNetNotes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            if (aspNetNote == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }

        // POST: AspNetNotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,SubjectID,CourseType,Price,CreationDate")] AspNetNote aspNetNote)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetNote).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }

        // GET: AspNetNotes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            if (aspNetNote == null)
            {
                return HttpNotFound();
            }
            return View(aspNetNote);
        }

        // POST: AspNetNotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            db.AspNetNotes.Remove(aspNetNote);
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
