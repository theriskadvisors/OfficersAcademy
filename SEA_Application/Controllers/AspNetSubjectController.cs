using System;
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
    [Authorize(Roles = "Admin,Principal")]
    public class AspNetSubjectController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);

        // GET: AspNetSubject
        public ActionResult Index()
        {
            var aspNetSubjects = db.AspNetSubjects.Where(x => x.AspNetClass.SessionID == SessionID).Include(a => a.AspNetClass).Include(a => a.AspNetUser);
            return View(aspNetSubjects.ToList());
        }

        public JsonResult AllSubjects()
        {
            var subjects = db.AspNetSubjects.Where(x => x.Status != "false" && x.AspNetClass.SessionID == SessionID).Select(x => new { x.Id, x.AspNetUser.Name, x.SubjectName, x.AspNetClass.ClassName }).ToList();

            return Json(subjects, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SubjectSearch(string searchdata)
        {
            var subjects = db.AspNetSubjects.Where(x => x.SubjectName.Contains(searchdata) && x.AspNetClass.SessionID == SessionID).Select(x => new
            {
                x.AspNetUser.Name,
                x.Id,
                x.SubjectName
            }).ToList();
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        //***************************************************************************************************************************************//


        public ActionResult ClassIndex()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            return View();

        }

        public ActionResult ClassIndexs()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            ViewBag.Error = "Subject Created successfully";
            return View("ClassIndex");

        }
        public void SubjectExcelRecord(int ClassId)
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetSubject> SubjectList;
            if (ClassId == 0)
            {
                SubjectList = db.AspNetSubjects.Select(x => x).Where(y => y.AspNetClass.SessionID == SessionID).ToList();
            }
            else
            {
                SubjectList = db.AspNetSubjects.Where(x => x.ClassID == ClassId).Where(x => x.AspNetClass.SessionID == SessionID).Select(x => x).ToList();
            }

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Subject Name";
            ws.Cells["B1"].Value = "Teacher";
            ws.Cells["C1"].Value = "Class Name";


            int rowStart = 2;
            foreach (var item in SubjectList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.SubjectName;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.AspNetUser.UserName;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.AspNetClass.Class;
                rowStart++;
            }
            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformates-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment:filename=" + "ExcelReport.xlsx");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();

            //return RedirectToAction("TeacherIndex");
        }

        public ActionResult Indexs()
        {
            var aspNetClasses = db.AspNetClasses.Where(x => x.SessionID == SessionID).Include(a => a.AspNetUser);
            ViewBag.Error = "Class created successfully";
            return View("Index", aspNetClasses.ToList());
        }


        [HttpGet]
        public JsonResult SubjectByClass(int id)
        {

            if (id != 0)
            {
                var subjects = (from subject in db.AspNetSubjects
                                where subject.ClassID == id && subject.Status != "false"
                                select new { subject.Id, subject.AspNetUser.Name, subject.SubjectName, subject.AspNetClass.ClassName }).ToList();


                return Json(subjects, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var AllSubjects = (from subject in db.AspNetSubjects
                                   where subject.Status != "false" && subject.AspNetClass.SessionID == SessionID
                                   select new { subject.Id, subject.AspNetUser.Name, subject.SubjectName }).ToList();

                return Json(AllSubjects, JsonRequestBehavior.AllowGet);
            }


        }



        //***************************************************************************************************************************************//


        // GET: AspNetSubject/Details/5
        public ActionResult Details(int id)
        {

            AspNetSubject aspNetSubject = db.AspNetSubjects.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }

            var teachers = (from teacher in db.AspNetUsers.Where(x => x.Status != "False")
                            join t2 in db.AspNetUsers_Session.Where(s => s.SessionID == SessionID)
                            on teacher.Id equals t2.UserID
                            where teacher.AspNetRoles.Select(y => y.Name).Contains("Teacher")
                            select new
                            {
                                teacher.Id,
                                teacher.Name,
                            }).ToList();

            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName", aspNetSubject.ClassID);
            ViewBag.TeacherID = new SelectList(teachers, aspNetSubject.TeacherID);

            return View(aspNetSubject);
        }

        // GET: AspNetSubject/Create
        public ActionResult Create()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher") && x.AspNetUsers_Session.Any(z => z.SessionID == SessionID)), "Id", "Name");
            return View();
        }

        // POST: AspNetSubject/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SubjectName,ClassID,TeacherID")] AspNetSubject aspNetSubject)
        {
            var class_id = db.AspNetClasses.Where(x => x.SessionID == SessionID).FirstOrDefault().Id;
            aspNetSubject.ClassID = class_id;
            aspNetSubject.CourseType = Request.Form["CourseType"];
          //  aspNetSubject.IsManadatory =  Request.Form["IsMandatory"];
        //    aspNetSubject.Points = Request.Form["Points"];


            var TransactionObj = db.Database.BeginTransaction();
            try
            {


                if (ModelState.IsValid)
                {
                    db.AspNetSubjects.Add(aspNetSubject);
                    db.SaveChanges();
                }
                TransactionObj.Commit();
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                var UserNameLog = User.Identity.Name;
                AspNetUser a = db.AspNetUsers.First(x => x.UserName == UserNameLog);
                string UserIDLog = a.Id;
                string user = db.AspNetUsers.Where(x => x.Id == aspNetSubject.TeacherID).Select(x => x.Name).FirstOrDefault();
                var logMessage = "New Subject Added, SubjectName: " + aspNetSubject.SubjectName + ", ClassID: " + aspNetSubject.ClassID + ", TeacherName: " + user;


                AspNetTeacherSubject ts = new AspNetTeacherSubject();
            //    var class_id = db.AspNetClasses.Where(x => x.SessionID == SessionID).FirstOrDefault().Id;
                 var sub_id = db.AspNetSubjects.Where(x => x.SubjectName == aspNetSubject.SubjectName && x.ClassID == class_id).FirstOrDefault().Id;

                var teacherid =  db.AspNetEmployees.Where(x => x.UserId == aspNetSubject.TeacherID).FirstOrDefault().Id;

                ts.TeacherID = teacherid;
                ts.SubjectID = sub_id;
                db.AspNetTeacherSubjects.Add(ts);
                db.SaveChanges();
                
                var LogControllerObj = new AspNetLogsController();
                LogControllerObj.CreateLogSave(logMessage, UserIDLog);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                return RedirectToAction("ClassIndexs");
            }
            catch
            {
                TransactionObj.Dispose();

                ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
                ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name");

                return View("Create", aspNetSubject);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubjectfromFile(AspNetSubject aspNetSubject)
        {
            string ErrorMsg = null;
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
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

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var Subject = new AspNetSubject();
                        // string TeacherName = workSheet.Cells[rowIterator, 2].Value.ToString();
                        //var Teacher = (from users in db.AspNetUsers
                        //               where users.UserName == TeacherName
                        //               select users).First();

                        var ClassName = workSheet.Cells[rowIterator, 2].Value.ToString();
                        var Class = (from classes in db.AspNetClasses
                                     where classes.ClassName == ClassName
                                     select classes).First();

                        Subject.SubjectName = workSheet.Cells[rowIterator, 1].Value.ToString();
                        Subject.Points = Int32.Parse(workSheet.Cells[rowIterator, 3].Value.ToString());
                        string Manad = workSheet.Cells[rowIterator, 4].Value.ToString();
                        bool value;
                        if (Manad == "Yes")
                        {
                            value = true;
                        }
                        else
                        {
                            value = false;
                        }
                       Subject.CourseType = workSheet.Cells[rowIterator, 8].Value.ToString();
                        Subject.IsManadatory = value;
                        Subject.SubjectName = workSheet.Cells[rowIterator, 1].Value.ToString();
                        //Subject.TeacherID = Teacher.Id;
                        Subject.ClassID = Class.Id;
                        db.AspNetSubjects.Add(Subject);
                        if (db.SaveChanges() > 0)
                        {
                            AspNetTeacherSubject teacher = new AspNetTeacherSubject();
                            string Teacher1 = workSheet.Cells[rowIterator, 5].Value.ToString();
                            var u_id = db.AspNetUsers.Where(x => x.UserName == Teacher1).FirstOrDefault().Id;
                            int teacher1_id = db.AspNetEmployees.Where(x => x.UserId == u_id).FirstOrDefault().Id;
                            int s_id = db.AspNetSubjects.Where(x => x.SubjectName == Subject.SubjectName).FirstOrDefault().Id;

                            teacher.TeacherID = teacher1_id;
                            teacher.SubjectID = s_id;
                            db.AspNetTeacherSubjects.Add(teacher);
                            if (db.SaveChanges() > 0)
                            {
                                AspNetTeacherSubject teacher2 = new AspNetTeacherSubject();
                                if (workSheet.Cells[rowIterator, 6].Value != null)
                                {
                                    string Teacher2 = workSheet.Cells[rowIterator, 6].Value.ToString();
                                    var u_id2 = db.AspNetUsers.Where(x => x.UserName == Teacher2).FirstOrDefault().Id;
                                    int teacher2_id = db.AspNetEmployees.Where(x => x.UserId == u_id2).FirstOrDefault().Id;
                                    int s_id2 = db.AspNetSubjects.Where(x => x.SubjectName == Subject.SubjectName).FirstOrDefault().Id;
                                    teacher2.TeacherID = teacher2_id;
                                    teacher2.SubjectID = s_id2;
                                    db.AspNetTeacherSubjects.Add(teacher2);

                                    if (db.SaveChanges() > 0)
                                    {
                                        AspNetTeacherSubject teacher3 = new AspNetTeacherSubject();
                                        if (workSheet.Cells[rowIterator, 7].Value != null)
                                        {
                                            string Teacher3 = workSheet.Cells[rowIterator, 7].Value.ToString();
                                            var u_id3 = db.AspNetUsers.Where(x => x.UserName == Teacher3).FirstOrDefault().Id;
                                            int teacher3_id = db.AspNetEmployees.Where(x => x.UserId == u_id3).FirstOrDefault().Id;
                                            int s_id3 = db.AspNetSubjects.Where(x => x.SubjectName == Subject.SubjectName).FirstOrDefault().Id;
                                            teacher3.TeacherID = teacher3_id;
                                            teacher3.SubjectID = s_id3;
                                            db.AspNetTeacherSubjects.Add(teacher3);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }

                        }
                    }
                    dbTransaction.Commit();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception e) { dbTransaction.Dispose(); }

            return View("Create", aspNetSubject);
        }


        


        // GET: AspNetSubject/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
            ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher") && x.AspNetUsers_Session.Any(z => z.SessionID == SessionID)), "Id", "Name");

           // ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", aspNetSubject.TeacherID);
            return View(aspNetSubject);
        }

        // POST: AspNetSubject/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SubjectName,ClassID,TeacherID")] AspNetSubject aspNetSubject)
        {
            try
            {
                aspNetSubject.CourseType = Request.Form["CourseType"];
                if (ModelState.IsValid)
                {
                    db.Entry(aspNetSubject).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("ClassIndex");
                }
                ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", aspNetSubject.ClassID);
                ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Email", aspNetSubject.TeacherID);
                return View(aspNetSubject);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return RedirectToAction("Edit", aspNetSubject);
            }

        }

        // GET: AspNetSubject/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            if (aspNetSubject == null)
            {
                return HttpNotFound();
            }
            return View(aspNetSubject);
        }

        // POST: AspNetSubject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetSubject aspNetSubject = db.AspNetSubjects.Find(id);
            db.AspNetSubjects.Remove(aspNetSubject);
            db.SaveChanges();
            return RedirectToAction("ClassIndex");
        }
        public ActionResult DeleteSubject(int id)
        {
            var subject = db.AspNetSubjects.Where(x => x.Id == id).Select(x => x).FirstOrDefault();

            try
            {
                subject.Status = "false";
                db.SaveChanges();
                return RedirectToAction("ClassIndex");

            }
            catch
            {
                ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName", subject.ClassID);
                ViewBag.TeacherID = new SelectList(db.AspNetUsers, "Id", "Name", subject.TeacherID);

                ViewBag.Error = "This subject can't be deleted. It contains students";
                return RedirectToAction("Details", subject);
            }
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
