using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspnetLessonsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetLessons
        public ActionResult Index()
        {
            var aspnetLessons = db.AspnetLessons.Include(a => a.AspnetSubjectTopic);
            return View(aspnetLessons.ToList());
        }

        // GET: AspnetLessons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }
            return View(aspnetLesson);
        }

        // GET: AspnetLessons/Create

        public ActionResult LoadSectionIdDDL()
        {
            var ClassList = db.AspNetClasses.ToList().Select(x => new { x.Id, x.ClassName });

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(ClassList);

            // return Json(SubjectsByClass, JsonRequestBehavior.AllowGet);
            return Content(status);

        }

        public ActionResult Create()
        {
            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics, "Id", "Name");

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return View();
        }

        // POST: AspnetLessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LessonViewModel LessonViewModel)
        {

            AspnetLesson Lesson = new AspnetLesson();

            Lesson.Name = LessonViewModel.LessonName;
            Lesson.Video_Url = LessonViewModel.LessonVideoURL;
            Lesson.TopicId = LessonViewModel.TopicId;
            Lesson.Duration = LessonViewModel.LessonDuration;
            Lesson.CreationDate = LessonViewModel.CreationDate;
            Lesson.Description = LessonViewModel.LessonDescription;
            Lesson.CreationDate = DateTime.Now;
            db.AspnetLessons.Add(Lesson);
            db.SaveChanges();


            HttpPostedFileBase Assignment = Request.Files["Assignment"];
            HttpPostedFileBase Attachment1 = Request.Files["Attachment1"];
            HttpPostedFileBase Attachment2 = Request.Files["Attachment2"];
            HttpPostedFileBase Attachment3 = Request.Files["Attachment3"];

            if (Assignment.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Assignment.FileName);
                Assignment.SaveAs(Server.MapPath("~/Content/StudentAssignments/") + fileName);
                AspnetStudentAssignment studentAssignment = new AspnetStudentAssignment();

                studentAssignment.FileName = fileName;

                studentAssignment.Name = LessonViewModel.AssignmentName;


                string DueDate = Convert.ToString(LessonViewModel.AssignmentDueDate);


                if (DueDate == "1/1/0001 12:00:00 AM")
                {
                    studentAssignment.DueDate = null;

                }
                else
                {

                    studentAssignment.DueDate = LessonViewModel.AssignmentDueDate;

                }


                studentAssignment.Description = LessonViewModel.AssignmentDescription;
                studentAssignment.CreationDate = DateTime.Now;
                studentAssignment.LessonId = Lesson.Id;

                db.AspnetStudentAssignments.Add(studentAssignment);
                db.SaveChanges();


            }

            if (Attachment1.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Attachment1.FileName);
                Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment1 = new AspnetStudentAttachment();

                studentAttachment1.Name = LessonViewModel.AttachmentName1;
                studentAttachment1.Path = fileName;
                studentAttachment1.CreationDate = DateTime.Now;
                studentAttachment1.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment1);
                db.SaveChanges();


            }
            if (Attachment2.ContentLength > 0)
            {

                var fileName = Path.GetFileName(Attachment2.FileName);
                Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment2 = new AspnetStudentAttachment();

                studentAttachment2.Name = LessonViewModel.AttachmentName2;
                studentAttachment2.Path = fileName;
                studentAttachment2.CreationDate = DateTime.Now;
                studentAttachment2.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment2);

                db.SaveChanges();

            }

            if (Attachment3.ContentLength > 0)
            {

                var fileName = Path.GetFileName(Attachment3.FileName);
                Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                studentAttachment3.Name = LessonViewModel.AttachmentName3;
                studentAttachment3.Path = fileName;
                studentAttachment3.CreationDate = DateTime.Now;
                studentAttachment3.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment3);
                db.SaveChanges();

            }

            if (LessonViewModel.LinkUrl1 != null)
            {
                AspnetStudentLink link1 = new AspnetStudentLink();

                link1.URL = LessonViewModel.LinkUrl1;
                link1.CreationDate = DateTime.Now;
                link1.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link1);
                db.SaveChanges();
            }

            if (LessonViewModel.LinkUrl2 != null)
            {
                AspnetStudentLink link2 = new AspnetStudentLink();

                link2.URL = LessonViewModel.LinkUrl2;
                link2.CreationDate = DateTime.Now;
                link2.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link2);
                db.SaveChanges();
            }


            if (LessonViewModel.LinkUrl3 != null)
            {
                AspnetStudentLink link3 = new AspnetStudentLink();

                link3.URL = LessonViewModel.LinkUrl3;
                link3.CreationDate = DateTime.Now;
                link3.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link3);
                db.SaveChanges();
            }



            return RedirectToAction("Index");

        }



        // GET: AspnetLessons/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }

            AspnetStudentAssignment studentAssignment = db.AspnetStudentAssignments.Where(x => x.LessonId == aspnetLesson.Id).FirstOrDefault();
            List<AspnetStudentAttachment> studentAttachments = db.AspnetStudentAttachments.Where(x => x.LessonId == aspnetLesson.Id).ToList();
            List<AspnetStudentLink> studentLinks = db.AspnetStudentLinks.Where(x => x.LessonId == aspnetLesson.Id).ToList();
            LessonViewModel lessonViewModel = new LessonViewModel();
            lessonViewModel.LessonDescription = aspnetLesson.Description;
            lessonViewModel.LessonVideoURL = aspnetLesson.Video_Url;
            lessonViewModel.LessonName = aspnetLesson.Name;
            lessonViewModel.LessonDuration = aspnetLesson.Duration;
            int? TopicId = aspnetLesson.TopicId;
            int? SubjectId = db.AspnetSubjectTopics.Where(x => x.Id == TopicId).FirstOrDefault().SubjectId;
            AspNetSubject Subject = db.AspNetSubjects.Where(x => x.Id == SubjectId).FirstOrDefault();
            int? ClassId = Subject.ClassID;
            var CourseType = Subject.CourseType;

            lessonViewModel.Id = aspnetLesson.Id;
            lessonViewModel.AssignmentName = studentAssignment.Name;
            lessonViewModel.AssignmentDescription = studentAssignment.Description;
            DateTime Date = Convert.ToDateTime(studentAssignment.DueDate);
            string date = Date.ToString("yyyy-MM-dd");
            //  lessonViewModel.AssignmentDueDate = studentAssignment.DueDate;
            ViewBag.Date = date;


            int count = 1;

            foreach (var link in studentLinks)
            {

                if (count == 1)
                {

                    lessonViewModel.LinkUrl1 = link.URL;

                }
                else if (count == 2)
                {

                    lessonViewModel.LinkUrl2 = link.URL;

                }
                else if (count == 3)
                {
                    lessonViewModel.LinkUrl3 = link.URL;


                }
                else
                {

                }

                count++;

            }

            count = 1;
            foreach (var attachment in studentAttachments)
            {

                if (count == 1)
                {

                    lessonViewModel.AttachmentName1 = attachment.Name;

                }
                else if (count == 2)
                {

                    lessonViewModel.AttachmentName2 = attachment.Name;

                }
                else if (count == 3)
                {
                    lessonViewModel.AttachmentName3 = attachment.Name;


                }
                else
                {

                }

                count++;

            }


            ViewBag.SecId = new SelectList(db.AspNetClasses, "Id", "ClassName", ClassId);
            ViewBag.SubId = new SelectList(db.AspNetSubjects.Where(x => x.ClassID == ClassId && x.CourseType == Subject.CourseType), "Id", "SubjectName", SubjectId);
            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId), "Id", "Name", aspnetLesson.TopicId);
            ViewBag.CTId = Subject.CourseType;



            return View(lessonViewModel);
        }

        // POST: AspnetLessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LessonViewModel LessonViewModel)
        {


            AspnetLesson Lesson = db.AspnetLessons.Where(x => x.Id == LessonViewModel.Id).FirstOrDefault();
            Lesson.Name = LessonViewModel.LessonName;
            Lesson.Video_Url = LessonViewModel.LessonVideoURL;
            Lesson.TopicId = LessonViewModel.TopicId;
            Lesson.Duration = LessonViewModel.LessonDuration;
            Lesson.Description = LessonViewModel.LessonDescription;
            db.SaveChanges();

          


            HttpPostedFileBase Assignment = Request.Files["Assignment"];
            HttpPostedFileBase Attachment1 = Request.Files["Attachment1"];
            HttpPostedFileBase Attachment2 = Request.Files["Attachment2"];
            HttpPostedFileBase Attachment3 = Request.Files["Attachment3"];

            var fileName = "";
            if (Assignment.ContentLength > 0)
            {
                fileName = Path.GetFileName(Assignment.FileName);
                Assignment.SaveAs(Server.MapPath("~/Content/StudentAssignments/") + fileName);

            }
            AspnetStudentAssignment studentAssignment = db.AspnetStudentAssignments.Where(x => x.LessonId == Lesson.Id).FirstOrDefault();

            if (fileName != "")
            {

                studentAssignment.FileName = fileName;

            }

            studentAssignment.Name = LessonViewModel.AssignmentName;
            string DueDate = Convert.ToString(LessonViewModel.AssignmentDueDate);

            if (DueDate == "1/1/0001 12:00:00 AM")
            {
                studentAssignment.DueDate = null;
            }
            else
            {
                studentAssignment.DueDate = LessonViewModel.AssignmentDueDate;

            }

            studentAssignment.Description = LessonViewModel.AssignmentDescription;
            db.SaveChanges();


            List<AspnetStudentAttachment> studentAttachments = db.AspnetStudentAttachments.Where(x => x.LessonId == Lesson.Id).ToList();
            List<AspnetStudentLink> studentLinks = db.AspnetStudentLinks.Where(x => x.LessonId == Lesson.Id).ToList();

            //db.AspnetStudentAttachments.RemoveRange(studentAttachments);
            //db.SaveChanges();

            db.AspnetStudentLinks.RemoveRange(studentLinks);
            db.SaveChanges();

            
            if (Attachment1.ContentLength > 0)
            {
                var fileName1 = Path.GetFileName(Attachment1.FileName);
                Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                AspnetStudentAttachment studentAttachment1 = new AspnetStudentAttachment();

                studentAttachment1.Name = LessonViewModel.AttachmentName1;
                studentAttachment1.Path = fileName1;
                studentAttachment1.CreationDate = DateTime.Now;
                studentAttachment1.LessonId = Lesson.Id;
           
                db.SaveChanges();


            }
            if (Attachment2.ContentLength > 0)
            {

                var fileName1 = Path.GetFileName(Attachment2.FileName);
                Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                AspnetStudentAttachment studentAttachment2 = new AspnetStudentAttachment();

                studentAttachment2.Name = LessonViewModel.AttachmentName2;
                studentAttachment2.Path = fileName1;
                studentAttachment2.CreationDate = DateTime.Now;
                studentAttachment2.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment2);

                db.SaveChanges();

            }

            if (Attachment3.ContentLength > 0)
            {

                var fileName1 = Path.GetFileName(Attachment3.FileName);
                Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                studentAttachment3.Name = LessonViewModel.AttachmentName3;
                studentAttachment3.Path = fileName1;
                studentAttachment3.CreationDate = DateTime.Now;
                studentAttachment3.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment3);
                db.SaveChanges();

            }

            if (LessonViewModel.LinkUrl1 != null)
            {
                AspnetStudentLink link1 = new AspnetStudentLink();

                link1.URL = LessonViewModel.LinkUrl1;
                link1.CreationDate = DateTime.Now;
                link1.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link1);
                db.SaveChanges();
            }

            if (LessonViewModel.LinkUrl2 != null)
            {
                AspnetStudentLink link2 = new AspnetStudentLink();

                link2.URL = LessonViewModel.LinkUrl2;
                link2.CreationDate = DateTime.Now;
                link2.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link2);
                db.SaveChanges();
            }


            if (LessonViewModel.LinkUrl3 != null)
            {
                AspnetStudentLink link3 = new AspnetStudentLink();

                link3.URL = LessonViewModel.LinkUrl3;
                link3.CreationDate = DateTime.Now;
                link3.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link3);
                db.SaveChanges();
            }


            return RedirectToAction("Index");
        }

        // GET: AspnetLessons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }
            return View(aspnetLesson);
        }

        // POST: AspnetLessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            db.AspnetLessons.Remove(aspnetLesson);
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
