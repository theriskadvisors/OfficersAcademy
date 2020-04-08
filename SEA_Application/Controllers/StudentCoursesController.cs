using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    public class StudentCoursesController : Controller
    {

        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public ActionResult Index()
        {



            return View();
        }

        public ActionResult AllSubjectsOfStudent()
        {

            var userID = User.Identity.GetUserId();

            var AllSubjectsOfStudent = from Subject in db.AspNetSubjects
                                       join StudentSubject in db.AspNetStudent_Subject on Subject.Id equals StudentSubject.SubjectID
                                       where StudentSubject.StudentID == userID
                                       select new
                                       {
                                           Subject.Id,
                                           Subject.SubjectName,
                                           Subject.CourseType,
                                           Subject.Points,

                                       };

            return Json(AllSubjectsOfStudent, JsonRequestBehavior.AllowGet);

        }

        public ActionResult SubjectTopics(int id)
        {


            ViewBag.SubjectId = id;


            return View();
        }

        public ActionResult StudentLessons(int id)
        {


            ViewBag.LessonID = id;


            return View();
        }
        public ActionResult GetLessonVideo(int LessonID)
        {
            AspnetLesson Lesson = db.AspnetLessons.Where(x => x.Id == LessonID).FirstOrDefault();
            string LessonVideoUrl = "";


            if (Lesson != null)
            {
                LessonVideoUrl = Lesson.Video_Url;

            }

            return Json(new { LessonId = Lesson.Id, LessonName = Lesson.Name, LessonVideo = Lesson.Video_Url, LessonDescription = Lesson.Description }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetSubjectTopicsAndLessons(int SubjectId)
        {

            //  var SubjectTopics = db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId).ToList();


            //var AllSubjectTopicsLessons = from SubjectTopic in db.AspnetSubjectTopics
            //                              join Lesson in db.AspnetLessons on SubjectTopic.Id equals Lesson.TopicId
            //                              where SubjectTopic.SubjectId == SubjectId
            //                              select new
            //                              {
            //                                  TopicId = SubjectTopic.Id,
            //                                  TopicName =   SubjectTopic.Name,
            //                                  LessonId = Lesson.Id,
            //                                  LessonName = Lesson.Name,
            //                                  Lesson.Duration,
            //                                  Lesson.Description

            //                              };

            var SubjectsTopics = db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId).ToList();



            List<Topic> TopicListObj = new List<Topic>();

            int Count = 0;
            foreach (var a in SubjectsTopics)
            {
                int count1 = 0;
                Topic TopicObj = new Topic();

                var list = db.AspnetLessons.Where(x => x.TopicId == a.Id).ToList();

                TopicObj.TopicId = a.Id;
                TopicObj.TopicName = a.Name;



                List<Lesson> LessonsList = new List<Lesson>();


                foreach (var lesson in list)
                {
                    Lesson lessonobj = new Lesson();
                    lessonobj.LessonId = lesson.Id;
                    lessonobj.LessonName = lesson.Name;
                    lessonobj.LessonDuration = lesson.Duration;

                    LessonsList.Add(lessonobj);
                    Count++;
                    count1++;
                }


                TopicObj.LessonList = LessonsList;

                TopicObj.TotalLessons = Count;
                TopicObj.TotalLessons1 = count1;

                TopicListObj.Add(TopicObj);
            }


            return Json(TopicListObj, JsonRequestBehavior.AllowGet);



        }
        public ActionResult GetCourseContent(int LessonID)
        {

            var TopicId = db.AspnetLessons.Where(x => x.Id == LessonID).FirstOrDefault().TopicId;
            var SubjectId = db.AspnetSubjectTopics.Where(x => x.Id == TopicId).FirstOrDefault().SubjectId;

            var SubjectsTopics = db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId).ToList();

            List<Topic> TopicListObj = new List<Topic>();

            int Count = 0;
            foreach (var a in SubjectsTopics)
            {
                int count1 = 0;
                Topic TopicObj = new Topic();

                var list = db.AspnetLessons.Where(x => x.TopicId == a.Id).ToList();

                TopicObj.TopicId = a.Id;
                TopicObj.TopicName = a.Name;



                List<Lesson> LessonsList = new List<Lesson>();


                foreach (var lesson in list)
                {
                    Lesson lessonobj = new Lesson();
                    lessonobj.LessonId = lesson.Id;
                    lessonobj.LessonName = lesson.Name;
                    lessonobj.LessonDuration = lesson.Duration;

                    LessonsList.Add(lessonobj);
                    Count++;
                    count1++;
                }


                TopicObj.LessonList = LessonsList;

                TopicObj.TotalLessons = Count;
                TopicObj.TotalLessons1 = count1;

                TopicListObj.Add(TopicObj);
            }


            return Json(TopicListObj, JsonRequestBehavior.AllowGet);



        }


        public ActionResult StudentAssignment(int LessonID)
        {

            //AspnetStudentAssignment SA  =    db.AspnetStudentAssignments.Where(x => x.LessonId == LessonID).FirstOrDefault();
            //    var AssignmentName = "";
            //    var AssignmentDueDate = "";
            //    int AssignmentId =;
            //    if(SA !=null)
            //    {

            //        AssignmentName = SA.FileName;
            //        AssignmentDueDate =  SA.DueDate.ToString();
            //        AssignmentId = SA.Id;
            //    }

            //  new { StudentAssigmentName = AssignmentName, StudentAssignmentDueDate = AssignmentDueDate, StudentAssignmentId = AssignmentId }


            var a = db.AspnetStudentAssignments.Where(x => x.LessonId == LessonID).Select(x => new { x.Id, x.FileName, x.DueDate }).FirstOrDefault();

            var AssignmentSubmission = db.AspnetStudentAssignmentSubmissions.Where(x => x.LessonId == LessonID).FirstOrDefault();

            var TeacherComments = "";
            if (AssignmentSubmission != null)
            {
                TeacherComments = AssignmentSubmission.TeacherComments;
            }



            return Json(new { StudentAssigmentName = a.FileName, StudentAssignmentDueDate = a.DueDate, StudentAssignmentId = a.Id, TeacherComments = TeacherComments }, JsonRequestBehavior.AllowGet);
        }




        public ActionResult ReadingMaterials(int LessonID)
        {

            var StudentAttachments = db.AspnetStudentAttachments.Where(x => x.LessonId == LessonID).Select(x => new { AttachmentId = x.Id, AttachmentName = x.Path, AttachmentPath = x.Path }).ToList();

            var StudentLinks = db.AspnetStudentLinks.Where(x => x.LessonId == LessonID).Select(x => new { LinkUrl = x.URL }).ToList();


            return Json(new { StuAttachment = StudentAttachments, StuLinks = StudentLinks }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadFile(int id)
        {

            AspnetStudentAttachment studentAttachment = db.AspnetStudentAttachments.Find(id);

            var filepath = System.IO.Path.Combine(Server.MapPath("~/Content/StudentAttachments/"), studentAttachment.Path);
            return File(filepath, MimeMapping.GetMimeMapping(filepath), studentAttachment.Path);


        }

        public ActionResult DownloadFileOfAssignment(int id)
        {

            AspnetStudentAssignment studentAssignment = db.AspnetStudentAssignments.Find(id);

            var filepath = System.IO.Path.Combine(Server.MapPath("~/Content/StudentAssignments/"), studentAssignment.FileName);

            return File(filepath, MimeMapping.GetMimeMapping(filepath), studentAssignment.FileName);


        }

        public ActionResult StudentAssignmentSubmission(int LessonID)
        {

            var File = Request.Files["file"];

            var fileName = "";
            if (File.ContentLength > 0)
            {
                fileName = Path.GetFileName(File.FileName);
                File.SaveAs(Server.MapPath("~/Content/StudentAssignments/") + fileName);

            }

            AspnetStudentAssignmentSubmission AssignmentSubmission = new AspnetStudentAssignmentSubmission();

            int? TopicId = db.AspnetLessons.Where(x => x.Id == LessonID).FirstOrDefault().TopicId;
            int? SubjectId = db.AspnetSubjectTopics.Where(x => x.Id == TopicId).FirstOrDefault().SubjectId;

            //  db.AspnetSubjectTopics.Where(x => x.Id == LessonID);

            var Subject = db.AspNetSubjects.Where(x => x.Id == SubjectId).FirstOrDefault();

            var id = User.Identity.GetUserId();
            var UserId = db.AspNetUsers.Where(x => x.Id == id).FirstOrDefault().Id;

            int StudentId = db.AspNetStudents.Where(x => x.StudentID == UserId).FirstOrDefault().Id;

            var AssignmentDueDate = db.AspnetStudentAssignments.Where(x => x.LessonId == LessonID).FirstOrDefault().DueDate;


            TimeZone time2 = TimeZone.CurrentTimeZone;
            DateTime test = time2.ToUniversalTime(DateTime.Now);
            var pakistan = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime pakistantime = TimeZoneInfo.ConvertTimeFromUtc(test, pakistan);
            AssignmentSubmission.LessonId = LessonID;
            AssignmentSubmission.TopicId = TopicId;
            AssignmentSubmission.SubjectId = SubjectId;
            AssignmentSubmission.ClassId = Subject.ClassID;
            AssignmentSubmission.CourseType = Subject.CourseType;
            AssignmentSubmission.StudentId = StudentId;
            AssignmentSubmission.AssignmentSubmittedDate = pakistantime;
            AssignmentSubmission.AssignmentDueDate = AssignmentDueDate;
            AssignmentSubmission.AssignmentFileName = fileName;

            db.AspnetStudentAssignmentSubmissions.Add(AssignmentSubmission);
            db.SaveChanges();


            return Json("Submitted Successfully", JsonRequestBehavior.AllowGet);
        }


        public class Lesson
        {
            public int LessonId { get; set; }
            public string LessonName { get; set; }
            public TimeSpan? LessonDuration { get; set; }

            public int LessonCount { get; set; }
        }

        public class Topic
        {
            public int TopicId { get; set; }
            public string TopicName { get; set; }
            public int TopicDuration { get; set; }

            public int TotalLessons { get; set; }
            public int TotalLessons1 { get; set; }
            public List<Lesson> LessonList { get; set; }

        }




    }
}