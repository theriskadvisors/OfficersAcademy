using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using OfficeOpenXml;
using iTextSharp.text;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;

namespace SEA_Application.Controllers
{
    [Authorize(Roles = "Admin,Principal,Accountant")]
    public class AspNetUserController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
         int SessionID =  Int32.Parse(SessionIDStaticController.GlobalSessionID);
        public AspNetUserController()
        {

        }

        public AspNetUserController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        //private object ConfigurationManager;

        /*************************************************************Student List Functions*******************************************************/
        [HttpGet]
        public JsonResult SubjectsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetSubject> sub = db.AspNetSubjects.Where(r => r.ClassID == id && r.Status!="false").OrderByDescending(r => r.Id).ToList();
            ViewBag.Subjects = sub;

            return Json(sub, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult SubjectsParentsByClass(int id)
        {
            var StSu = new StudentAndSubjects();
            var subject = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).Select(x => new { x.Id, x.SubjectName, x.AspNetClass.ClassName }).ToList();
            List<Subjects> sub = new List<Subjects>();

            foreach (var item in subject)
            {
                Subjects s = new Subjects();
                s.id = item.Id;
                s.Subjectname = item.SubjectName;
                s.Classname = item.ClassName;
                sub.Add(s);
            }

            StSu.Subjects = sub;

            StSu.Students = new List<Students>();

            if (id != 0)
            {
                var Students = (from parent in db.AspNetParent_Child
                                join student in db.AspNetStudents on parent.ChildID equals student.StudentID
                                where student.ClassID == id
                                select new { parent.AspNetUser1.Name, parent.AspNetUser1.UserName, parent.AspNetUser1.Email, childName = parent.AspNetUser.Name }).Distinct().ToList();

                foreach (var item in Students)
                {
                    var student = new Students();
                    student.Name = item.Name;
                    student.Email = item.Email;
                    student.RollNo = item.UserName;
                    student.ClassName = item.childName;
                    StSu.Students.Add(student);
                }
            }

            if (id == 0)
            {
                var Students = (from parent in db.AspNetParent_Child
                                join student in db.AspNetStudents on parent.ChildID equals student.StudentID
                                join session in db.AspNetUsers_Session on parent.ChildID equals session.AspNetUser.Id
                                where session.SessionID == SessionID
                                select new { parent.AspNetUser1.Name, parent.AspNetUser1.UserName, parent.AspNetUser1.Email, childName = parent.AspNetUser.Name }).Distinct().ToList();


                foreach (var item in Students)
                {
                    var student = new Students();
                    student.Name = item.Name;
                    student.Email = item.Email;
                    student.RollNo = item.UserName;
                    student.ClassName = item.childName;
                    StSu.Students.Add(student);
                }
            }

            return Json(StSu, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SubjectsStudentsByClass(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var StSu = new StudentAndSubjects();
            var subject = db.AspNetSubjects.Where(r => r.ClassID == id).OrderByDescending(r => r.Id).Select(x => new { x.Id, x.SubjectName, x.AspNetClass.ClassName }).ToList();
            List<Subjects> sub = new List<Subjects>();

            foreach (var item in subject)
            {
                Subjects s = new Subjects();
                s.id = item.Id;
                s.Subjectname = item.SubjectName;
                s.Classname = item.ClassName;
                sub.Add(s);
            }

            StSu.Subjects = sub;

            var students = (from student in db.AspNetStudents
                            where student.ClassID == id && student.AspNetUser.Status!="False"
                            select new { student.AspNetUser.PhoneNumber, student.AspNetUser.Email, student.AspNetUser.UserName, student.AspNetUser.Name, student.AspNetClass.ClassName }).Distinct().ToList();

            StSu.Students = new List<Students>();

            foreach (var item in students)
            {
                var student = new Students();
                student.Name = item.Name;
                student.PhoneNo = item.PhoneNumber;
                student.Email = item.Email;
                student.RollNo = item.UserName;
                student.ClassName = item.ClassName;
                StSu.Students.Add(student);
            }

            if (id == 0)
            {
                var Students = db.AspNetStudents.Where(x => x.AspNetUser.Status != "False" && x.AspNetStudent_Session_class.Any(z=> z.SessionID == SessionID)).Select(x => new
                {
                    x.AspNetUser.Name,
                    x.AspNetUser.Email,
                    x.AspNetUser.PhoneNumber,
                    x.AspNetClass.ClassName,
                    x.AspNetUser.UserName
                }).ToList();

                foreach (var item in Students)
                {
                    var student = new Students();
                    student.Name = item.Name;
                    student.PhoneNo = item.PhoneNumber;
                    student.Email = item.Email;
                    student.RollNo = item.UserName;
                    student.ClassName = item.ClassName;
                    StSu.Students.Add(student);
                }
            }

            return Json(StSu, JsonRequestBehavior.AllowGet);

        }

        public class Students
        {
            public string Name { set; get; }
            public string RollNo { set; get; }
            public string Email { set; get; }
            public string PhoneNo { set; get; }
            public string ClassName { set; get; }
        }

        public class StudentAndSubjects
        {
            public List<Students> Students { set; get; }
            public List<Subjects> Subjects { set; get; }
        }

        public class Subjects
        {
            public int id { set; get; }
            public string Subjectname { set; get; }
            public string Classname { set; get; }
        }

        [HttpGet]
        public JsonResult SubjectsByStudent(string id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var subs = (from sub in db.AspNetStudent_Subject
                        where sub.StudentID == id
                        select new { sub.AspNetSubject.Id, sub.AspNetSubject.SubjectName, sub.AspNetSubject.ClassID }).ToList();


            return Json(subs, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult StudentsBySubject(int id)
        {

            var students = (from student in db.AspNetStudent_Subject
                            where student.SubjectID == id && student.AspNetUser.Status != "False"
                            select new
                            {
                                student.AspNetUser.Id,
                                student.AspNetUser.Email,
                                student.AspNetUser.UserName,
                                student.AspNetUser.Name,
                                student.AspNetUser.PhoneNumber,
                                student.AspNetSubject.AspNetClass.ClassName
                            }).ToList();


            return Json(students, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TeachersByClass(int id)
        {

            var teachers = (from teacher in db.AspNetSubjects
                            where teacher.ClassID == id && teacher.AspNetUser.Status != "False"
                            select new
                            {
                                teacher.AspNetUser.Id,
                                teacher.ClassID,
                                teacher.TeacherID,
                                teacher.AspNetUser.Email,
                                teacher.AspNetUser.PhoneNumber,
                                teacher.AspNetUser.UserName,
                                teacher.AspNetUser.Name,
                                teacher.AspNetClass.ClassName
                            }).Distinct().ToList();


            return Json(teachers, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AllTeachers()
        {

            var teachers = (from teacher in db.AspNetUsers.Where(x => x.Status != "False") join t2 in db.AspNetUsers_Session.Where(s=>s.SessionID == SessionID)
                            on teacher.Id  equals t2.UserID
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

        public ActionResult TeacherDetail(string UserName)
        {
            var TeacherDetail = db.AspNetEmployees.Where(x => x.AspNetUser.UserName == UserName).Select(x => x).FirstOrDefault();
            AspNetUser aspNetUser = db.AspNetUsers.Where(x => x.UserName == UserName).Select(x => x).FirstOrDefault();
            ViewBag.date = Convert.ToDateTime(TeacherDetail.DateAvailable);
            string[] date = TeacherDetail.DateAvailable.Split(' ');
            string[] join = TeacherDetail.JoiningDate.Split(' ');


            ViewBag.TeacherDetail = TeacherDetail;

            return View(aspNetUser);
        }


        public ActionResult EmployeeDetail(string UserName)
        {
            var employee = db.AspNetEmployees.Where(x => x.UserName == UserName).Select(x => x).FirstOrDefault();
            return View(employee);
        }

        public JsonResult AllParents()
        {
            var AllParent = db.AspNetParent_Child.Select(x => new { x.ParentID, x.ChildID }).ToList();

            var ParentList = new List<parent>();

            foreach (var item in AllParent)
            {
                var parent = new parent();
                // var parentuser = db.AspNetUsers.Where(x => x.Id == item.ParentID & x.Status != "False").Select(x => x).FirstOrDefault();
             //   var parentuser = db.AspNetUsers.Where(x => x.Id == item.ParentID & x.Status != "False").Select(x => x).FirstOrDefault();
                var parentuser = (from usr in db.AspNetUsers.Where(x => x.Id == item.ParentID & x.Status != "False") join t2 in db.AspNetUsers_Session.Where(x=>x.SessionID == SessionID) on usr.Id equals t2.UserID select usr).FirstOrDefault();
                try
                {
                    parent.Id = parentuser.Id;
                    parent.Name = parentuser.Name;
                    parent.UserName = parentuser.UserName;
                    parent.Email = parentuser.Email;
                    parent.PhoneNumber = parentuser.PhoneNumber;
                    parent.ChildName = db.AspNetUsers.Where(x => x.Id == item.ChildID).Select(x => x.Name).FirstOrDefault();
                    ParentList.Add(parent);
                }
                catch
                {

                }
            }

            return Json(ParentList, JsonRequestBehavior.AllowGet);
        }

        public class parent
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string UserName { get; set; }
            public string ChildName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }

        }

        [HttpGet]
        public JsonResult StudentClass(string id)
        {


            var subID = db.AspNetStudent_Subject.FirstOrDefault(x => x.StudentID == id);
            var classidcheck = db.AspNetSubjects.FirstOrDefault(x => x.Id == subID.SubjectID);
            var ClassIDscheck = db.AspNetClasses.FirstOrDefault(x => x.Id == classidcheck.ClassID);
            var ClassIDNAMEv = ClassIDscheck.Id;

            return Json(ClassIDNAMEv, JsonRequestBehavior.AllowGet);
        }

        // GET: AspNetUser/Edit/5
        public ActionResult EditStudent(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            var employee = db.AspNetStudents.Where(x => x.StudentID == id).Select(x => x).FirstOrDefault();
            ViewBag.employee = employee;
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(RegisterViewModel model)
        {
            if (1 == 1)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, Name = model.Name, PhoneNumber = Request.Form["cellNo"] };
                var result = await UserManager.CreateAsync(user, model.Password);

                AspNetUser obj = new AspNetUser();
                obj.Name = user.Name;
                obj.UserName = user.UserName;
                obj.Email = user.Email;
                obj.PasswordHash = user.PasswordHash;
                obj.Status = "Active";
                obj.PhoneNumber = Request.Form["cellNo"];

                AspNetEmployee emp = new AspNetEmployee();
                emp.Name = obj.Name;
                emp.UserName = obj.UserName;
                emp.Email = obj.Email;
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
                if(Request.Form["appliedFor"]=="Admin" || Request.Form["appliedFor"] == "Principal")
                {
                    emp.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Directive Staff").Select(x => x.Id).FirstOrDefault();

                }
                if (Request.Form["appliedFor"] == "Supervisor")
                {
                    emp.VirtualRoleId = db.AspNetVirtualRoles.Where(x => x.Name == "Management Staff").Select(x => x.Id).FirstOrDefault();
                }
                emp.UserId = user.Id;
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, Request.Form["appliedFor"]);

                    db.AspNetEmployees.Add(emp);
                    db.SaveChanges();
                    TempData["sMessage"] = "Employee successfully saved.";
                    return RedirectToAction("Index", "AspNetEmployees");
                }
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public JsonResult GetUserName(string userName)
        {
            check Check = new check();
            Check.count = 0;
            try
            {
                var user = db.AspNetUsers.Where(x => x.UserName == userName);
                if (user.Count() > 0)
                {
                    Check.count = 1;
                    Check.by = user.Select(x => x.AspNetRoles.Select(y => y.Name).FirstOrDefault()).FirstOrDefault();
                }
                else
                {
                    Check.count = 0;
                }
            }
            catch
            {
                Check.count = 0;
            }

            return Json(Check, JsonRequestBehavior.AllowGet);
        }

        public class check
        {
            public int count { get; set; }
            public string by { get; set; }
        }

        public JsonResult Email(string Email)
        {
            int count;
            try
            {
                var user = db.AspNetUsers.Where(x => x.Email == Email);
                if (user.Count() > 0)
                {
                    count = 1;
                }
                else
                {
                    count = 0;
                }
            }
            catch
            {
                count = 0;
            }

            return Json(count, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudent([Bind(Include = "Id,Email,PasswordHash,SecurityStamp,PhoneNumber,UserName,Name")] AspNetUser aspNetUser)
        {
            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                if (ModelState.IsValid)
                {
                    ApplicationDbContext context = new ApplicationDbContext();
                    IEnumerable<string> selectedsubjects = Request.Form["subjects"].Split(',');
                    if(selectedsubjects==null)
                    {
                        ViewBag.SubError = "Please select subjects";
                    }

                    string selectedClass = Request.Form["ClassID"];
                   
                    AspNetStudent_Subject stu_sub_rem = new AspNetStudent_Subject();
                    do
                    {
                        stu_sub_rem = db.AspNetStudent_Subject.FirstOrDefault(x => x.StudentID == aspNetUser.Id);
                        try
                        {
                            db.AspNetStudent_Subject.Remove(stu_sub_rem);
                            var student = db.AspNetStudents.Where(x => x.StudentID == aspNetUser.Id).Select(x => x).FirstOrDefault();
                            student.ClassID = Convert.ToInt32(selectedClass);
                            student.Nationality = Request.Form["Nationality"];
                            student.BirthDate = Request.Form["BirthDate"];
                            student.Religion = Request.Form["Religion"];
                            student.Gender = Request.Form["Gender"];
                            student.SchoolName = Request.Form["SchoolName"];

                            db.SaveChanges();
                        }
                        catch
                        {

                        }
                    }
                    while (stu_sub_rem != null);


                    foreach (var item in selectedsubjects)
                    {
                        AspNetStudent_Subject stu_sub = new AspNetStudent_Subject();
                        stu_sub.StudentID = aspNetUser.Id;
                        stu_sub.SubjectID = Convert.ToInt32(item);
                        db.AspNetStudent_Subject.Add(stu_sub);
                        db.SaveChanges();
                    }

                    db.Entry(aspNetUser).State = EntityState.Modified;
                    db.SaveChanges();
                }
                dbTransaction.Commit();
                return RedirectToAction("StudentsIndex");
            }
            catch (Exception) { dbTransaction.Dispose(); }

            return View("StudentsIndex");
        }
   
        public ActionResult StudentDetail(string userName)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Where(x => x.UserName == userName).Select(x => x).FirstOrDefault();
            var employ = db.AspNetStudents.Where(x => x.StudentID == aspNetUser.Id).Select(x => x).FirstOrDefault();


            AspNetStudent employee = new AspNetStudent();
            employee.Id = employ.Id;
            employee.StudentID = employ.StudentID;
            employee.ClassID = employ.ClassID;
            employee.Level = employ.Level;
            employee.SchoolName = employ.SchoolName;
            
            var niio = DateTime.Now;
            var noo = niio.ToString();
            var day = niio.Day;
            var mo = niio.Month;
            var yea = niio.Year;

            string datooo = "";
            var empl = employ.BirthDate;//.Replace("/", "-");
            var tyuio = empl.Replace("/", "-");
            var diib = employ.BirthDate.Split('-');
            if(diib[0].Length == 4)
            {
                datooo = employ.BirthDate;
            }
            else
            {
                var dab = employ.BirthDate.Split('/');
                datooo = dab[2] + "-" + dab[1] + "-" + dab[0];

                db.AspNetStudents.Where(x => x.StudentID == aspNetUser.Id).Select(x => x).FirstOrDefault().BirthDate = datooo;
                db.SaveChanges();
            }

            //var yu = employ.BirthDate+ " 0:00:17 AM";
            //var date = DateTime.ParseExact(noo, "yy/MM/dd h:mm:ss tt", CultureInfo.InvariantCulture);

            //DateTime result = DateTime.ParseExact(noo, "yy/MM/dd h:mm:ss tt", CultureInfo.InvariantCulture);


            employee.BirthDate = datooo;
            employee.Nationality = employ.Nationality;
            employee.Religion = employ.Religion;
            employee.Gender = employ.Gender;

            employee.PhoneNumber = employ.PhoneNumber;


            ViewBag.employee = employee;

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.Detail = "data";

            return View(aspNetUser);
        }

        /**********************************************************************************************************************************************/
        // GET: AspNetUser
        public ViewResult StudentsIndex()
        {
            //var students = db.AspNetStudents.ToList();
            //foreach (var st in students)
            //{
            //    var iopd = st.StudentID;
            //    var user = db.AspNetUsers.Where(p => p.Id == iopd).FirstOrDefault();
            //    if (user != null)
            //    {
            //        user.PhoneNumber = st.PhoneNumber;
            //        //user.bi
            //        db.SaveChanges();
            //    }
            //    else
            //    {
            //        ruffdata oo = new ruffdata();
            //        oo.StudentPassword = st.StudentID;
            //        oo.StudentName = db.AspNetClasses.Where(p => p.Id == st.ClassID).FirstOrDefault().ClassName;

            //        db.ruffdatas.Add(oo);
            //        db.SaveChanges();

            //    }
            //}


           // ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            return View();
        }

        public JsonResult AllStudents()
        {

            
           
            var students =(from std in  db.AspNetStudents.Where(x => x.AspNetUser.Status != "False") join t2 in
                               db.AspNetStudent_Session_class.Where(x => x.SessionID == SessionID) on std.Id equals t2.StudentID
                          select new {
                              
                std.AspNetUser.Name,
                std.AspNetUser.Email,
                std.AspNetUser.PhoneNumber,
                std.AspNetClass.ClassName,
                std.AspNetUser.UserName
            }).ToList();

            return Json(students, JsonRequestBehavior.AllowGet);

        }


        public ViewResult StudentIndex(string Error)
        {
           // ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            ViewBag.Error = Error;
            return View("StudentsIndex");
        }

        public ViewResult DisableStudentIndex(string Error)
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.Error = Error;
            return View();
        }

        public JsonResult AllDisableStudents()
        {
            var students = db.AspNetStudents.Where(x => x.AspNetUser.Status == "False").Select(x => new
            {
                x.AspNetUser.Name,
                x.AspNetUser.Email,
                x.AspNetUser.PhoneNumber,
                x.AspNetClass.ClassName,
                x.AspNetUser.UserName
            }).ToList();

            return Json(students, JsonRequestBehavior.AllowGet);

        }


        public ViewResult AccountantIndex()
        {
            ViewBag.data = "Accountant";
            //  db.AspNetEmployees.Where(x => x.Aspnet_Employee_Session.Select(y => y.Session_Id.ToString()).Contains(GetSessionID)).ToList();
            //var data =  db.GetAccountantListingData("17").ToList(); 
            var data2 = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Accountant") && x.Status != "False").ToList();
            var rr =   db.GetAccountantListingData(SessionID.ToString()).ToList();
            List<GetAccountantListingData_Result> list = new List<GetAccountantListingData_Result>();
            
            return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Accountant") && x.Status != "False").ToList());
        }

        public ActionResult GetAccountantList()
        {
            string Status = "error";
            var data2 = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Accountant") && x.Status != "False").ToList();
            var rr = db.GetAccountantListingData(SessionID.ToString()).ToList();
            List<GetAccountantListingData_Result> list = new List<GetAccountantListingData_Result>();
            list = db.GetAccountantListingData(SessionID.ToString()).ToList();
            Status = JsonConvert.SerializeObject(list);

            return Content(Status);
        }

        public ViewResult DisableAccountantIndex()
        {
            ViewBag.data = "Accountant";
            return View(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Accountant") && x.Status == "False").ToList());
        }

        public ViewResult AccountantDetail(string UserName)
        {
            var User = db.AspNetUsers.Where(x => x.UserName == UserName).Select(x => x).FirstOrDefault();
            var Detail = db.AspNetEmployees.Where(x => x.AspNetUser.UserName == UserName).Select(x => x).FirstOrDefault();

            ViewBag.User = User;
            ViewBag.Detail = Detail;
            return View(db.AspNetUsers.Where(x => x.UserName == UserName).FirstOrDefault());
        }

        public ViewResult AccountantEdit(string id)
        {
            var User = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            var Detail = db.AspNetEmployees.Where(x => x.UserId == id).Select(x => x).FirstOrDefault();

            ViewBag.User = User;
            ViewBag.Detail = Detail;

            return View(db.AspNetUsers.Where(x => x.Id == id).FirstOrDefault());
        }

        public ViewResult AccountantsIndex(string Error)
        {
            ViewBag.data = "Accountant";
            ViewBag.Error = Error;
            return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Accountant")).ToList());
        }

        public ViewResult ParentDetail(string UserName)
        {
            var parent = db.AspNetParents.Where(x => x.AspNetUser.UserName == UserName).Select(x => x).FirstOrDefault();
            AspNetUser aspNetUser = db.AspNetUsers.Where(x => x.UserName == UserName).Select(x => x).FirstOrDefault();
            ViewBag.Id = aspNetUser.Id;
            ViewBag.Parent = parent;
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            var childs = db.AspNetParent_Child.Where(x => x.ParentID == parent.UserID).Select(x => x).ToList();
            List<int> Classes = new List<int>();
            foreach (var item in childs)
            {
                var subjects = db.AspNetStudent_Subject.Where(x => x.StudentID == item.ChildID).FirstOrDefault();

                var classId = (int)db.AspNetSubjects.Where(x => x.Id == subjects.SubjectID).Select(x => x.ClassID).FirstOrDefault();
                Classes.Add(classId);
            }

            var Children = db.AspNetParent_Child.Where(x => x.ParentID == parent.UserID).Select(x => x.ChildID).ToList();

            ViewBag.Classes = Classes;
            ViewBag.Children = Children;
            return View(aspNetUser);
        }

        public ViewResult ParentIndex()
        {
          //  ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            ViewBag.data = "Parent";

            return View(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Parent") && x.Status != "False").ToList());
        }

        public void EmployeeExcelRecord()
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetEmployee> EmployeeList;

            EmployeeList = db.AspNetEmployees.Select(x => x).ToList();

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Email";
            ws.Cells["B1"].Value = "Name";
            ws.Cells["C1"].Value = "User Name";
            ws.Cells["D1"].Value = "Position Applied";
            ws.Cells["E1"].Value = "Date Available";
            ws.Cells["F1"].Value = "joining Date";
            ws.Cells["G1"].Value = "Birth Date";
            ws.Cells["H1"].Value = "Nationality";
            ws.Cells["I1"].Value = "Religion";
            ws.Cells["J1"].Value = "Gender";
            ws.Cells["K1"].Value = "Mailing Address";
            ws.Cells["L1"].Value = "Cell No";
            ws.Cells["M1"].Value = "LandLine";
            ws.Cells["N1"].Value = "Name Of Spouse";
            ws.Cells["O1"].Value = "Highest Degree";
            ws.Cells["P1"].Value = "Occupation";
            ws.Cells["Q1"].Value = "Business Address";
            ws.Cells["R1"].Value = "Illness";
            ws.Cells["S1"].Value = "Gross Salary";
            ws.Cells["T1"].Value = "Basic Salary";
            ws.Cells["U1"].Value = "Medical Allowance";
            ws.Cells["V1"].Value = "Acomodation";
            ws.Cells["W1"].Value = "Provided Fund";
            ws.Cells["X1"].Value = "Tax";
            ws.Cells["Y1"].Value = "EOP";

            int rowStart = 2;
            foreach (var item in EmployeeList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.Email;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Name;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.UserName;
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.PositionAppliedFor;
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.DateAvailable;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.JoiningDate;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.BirthDate;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.Nationality;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.Religion;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.Gender;
                ws.Cells[string.Format("K{0}", rowStart)].Value = item.MailingAddress;
                ws.Cells[string.Format("L{0}", rowStart)].Value = item.CellNo;
                ws.Cells[string.Format("M{0}", rowStart)].Value = item.Landline;
                ws.Cells[string.Format("N{0}", rowStart)].Value = item.SpouseName;
                ws.Cells[string.Format("O{0}", rowStart)].Value = item.SpouseHighestDegree;
                ws.Cells[string.Format("P{0}", rowStart)].Value = item.SpouseOccupation;
                ws.Cells[string.Format("Q{0}", rowStart)].Value = item.SpouseBusinessAddress;
                ws.Cells[string.Format("R{0}", rowStart)].Value = item.Illness;
                ws.Cells[string.Format("S{0}", rowStart)].Value = item.GrossSalary;
                ws.Cells[string.Format("T{0}", rowStart)].Value = item.BasicSalary;
                ws.Cells[string.Format("U{0}", rowStart)].Value = item.MedicalAllowance;
                ws.Cells[string.Format("V{0}", rowStart)].Value = item.Accomodation;
                ws.Cells[string.Format("W{0}", rowStart)].Value = item.ProvidedFund;
                ws.Cells[string.Format("X{0}", rowStart)].Value = item.Tax;
                ws.Cells[string.Format("Y{0}", rowStart)].Value = item.EOP;

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

        public void AccountantExcelRecord()
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetEmployee> AccountantList;

            AccountantList = db.AspNetEmployees.Where(x => x.AspNetVirtualRole.Name == "Management Staff").Select(x => x).ToList();

            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Email";
            ws.Cells["B1"].Value = "Name";
            ws.Cells["C1"].Value = "User Name";
            ws.Cells["D1"].Value = "Password";
            ws.Cells["E1"].Value = "Confirm Password";
            ws.Cells["F1"].Value = "Position Applied";
            ws.Cells["G1"].Value = "Date Available";
            ws.Cells["H1"].Value = "joining Date";
            ws.Cells["I1"].Value = "Birth Date";
            ws.Cells["J1"].Value = "Nationality";
            ws.Cells["K1"].Value = "Religion";
            ws.Cells["L1"].Value = "Gender";
            ws.Cells["M1"].Value = "Mailing Address";
            ws.Cells["N1"].Value = "Cell No";
            ws.Cells["O1"].Value = "LandLine";
            ws.Cells["P1"].Value = "Name Of Spouse";
            ws.Cells["Q1"].Value = "Highest Degree";
            ws.Cells["R1"].Value = "Occupation";
            ws.Cells["S1"].Value = "Business Address";
            ws.Cells["T1"].Value = "Illness";
            ws.Cells["U1"].Value = "Gross Salary";
            ws.Cells["V1"].Value = "Basic Salary";
            ws.Cells["W1"].Value = "Medical Allowance";
            ws.Cells["X1"].Value = "Acomodation";
            ws.Cells["Y1"].Value = "Provided Fund";
            ws.Cells["Z1"].Value = "Tax";
            ws.Cells["AA1"].Value = "EOP";

            int rowStart = 2;
            foreach (var item in AccountantList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.Email;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Name;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.UserName;
                ws.Cells[string.Format("D{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("E{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.PositionAppliedFor;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.DateAvailable;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.JoiningDate;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.BirthDate;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.Nationality;
                ws.Cells[string.Format("K{0}", rowStart)].Value = item.Religion;
                ws.Cells[string.Format("L{0}", rowStart)].Value = item.Gender;
                ws.Cells[string.Format("M{0}", rowStart)].Value = item.MailingAddress;
                ws.Cells[string.Format("N{0}", rowStart)].Value = item.CellNo;
                ws.Cells[string.Format("O{0}", rowStart)].Value = item.Landline;
                ws.Cells[string.Format("P{0}", rowStart)].Value = item.SpouseName;
                ws.Cells[string.Format("Q{0}", rowStart)].Value = item.SpouseHighestDegree;
                ws.Cells[string.Format("R{0}", rowStart)].Value = item.SpouseOccupation;
                ws.Cells[string.Format("S{0}", rowStart)].Value = item.SpouseBusinessAddress;
                ws.Cells[string.Format("T{0}", rowStart)].Value = item.Illness;
                ws.Cells[string.Format("U{0}", rowStart)].Value = item.GrossSalary;
                ws.Cells[string.Format("V{0}", rowStart)].Value = item.BasicSalary;
                ws.Cells[string.Format("W{0}", rowStart)].Value = item.MedicalAllowance;
                ws.Cells[string.Format("X{0}", rowStart)].Value = item.Accomodation;
                ws.Cells[string.Format("Y{0}", rowStart)].Value = item.ProvidedFund;
                ws.Cells[string.Format("Z{0}", rowStart)].Value = item.Tax;
                ws.Cells[string.Format("AA{0}", rowStart)].Value = item.EOP;

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

        public ViewResult DisableParentIndex()
        {
            ViewBag.data = "Parent";

            return View(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Parent") && x.Status == "False").ToList());
        }

        //public ViewResult ParentDetails(string userName)
        //{
        //    var parent = db.AspNetParents.Where(x => x.UserID == userName).Select(x => x).FirstOrDefault();
        //    AspNetUser aspNetUser = db.AspNetUsers.Where(x=>x.UserName==userName).Select(x => x).FirstOrDefault();
        //    ViewBag.Id = aspNetUser.Id;
        //    ViewBag.Parent = parent;
        //    ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

        //    var childs = db.AspNetParent_Child.Where(x => x.ParentID == userName).Select(x => x).ToList();
        //    List<int> Classes = new List<int>();
        //    foreach (var item in childs)
        //    {
        //        var subjects = db.AspNetStudent_Subject.Where(x => x.StudentID == userName).FirstOrDefault();

        //        var classId = (int)db.AspNetSubjects.Where(x => x.Id == subjects.SubjectID).Select(x => x.ClassID).FirstOrDefault();
        //        Classes.Add(classId);
        //    }

        //    var Children = db.AspNetParent_Child.Where(x => x.ParentID == userName).Select(x => x.ChildID).ToList();

        //    ViewBag.Classes = Classes;
        //    ViewBag.Children = Children;
        //    return View(aspNetUser);
        //}

        public ViewResult ParentsIndex(string Error)
        {
            ViewBag.data = "Parent";
            ViewBag.Error = Error;
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            //return View();

            return View("ParentIndex", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Parent") && x.Status != "False").ToList());
        }

        public void ParentExcelRecord(int ClassId)
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetParent> parentLIst;

            if (ClassId == 0)
            {

                parentLIst = db.AspNetParents.ToList();
            }
            else
            {
                int classID = ClassId;

                var parents = (from parent in db.AspNetParent_Child
                               join student in db.AspNetStudents on parent.ChildID equals student.StudentID
                               where student.ClassID == classID
                               select new { parent.AspNetUser1.Id }).Distinct().ToList();

                parentLIst = new List<AspNetParent>();
                foreach (var item in parents)
                {
                    AspNetParent Parent = db.AspNetParents.Where(x => x.UserID == item.Id).FirstOrDefault();
                    parentLIst.Add(Parent);
                }
            }
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Email";
            ws.Cells["B1"].Value = "Name";
            ws.Cells["C1"].Value = "UserName";
            ws.Cells["D1"].Value = "Father Name";
            ws.Cells["E1"].Value = "Father Cell No";
            ws.Cells["F1"].Value = "Email";
            ws.Cells["G1"].Value = "Occupation";
            ws.Cells["H1"].Value = "Employeer";
            ws.Cells["I1"].Value = "Mother Name";
            ws.Cells["J1"].Value = "Cell No";
            ws.Cells["K1"].Value = "Email";       
            ws.Cells["L1"].Value = "Mother Occupation";
            ws.Cells["M1"].Value = "Mother Employer";
            ws.Cells["N1"].Value = "Child1 ID";
            ws.Cells["O1"].Value = "Child2 ID";
            ws.Cells["P1"].Value = "Child3 ID";
            ws.Cells["Q1"].Value = "Child4 ID";


            int rowStart = 2;
            foreach (var item in parentLIst)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.AspNetUser.Email;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.AspNetUser.Name;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.AspNetUser.UserName;          
                ws.Cells[string.Format("D{0}", rowStart)].Value = item.FatherName;
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.FatherCellNo;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.FatherEmail;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.FatherOccupation;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.FatherEmployer;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.MotherName;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.MotherCellNo;
                ws.Cells[string.Format("K{0}", rowStart)].Value = item.MotherEmail;
                ws.Cells[string.Format("L{0}", rowStart)].Value = item.MotherOccupation;
                ws.Cells[string.Format("M{0}", rowStart)].Value = item.MotherEmployer;
                ws.Cells[string.Format("N{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("O{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("P{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("Q{0}", rowStart)].Value = "-";
                var child = db.AspNetParent_Child.Where(x => x.ParentID==item.UserID).Select(x => x.AspNetUser.UserName).ToList();
                for(int i=0;i<child.Count;i++)
                {
                    if(i==0)
                    ws.Cells[string.Format("N{0}", rowStart)].Value = child[i];
                    if(i==1)
                    ws.Cells[string.Format("O{0}", rowStart)].Value = child[i];
                    if(i==2)
                    ws.Cells[string.Format("P{0}", rowStart)].Value = child[i];
                    if(i==3)
                    ws.Cells[string.Format("Q{0}", rowStart)].Value = child[i];
                }
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


        public ViewResult TeachersIndex()
        {
            //ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x => x.SessionID == SessionID), "Id", "ClassName");
            
            return View();
        }

        public ViewResult TeacherIndex(string Error)
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses.Where(x=>x.SessionID == SessionID), "Id", "ClassName");
            ViewBag.Error = Error;
            return View("TeachersIndex");
        }

        public void StudentExcelRecord(string ClassId)
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetStudent> studentList;

            if (ClassId == "0")
            {

                studentList = db.AspNetStudents.Select(x => x).ToList();
            }
            else
            {
                int classID = int.Parse(ClassId);
                var students = (from student in db.AspNetStudents
                                join Subjects in db.AspNetStudents on student.Id equals Subjects.Id
                                where Subjects.ClassID == classID
                                select new { student.AspNetUser.Id }).Distinct().ToList();

                studentList = new List<AspNetStudent>();
                foreach (var item in students)
                {
                    AspNetStudent Student = db.AspNetStudents.Where(x => x.StudentID == item.Id).FirstOrDefault();
                    studentList.Add(Student);
                }
            }
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");


            ws.Cells["A1"].Value = "Name";
            ws.Cells["B1"].Value = "User Name";
            ws.Cells["C1"].Value = "Password";
            ws.Cells["D1"].Value = "Confirm Password";
            ws.Cells["E1"].Value = "Class";
            ws.Cells["F1"].Value = "Subject1";
            ws.Cells["G1"].Value = "Subject2";
            ws.Cells["H1"].Value = "Subject3";
            ws.Cells["I1"].Value = "Subject4";
            ws.Cells["J1"].Value = "Subject5";
            ws.Cells["K1"].Value = "Subject6";
            ws.Cells["L1"].Value = "Subject7";
            ws.Cells["M1"].Value = "Subject8";
            ws.Cells["N1"].Value = "Subject9";
            ws.Cells["O1"].Value = "Subject10";
            ws.Cells["P1"].Value = "School Name";
            ws.Cells["Q1"].Value = "BirhtDay";
            ws.Cells["R1"].Value = "Nationality";
            ws.Cells["S1"].Value = "Religion";
            ws.Cells["T1"].Value = "Gender";
            ws.Cells["U1"].Value = "Phone Number";

            int rowStart = 2;
            foreach (var item in studentList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.AspNetUser.Name;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.AspNetUser.UserName;
                ws.Cells[string.Format("C{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("D{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.AspNetClass.Class;

                var Subjects = db.AspNetStudent_Subject.Where(x => x.StudentID == item.StudentID).Select(x => x.AspNetSubject.SubjectName).ToList();
                ws.Cells[string.Format("F{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("G{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("H{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("I{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("J{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("K{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("L{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("M{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("N{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("O{0}", rowStart)].Value = "-";

                for (int i = 0; i < Subjects.Count; i++)
                {
                    if (i == 0)
                        ws.Cells[string.Format("F{0}", rowStart)].Value = Subjects[i];
                    if (i == 1)
                        ws.Cells[string.Format("G{0}", rowStart)].Value = Subjects[i];
                    if (i == 2)
                        ws.Cells[string.Format("H{0}", rowStart)].Value = Subjects[i];
                    if (i == 3)
                        ws.Cells[string.Format("I{0}", rowStart)].Value = Subjects[i];
                    if (i == 4)
                        ws.Cells[string.Format("J{0}", rowStart)].Value = Subjects[i];
                    if (i == 5)
                        ws.Cells[string.Format("K{0}", rowStart)].Value = Subjects[i];
                    if (i == 6)
                        ws.Cells[string.Format("L{0}", rowStart)].Value = Subjects[i];
                    if (i == 7)
                        ws.Cells[string.Format("M{0}", rowStart)].Value = Subjects[i];
                    if (i == 8)
                        ws.Cells[string.Format("N{0}", rowStart)].Value = Subjects[i];
                    if (i == 9)
                        ws.Cells[string.Format("O{0}", rowStart)].Value = Subjects[i];

                }
                ws.Cells[string.Format("P{0}", rowStart)].Value = item.SchoolName;
                ws.Cells[string.Format("Q{0}", rowStart)].Value = item.BirthDate;
                ws.Cells[string.Format("R{0}", rowStart)].Value = item.Nationality;
                ws.Cells[string.Format("S{0}", rowStart)].Value = item.Religion;
                ws.Cells[string.Format("T{0}", rowStart)].Value = item.Gender;
                ws.Cells[string.Format("U{0}", rowStart)].Value = item.AspNetUser.PhoneNumber;



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

        public void TeacherExcelRecord(string ClassId)
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();
            List<AspNetEmployee> teacherList;

            if (ClassId == "ALL")
            {

                teacherList = db.AspNetEmployees.Where(x => x.AspNetVirtualRole.Name == "Teaching Staff").Select(x => x).ToList();
            }
            else
            {
                int classID = int.Parse(ClassId);
                var teachers = (from teacher in db.AspNetSubjects
                                where teacher.ClassID == classID && teacher.AspNetUser.Status != "False"
                                select new
                                {
                                    teacher.AspNetUser.Id,
                                    teacher.ClassID,
                                    teacher.TeacherID,
                                    teacher.AspNetUser.Email,
                                    teacher.AspNetUser.PhoneNumber,
                                    teacher.AspNetUser.UserName,
                                    teacher.AspNetUser.Name,
                                    teacher.AspNetClass.ClassName
                                }).Distinct().ToList();

                teacherList = new List<AspNetEmployee>();
                foreach (var item in teachers)
                {
                    AspNetEmployee Employee = db.AspNetEmployees.Where(x => x.UserId == item.Id).FirstOrDefault();
                    teacherList.Add(Employee);
                }
            }
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");


            ws.Cells["A1"].Value = "Email";
            ws.Cells["B1"].Value = "Name";
            ws.Cells["C1"].Value = "User Name";
            ws.Cells["D1"].Value = "Password";
            ws.Cells["E1"].Value = "Confirm Password";
            ws.Cells["F1"].Value = "Position Applied";
            ws.Cells["G1"].Value = "Date Available";
            ws.Cells["H1"].Value = "joining Date";
            ws.Cells["I1"].Value = "Birth Date";
            ws.Cells["J1"].Value = "Nationality";
            ws.Cells["K1"].Value = "Religion";
            ws.Cells["L1"].Value = "Gender";
            ws.Cells["M1"].Value = "Mailing Address";
            ws.Cells["N1"].Value = "Cell No";
            ws.Cells["O1"].Value = "LandLine";
            ws.Cells["P1"].Value = "Name Of Spouse";
            ws.Cells["Q1"].Value = "Highest Degree";
            ws.Cells["R1"].Value = "Occupation";
            ws.Cells["S1"].Value = "Business Address";
            ws.Cells["T1"].Value = "Illness";
            ws.Cells["U1"].Value = "Gross Salary";
            ws.Cells["V1"].Value = "Basic Salary";
            ws.Cells["W1"].Value = "Medical Allowance";
            ws.Cells["X1"].Value = "Acomodation";
            ws.Cells["Y1"].Value = "Provided Fund";
            ws.Cells["Z1"].Value = "Tax";
            ws.Cells["AA1"].Value = "EOP";

            int rowStart = 2;
            foreach (var item in teacherList)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.Email;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.Name;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.UserName;
                ws.Cells[string.Format("D{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("E{0}", rowStart)].Value = "-";
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.PositionAppliedFor;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.DateAvailable;
                ws.Cells[string.Format("H{0}", rowStart)].Value = item.JoiningDate;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.BirthDate;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.Nationality;
                ws.Cells[string.Format("K{0}", rowStart)].Value = item.Religion;
                ws.Cells[string.Format("L{0}", rowStart)].Value = item.Gender;
                ws.Cells[string.Format("M{0}", rowStart)].Value = item.MailingAddress;
                ws.Cells[string.Format("N{0}", rowStart)].Value = item.CellNo;
                ws.Cells[string.Format("O{0}", rowStart)].Value = item.Landline;
                ws.Cells[string.Format("P{0}", rowStart)].Value = item.SpouseName;
                ws.Cells[string.Format("Q{0}", rowStart)].Value = item.SpouseHighestDegree;
                ws.Cells[string.Format("R{0}", rowStart)].Value = item.SpouseOccupation;
                ws.Cells[string.Format("S{0}", rowStart)].Value = item.SpouseBusinessAddress;
                ws.Cells[string.Format("T{0}", rowStart)].Value = item.Illness;
                ws.Cells[string.Format("U{0}", rowStart)].Value = item.GrossSalary;
                ws.Cells[string.Format("V{0}", rowStart)].Value = item.BasicSalary;
                ws.Cells[string.Format("W{0}", rowStart)].Value = item.MedicalAllowance;
                ws.Cells[string.Format("X{0}", rowStart)].Value = item.Accomodation;
                ws.Cells[string.Format("Y{0}", rowStart)].Value = item.ProvidedFund;
                ws.Cells[string.Format("Z{0}", rowStart)].Value = item.Tax;
                ws.Cells[string.Format("AA{0}", rowStart)].Value = item.EOP;

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

        // GET: AspNetUser/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // GET: AspNetUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,Name")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.AspNetUsers.Add(aspNetUser);
                db.SaveChanges();
                return RedirectToAction("Index");

            }

            return View(aspNetUser);
        }

        public ActionResult ParentEdit(string id)
        {
            var parent = db.AspNetParents.Where(x => x.UserID == id).Select(x => x).FirstOrDefault();
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            ViewBag.Id = aspNetUser.Id;
            ViewBag.Parent = parent;
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            var childs = db.AspNetParent_Child.Where(x => x.ParentID == id).Select(x => x.ChildID).ToList();
            List<int> Classes = new List<int>();
            foreach (var item in childs)
            {
                var subjects = db.AspNetStudent_Subject.Where(x => x.StudentID == item).FirstOrDefault();

                var classId = (int)db.AspNetSubjects.Where(x => x.Id == subjects.SubjectID).Select(x => x.ClassID).FirstOrDefault();
                Classes.Add(classId);
            }

            var Children = db.AspNetParent_Child.Where(x => x.ParentID == id).Select(x => x.ChildID).ToList();

            ViewBag.Classes = Classes;
            ViewBag.Children = Children;
            return View(aspNetUser);
        }

        public JsonResult CheckPassword(string Password, string id)
        {
            //int check = 0;

            ApplicationDbContext context = new ApplicationDbContext();

            var userStore = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(userStore);
            UserManager.PasswordHasher = new PasswordHasher();

            string hash = UserManager.PasswordHasher.HashPassword(Password);

            return Json(hash, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsHeadTeacher(int ClassID)
        {
            string count = "No";
            try
            {
                var headTeacher = db.AspNetClasses.Where(x => x.Id == ClassID).Select(x => x.TeacherID).FirstOrDefault();
                string TeacherID = Convert.ToString(System.Web.HttpContext.Current.Session["TeacherID"]);

                if (TeacherID == headTeacher)
                {
                    count = "yes";
                }
                return Json(count, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                return Json(count, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditTeacher(string id)
        {
            var TeacherDetail = db.AspNetEmployees.Where(x => x.UserId == id).Select(x => x).FirstOrDefault();
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            ViewBag.date = Convert.ToDateTime(TeacherDetail.DateAvailable);
            string[] date = TeacherDetail.DateAvailable.Split(' ');
            string[] join = TeacherDetail.JoiningDate.Split(' ');

            ViewBag.TeacherDetail = TeacherDetail;

            return View(aspNetUser);
        }
        // GET: AspNetUser/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);

            ViewBag.Parent = db.AspNetParents.Where(x => x.UserID == id).ToList();

            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,Name")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetUser);
        }

        // GET: AspNetUser/Delete/5
        public ActionResult Delete(string id, string type)
        {
            AspNetUser User = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();

            try
            {
              
             
                if (type != "Student" && type != "Parent")
                {
                    var employee = db.AspNetEmployees.Where(x => x.UserId == User.Id).Select(x => x).FirstOrDefault();
                    employee.Status = "False";
                }

                User.Status = "False";
                db.SaveChanges();

                if (type == "Student")
                {
                    return RedirectToAction("StudentsIndex");
                }
                else if (type == "Teacher")
                {
                    return RedirectToAction("teachersIndex");
                }
                else if (type == "Accountant")
                {
                    return RedirectToAction("AccountantIndex");
                }
                else if (type == "Parent")
                {
                    return RedirectToAction("ParentIndex");
                }
                else
                {
                    return RedirectToAction("ParentIndex");
                }
            }
            catch(Exception e)
            {
                if(type=="Student")
                {
                    ModelState.AddModelError("", e.Message);
                    return RedirectToAction("StudentDetail", new { userName = User.UserName });
                }
                else if(type=="Teacher")
                {
                    ModelState.AddModelError("", e.Message);
                    return RedirectToAction("TeacherDetail", new { userName = User.UserName });
                }
                else if (type == "Accountant")
                {
                    ModelState.AddModelError("", e.Message);
                    return RedirectToAction("AccountantDetail", new { userName = User.UserName });
                }
                else if (type == "Parent")
                {
                    ModelState.AddModelError("", e.Message);
                    return RedirectToAction("ParentDetail", new { userName = User.UserName });
                }
                else
                {
                    ModelState.AddModelError("", e.Message);
                    return RedirectToAction("EmployeeDetail", new { userName = User.UserName });
                }
            }

        }

        public ActionResult Enable(string id, string type)
        {
            AspNetUser User = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            User.Status = "True";
            if (type != "Student" && type != "Parent")
            {
                var employee = db.AspNetEmployees.Where(x => x.UserId == User.Id).Select(x => x).FirstOrDefault();
                employee.Status = "True";
            }

            db.SaveChanges();

            if (type == "Student")
            {
                return RedirectToAction("StudentsIndex");
            }
            else if (type == "Teacher")
            {
                return RedirectToAction("teachersIndex");
            }
            else if (type == "Accountant")
            {
                return RedirectToAction("AccountantIndex");
            }
            else if (type == "Parent")
            {
                return RedirectToAction("ParentIndex");
            }
            else
            {
                return RedirectToAction("ParentIndex");
            }
        }

        // POST: AspNetUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUser User = db.AspNetUsers.Where(x => x.Id == id).Select(x => x).FirstOrDefault();
            User.Status = "false";
            db.SaveChanges();
            return RedirectToAction("StudentsIndex");
        }


        // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //

        public bool usernameExist(string username)
        {
            bool check;
            try
            {
                var user = db.AspNetUsers.Where(x => x.UserName == username);
                if (user != null)
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
            }
            catch
            {
                check = false;
            }
            return check;
        }

        public JsonResult StudentSearch(int Searchtype, string searchdata)
        {
            List<userdata> UserList = new List<userdata>();
            string userType = "Student";
            try
            {
                if (Searchtype == 1) // roll no.
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Id == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    userdata.Class = user.AspNetStudents.Select(x => x.AspNetClass.ClassName).FirstOrDefault();
                    UserList.Add(userdata);
                }
                else if (Searchtype == 2) // username
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    userdata.Class = user.AspNetStudents.Select(x => x.AspNetClass.ClassName).FirstOrDefault();
                    UserList.Add(userdata);
                }
                else if (Searchtype == 3) // name
                {
                    var users = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Name.Contains(searchdata)) && x.Status != "False").ToList();
                    foreach (var item in users)
                    {
                        var userdata = new userdata();
                        userdata.Id = item.Id;
                        userdata.Email = item.Email;
                        userdata.Name = item.Name;
                        userdata.UserName = item.UserName;
                        userdata.PhoneNumber = item.PhoneNumber;
                        userdata.Class = item.AspNetStudents.Select(x => x.AspNetClass.ClassName).FirstOrDefault();
                        UserList.Add(userdata);
                    }
                }
            }//try 
            catch
            {

            }


            return Json(UserList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AccountantSearch(int Searchtype, string searchdata)
        {
            List<userdata> UserList = new List<userdata>();
            string userType = "Accountant";
            try
            {
                if (Searchtype == 1) // roll no.
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Id == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    UserList.Add(userdata);
                }
                else if (Searchtype == 2) // username
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    UserList.Add(userdata);
                }
                else if (Searchtype == 3) // name
                {
                    var users = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Name.Contains(searchdata)) && x.Status != "False").ToList();
                    foreach (var item in users)
                    {
                        var userdata = new userdata();
                        userdata.Id = item.Id;
                        userdata.Email = item.Email;
                        userdata.Name = item.Name;
                        userdata.UserName = item.UserName;
                        userdata.PhoneNumber = item.PhoneNumber;
                        UserList.Add(userdata);
                    }
                }
            }//try 
            catch
            {

            }


            return Json(UserList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ParentSearch(int Searchtype, string searchdata)
        {
            List<userdata> UserList = new List<userdata>();
            string userType = "Parent";
            try
            {
                if (Searchtype == 1) // roll no.
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Id == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    userdata.Class = user.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                    UserList.Add(userdata);
                }
                else if (Searchtype == 2) // username
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    userdata.Class = user.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                    UserList.Add(userdata);
                }
                else if (Searchtype == 3) // name
                {
                    var users = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Name.Contains(searchdata)) && x.Status != "False").ToList();
                    foreach (var item in users)
                    {
                        var userdata = new userdata();
                        userdata.Id = item.Id;
                        userdata.Email = item.Email;
                        userdata.Name = item.Name;
                        userdata.UserName = item.UserName;
                        userdata.PhoneNumber = item.PhoneNumber;
                        userdata.Class = item.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                        UserList.Add(userdata);
                    }
                }
            }//try 
            catch
            {

            }


            return Json(UserList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TeacherSearch(int Searchtype, string searchdata)
        {
            List<userdata> UserList = new List<userdata>();
            string userType = "Teacher";
            try
            {
                if (Searchtype == 1) // roll no.
                {
                    var user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Id == searchdata) && x.Status != "False").Select(x => x).FirstOrDefault();
                    var userdata = new userdata();
                    userdata.Id = user.Id;
                    userdata.Email = user.Email;
                    userdata.Name = user.Name;
                    userdata.UserName = user.UserName;
                    userdata.PhoneNumber = user.PhoneNumber;
                    userdata.Class = user.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                    UserList.Add(userdata);
                }
                else if (Searchtype == 2) // username
                {
                    List<AspNetUser> user = new List<AspNetUser>();
                    if (searchdata.Length == 1)
                    {
                        user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName.StartsWith(searchdata)) && x.Status != "False").Select(x => x).ToList();
                    }
                    else if (searchdata.Length == 2)
                    {
                        user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName.StartsWith(searchdata)) && x.Status != "False").Select(x => x).ToList();
                    }
                    else
                    {
                        user = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.UserName.Contains(searchdata)) && x.Status != "False").Select(x => x).ToList();
                    }
                    foreach (var item in user)
                    {
                        var userdata = new userdata();
                        userdata.Id = item.Id;
                        userdata.Email = item.Email;
                        userdata.Name = item.Name;
                        userdata.UserName = item.UserName;
                        userdata.PhoneNumber = item.PhoneNumber;
                        userdata.Class = item.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                        UserList.Add(userdata);
                    }
                }
                else if (Searchtype == 3) // name
                {
                    var users = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains(userType) && (x.Name.Contains(searchdata)) && x.Status != "False").ToList();
                    foreach (var item in users)
                    {
                        var userdata = new userdata();
                        userdata.Id = item.Id;
                        userdata.Email = item.Email;
                        userdata.Name = item.Name;
                        userdata.UserName = item.UserName;
                        userdata.PhoneNumber = item.PhoneNumber;
                        userdata.Class = item.AspNetClasses.Select(x => x.ClassName).FirstOrDefault();
                        UserList.Add(userdata);
                    }
                }
            }//try 
            catch
            {

            }


            return Json(UserList, JsonRequestBehavior.AllowGet);
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
    //public class Parent
    //{
    //    public string Child1 { get; set; }
    //    public string Child2 { get; set; }
    //    public string Child3 { get; set; }
    //    public string Child4 { get; set; }

    //}
    //public class Subject
    //{
    //    public string Subject1 { get; set; }
    //    public string Subject2 { get; set; }
    //    public string Subject3 { get; set; }
    //    public string Subject4 { get; set; }
    //    public string Subject5 { get; set; }
    //    public string Subject6 { get; set; }
    //    public string Subject7 { get; set; }
    //    public string Subject8 { get; set; }

    //}
    public class userdata
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Class { set; get; }
    }

}
