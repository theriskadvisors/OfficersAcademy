﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OfficeOpenXml;
using SEA_Application.CustomModel;
using SEA_Application.libs;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Accountant,Admin,Principal")]
    public class Admin_DashboardController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
          int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);
        // GET: Admin_Dashboard
        public ActionResult Index()
        {
            return View();
            
        }
        public JsonResult GetEvents()
        {
            using (SEA_DatabaseEntities dc = new SEA_DatabaseEntities())
            {
                var id = User.Identity.GetUserId();
                var events = dc.Events.Where(x => x.UserId == id || x.IsPublic == true).Select(x => new { x.Description, x.End, x.EventID, x.IsFullDay, x.Subject, x.ThemeColor, x.Start, x.IsPublic }).ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        [HttpPost]
        public JsonResult SaveEvent(Event e)
        {
            e.UserId = User.Identity.GetUserId();
            var status = false;
            using (SEA_DatabaseEntities dc = new SEA_DatabaseEntities())
            {
                if (e.EventID > 0)
                {
                    //Update the event
                    var v = dc.Events.Where(a => a.EventID == e.EventID).FirstOrDefault();
                    if (v != null)
                    {
                        v.Subject = e.Subject;
                        v.Start = e.Start;
                        v.End = e.End;
                        v.Description = e.Description;
                        v.IsFullDay = e.IsFullDay;
                        v.ThemeColor = e.ThemeColor;
                    }
                }
                else
                {
                    dc.Events.Add(e);
                }

                dc.SaveChanges();
                status = true;

            }
            return new JsonResult { Data = new { status = status } };
        }

        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;
            using (SEA_DatabaseEntities dc = new SEA_DatabaseEntities())
            {
                var v = dc.Events.Where(a => a.EventID == eventID).FirstOrDefault();
                if (v != null)
                {
                    dc.Events.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status = status } };
        }

        public ActionResult test1()
        {
            return View();
        }

        public Admin_DashboardController()
        {
            
        }
        public ActionResult Auto_Attendance()
        {
            return View();
        }
        public ActionResult checkdata(string valval)
        {

            return Json(valval,JsonRequestBehavior.AllowGet);
        }
        public ActionResult Student_AssessmentReport(string StudentId, string TermId)
        {
            ViewBag.StudentId = StudentId;
            ViewBag.TermId= TermId;
            return View();
        }
        public ActionResult CalendarNotification()
        {
            var id = User.Identity.GetUserId();
            var checkdate = DateTime.Now;
            var date = TimeZoneInfo.ConvertTime(DateTime.UtcNow.ToUniversalTime(), TimeZoneInfo.Local);
            var name = "";

            name = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            var day = date.DayOfWeek;
            var dd = date.Day;
            var mm = date.Month;
            var yy = date.Year;
            string[] array = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

            var Date = day + ", " + dd + " " + array[mm - 1] + " " + yy;
            var result = new { checkdate, Date, name };
            return Json(result, JsonRequestBehavior.AllowGet);

        }
       
        public class event1
        {
            public int Id { set; get; }
            public string Name { set; get; }
            public string StartDate { set; get; }
            public string EndDate { set; get; }
            public string StartTime { set; get; }
            public string EndTime { set; get; }
            public string Color { set; get; }
            public string Url { set; get; }

        }
        public ActionResult PrintPreviewData(string StudentId, string type, string TermId)
        {
            int TId = int.Parse(TermId);
            var subjects = db.GetStudentSubjects(StudentId).ToList();

            var comments = db.AspNetStudent_Term_Assessment.Where(x => x.StudentID == StudentId && x.TermID == TId).FirstOrDefault();
            string parentComent = "";
            string teacherComent = "";
            string prinpipalComent = "";
            if (comments != null)
            {
                parentComent = comments.ParentsComments;
                teacherComent = comments.TeacherComments;
                prinpipalComent = comments.PrincipalComments;
            }
            else
            {
                parentComent = "";
                teacherComent = "";
                prinpipalComent = "";
            }
            var tn = User.Identity.GetUserId();
            var teachername = db.AspNetUsers.Where(x => x.Id == tn).Select(x => x.Name).FirstOrDefault();
            var ClassId = db.AspNetStudents.Where(p => p.StudentID == StudentId).FirstOrDefault().ClassID;

            var subID = db.AspNetSubjects.Where(x => x.ClassID == ClassId).Select(x => x.Id).ToList();



            var secnumber = db.AspNetTerms.Where(p => p.Id == TId).FirstOrDefault().TermNo;
            var category = (from sub in db.AspNetSubjects
                            join aq in db.AspNetAssessment_Question on sub.Id equals aq.SubjectID
                            join cat in db.AspNetAssessment_Questions_Category on aq.QuestionCategory equals cat.Id
                            where sub.SubjectName == "English Language Development" && sub.ClassID == ClassId
                            select new { cat.CategoryName }).Distinct().ToList();
            var classname = db.AspNetClasses.Where(x => x.Id == ClassId).Select(x => x.Class).FirstOrDefault();
            var studentname = db.AspNetUsers.Where(x => x.Id == StudentId).Select(x => x.UserName).FirstOrDefault();
            var dinterm = db.AspNetTerms.Where(p => p.Id == TId).FirstOrDefault().TermEndDate - db.AspNetTerms.Where(p => p.Id == TId).FirstOrDefault().TermStartDate;

            var result2 = new { status = "success", subID = subID, categoryname = category, TId = secnumber, ClassId = ClassId, classname = classname, teachername = teachername, studentname = studentname, parentComent = parentComent, teacherComent = teacherComent, prinpipalComent = prinpipalComent, subValue = subjects, dinterm = dinterm };

            return Json(result2, JsonRequestBehavior.AllowGet);



        }
        public ActionResult Assessment_PrintPreview(string StudentId, int SID,string TermId)
        {
            var TId = int.Parse(TermId);
            var ClassId = db.AspNetStudents.Where(p => p.StudentID == StudentId).FirstOrDefault().ClassID;

            var sessionid = db.AspNetSessions.Where(p => p.Status == "Active").FirstOrDefault().Id;
            var category = (from sub in db.AspNetSubjects
                            join aq in db.AspNetAssessment_Question on sub.Id equals aq.SubjectID
                            join cat in db.AspNetAssessment_Questions_Category on aq.QuestionCategory equals cat.Id
                            where sub.SubjectName == "English Language Development" && sub.ClassID == ClassId
                            select new { cat.CategoryName }).Distinct().ToList();
            var sessionassesques = db.GetStudentSessionSubjectAssessment(StudentId, sessionid, SID).ToList();
            var cgjhk = sessionassesques.Count();
            if (cgjhk > 0)
            {
                var result2 = new { Value = sessionassesques };

                return Json(result2, JsonRequestBehavior.AllowGet);
            }

            else
            {
                var Aquestions = db.GetSubjctAssessmentQuestions(SID).ToList();

                foreach (var ques in Aquestions)
                {
                    AspNetStudent_Term_Assessment obj = new AspNetStudent_Term_Assessment();

                    obj.StudentID = StudentId;
                    obj.SubjectID = SID;
                    obj.SessionId = sessionid;
                    obj.TermID = TId;

                    db.AspNetStudent_Term_Assessment.Add(obj);
                    db.SaveChanges();

                    AspNetStudent_Term_Assessments_Answers obje = new AspNetStudent_Term_Assessments_Answers();

                    obje.STAID = obj.Id;
                    obje.Question = ques.Question;
                    obje.Catageory = ques.CategoryName;
                    obje.FirstTermGrade = "";
                    obje.SecondTermGrade = "";
                    obje.ThirdTermGrade = "";
                    //obje = ;
                    db.AspNetStudent_Term_Assessments_Answers.Add(obje);
                    db.SaveChanges();
                }
                var secnumber = db.AspNetTerms.Where(p => p.Id == TId).FirstOrDefault().TermNo;

                var sessionassesrtques = db.GetStudentSessionSubjectAssessment(StudentId, sessionid, SID).ToList();
                var result1 = new { status = "success", Value = sessionassesrtques, categoryname = category, TId = secnumber, ClassId = ClassId };

                return Json(result1, JsonRequestBehavior.AllowGet);
            }
        }
        public ViewResult Class_Assessment()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName");
            var sessiionid = db.AspNetSessions.Where(p => p.Status == "Active").FirstOrDefault().Id;
            ViewBag.TermID = new SelectList(db.AspNetTerms.Where(x => x.SessionID == sessiionid), "Id", "TermName", "TermNo");
            return View("_ClassAssessment");
        }

        public Admin_DashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ActionResult Dashboard()
        {
            var CurrentUserId = User.Identity.GetUserId();
            var allMessages = (from a in db.AspNetMessages
                               join b in db.AspNetMessage_Receiver
                               on a.Id equals b.MessageID
                               where b.ReceiverID == CurrentUserId && b.Seen == "Not Seen"
                               join c in db.AspNetUsers
                              on a.SenderID equals c.Id
                               select new { a.Message, a.Time, c.Name }).ToList();
            List<Message> messages = new List<Message>();
            foreach (var item in allMessages)
            {
                Message m = new Message();
                m.Name = item.Name;
                m.message = item.Message;
                string monthName = item.Time.Value.ToString("MMM", CultureInfo.InvariantCulture);
                m.date = monthName + " " + item.Time.Value.Day + "," + item.Time.Value.Year;
                messages.Add(m);

            }
            ViewBag.Messages = messages;

            var ty = (from classid in db.AspNetHomeworks
                      join classname in db.AspNetClasses
                      on classid.ClassId equals classname.Id
                      where classid.PrincipalApproved_status == "Created" || classid.PrincipalApproved_status == "Edited"
                      select new { classname.ClassName, classid.Date, classid.Id }).ToList().OrderByDescending(a => a.Date);
            ViewBag.TotalStudents = (from uid in db.AspNetUsers
                                    join sid in db.AspNetStudents
                                    on uid.Id equals sid.StudentID
                                    where uid.Status != "False"
                                    select sid.StudentID).Count();

            ViewBag.TotalMessages = db.AspNetMessage_Receiver.Where(m => m.ReceiverID == CurrentUserId && m.Seen == "Not Seen").Count();
            ViewBag.TotalNotifications = db.AspNetNotification_User.Where(m => m.UserID == CurrentUserId && m.Seen == false).Count();

            List<TODOLIST> kjlk = new List<TODOLIST>();
            foreach (var t in ty)
            {
                TODOLIST tyr = new TODOLIST();
                tyr.Classname = t.ClassName;
                tyr.HomeWorkId = t.Id;

                if (t.Date.Value.Day + "/" + t.Date.Value.Month + "/" + t.Date.Value.Year != DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year)
                {
                    tyr.isToDay = false;

                }
                else
                {
                    tyr.isToDay = true;
                }
                tyr.date = t.Date.Value.Day + "/" + t.Date.Value.Month + "/" + t.Date.Value.Year;
                if (t.Date < DateTime.Now)
                {
                    int ActualTime = (DateTime.Now - t.Date.Value.Date).Days;
                    if (ActualTime == 1)
                    {
                        tyr.Actualdate = ActualTime + " day ago";
                    }
                    else if (ActualTime >= 30)
                    {
                        int months = ActualTime / 30;
                        if (months == 1)
                        {
                            tyr.Actualdate = months + " month ago";
                        }
                        else
                        {
                            tyr.Actualdate = months + " months ago";
                        }
                    }

                    else
                    {
                        int weeks = ActualTime / 7;
                        if (weeks == 0)
                        {
                            tyr.Actualdate = ActualTime + " days ago";
                        }
                        else
                        {
                            if (weeks == 1)
                                tyr.Actualdate = weeks + " week ago";
                            else
                                tyr.Actualdate = weeks + " weeks ago";
                        }
                    }
                }
                else
                {
                    tyr.Actualdate = "Today";
                }
                tyr.date = t.Date.Value.Day + "/" + t.Date.Value.Month + "/" + t.Date.Value.Year;
                kjlk.Add(tyr);
            }
            ViewBag.ClassName = kjlk;

            return View("BlankPage");
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                                                             //
        //                                                                                             // 
        //                                             ATTENDANCE                                      //
        //                                                                                             //
        /////////////////////////////////////////////////////////////////////////////////////////////////
        public ActionResult TheKssMissionVision()
        {
            return View();
        }


        public ActionResult View_Attendance()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return View();
        }

        public ActionResult GetAttendance()
        {
            var d = DateTime.Now;
            var currentdate = d.Date;
            var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate).ToList();
            var length = att_list.Count;
            var nn = (from un in db.AspNetUsers
                      join r in db.AspNetStudent_AutoAttendance
                      on un.UserName equals r.Roll_Number
                      where un.UserName == r.Roll_Number && r.Date == currentdate
                      select un.Name).ToList();


            List<AutoAttendance> attendance = new List<AutoAttendance>();
            for (int i = 0; i < length; i++)
            {
                AspNetStudent_AutoAttendance at = att_list[i];
                var ss = nn[i];
                AutoAttendance att = new AutoAttendance();
                att.Class = at.Class;
                att.Date = at.Date;
                att.RollNumber = at.Roll_Number;
                att.Timein = at.TimeIn;
                att.Timeout = at.TimeOut;
                att.Name = nn[i];
                attendance.Add(att);
            }

            return Json(attendance, JsonRequestBehavior.AllowGet);
        }
        //////////////////////////////////////////////Attendacne Class Filter/////////////////////////////////////////////
        public ActionResult Att_Class(int Classid)
        {
            if (Classid != 0)
            {
                var d = DateTime.Now;
                var currentdate = d.Date;
                var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Class == cname && x.Date==currentdate).ToList();
                var nn = (from un in db.AspNetUsers
                          join r in db.AspNetStudent_AutoAttendance
                          on un.UserName equals r.Roll_Number
                          where un.UserName == r.Roll_Number
                          select new { un.Name, un.UserName }).ToList().Distinct();
                var result = new { att_list, nn };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var d = DateTime.Now;
                var currentdate = d.Date;
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate).ToList();

                var nn = (from un in db.AspNetUsers
                          join r in db.AspNetStudent_AutoAttendance
                          on un.UserName equals r.Roll_Number
                          where un.UserName == r.Roll_Number
                          select new { un.Name, un.UserName }).ToList().Distinct();
                var result = new { att_list, nn };
                return Json(result, JsonRequestBehavior.AllowGet);
            }


        }
        ////////////////////////////////////////////////////Status Filter//////////////////////////////////////
        public ActionResult Filter_Attendance(string type, int Classid)
        {
            List<AutoAttendance> attendance = new List<AutoAttendance>();
            var d = DateTime.Now;
            var currentdate = d.Date;
            if (type == "Present")
            {
                if(Classid==0)
                {
                    var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate).ToList();
                    var length = att_list.Count;
                    var nn = (from un in db.AspNetUsers
                              join r in db.AspNetStudent_AutoAttendance
                              on un.UserName equals r.Roll_Number
                              where un.UserName == r.Roll_Number && r.Date == currentdate
                              select un.Name).ToList();

                    for (int i = 0; i < length; i++)
                    {
                        AspNetStudent_AutoAttendance at = att_list[i];
                        var ss = nn[i];
                        AutoAttendance att = new AutoAttendance();
                        att.Class = at.Class;
                        att.Date = at.Date;
                        att.RollNumber = at.Roll_Number;
                        att.Timein = at.TimeIn;
                        att.Timeout = at.TimeOut;
                        att.Name = nn[i];
                        attendance.Add(att);
                    }

                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                    var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate && x.Class==cname).ToList();
                    var length = att_list.Count;
                  

                    for (int i = 0; i < length; i++)
                    {
                        AspNetStudent_AutoAttendance at = att_list[i];

                        var nn = (from un in db.AspNetUsers
                                  join r in db.AspNetStudent_AutoAttendance
                                  on un.UserName equals r.Roll_Number
                                  where un.UserName == at.Roll_Number && r.Date == currentdate
                                  select un.Name).FirstOrDefault();
                       
                        AutoAttendance att = new AutoAttendance();
                        att.Class = at.Class;
                        att.Date = at.Date;
                        att.RollNumber = at.Roll_Number;
                        att.Timein = at.TimeIn;
                        att.Timeout = at.TimeOut;
                        att.Name = nn;
                        attendance.Add(att);
                    }

                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }
        
            }
            else if (type == "Absent")
            {
                if (Classid == 0)
                {
                    var pstd = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate).Select(x => x.Roll_Number).ToList();
                    var tstd = db.AspNetStudents.Where(x => x.AspNetUser.Status != "False").Select(x => x.AspNetUser.UserName).ToList();
                    var astd = tstd.Except(pstd).ToList();
                    foreach (var item in astd)
                    {
                        AutoAttendance at = new AutoAttendance();
                        var att = (from user in db.AspNetUsers
                                   join std in db.AspNetStudents
                                   on user.Id equals std.StudentID
                                   where user.UserName == item && std.StudentID == user.Id
                                   select new { user.Name, user.UserName, std.AspNetClass.ClassName }).FirstOrDefault();

                        at.Class = att.ClassName;
                        at.Date = currentdate;
                        at.RollNumber = att.UserName;
                        at.Timein = null;
                        at.Timeout = null;
                        at.Name = att.Name;
                        attendance.Add(at);
                    }

                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();

                    var pstd = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate && x.Class == cname).Select(x => x.Roll_Number).ToList();
                    var tstd = db.AspNetStudents.Where(x => x.AspNetUser.Status != "False" && x.ClassID == Classid).Select(x => x.AspNetUser.UserName).ToList();
                    var astd = tstd.Except(pstd).ToList();
                    foreach (var item in astd)
                    {
                        AutoAttendance at = new AutoAttendance();
                        var att = (from user in db.AspNetUsers
                                   join std in db.AspNetStudents
                                   on user.Id equals std.StudentID
                                   where user.UserName == item && std.StudentID == user.Id
                                   select new { user.Name, user.UserName, std.AspNetClass.ClassName }).FirstOrDefault();

                        at.Class = att.ClassName;
                        at.Date = currentdate;
                        at.RollNumber = att.UserName;
                        at.Timein = null;
                        at.Timeout = null;
                        at.Name = att.Name;
                        attendance.Add(at);
                    }
                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                //var timecheck = Convert.ToDateTime("10:00:00 AM");
                //var time = timecheck.TimeOfDay;
                var time = db.AspNetTime_Setting.Where(x => x.Id == 1).FirstOrDefault();
                var lateTime = time.LateTime;
                if (Classid==0)
                {
                    var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == currentdate && x.TimeIn> lateTime).ToList();
                    var length = att_list.Count;
                    var nn = (from un in db.AspNetUsers
                              join r in db.AspNetStudent_AutoAttendance
                              on un.UserName equals r.Roll_Number
                              where un.UserName == r.Roll_Number && r.Date == currentdate && r.TimeIn > lateTime
                              select un.Name).ToList();

                    for (int i = 0; i < length; i++)
                    {
                        AspNetStudent_AutoAttendance at = att_list[i];
                        var ss = nn[i];
                        AutoAttendance att = new AutoAttendance();
                        att.Class = at.Class;
                        att.Date = at.Date;
                        att.RollNumber = at.Roll_Number;
                        att.Timein = at.TimeIn;
                        att.Timeout = at.TimeOut;
                        att.Name = nn[i];
                        attendance.Add(att);
                    }
                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                    var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.TimeIn > lateTime && x.Date == currentdate && x.Class == cname).ToList();
                    var length = att_list.Count;
                    var nn = (from un in db.AspNetUsers
                              join r in db.AspNetStudent_AutoAttendance
                              on un.UserName equals r.Roll_Number
                              where un.UserName == r.Roll_Number && r.Date == currentdate && r.TimeIn > lateTime
                              select un.Name).ToList();

                    for (int i = 0; i < length; i++)
                    {
                        AspNetStudent_AutoAttendance at = att_list[i];
                        //  var ss = nn[i];
                        AutoAttendance att = new AutoAttendance();
                        att.Class = at.Class;
                        att.Date = at.Date;
                        att.RollNumber = at.Roll_Number;
                        att.Timein = at.TimeIn;
                        att.Timeout = at.TimeOut;
                        att.Name = nn[i];
                        attendance.Add(att);

                    }
                    return Json(attendance, JsonRequestBehavior.AllowGet);
                }
                
            }

        }
        public ActionResult Att_Details(string userName)
        {
           
                ViewBag.username = userName;
            return View();
        }
         public ActionResult GetAttendance_Details(string RollNumber)
        {
            var std=db.AspNetStudent_AutoAttendance.Where(x => x.Roll_Number == RollNumber).ToList();
            var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
            var result = new { std, name };
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        /////////////////////////////////////////////////////STUDENT STATUS_DETAILS////////////////////////////////////////////

        public ActionResult Std_Status_Details( string RollNumber, string status)
        {
            if(status=="Present")
            {
                var attendance=db.AspNetStudent_AutoAttendance.Where(x => x.Roll_Number == RollNumber).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if(status=="Absent")
            {
                var attendance = db.AspNetAbsent_Student.Where(x => x.Roll_Number == RollNumber).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var time = db.AspNetTime_Setting.Where(x => x.Id == 1).FirstOrDefault();
                var lateTime = time.LateTime;
                var attendance = db.AspNetStudent_AutoAttendance.Where(x => x.Roll_Number == RollNumber && x.TimeIn>lateTime).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
        }
        public ActionResult Start_End_Date(string RollNumber,string status,DateTime start ,DateTime end)
        {
            if(status=="Present")
            {
                var attendance = db.AspNetStudent_AutoAttendance.Where(x => x.Roll_Number == RollNumber && x.Date >= start && x.Date <= end).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else if (status == "Absent")
            {
                var attendance = db.AspNetAbsent_Student.Where(x => x.Roll_Number == RollNumber && x.Date >= start && x.Date <= end).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var time = db.AspNetTime_Setting.Where(x => x.Id == 1).FirstOrDefault();
                var lateTime = time.LateTime;
                var attendance = db.AspNetStudent_AutoAttendance.Where(x => x.Roll_Number == RollNumber && x.Date >= start && x.Date <= end && x.TimeIn>=lateTime ).ToList();
                var name = db.AspNetUsers.Where(x => x.UserName == RollNumber).Select(x => x.Name).FirstOrDefault();
                var result = new { attendance, name };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
           
        }
        ///////////////////////////////////////////ATTENDANCE DATE FILTER//////////////////////////////////////

        public ActionResult Att_DateFilter(string type, int Classid, DateTime start)
        {
            List<AutoAttendance> attendance = new List<AutoAttendance>();
            var time = db.AspNetTime_Setting.Where(x => x.Id == 1).FirstOrDefault();
            var lateTime = time.LateTime;
            if (Classid == 0 && type == "Present")
            {
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == start).ToList();
                var length = att_list.Count;
                var nn = (from un in db.AspNetUsers
                          join r in db.AspNetStudent_AutoAttendance
                          on un.UserName equals r.Roll_Number
                          where un.UserName == r.Roll_Number && r.Date == start
                          select un.Name).ToList();

                for (int i = 0; i < length; i++)
                {
                    AspNetStudent_AutoAttendance at = att_list[i];                    
                    AutoAttendance att = new AutoAttendance();
                    att.Class = at.Class;
                    att.Date = at.Date;
                    att.RollNumber = at.Roll_Number;
                    att.Timein = at.TimeIn;
                    att.Timeout = at.TimeOut;
                    att.Name = nn[i];
                    attendance.Add(att);
                }
                return Json(attendance, JsonRequestBehavior.AllowGet);
            }
            else if (Classid == 0 && type == "Absent")
            {
                var pstd = db.AspNetStudent_AutoAttendance.Where(x => x.Date == start).Select(x => x.Roll_Number).ToList();
                var tstd = db.AspNetStudents.Where(x => x.AspNetUser.Status != "False").Select(x => x.AspNetUser.UserName).ToList();
                var astd = tstd.Except(pstd).ToList();

                foreach (var item in astd)
                {
                    AutoAttendance at = new AutoAttendance();
                    var att = (from user in db.AspNetUsers
                               join std in db.AspNetStudents
                               on user.Id equals std.StudentID
                               where user.UserName == item && std.StudentID == user.Id
                               select new { user.Name, user.UserName, std.AspNetClass.ClassName }).FirstOrDefault();

                    at.Class = att.ClassName;
                    at.Date = start;
                    at.RollNumber = att.UserName;
                    at.Timein = null;
                    at.Timeout = null;
                    at.Name = att.Name;
                    attendance.Add(at);
                }
                return Json(attendance, JsonRequestBehavior.AllowGet);

            }
            else if (Classid == 0 && type == "Late")
            {
                //var timecheck = Convert.ToDateTime("10:00:00 AM");
                //var time = timecheck.TimeOfDay;
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == start && x.TimeIn > lateTime).ToList();
                var length = att_list.Count;
                var nn = (from un in db.AspNetUsers
                          join r in db.AspNetStudent_AutoAttendance
                          on un.UserName equals r.Roll_Number
                          where un.UserName == r.Roll_Number && r.Date == start && r.TimeIn > lateTime
                          select un.Name).ToList();

                for (int i = 0; i < length; i++)
                {
                    AspNetStudent_AutoAttendance at = att_list[i];
                    var ss = nn[i];
                    AutoAttendance att = new AutoAttendance();
                    att.Class = at.Class;
                    att.Date = at.Date;
                    att.RollNumber = at.Roll_Number;
                    att.Timein = at.TimeIn;
                    att.Timeout = at.TimeOut;
                    att.Name = nn[i];
                    attendance.Add(att);
                }
                return Json(attendance, JsonRequestBehavior.AllowGet);
            }
            else if (Classid != 0 && type == "Present")
            {
                var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.Date == start && x.Class == cname).ToList();
                var length = att_list.Count;


                for (int i = 0; i < length; i++)
                {
                    AspNetStudent_AutoAttendance at = att_list[i];

                    var nn = (from un in db.AspNetUsers
                              join r in db.AspNetStudent_AutoAttendance
                              on un.UserName equals r.Roll_Number
                              where un.UserName == at.Roll_Number && r.Date == start
                              select un.Name).FirstOrDefault();
                    AutoAttendance att = new AutoAttendance();
                    att.Class = at.Class;
                    att.Date = at.Date;
                    att.RollNumber = at.Roll_Number;
                    att.Timein = at.TimeIn;
                    att.Timeout = at.TimeOut;
                    att.Name = nn;
                    attendance.Add(att);

                }
                return Json(attendance, JsonRequestBehavior.AllowGet);
            }
            else if (Classid != 0 && type == "Absent")
            {
                var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                var pstd = db.AspNetStudent_AutoAttendance.Where(x => x.Date == start && x.Class==cname).Select(x => x.Roll_Number).ToList();
                var tstd = db.AspNetStudents.Where(x => x.AspNetUser.Status != "False" && x.ClassID == Classid).Select(x => x.AspNetUser.UserName).ToList();
                var astd = tstd.Except(pstd).ToList();

                foreach (var item in astd)
                {
                    AutoAttendance at = new AutoAttendance();
                    var att = (from user in db.AspNetUsers
                               join std in db.AspNetStudents
                               on user.Id equals std.StudentID
                               where user.UserName == item && std.StudentID == user.Id
                               select new { user.Name, user.UserName, std.AspNetClass.ClassName }).FirstOrDefault();

                    at.Class = att.ClassName;
                    at.Date = start;
                    at.RollNumber = att.UserName;
                    at.Timein = null;
                    at.Timeout = null;
                    at.Name = att.Name;
                    attendance.Add(at);
                }
                return Json(attendance,JsonRequestBehavior.AllowGet);
            }
            else if (Classid != 0 && type == "Late")
            {
               
                //  var timecheck = Convert.ToDateTime("10:00:00");
               // var time = timecheck.TimeOfDay;
                var cname = db.AspNetClasses.Where(x => x.Id == Classid).Select(x => x.ClassName).FirstOrDefault();
                var att_list = db.AspNetStudent_AutoAttendance.Where(x => x.TimeIn > lateTime && x.Date == start && x.Class == cname).ToList();
                var length = att_list.Count;
               

                for (int i = 0; i < length; i++)
                {
                    AspNetStudent_AutoAttendance at = att_list[i];
                    var nn = (from un in db.AspNetUsers
                              join r in db.AspNetStudent_AutoAttendance
                              on un.UserName equals r.Roll_Number
                              where un.UserName == at.Roll_Number && r.Date == start && r.TimeIn > lateTime
                              select un.Name).FirstOrDefault();
                    AutoAttendance att = new AutoAttendance();
                    att.Class = at.Class;
                    att.Date = at.Date;
                    att.RollNumber = at.Roll_Number;
                    att.Timein = at.TimeIn;
                    att.Timeout = at.TimeOut;
                    att.Name = nn;
                    attendance.Add(att);

                }
                return Json(attendance, JsonRequestBehavior.AllowGet);

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }
        ///////////////////////////////////////////////END/////////////////////////////////////////////////////////////

        public class Message
        {
            public string Name { get; set; }
            public string message { get; set; }

            public string date { get; set; }
        }
        public class AutoAttendance
        {
            public string Name { get; set; }
            public string Class { get; set; }
            public DateTime? Date { get; set; }
            public string RollNumber { get; set; }
            public TimeSpan? Timein { get; set; }
            public TimeSpan? Timeout { get; set; }

        }
        public class TODOLIST
        {

            public int HomeWorkId { get; set; }
            public string date { get; set; }
            public string Actualdate { get; set; }
            public string Classname { get; set; }

            public bool isToDay { get; set; }
        }

        /*******************************************************************************************************************/
        /*                                                                                                                 */
        /*                                    Accountant's Functions                                                       */
        /*                                                                                                                 */
        /*******************************************************************************************************************/
        public ActionResult Student_Data(string studentId)
        {
           var name= db.AspNetUsers.Where(x => x.Id == studentId).Select(x => x.Name).FirstOrDefault();
           var username= db.AspNetUsers.Where(x => x.Id == studentId).Select(x => x.UserName).FirstOrDefault();
            var cid=db.AspNetStudents.Where(x => x.StudentID == studentId).Select(x => x.ClassID).FirstOrDefault();
            var tid = db.AspNetClasses.Where(x => x.Id == cid).Select(x => x.TeacherID).FirstOrDefault();
            var tname = db.AspNetUsers.Where(x => x.Id == tid).Select(x => x.Name).FirstOrDefault();
            var resutl = new { name, username, tname };
            return Json(resutl, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AccountantEdit()
        {
            string id = Request.Form["id"];

            var user = db.AspNetUsers.Where(x => x.Id == id).FirstOrDefault();
            var details = db.AspNetEmployees.Where(x => x.UserId == id).FirstOrDefault();
            var transaction = db.Database.BeginTransaction();
            try
            {
                user.Name = Request.Form["Name"];
                user.UserName = Request.Form["UserName"];
                user.Email = Request.Form["Email"];
                user.PhoneNumber = Request.Form["cellNo"];

                details.PositionAppliedFor = Request.Form["appliedFor"];
                details.DateAvailable = Request.Form["dateAvailable"];
                details.JoiningDate = Request.Form["JoiningDate"];
                details.BirthDate = Request.Form["birthDate"];
                details.Nationality = Request.Form["nationality"];
                details.Religion = Request.Form["religion"];
                details.Gender = Request.Form["gender"];
                details.MailingAddress = Request.Form["mailingAddress"];
                details.Email = user.Email;
                details.Name = user.Name;
                details.UserName = user.UserName;
                details.CellNo = user.PhoneNumber;
                details.Landline = Request.Form["landLine"];
                details.SpouseName = Request.Form["spouseName"];
                details.SpouseHighestDegree = Request.Form["spouseHighestDegree"];
                details.SpouseOccupation = Request.Form["spouseOccupation"];
                details.SpouseBusinessAddress = Request.Form["spouseBusinessAddress"];
                details.GrossSalary = Convert.ToInt32(Request.Form["GrossSalary"]);
                details.BasicSalary = Convert.ToInt32(Request.Form["BasicSalary"]);
                details.MedicalAllowance = Convert.ToInt32(Request.Form["MedicalAllowance"]);
                details.Accomodation = Convert.ToInt32(Request.Form["Accomodation"]);
                details.ProvidedFund = Convert.ToInt32(Request.Form["ProvidedFund"]);
                details.Tax = Convert.ToInt32(Request.Form["Tax"]);
                details.EOP = Convert.ToInt32(Request.Form["EOP"]);

                db.SaveChanges();
                transaction.Commit();
                return RedirectToAction("AccountantIndex", "AspNetUser");
            }
            catch
            {
                transaction.Dispose();
                return View("Sonething Went wrong");
            }

        }

        public ActionResult AccountantRegister()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AccountantRegister(RegisterViewModel model)
        {
            if (1 == 1)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name, PhoneNumber = Request.Form["cellNo"] };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ruffdata rd = new ruffdata();
                    rd.SessionID = SessionID;
                    rd.StudentName = model.Name;
                    rd.StudentUserName = model.UserName;
                    rd.StudentPassword = model.Password;
                    db.ruffdatas.Add(rd);
                    db.SaveChanges();
                }
                AspNetUser Accountant = new AspNetUser();
                Accountant.Name = user.Name;
                Accountant.UserName = user.UserName;
                Accountant.Email = user.Email;
                Accountant.PasswordHash = user.PasswordHash;
                Accountant.Status = "Active";
                Accountant.PhoneNumber = Request.Form["cellNo"];

                AspNetEmployee emp = new AspNetEmployee();
                emp.Email = Accountant.Email;
                emp.UserName = Accountant.UserName;
                emp.Name = Accountant.Name;
                emp.PositionAppliedFor = Request.Form["appliedFor"];
                emp.DateAvailable = Request.Form["dateAvailable"];
                emp.JoiningDate = Request.Form["JoiningDate"];
                emp.BirthDate = Request.Form["birthDate"];
                emp.Nationality = Request.Form["nationality"];
                emp.Religion = Request.Form["religion"];
                emp.Gender = Request.Form["gender"];
                emp.MailingAddress = Request.Form["mailingAddress"];
                emp.CellNo = Request.Form["cellNo"];
                emp.Landline = Request.Form["landLine"];
                emp.SpouseName = Request.Form["spouseName"];
                emp.SpouseHighestDegree = Request.Form["spouseHighestDegree"];
                emp.SpouseOccupation = Request.Form["spouseOccupation"];
                emp.SpouseBusinessAddress = Request.Form["spouseBusinessAddress"];
                emp.GrossSalary = Convert.ToInt32(Request.Form["GrossSalary"]);
                emp.BasicSalary = Convert.ToInt32(Request.Form["BasicSalary"]);
                emp.MedicalAllowance = Convert.ToInt32(Request.Form["MedicalAllowance"]);
                emp.Accomodation = Convert.ToInt32(Request.Form["Accomodation"]);
                emp.ProvidedFund = Convert.ToInt32(Request.Form["ProvidedFund"]);
                emp.Tax = Convert.ToInt32(Request.Form["Tax"]);
                emp.EOP = Convert.ToInt32(Request.Form["EOP"]);
                emp.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Management Staff").Select(x => x.Id).FirstOrDefault();
                emp.UserId = user.Id;
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Accountant");

                    db.AspNetEmployees.Add(emp);
                    db.SaveChanges();
                    AspNetUsers_Session US = new AspNetUsers_Session();
                    US.UserID = emp.UserId;
                    US.SessionID = SessionID;
                    db.AspNetUsers_Session.Add(US);
                    db.SaveChanges();

                    string Error = "Accountant Saved successfully";
                    return RedirectToAction("AccountantsIndex", "AspNetUser" , new { Error});
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult DisabledTeachers()
        {
            return View();
        }

        public JsonResult DisableTeachers()
        {
            var teachers = (from teacher in db.AspNetUsers.Where(x => x.Status == "False")
                            where teacher.AspNetRoles.Select(y => y.Name).Contains("Teacher")
                            select new
                            {
                                teacher.Id,
                                Class = teacher.AspNetClasses.Select(x => x.ClassName).FirstOrDefault(),
                                Subject = "-",
                                teacher.Email,
                                teacher.PhoneNumber,
                                teacher.UserName,
                                teacher.Name,
                            }).ToList();

            return Json(teachers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AccountantfromFile(RegisterViewModel model)
        {
            // if (ModelState.IsValid)
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["Accountants"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
                var AccountantList = new List<RegisterViewModel>();
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    ApplicationDbContext context = new ApplicationDbContext();
                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var Accountant = new RegisterViewModel();
                        Accountant.Email = workSheet.Cells[rowIterator, 1].Value.ToString();
                        Accountant.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                        Accountant.UserName = workSheet.Cells[rowIterator, 3].Value.ToString();
                        Accountant.Password = workSheet.Cells[rowIterator, 4].Value.ToString();
                        Accountant.ConfirmPassword = workSheet.Cells[rowIterator, 5].Value.ToString();
                        string number = workSheet.Cells[rowIterator, 14].Value.ToString();
                        var user = new ApplicationUser { UserName = Accountant.UserName, Email = Accountant.Email, Name = Accountant.Name, PhoneNumber = number };
                        var result = await UserManager.CreateAsync(user, Accountant.Password);
                        if (result.Succeeded)
                        {
                            AspNetEmployee AccountantDetail = new AspNetEmployee();
                            AccountantDetail.Name = Accountant.Name;
                            AccountantDetail.Email = Accountant.Email;
                            AccountantDetail.UserName = Accountant.UserName;
                            AccountantDetail.UserId = user.Id;
                            AccountantDetail.CellNo = user.PhoneNumber;
                            AccountantDetail.PositionAppliedFor = workSheet.Cells[rowIterator, 6].Value.ToString();
                            AccountantDetail.DateAvailable = workSheet.Cells[rowIterator, 7].Value.ToString();
                            AccountantDetail.JoiningDate = workSheet.Cells[rowIterator, 8].Value.ToString();
                            AccountantDetail.BirthDate = workSheet.Cells[rowIterator, 9].Value.ToString();
                            AccountantDetail.Nationality = workSheet.Cells[rowIterator, 10].Value.ToString();
                            AccountantDetail.Religion = workSheet.Cells[rowIterator, 11].Value.ToString();
                            AccountantDetail.Gender = workSheet.Cells[rowIterator, 12].Value.ToString(); ;
                            AccountantDetail.MailingAddress = workSheet.Cells[rowIterator, 13].Value.ToString();
                            AccountantDetail.CellNo = workSheet.Cells[rowIterator, 14].Value.ToString();
                            AccountantDetail.Landline = workSheet.Cells[rowIterator, 15].Value.ToString();
                            AccountantDetail.SpouseName = workSheet.Cells[rowIterator, 16].Value.ToString();
                            AccountantDetail.SpouseHighestDegree = workSheet.Cells[rowIterator, 17].Value.ToString();
                            AccountantDetail.SpouseOccupation = workSheet.Cells[rowIterator, 18].Value.ToString();
                            AccountantDetail.SpouseBusinessAddress = workSheet.Cells[rowIterator, 19].Value.ToString();
                            AccountantDetail.Illness = workSheet.Cells[rowIterator, 20].Value.ToString();
                            AccountantDetail.GrossSalary = Convert.ToInt32(workSheet.Cells[rowIterator, 21].Value.ToString());
                            AccountantDetail.BasicSalary = Convert.ToInt32(workSheet.Cells[rowIterator, 22].Value.ToString());
                            AccountantDetail.MedicalAllowance = Convert.ToInt32(workSheet.Cells[rowIterator, 23].Value.ToString());
                            AccountantDetail.Accomodation = Convert.ToInt32(workSheet.Cells[rowIterator, 24].Value.ToString());
                            AccountantDetail.ProvidedFund = Convert.ToInt32(workSheet.Cells[rowIterator, 25].Value.ToString());
                            AccountantDetail.Tax = Convert.ToInt32(workSheet.Cells[rowIterator, 26].Value.ToString());
                            AccountantDetail.EOP = Convert.ToInt32(workSheet.Cells[rowIterator, 27].Value.ToString());
                            AccountantDetail.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Management Staff").Select(x => x.Id).FirstOrDefault();
                            db.AspNetEmployees.Add(AccountantDetail);
                            db.SaveChanges();

                            var roleStore = new RoleStore<IdentityRole>(context);
                            var roleManager = new RoleManager<IdentityRole>(roleStore);
                            var userStore = new UserStore<ApplicationUser>(context);
                            var userManager = new UserManager<ApplicationUser>(userStore);
                            userManager.AddToRole(user.Id, "Accountant");
                        }
                        else
                        {
                            dbTransaction.Dispose();
                            AddErrors(result);
                            return View("AccountantRegister", model);
                        }

                    }
                    dbTransaction.Commit();
                    return RedirectToAction("AccountantIndex", "AspNetUser");
                }
            }
            catch (Exception)
            { dbTransaction.Dispose(); }

            return RedirectToAction("AccountantRegister", model);
        }

        /*******************************************************************************************************************
        * 
        *                                   Parent's Functions
        *                                    
        *******************************************************************************************************************/

        public ActionResult ParentRegister()
        {
        //    ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            return View();
        }

        public async Task<bool> SendMail(string toemail, string subject, string body)
        {
            bool IsMailSent = false;

            try
            {
                MailMessage msg = new MailMessage();
                //msg.To.Add(new MailAddress(toemail));
                msg.From = new MailAddress("talhaghaffar98@gmail.com", "SEA | Smarter Education Analytics");
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                string ccMail = string.Empty;
                string bccMail = string.Empty;
                ccMail = "talhaghaffar98@gmail.com";
                bccMail = "talhaghaffar98@gmail.com";
                msg.To.Add(toemail);
                msg.Bcc.Add(bccMail);
                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("talhaghaffar98@gmail.com", "Orion@123");
                client.Port = 25; // You can use Port 25 if 587 is blocked (mine is!)
                client.Host = "smtp.gmail.com";
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                string userState = "call back object on success";
                await Task.Run(() => client.SendAsync(msg, userState));
                IsMailSent = true;
            }
            catch (Exception ex)
            {
            //    logAppException(ex.ToString(), "email send");
            }

            return IsMailSent;
        }

        
public ActionResult ConfirmAccount(string id)
        {
            ViewBag.res = "error";
            var user = db.AspNetUsers.Where(x => x.Id == id).FirstOrDefault();
            if (user != null)
            {
                if (user.Status == "True")
                {
                    ViewBag.res = "Hi " + user.UserName + "! Your Account is already activated";
                }
                else
                {
                    user.Status = "True";
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.res = "Hi " + user.UserName + "! Your Account has been activated";

                }
            }
            else
            {
                ViewBag.res = "Your Account can not be activated";

            }
            return View();
        }
        public ActionResult SendConformationEmail(string id)
        {
            string status = "error";
            try
            {
                string DomainName = Request.Url.GetLeftPart(UriPartial.Authority);
                var u = db.AspNetUsers.FirstOrDefault(x => x.Id == id);
              //  string ConfirmLink = "<a href/Security/EmailConfirmation/?ConfirmAccount={0}</a>";

                string ConfirmLink = "<a href='" + String.Format(DomainName + "/Parent_Dashboard/ConfirmAccount/?id={0}", u.Id.ToString() + "' target='_blank'>Click Here To Confirm Your Email </a>'");
                SendMail(u.Email, "Account confirmation", "" + EmailDesign.SignupEmailTemplate(ConfirmLink, u.UserName));
                status = "Success";
            }
            catch (Exception ex)
            {
            //    logAppException(ex.StackTrace.ToString(), "Send Conformation Email");
            }
            return Content(status);
        }
        public ActionResult test()
        {
            SendConformationEmail("3e6f6b9b-e45b-4ee3-85bc-9848ba666fac");
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ParentRegister(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dbTransaction = db.Database.BeginTransaction();
                try
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    IEnumerable<string> selectedstudents = Request.Form["StudentID"].Split(',');
                    var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name, PhoneNumber = Request.Form["fatherCell"] };
                    //                    SendConformationEmail(user);
                  
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {

                        ruffdata rd = new ruffdata();
                        rd.SessionID = SessionID;
                        rd.StudentName = model.Name;
                        rd.StudentUserName = model.UserName;
                        rd.StudentPassword = model.Password;
                        db.ruffdatas.Add(rd);
                        db.SaveChanges();

                        AspNetParent parent = new AspNetParent();
                        parent.FatherName = Request.Form["fatherName"];
                        parent.FatherCellNo = Request.Form["fatherCell"];
                        parent.FatherEmail = Request.Form["fatherEmail"];
                        parent.FatherOccupation = Request.Form["fatherOccupation"];
                        parent.FatherEmployer = Request.Form["fatherEmployer"];
                        parent.MotherName = Request.Form["motherName"];
                        parent.MotherCellNo = Request.Form["motherCell"];
                        parent.MotherEmail = Request.Form["motherEmail"];
                        parent.MotherOccupation = Request.Form["motherOccupation"];
                        parent.MotherEmployer = Request.Form["motherEmployer"];
                        parent.UserID = user.Id;
                        db.AspNetParents.Add(parent);

                        AspNetUsers_Session US = new AspNetUsers_Session();
                        US.UserID = user.Id;
                        US.SessionID = SessionID;
                        db.AspNetUsers_Session.Add(US);
                        db.SaveChanges();
                        
                        foreach (var item in selectedstudents)
                        {
                            AspNetParent_Child par_stu = new AspNetParent_Child();
                            par_stu.ChildID = item;
                            par_stu.ParentID = user.Id;
                            db.AspNetParent_Child.Add(par_stu);
                            db.SaveChanges();
                        }

                        var roleStore = new RoleStore<IdentityRole>(context);
                        var roleManager = new RoleManager<IdentityRole>(roleStore);

                        var userStore = new UserStore<ApplicationUser>(context);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        userManager.AddToRole(user.Id, "Parent");

                        dbTransaction.Commit();
                        SendConformationEmail(user.Id);
                    }
                    else
                    {
                        dbTransaction.Dispose();
                        AddErrors(result);
                        return View(model);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.InnerException);
                    dbTransaction.Dispose();
                    return View(model);
                }
            }
            string Error = "Parent successfully saved";
            return RedirectToAction("ParentsIndex", "AspNetUser", new { Error});
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ParentEdit()
        {
            string id = Request.Form["Id"];
            var parent = db.AspNetParents.Where(x => x.UserID == id).Select(x => x).FirstOrDefault();
            var user = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();

            IEnumerable<string> selectedstudents = Request.Form["StudentID"].Split(',');
            user.UserName = Request.Form["UserName"];
            user.Name = Request.Form["Name"];
            user.Email = Request.Form["Email"];

            parent.FatherName = Request.Form["fatherName"];
            parent.FatherCellNo = Request.Form["fatherCell"];
            parent.FatherEmail = Request.Form["fatherEmail"];
            parent.FatherOccupation = Request.Form["fatherOccupation"];
            parent.FatherEmployer = Request.Form["fatherEmployer"];
            parent.MotherName = Request.Form["motherName"];
            parent.MotherCellNo = Request.Form["motherCell"];
            parent.MotherEmail = Request.Form["motherEmail"];
            parent.MotherOccupation = Request.Form["motherOccupation"];
            parent.MotherEmployer = Request.Form["motherEmployer"];

            var childs = db.AspNetParent_Child.Where(x => x.ParentID == user.Id).ToList();
            foreach (var item in childs)
            {
                db.AspNetParent_Child.Remove(item);
            }

            db.SaveChanges();

            db.AspNetUsers.Where(p => p.Id == id).FirstOrDefault().PhoneNumber = Request.Form["fatherCell"];
            db.SaveChanges();
            foreach (var item in selectedstudents)
            {
                AspNetParent_Child par_stu = new AspNetParent_Child();
                par_stu.ChildID = item;
                par_stu.ParentID = user.Id;
                db.AspNetParent_Child.Add(par_stu);
            }

            db.SaveChanges();

            return RedirectToAction("ParentIndex", "AspNetUser");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ParentRegisterFromFile(RegisterViewModel model)
        {
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["parents"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        ApplicationDbContext context = new ApplicationDbContext();
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        {
                            var parent = new RegisterViewModel();
                            parent.Email = workSheet.Cells[rowIterator, 1].Value.ToString();
                            parent.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                            parent.UserName = workSheet.Cells[rowIterator, 3].Value.ToString();
                            parent.Password = workSheet.Cells[rowIterator, 4].Value.ToString();
                            parent.ConfirmPassword = workSheet.Cells[rowIterator, 5].Value.ToString();

                            var user = new ApplicationUser { UserName = parent.UserName, Email = parent.Email, Name = parent.Name };
                            var result = await UserManager.CreateAsync(user, parent.Password);
                            if (result.Succeeded)
                            {
                                AspNetParent parentDetail = new AspNetParent();
                                parentDetail.UserID = user.Id;
                                parentDetail.FatherName= workSheet.Cells[rowIterator, 6].Value.ToString(); 
                                parentDetail.FatherCellNo= workSheet.Cells[rowIterator, 7].Value.ToString();
                                parentDetail.FatherEmail= workSheet.Cells[rowIterator, 8].Value.ToString();
                                parentDetail.FatherOccupation= workSheet.Cells[rowIterator, 9].Value.ToString();
                                parentDetail.FatherEmployer= workSheet.Cells[rowIterator, 10].Value.ToString();
                                parentDetail.MotherName= workSheet.Cells[rowIterator, 11].Value.ToString();
                                parentDetail.MotherCellNo= workSheet.Cells[rowIterator, 12].Value.ToString();
                                parentDetail.MotherEmail= workSheet.Cells[rowIterator, 13].Value.ToString();
                                parentDetail.MotherOccupation= workSheet.Cells[rowIterator, 14].Value.ToString();
                                parentDetail.MotherEmployer= workSheet.Cells[rowIterator, 15].Value.ToString();
                                db.AspNetParents.Add(parentDetail);
                                db.SaveChanges();

                                var childUsernames = new List<string>();
                                childUsernames.Add(workSheet.Cells[rowIterator, 16].Value.ToString());
                                childUsernames.Add(workSheet.Cells[rowIterator, 17].Value.ToString());
                                childUsernames.Add(workSheet.Cells[rowIterator, 18].Value.ToString());
                                childUsernames.Add(workSheet.Cells[rowIterator, 19].Value.ToString());

                                var childIDs = (from student in db.AspNetUsers
                                                where childUsernames.Contains(student.UserName)
                                                select student.Id).ToList();
                                foreach (var item in childIDs)
                                {
                                    AspNetParent_Child par_stu = new AspNetParent_Child();
                                    par_stu.ChildID = item;
                                    par_stu.ParentID = user.Id;
                                    db.AspNetParent_Child.Add(par_stu);
                                    db.SaveChanges();
                                }

                                var roleStore = new RoleStore<IdentityRole>(context);
                                var roleManager = new RoleManager<IdentityRole>(roleStore);

                                var userStore = new UserStore<ApplicationUser>(context);
                                var userManager = new UserManager<ApplicationUser>(userStore);
                                userManager.AddToRole(user.Id, "Parent");

                            }
                            else
                            {
                                dbTransaction.Dispose();
                                AddErrors(result);
                                return View("ParentRegister", model);
                            }
                        }
                        dbTransaction.Commit();
                    }
                }
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.InnerException);
                dbTransaction.Dispose();
                return View("ParentRegister", model);
            }
            return RedirectToAction("ParentIndex", "AspNetUser");
        }

        /*******************************************************************************************************************
         * 
         *                                    Teacher's Functions
         *                                    
         *******************************************************************************************************************/


        public ActionResult TeacherRegister()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult TeacherEdit(string id)
        {
            var user = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            
            user.Name = Request.Form["Name"];
            user.UserName = Request.Form["UserName"];
            user.Email = Request.Form["Email"];
            user.PhoneNumber = Request.Form["cellNo"];

            var emp = db.AspNetEmployees.Where(x => x.UserId == id).Select(x => x).FirstOrDefault();

            emp.Name = user.Name;
            emp.UserName = user.UserName;
            emp.Email = user.Email;
            emp.PositionAppliedFor = Request.Form["appliedFor"];
            emp.DateAvailable = Request.Form["dateAvailable"];
            emp.JoiningDate = Request.Form["JoiningDate"];
            emp.BirthDate = Request.Form["birthDate"];
            emp.Nationality = Request.Form["nationality"];
            emp.Religion = Request.Form["religion"];
            emp.Gender = Request.Form["gender"];
            emp.MailingAddress = Request.Form["mailingAddress"];
            emp.CellNo = Request.Form["cellNo"];
            emp.Landline = Request.Form["landLine"];
            emp.SpouseName = Request.Form["spouseName"];
            emp.SpouseHighestDegree = Request.Form["spouseHighestDegree"];
            emp.SpouseOccupation = Request.Form["spouseOccupation"];
            emp.SpouseBusinessAddress = Request.Form["spouseBusinessAddress"];

            //emp.GrossSalary = Convert.ToInt32(Request.Form["GrossSalary"]);
            //emp.BasicSalary = Convert.ToInt32(Request.Form["BasicSalary"]);
            //emp.MedicalAllowance = Convert.ToInt32(Request.Form["MedicalAllowance"]);
            //emp.Accomodation = Convert.ToInt32(Request.Form["Accomodation"]);
            //emp.ProvidedFund = Convert.ToInt32(Request.Form["ProvidedFund"]);
            //emp.Tax = Convert.ToInt32(Request.Form["Tax"]);
            //emp.EOP = Convert.ToInt32(Request.Form["EOP"]);

            db.SaveChanges();

            return RedirectToAction("TeachersIndex", "AspNetUser");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TeacherRegister(RegisterViewModel model)
        {
            if (1==1)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name=model.Name, PhoneNumber=Request.Form["cellNo"]  };
                var result = await UserManager.CreateAsync(user, model.Password);
                ruffdata rd = new ruffdata();
                rd.SessionID = SessionID;
                rd.StudentName = model.Name;
                rd.StudentUserName = model.UserName;
                rd.StudentPassword = model.Password;
                db.ruffdatas.Add(rd);
                db.SaveChanges();

                AspNetUser Teacher = new AspNetUser();
                Teacher.Name = user.Name;
                Teacher.UserName = user.UserName;
                Teacher.Email = user.Email;
                Teacher.PasswordHash = user.PasswordHash;
                Teacher.Status = "Active";
                Teacher.PhoneNumber = Request.Form["cellNo"];

                AspNetEmployee emp = new AspNetEmployee();
                emp.Name = Teacher.Name;
                emp.UserName = Teacher.UserName;
                emp.Email = Teacher.Email;
                emp.PositionAppliedFor = Request.Form["appliedFor"];
                emp.DateAvailable = Request.Form["dateAvailable"];
                emp.JoiningDate =Request.Form["JoiningDate"];
                emp.BirthDate = Request.Form["birthDate"];
                emp.Nationality = Request.Form["nationality"];
                emp.Religion = Request.Form["religion"];
                emp.Gender = Request.Form["gender"];
                emp.MailingAddress = Request.Form["mailingAddress"];
                emp.CellNo = Request.Form["cellNo"];
                emp.Landline = Request.Form["landLine"];
                emp.SpouseName = Request.Form["spouseName"];
                emp.SpouseHighestDegree = Request.Form["spouseHighestDegree"];
                emp.SpouseOccupation = Request.Form["spouseOccupation"];
                emp.SpouseBusinessAddress = Request.Form["spouseBusinessAddress"];
                
                //emp.GrossSalary = Convert.ToInt32(Request.Form["GrossSalary"]);
                //emp.BasicSalary = Convert.ToInt32(Request.Form["BasicSalary"]);
                //emp.MedicalAllowance = Convert.ToInt32(Request.Form["MedicalAllowance"]);
                //emp.Accomodation = Convert.ToInt32(Request.Form["Accomodation"]);
                //emp.ProvidedFund = Convert.ToInt32(Request.Form["ProvidedFund"]);
                //emp.Tax = Convert.ToInt32(Request.Form["Tax"]);
                //emp.EOP = Convert.ToInt32(Request.Form["EOP"]);
                
                emp.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Teaching Staff").Select(x => x.Id).FirstOrDefault();
                emp.UserId = user.Id;
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Teacher");
                    
                    db.AspNetEmployees.Add(emp);
                    db.SaveChanges();
                  
                    AspNetUsers_Session US = new AspNetUsers_Session();
                    US.UserID = emp.UserId;
                    int sessionid = db.AspNetSessions.Where(x => x.Status == "Active").FirstOrDefault().Id;
                    US.SessionID = sessionid;
                    db.AspNetUsers_Session.Add(US);
                    if(db.SaveChanges()>0)
                    {
                        Aspnet_Employee_Session aes = new Aspnet_Employee_Session();
                        aes.Emp_Id = emp.Id;
                        aes.Session_Id = sessionid;
                        db.Aspnet_Employee_Session.Add(aes);
                        db.SaveChanges();
                    }

                    string Error = "Teacher successfully saved.";
                    return RedirectToAction("TeacherIndex", "AspNetUser", new { Error });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TeacherfromFile(RegisterViewModel model)
        {
            // if (ModelState.IsValid)
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["teachers"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
                var teacherList = new List<RegisterViewModel>();
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    ApplicationDbContext context = new ApplicationDbContext();
                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        var teacher = new RegisterViewModel();
                        teacher.Email = workSheet.Cells[rowIterator, 1].Value.ToString();
                        teacher.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                        teacher.UserName = workSheet.Cells[rowIterator, 3].Value.ToString();
                        teacher.Password = workSheet.Cells[rowIterator, 4].Value.ToString();
                        teacher.ConfirmPassword = workSheet.Cells[rowIterator, 5].Value.ToString();
                        string number = workSheet.Cells[rowIterator, 14].Value.ToString();
                        var user = new ApplicationUser { UserName = teacher.UserName, Email = teacher.Email, Name = teacher.Name, PhoneNumber = number };
                        var result = await UserManager.CreateAsync(user, teacher.Password);
                        if (result.Succeeded)
                        {
                            AspNetEmployee teacherDetail = new AspNetEmployee();
                            teacherDetail.Name = teacher.Name;
                            teacherDetail.Email = teacher.Email;
                            teacherDetail.UserName = teacher.UserName;
                            teacherDetail.UserId = user.Id;
                            teacherDetail.PositionAppliedFor = workSheet.Cells[rowIterator, 6].Value.ToString();
                            teacherDetail.DateAvailable = workSheet.Cells[rowIterator, 7].Value.ToString();
                            teacherDetail.JoiningDate = workSheet.Cells[rowIterator, 8].Value.ToString();
                            teacherDetail.BirthDate = workSheet.Cells[rowIterator, 9].Value.ToString();
                            teacherDetail.Nationality = workSheet.Cells[rowIterator, 10].Value.ToString();
                            teacherDetail.Religion = workSheet.Cells[rowIterator, 11].Value.ToString();
                            teacherDetail.Gender = workSheet.Cells[rowIterator, 12].Value.ToString(); ;
                            teacherDetail.MailingAddress = workSheet.Cells[rowIterator, 13].Value.ToString();
                            teacherDetail.CellNo = workSheet.Cells[rowIterator, 14].Value.ToString();
                            teacherDetail.Landline = workSheet.Cells[rowIterator, 15].Value.ToString();
                            teacherDetail.SpouseName = workSheet.Cells[rowIterator, 16].Value.ToString();
                            teacherDetail.SpouseHighestDegree = workSheet.Cells[rowIterator, 17].Value.ToString();
                            teacherDetail.SpouseOccupation = workSheet.Cells[rowIterator, 18].Value.ToString();
                            teacherDetail.SpouseBusinessAddress = workSheet.Cells[rowIterator, 19].Value.ToString();
                            teacherDetail.Illness = workSheet.Cells[rowIterator, 20].Value.ToString();
                            teacherDetail.GrossSalary = Convert.ToInt32(workSheet.Cells[rowIterator, 21].Value.ToString());
                            teacherDetail.BasicSalary = Convert.ToInt32(workSheet.Cells[rowIterator, 22].Value.ToString());
                            teacherDetail.MedicalAllowance = Convert.ToInt32(workSheet.Cells[rowIterator, 23].Value.ToString());
                            teacherDetail.Accomodation = Convert.ToInt32(workSheet.Cells[rowIterator, 24].Value.ToString());
                            teacherDetail.ProvidedFund = Convert.ToInt32(workSheet.Cells[rowIterator, 25].Value.ToString());
                            teacherDetail.Tax = Convert.ToInt32(workSheet.Cells[rowIterator, 26].Value.ToString());
                            teacherDetail.EOP = Convert.ToInt32(workSheet.Cells[rowIterator, 27].Value.ToString());
                            teacherDetail.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Teaching Staff").Select(x => x.Id).FirstOrDefault();
                            db.AspNetEmployees.Add(teacherDetail);
                            db.SaveChanges();

                            var roleStore = new RoleStore<IdentityRole>(context);
                            var roleManager = new RoleManager<IdentityRole>(roleStore);
                            var userStore = new UserStore<ApplicationUser>(context);
                            var userManager = new UserManager<ApplicationUser>(userStore);
                            userManager.AddToRole(user.Id, "Teacher");
                        }
                        else
                        {
                            dbTransaction.Dispose();
                            AddErrors(result);
                            return View("TeacherRegister", model);
                        }

                    }
                    dbTransaction.Commit();
                }
            }
            catch (Exception e)
            {
             //   ModelState.AddModelError("Error", e.InnerException);
                dbTransaction.Dispose();
                return View("TeacherRegister", model);
               
            }
            return RedirectToAction("TeachersIndex", "AspNetUser");
        }



        /*******************************************************************************************************************
         * 
         *                                    Student's Functions
         *                                    
         *******************************************************************************************************************/
        public ActionResult StudentRegister()
        {
            //var data = db.AspNetClasses 
           // ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
           ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentRegister(RegisterViewModel model)
        {
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
               
                if (ModelState.IsValid)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    IEnumerable<string> selectedsubjects = Request.Form["subjects"].Split(',');
                    var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name, PhoneNumber = Request.Form["cellNo"] };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        ruffdata rd = new ruffdata();
                        rd.SessionID = SessionID;
                        rd.StudentName = model.Name;
                        rd.StudentUserName = model.UserName;
                        rd.StudentPassword = model.Password;
                        db.ruffdatas.Add(rd);
                        db.SaveChanges();


                        AspNetStudent student = new AspNetStudent();
                        student.StudentID = user.Id;
                        student.SchoolName = Request.Form["SchoolName"];
                        student.BirthDate = Request.Form["BirthDate"];
                        student.Nationality = Request.Form["Nationality"];
                        student.Religion = Request.Form["Religion"];
                        student.Gender = Request.Form["Gender"];
                        student.ClassID = Convert.ToInt32(Request.Form["ClassID"]);
                        db.AspNetStudents.Add(student);
                        db.SaveChanges();

                        AspNetStudent_Session_class asc = new AspNetStudent_Session_class();
                        asc.ClassID = student.ClassID;
                        Aspnet_Employee_Session ES = new Aspnet_Employee_Session();
                        int sessionid = db.AspNetSessions.Where(x => x.Status == "Active").FirstOrDefault().Id;
                        asc.SessionID = sessionid;
                        asc.StudentID =   student.Id;
                        db.AspNetStudent_Session_class.Add(asc);
                        if (db.SaveChanges() > 0)
                        {

                            AspNetUsers_Session AS = new AspNetUsers_Session();
                            AS.UserID = student.StudentID;
                            AS.SessionID = sessionid;
                            db.AspNetUsers_Session.Add(AS);
                            db.SaveChanges();
                        }


                        foreach (var item in selectedsubjects)
                        {
                            AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                            stu_sub.StudentID = user.Id;
                            stu_sub.SubjectID = Convert.ToInt32(item);
                            db.AspNetStudent_Subject.Add(stu_sub);
                            db.SaveChanges();
                        }

                        var roleStore = new RoleStore<IdentityRole>(context);
                        var roleManager = new RoleManager<IdentityRole>(roleStore);

                        var userStore = new UserStore<ApplicationUser>(context);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        userManager.AddToRole(user.Id, "Student");

                        dbTransaction.Commit();
                        string Error = "Student successfully saved.";
                        return RedirectToAction("StudentIndex", "AspNetUser", new { Error } );
                    }
                    else
                    {
                        dbTransaction.Dispose();
                        AddErrors(result);
                    }
                }
            }
            catch (Exception e)
            {
                dbTransaction.Dispose();
                ModelState.AddModelError("", e.Message);
            }
             ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return View(model);
        }


          

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StudentfromFile(RegisterViewModel model)
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            // if (ModelState.IsValid)
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                HttpPostedFileBase file = Request.Files["students"];
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
                        var student = new RegisterViewModel();
                        student.Email = workSheet.Cells[rowIterator, 1].Value.ToString();
                        student.Name = workSheet.Cells[rowIterator, 2].Value.ToString();
                        student.UserName = workSheet.Cells[rowIterator, 3].Value.ToString();
                        student.Password = workSheet.Cells[rowIterator, 4].Value.ToString();
                        student.ConfirmPassword = workSheet.Cells[rowIterator, 5].Value.ToString();

                        ApplicationDbContext context = new ApplicationDbContext();
                        var user = new ApplicationUser { UserName = student.UserName, Email = student.Email, Name = student.Name };
                        var result = await UserManager.CreateAsync(user, student.Password);
                        if (result.Succeeded)
                        {
                            var subjects = new List<string>();
                            var Class = workSheet.Cells[rowIterator, 6].Value.ToString();
                            subjects.Add(workSheet.Cells[rowIterator, 7].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 8].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 9].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 10].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 11].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 12].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 13].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 14].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 15].Value.ToString());
                            subjects.Add(workSheet.Cells[rowIterator, 16].Value.ToString());

                            var subjectIDs = (from subject in db.AspNetSubjects
                                     join Classes in db.AspNetClasses on subject.ClassID equals Classes.Id
                                     where Classes.ClassName == Class && subjects.Contains(subject.SubjectName)
                                     select subject).ToList();

                            foreach (var subjectid in subjectIDs)
                            {
                                AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                                stu_sub.StudentID = user.Id;
                                stu_sub.SubjectID = subjectid.Id;
                                db.AspNetStudent_Subject.Add(stu_sub);
                                db.SaveChanges();
                            }

                            AspNetStudent studentDetail = new AspNetStudent();
                            studentDetail.StudentID = user.Id;
                            studentDetail.SchoolName= workSheet.Cells[rowIterator, 17].Value.ToString();
                            studentDetail.BirthDate= workSheet.Cells[rowIterator, 18].Value.ToString();
                            studentDetail.Nationality= workSheet.Cells[rowIterator, 19].Value.ToString();
                            studentDetail.Religion= workSheet.Cells[rowIterator, 20].Value.ToString();
                            studentDetail.Gender= workSheet.Cells[rowIterator, 21].Value.ToString();
                            studentDetail.ClassID = db.AspNetClasses.Where(x => x.ClassName == Class).Select(x => x.Id).FirstOrDefault();
                            db.AspNetStudents.Add(studentDetail);
                            db.SaveChanges();

                            var roleStore = new RoleStore<IdentityRole>(context);
                            var roleManager = new RoleManager<IdentityRole>(roleStore);
                            var userStore = new UserStore<ApplicationUser>(context);
                            var userManager = new UserManager<ApplicationUser>(userStore);
                            userManager.AddToRole(user.Id, "Student");

                        }
                        else
                        {
                            dbTransaction.Dispose();
                            AddErrors(result);
                            return View("StudentRegister", model);
                        }
                    }
                    dbTransaction.Commit();
                }  
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.InnerException);
                dbTransaction.Dispose();
                return View("StudentRegister", model);
            }
            return RedirectToAction("StudentsIndex", "AspNetUser");
        }

        /**********************************************************************************************************************************/


        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;
            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult StudentsByClassMethod(int id)
        {
            string ClassHead = db.AspNetClasses.Where(x => x.Id == id).Select(x => x.TeacherID).First();
            string currentTeacher = User.Identity.GetUserId();

            // if(String.Compare( ClassHead , currentTeacher) == 0)

            var students = (from student in db.AspNetUsers
                            join student_subject in db.AspNetStudent_Subject on student.Id equals student_subject.StudentID
                            join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                            where subject.ClassID == id
                            select new { student.Id, student.UserName, student.Name }).Distinct().ToList();

            return Json(students, JsonRequestBehavior.AllowGet);



        }

        [HttpGet]
        public JsonResult StudentsByClass(string[] bdoIds)
        {
            try
            {
                List<int?> ids = new List<int?>();
                foreach (var item in bdoIds)
                {
                    int a = Convert.ToInt32(item);
                    ids.Add(a);
                }

                var aIDs = db.AspNetParent_Child.AsEnumerable().Select(r => r.ChildID);

                var students = (from student in db.AspNetUsers.AsEnumerable()
                                join student_subject in db.AspNetStudent_Subject on student.Id equals student_subject.StudentID
                                join subject in db.AspNetSubjects on student_subject.SubjectID equals subject.Id
                                where ids.Contains(subject.ClassID)
                                orderby subject.ClassID ascending
                                select new { student.Id, student.Name, student.UserName } ).Distinct().OrderBy(x=> x.Name).ToList();

               // var diff = aIDs.Except(students);
               

                return Json(students, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

       

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion



    }

}