using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using System.IO;
using System.Web.UI;
using System.Text;
using NReco.PdfGenerator;
using Microsoft.AspNet.Identity;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.parser;

namespace SEA_Application.Controllers
{
    
    [Authorize(Roles = "Student,Principal")]
    public class Student_DashboardController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        private string StudentID;

        public Student_DashboardController()
        {

            StudentID = Convert.ToString(System.Web.HttpContext.Current.Session["StudentID"]);
        }

        // GET: Student_Dashboard
        public ActionResult Dashboard()
        {
            return View("BlankPage");
        }


        public ViewResult View_Project()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View();
        }

        public ViewResult PerformanceReports()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View();
        }

        public ActionResult Student_Dairy()
        {
            return RedirectToAction("Index","AspNetHomeworks");
        }

        public ActionResult PerformanceReport_Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspNetStudentPerformanceReport aspNetStudentPerformanceReport = db.AspNetStudentPerformanceReports.Where(x => x.Id == id && x.StudentID == StudentID).FirstOrDefault();
            if (aspNetStudentPerformanceReport == null)
            {
                return HttpNotFound();
            }
            return View(aspNetStudentPerformanceReport);
        }

        public ActionResult Project_Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspNetStudent_Project aspNetStudentProject = db.AspNetStudent_Project.Where(x => x.ProjectID == id && x.StudentID == StudentID).FirstOrDefault();
            if (aspNetStudentProject == null)
            {
                return HttpNotFound();
            }
            return View(aspNetStudentProject);
        }
        public ViewResult View_Assessment()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View();
        }


        public ActionResult Assessment_Detail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspNetStudent_Assessment aspNetStudentAssessment = db.AspNetStudent_Assessment.Where(x => x.AssessmentID == id && x.StudentID == StudentID).FirstOrDefault();
            if (aspNetStudentAssessment == null)
            {
                return HttpNotFound();
            }
            return View(aspNetStudentAssessment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Student_Assessment_Submission(AspNetStudent_Assessment stu_assessment)
        {
            int id = Convert.ToInt32(Request.Form["id"]);
            AspNetStudent_Assessment result = (from p in db.AspNetStudent_Assessment
                                               where p.Id == id
                                         select p).SingleOrDefault();

            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["document"];
                if (ModelState.IsValid)
                {
                    if (file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/App_Data/AssessmentSubmission"), fileName);
                        file.SaveAs(path);
                        result.SubmittedFileName = fileName;
                    }
                }


                result.SubmissionStatus = true;
                result.SubmissionDate = DateTime.Now;

                db.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception) { dbTransaction.Dispose(); }
            return RedirectToAction("Assessment_Detail", new { id = result.AssessmentID });

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Student_Project_Submission(AspNetStudent_Project stu_project)
        {
            int id = Convert.ToInt32(Request.Form["id"]);
            AspNetStudent_Project student_project = (from p in db.AspNetStudent_Project
                                                     where p.Id == id
                                               select p).SingleOrDefault();

            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["document"];
                if (ModelState.IsValid)
                {
                    if (file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/App_Data/ProjectsSubmission"), fileName);
                        file.SaveAs(path);
                        student_project.SubmittedFileName = fileName;
                    }
                }


                student_project.SubmissionStatus = true;
                student_project.SubmissionDate = DateTime.Now;

                db.SaveChanges();
                dbTransaction.Commit();
            }
            catch (Exception) { dbTransaction.Dispose(); }
            return RedirectToAction("Project_Detail", new { id = student_project.ProjectID });

        }


        public ViewResult Student_Marks()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View("_Student_Marks");
        }

        public ViewResult Student_Attendance()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View("_Student_Attendance");
        }

       
        // GET: Assignment/Details/5
        public ActionResult AnnouncementDetail(int? id)
        {

            var announcementID = db.AspNetAnnouncement_Subject.Where(s => s.Id == id).Select(s => s.AnnouncementID).SingleOrDefault();
            AspNetStudent_Announcement result = db.AspNetStudent_Announcement.Where(x => x.StudentID == StudentID && x.AnnouncementID == announcementID).Select(x => x).FirstOrDefault();

            result.Seen = true;
            db.SaveChanges();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           

            var Announcement = db.AspNetAnnouncement_Subject.Find(id);

            if (Announcement == null)
            {
                return HttpNotFound();
            }
            return View(Announcement);
        }

        public ViewResult Student_Announcement()
        {
            List<int> subjectIDs = (from student_subject in db.AspNetStudent_Subject
                                    where student_subject.StudentID == StudentID
                                    select student_subject.SubjectID).ToList();

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => subjectIDs.Contains(x.Id)), "Id", "SubjectName");
            return View("_Student_Announcement");
        }



        [HttpGet]
        public JsonResult StudentAnnouncement()
        {
            db.Configuration.ProxyCreationEnabled = false;
            var ann2 = (from t1 in db.AspNetAnnouncement_Subject
                        join t3 in db.AspNetStudent_Subject on t1.SubjectID equals t3.SubjectID
                        join t4 in db.AspNetUsers on t3.StudentID equals t4.Id
                        join t2 in db.AspNetStudent_Announcement on t1.AnnouncementID equals t2.AnnouncementID

                        where t2.StudentID == StudentID && t2.Seen == false && t4.Id == StudentID
                        select new { t1.AspNetAnnouncement.Title, t1.Id }).ToList();

            ViewBag.Subjects = ann2;
            return Json(ann2, JsonRequestBehavior.AllowGet);

        }

        public FileResult downloadProjectFile(int id)
        {
            AspNetProject Project = db.AspNetProjects.Find(id);

            var filepath = System.IO.Path.Combine(Server.MapPath("~/App_Data/Projects/"), Project.FileName);
            return File(filepath, MimeMapping.GetMimeMapping(filepath), Project.FileName);

        }

        public FileResult downloadStudentSubmittedFile(int id)
        {
            AspNetStudent_Project Student_Project = db.AspNetStudent_Project.Find(id);

            var filepath = System.IO.Path.Combine(Server.MapPath("~/App_Data/ProjectsSubmission/"), Student_Project.SubmittedFileName);
            return File(filepath, MimeMapping.GetMimeMapping(filepath), Student_Project.SubmittedFileName);

        }

        [HttpGet]
        public JsonResult MarksBySubject(int subjectID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            var marks = (from student_assessment in db.AspNetStudent_Assessment
                         where student_assessment.AspNetAssessment.AspNetSubject_Catalog.AspNetSubject.Id == subjectID && student_assessment.StudentID == StudentID
                         group new { student_assessment.AspNetAssessment.Title, student_assessment.AspNetAssessment.DueDate, student_assessment.AspNetAssessment.TotalMarks, student_assessment.MarksGot } by student_assessment.AspNetAssessment.AspNetSubject_Catalog.AspNetCatalog.CatalogName into g
                         select new { Key = g.Key, Marks = g.ToList() }).ToList();
            
            return Json(marks, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AttendanceBySubject(int subjectID)
        {
            var attendances = (from attendance in db.AspNetStudent_Attendance
                               where attendance.StudentID == StudentID && attendance.AspNetAttendance.SubjectID == subjectID
                               select new { attendance.Reason, attendance.Status, attendance.AspNetAttendance.Date }).ToList();
            return Json(attendances, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult AnnouncementBySubject(int subjectID)
        {
            var announcements = (from announcement in db.AspNetAnnouncement_Subject
                                 where announcement.SubjectID == subjectID
                                 select new { announcement.AspNetAnnouncement.Title, announcement.AspNetSubject.SubjectName, announcement.Id }).ToList();
            return Json(announcements, JsonRequestBehavior.AllowGet);
        }

       
        public ActionResult StudentAssessment()
        {
            try
            { 
            var session =  db.AspNetSessions.OrderByDescending(x=> x.Id).First() ;
            ViewBag.TermId = new SelectList(db.AspNetTerms.Where(x => x.SessionID == session.Id), "Id", "TermName");      // ).ToList();
            }
            catch
            {
                ViewBag.Error = "No session or term has started yet";
            }

            return View();
        }

        public JsonResult Assessment(int termId)
        {
            string studentId = User.Identity.GetUserId();

            bool Submited_flag;

            try
            {
                var check = db.AspNetStudent_Term_Assessment.Where(x => x.TermID == termId && x.StudentID == StudentID && x.Status == "Principle_submit").Select(x => x.Id).ToList();
                if (check.Count == 0)
                    Submited_flag = false;
                else
                    Submited_flag = true;
            }
            catch (Exception ex)
            {
                Submited_flag = false;
            }

            Student_data Assessment_DATA = new Student_data();
            Assessment_DATA.assessment_data = new List<subject>();

            if (Submited_flag)
            {
                var STAID = db.AspNetStudent_Term_Assessment.Where(x => x.TermID == termId && x.StudentID == StudentID).Select(x => x.Id).ToList();
                Assessment_DATA.PrincipleComment = db.AspNetStudent_Term_Assessment.Where(x => x.TermID == termId && x.StudentID == StudentID).Select(x => x.PrincipalComments).FirstOrDefault();


                var catageory = db.AspNetStudent_Term_Assessments_Answers.Where(x => x.AspNetStudent_Term_Assessment.TermID == termId && x.AspNetStudent_Term_Assessment.StudentID == StudentID).Select(x => x.Catageory).Distinct().ToList();

                foreach (var item in STAID)
                {
                    subject Subject = new subject();
                    Subject.SubjectName = db.AspNetStudent_Term_Assessments_Answers.Where(x => x.STAID == item).Select(x => x.AspNetStudent_Term_Assessment.AspNetSubject.SubjectName).First();
                    Subject.TeacherComment = db.AspNetStudent_Term_Assessment.Where(x => x.Id == item).Select(x => x.TeacherComments).FirstOrDefault();
                    Subject.CatageoryList = new List<Catageory>();

                    foreach (var Catag in catageory)
                    {
                        Catageory Catageory = new Catageory();
                        Catageory.catageoryName = Catag;
                        Catageory.QuestionList = new List<Questions>();

                        var AQID = db.AspNetStudent_Term_Assessments_Answers.Where(x => x.STAID == item && x.Catageory == Catag).ToList();

                        if (AQID.Count != 0)
                        {
                            foreach (var aqid in AQID)
                            {
                                Questions question = new Questions();
                                question.Question = aqid.Question;
                                question.Answer = aqid.Answer;
                                Catageory.QuestionList.Add(question);
                            }
                            Subject.CatageoryList.Add(Catageory);
                        }
                    }

                    Assessment_DATA.assessment_data.Add(Subject);
                }

            }

            return Json(Assessment_DATA, JsonRequestBehavior.AllowGet);


        }

        public class Student_data
        {
            public List<subject> assessment_data{ get; set; }
            public string PrincipleComment { set; get; }
        }
        public class subject
        {
            public string SubjectName { set; get; }
            public string TeacherComment { set; get; }
            public List<Catageory> CatageoryList { set; get; }
        }

        public class Catageory
        {
            public string catageoryName { get; set; }
            public List<Questions> QuestionList { get; set; }
        }

        public class Questions
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }
    }
}