﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using OfficeOpenXml;

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

        public ActionResult TimeTableFromFile()
        {
            var dbTransaction = db.Database.BeginTransaction();

            HttpPostedFileBase file = Request.Files["subjects"];
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
            }
            var studentList = new List<RegisterViewModel>();
            using (var package = new ExcelPackage(file.InputStream))
            {

                var currentSheet = package.Workbook.Worksheets;
                var workSheet = currentSheet.First();
                var noOfCol = workSheet.Dimension.End.Column;
                var noOfRow = workSheet.Dimension.End.Row;

                string ErrorMsg = null;
                int rowIterator;
                for (rowIterator = 2; rowIterator < noOfRow; rowIterator++)
                {
                    var TimeTable = new AspNetTimeTable();

                    var RoomName = workSheet.Cells[rowIterator, 1].Value.ToString();
                    //RoomID;
                    int RoomID = db.AspNetRooms.Where(x => x.Name == RoomName).FirstOrDefault().Id;
                    AspNetRoom room = new AspNetRoom();

                    room = db.AspNetRooms.Where(x => x.Name == RoomName).FirstOrDefault();

                    if (room == null)
                    {
                        ErrorMsg = "Error in Row " + Convert.ToString(rowIterator - 1) + "Room Name is not valid";

                        TempData["ErrorMsg"] = ErrorMsg;

                        return RedirectToAction("Create", "AspNetTimeTable");
                    }
                    else
                    {
                        RoomID = room.Id;

                    }

                    var SlotName = workSheet.Cells[rowIterator, 2].Value.ToString();
                    //var SlotID = db.AspNetTimeslots.Where(x => x.Name == SlotName).FirstOrDefault().Id;
                    
                    int SlotID = db.AspNetTimeslots.Where(x => x.Name == SlotName).FirstOrDefault().Id;
                    AspNetTimeslot slot = new AspNetTimeslot();

                    slot = db.AspNetTimeslots.Where(x => x.Name == SlotName).FirstOrDefault();

                    if (slot == null)
                    {
                        ErrorMsg = "Error in Row " + Convert.ToString(rowIterator) + " Slot Name is not valid";

                        TempData["ErrorMsg"] = ErrorMsg;

                        return RedirectToAction("Create", "AspNetTimeTable");
                    }
                    else
                    {
                        SlotID = slot.Id;

                    }

                    string SubName = workSheet.Cells[rowIterator, 3].Value.ToString();
                    // var Session_id = db.AspNetSessions.Where(x => x.Id == SessionID && x.Status == "Active").FirstOrDefault().Id;


                    int SessionId = SessionID;
                    AspNetSession session = new AspNetSession();

                    session = db.AspNetSessions.Where(x => x.Id == SessionID).FirstOrDefault();

                    if (room == null)
                    {
                        ErrorMsg = "Error in Row " + Convert.ToString(rowIterator) + " Subject Name is not valid";

                        TempData["ErrorMsg"] = ErrorMsg;

                        return RedirectToAction("Create", "AspNetTimeTable");

                    }
                    else
                    {
                        SessionId = session.Id;
                    }
                    
                    var ClassID = db.AspNetClasses.Where(x => x.SessionID == SessionId).FirstOrDefault().Id;
                    
                    int SubjectID;
                    AspNetSubject subject = new AspNetSubject();

                    subject = db.AspNetSubjects.Where(x => x.ClassID == ClassID && x.SubjectName == SubName).FirstOrDefault();

                    if (subject == null)
                    {
                        ErrorMsg = "Error in Row " + Convert.ToString(rowIterator) + " Subject Name is not valid";

                        TempData["ErrorMsg"] = ErrorMsg;

                        return RedirectToAction("Create", "AspNetTimeTable");
                    }
                    else
                    {
                        SubjectID = subject.Id;
                    }
                    
                    var UserName = workSheet.Cells[rowIterator, 4].Value.ToString();
                    
                    string TeacherID;
                    AspNetUser teacher = new AspNetUser();

                    teacher = db.AspNetUsers.Where(x => x.UserName == UserName).FirstOrDefault();

                    if (teacher == null)
                    {
                        ErrorMsg = "Error in Row " + Convert.ToString(rowIterator) + " Teacher Name is not valid";

                        TempData["ErrorMsg"] = ErrorMsg;

                        return RedirectToAction("Create", "AspNetTimeTable");

                    }
                    else
                    {
                        TeacherID = teacher.Id;
                    }
                    
                    DateTime Day = Convert.ToDateTime(workSheet.Cells[rowIterator, 5].Value.ToString());

                    TimeTable.RoomID = RoomID;
                    TimeTable.SlotID = SlotID;
                    TimeTable.SubjectID = SubjectID;
                    TimeTable.Teacher_ID = TeacherID;
                    TimeTable.Day = Day.ToString();
                    TimeTable.IsPopulated = false;

                    db.AspNetTimeTables.Add(TimeTable);
                    db.SaveChanges();
                }
            }
            dbTransaction.Commit();

         //   return RedirectToAction("Index", "AspNetTimeTable");

            //catch (Exception e)
            //{
            //    dbTransaction.Dispose();
            //}

            return RedirectToAction("CreateTimetable");


        }

        public ActionResult CreateTimetable()
        {
            var timelist = db.AspNetTimeTables.Where(x=> x.IsPopulated==false).ToList();

            foreach (var item in timelist)
            {
                var subjectID = item.SubjectID;
                var students = db.AspNetStudent_Subject.Where(x => x.SubjectID == subjectID).Select(x => x.StudentID).ToList();

                string[] color = { "Red", "Blue", "Green", "Pink", "Orange" };

                foreach (var items in students)
                {
                    Random random = new Random();
                    int colorcode = random.Next(1, 5);
                    var newEvent = new Event();
                    newEvent.UserId = items;
                    newEvent.IsFullDay = false;
                    newEvent.IsPublic = false;
                    newEvent.Subject = item.AspNetRoom.Name + "_" + item.AspNetSubject.SubjectName;
                    newEvent.SessionID = db.AspNetSessions.Where(x => x.Status == "Active").FirstOrDefault().Id;
                    newEvent.ThemeColor = color[colorcode];
                    var starttime = ((DateTime)item.AspNetTimeslot.Start_Time).ToString("hh:mm");
                    var Endtime = ((DateTime)item.AspNetTimeslot.End_Time).ToString("hh:mm");
                    var day = item.Day.Split(' ');
                    newEvent.Start = Convert.ToDateTime(day[0] + " " + starttime);
                    newEvent.End = Convert.ToDateTime(day[0] + " " + Endtime);
                    db.Events.Add(newEvent);
                }
            }

            foreach (var item in timelist)
            {
                AspNetTimeTable aspNetTimeTable = db.AspNetTimeTables.Find(item.Id);
                aspNetTimeTable.IsPopulated = true;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
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
