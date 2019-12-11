using SEA_Application.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System.IO;
using System.Text;

namespace SEA_Application.Controllers
{
    public class GrandTotalController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        //
        // GET: /GrandTotal/
        public ActionResult Index()
        {
            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");
            return View();
        }

        public ActionResult ListofStudents(int id)
        {
            if (id == 0)
            {
                var result1 = (from std in db.AspNetStudents
                               join usr in db.AspNetUsers on std.StudentID equals usr.Id
                               where usr.Status != "False"
                               select new { usr.Id, usr.Name, usr.PhoneNumber, usr.Email, usr.UserName, std.AspNetClass.ClassName }).ToList();
                return Json(result1, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result1 = (from std in db.AspNetStudents
                               join usr in db.AspNetUsers on std.StudentID equals usr.Id
                               where usr.Status != "False" && std.ClassID == id
                               select new { usr.Id, usr.Name, usr.PhoneNumber, usr.Email, usr.UserName, std.AspNetClass.ClassName }).ToList();
                return Json(result1, JsonRequestBehavior.AllowGet);
            }

        }
        public void FeeChallan_ExcelReport(string month)
        {
            SEA_DatabaseEntities db = new SEA_DatabaseEntities();

                 var challanlist = (from feemonth in db.StudentFeeMonths
                         join std in db.AspNetStudents on feemonth.StudentId equals std.Id
                         join challan in db.StudentChallanForms on std.Id equals challan.StudentId
                         where feemonth.Status == "Pending" && feemonth.Months == month && feemonth.Id== challan.StudentFeeMonthId
                         select new { challan.ChallanNo, feemonth.FeePayable, feemonth.IssueDate, std.AspNetUser.Name, std.AspNetClass.ClassName, feemonth.DueDate, feemonth.ValildityDate }).ToList();

          
            ExcelPackage pck = new ExcelPackage();
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");

            ws.Cells["A1"].Value = "Challan No";
            ws.Cells["B1"].Value = "Payable before due date";
            ws.Cells["C1"].Value = "Issue Date";
            ws.Cells["D1"].Value = "Registration No";
            ws.Cells["E1"].Value = "Student Name";
            ws.Cells["F1"].Value = "Class";
            ws.Cells["G1"].Value = "Due Date";
            ws.Cells["H1"].Value = "For the month of";
            ws.Cells["I1"].Value = "Payable after due date";
            ws.Cells["J1"].Value = "Validity Date";


            int rowStart = 2;
            foreach (var item in challanlist)
            {
                ws.Cells[string.Format("A{0}", rowStart)].Value = item.ChallanNo;
                ws.Cells[string.Format("B{0}", rowStart)].Value = item.FeePayable;
                ws.Cells[string.Format("C{0}", rowStart)].Value = item.IssueDate;
                ws.Cells[string.Format("D{0}", rowStart)].Value = "";
                ws.Cells[string.Format("E{0}", rowStart)].Value = item.Name;
                ws.Cells[string.Format("F{0}", rowStart)].Value = item.ClassName;
                ws.Cells[string.Format("G{0}", rowStart)].Value = item.DueDate;
                ws.Cells[string.Format("H{0}", rowStart)].Value = month;
                ws.Cells[string.Format("I{0}", rowStart)].Value = item.FeePayable;
                ws.Cells[string.Format("J{0}", rowStart)].Value = item.ValildityDate;

                rowStart++;
            }
            ws.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("content-disposition", "attachment:filename=ExcelReport.xls");
            Response.BinaryWrite(pck.GetAsByteArray());
            Response.End();

            //return RedirectToAction("TeacherIndex");
        }
        public ActionResult ListAllStudents()
        {
            var result1 = (from std in db.AspNetStudents
                           join usr in db.AspNetUsers on std.StudentID equals usr.Id
                           where usr.Status != "False"
                           select new { usr.Id, usr.Name, usr.PhoneNumber, usr.Email, usr.UserName, std.AspNetClass.ClassName }).ToList();
            return Json(result1, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddChallanDate(FeeDateSetting FDS)
        {
            string status = "error";
            if (FDS.ValidityDate != null && FDS.DueDate != null)
            {
                if (db.FeeDateSettings.ToList().Count > 0)
                {
                    FeeDateSetting settings = db.FeeDateSettings.Where(x => x.Id == 1).FirstOrDefault();
                    settings.DueDate = FDS.DueDate;
                    settings.ValidityDate = FDS.ValidityDate;

                }
                else
                {
                    db.FeeDateSettings.Add(FDS);
                }
                if (db.SaveChanges() > 0)
                {
                    status = "success";

                }
            }

            return Content(status);
        }
        public ActionResult GetChallanDate()
        {

            string status = "error";
            FeeDateSetting List = db.FeeDateSettings.FirstOrDefault();
            if (List != null)
            {
                status = Newtonsoft.Json.JsonConvert.SerializeObject(List);
            }
            return Content(status);
        }


        public ActionResult ChallanPrintDetails(string S_UID)
        {
            var StudentID = db.AspNetStudents.Where(x => x.StudentID == S_UID).FirstOrDefault().Id;
            var studentFeeMonths = db.StudentFeeMonths.Include(s => s.AspNetStudent).Where(x => x.StudentId == StudentID).ToList();

            return View(studentFeeMonths);
        }

        public ActionResult ClassList()
        {
            List<AspNetClass> list = new List<AspNetClass>();
            list = db.AspNetClasses.ToList();
            string status = Newtonsoft.Json.JsonConvert.SerializeObject(list);

            return Content(status);
        }
        public ActionResult UpdateStudentFeeMonth_Dates(int StudentID, string Month, DateTime validitydate, DateTime duedate)
        {

            string status = "error";
            StudentFeeMonth settings = db.StudentFeeMonths.Where(x => x.StudentId == StudentID && x.Months == Month).FirstOrDefault();
            settings.ValildityDate = validitydate;
            settings.DueDate = duedate;
            settings.IsPrinted = true;
            if (db.SaveChanges() > 0)
            {
                status = "success";

            }

            return Content(status);
        }

        public ActionResult StudentFeeDetails(int sid, string month)
        {

            if (sid != null && month != "")
            {

                var StudentID = sid;
                ViewBag.StudentID = StudentID;
                ViewBag.Month = month;
                string S_UID = db.AspNetStudents.Where(x => x.Id == sid).FirstOrDefault().StudentID;
                AspNetUser studentdetails = db.AspNetUsers.Where(x => x.Id == S_UID).FirstOrDefault();
                ViewBag.RollNo = studentdetails.UserName;
                ViewBag.Name = studentdetails.Name;
                ViewBag.PhoneNo = studentdetails.PhoneNumber;
            }
            else
            {
                ViewBag.Error = "NotFound";
            }
            // AspNetStudent student =  db.AspNetStudents.Where(x => x.Id == StudentID).FirstOrDefault();
            return View();
        }
        public class feeType
        {
            public string typeName { get; set; }
            public int amount { get; set; }
        }
        public class challanform
        {
            public string SchoolName { get; set; }
            public string BranchName { get; set; }
            public List<string> ChallanCopy { get; set; }
            public DateTime? AcademicSessionStart { get; set; }
            public DateTime? AcademicSessionEnd { get; set; }
            public decimal? ChallanID { get; set; }
            public string UserID { get; set; }
            public string StudentName { get; set; }
            public string StudentUserName { get; set; }
            public string StudentClass { get; set; }
            public List<FeeTypes> FeeType { get; set; }
            public List<String> DiscountNotes { get; set; }
            public string DueDate { get; set; }
            public List<String> Notes { get; set; }
            public DateTime PrintedDate { get; set; }
            public double? PayableFee { get; set; }
            public double? TotalAmount { get; set; }
            public double? Penalty { get; set; }
            public string ValidDate { get; set; }
            public double? Arrears { get; set; }
            public string FeeMonth { get; set; }
            public double? TripCharges { get; set; }


        }

        public class FeeTypes
        {
            public int? Amount { get; set; }
            public string Name { get; set; }
        }
        public JsonResult GetChallanForm(string month, string idlist)
        {
           
            var userid = idlist.Split(',');
            var Nextmonth = "";
            var dates = db.FeeDateSettings.FirstOrDefault();
            List<challanform> ChallanList = new List<challanform>();
            try
            {
                foreach (var item in userid)
                {
                    var student = db.AspNetStudents.Where(x => x.StudentID == item).FirstOrDefault();
                    StudentFeeMonth Student_FeeMonth = db.StudentFeeMonths.Where(x => x.StudentId == student.Id && x.Months == month).FirstOrDefault();
                    Student_FeeMonth.ValildityDate = dates.ValidityDate;
                    Student_FeeMonth.DueDate = dates.DueDate;
                    Student_FeeMonth.Status = "Printed";
                    db.SaveChanges();

                    int recordid = Student_FeeMonth.Id;
                    recordid++;
                   var Studentdata =  db.StudentFeeMonths.Where(x => x.Id == recordid).FirstOrDefault();
                    if(Studentdata.Multiplier == 0)
                    {
                         Nextmonth =   month+ '-'+ Studentdata.Months;

                    }
                    double? trip = 0;
                    NonRecurringFeeMultiplier nonrecurringfee = db.NonRecurringFeeMultipliers.Where(x => x.StudentId == student.Id && x.Month == Student_FeeMonth.Months).FirstOrDefault();
                    if(nonrecurringfee!=null)
                    {
                         trip = nonrecurringfee.TutionFee;
                    }

                   

                    AspNetSession Session = db.AspNetSessions.OrderByDescending(x => x.Id).Select(x => x).FirstOrDefault();
                    decimal? No = 0;
                    var Student_Penalty = db.StudentPenalties.Where(x => x.StudentId == student.Id && x.Status == "Pending").Select(x => x.PenaltyFee.Amount).FirstOrDefault();

                    if (Student_Penalty == null)
                    {

                        Student_Penalty = 0;
                    }
                    var Student_discount = db.StudentDiscounts.Where(x => x.StudentId == student.Id).Select(x => x.FeeDiscount.Amount).FirstOrDefault();

                    if (Student_discount == null)
                    {
                        Student_discount = 0;
                    }

                    ///////////////arrears////////////
                    var _SessionId = Student_FeeMonth.SessionId;
                    int _Id = Student_FeeMonth.Id - 1;
                    bool Flag = true;
                    double? Arrear = 0;
                    while (Flag)
                    {
                        try
                        {
                            double? payableamount = db.StudentFeeMonths.Where(x => x.StudentId == student.Id && x.SessionId == _SessionId && x.Status == "Pending" && x.Id == _Id).FirstOrDefault().FeePayable;

                            Arrear += payableamount;
                            _Id--;
                        }
                        catch (Exception ex)
                        {
                            Flag = false;
                        }
                    }
                    /////////End/////////

                    challanform Challan = new challanform();
                    Challan.AcademicSessionStart = Session.SessionStartDate;
                    Challan.AcademicSessionEnd = Session.SessionEndDate;
                    Challan.StudentName = student.AspNetUser.Name;
                    Challan.StudentUserName = student.AspNetUser.UserName;
                    Challan.StudentClass = student.AspNetClass.ClassName;
                    Challan.SchoolName = "IPC Aziz Avenue Campus";
                    Challan.BranchName = "Canal Branch";
                    Challan.ChallanCopy = new List<string>();
                    Challan.ChallanCopy.Add("Parent Copy");
                    Challan.ChallanCopy.Add("Bank Copy");
                    Challan.ChallanCopy.Add("School Copy");


                    if (Nextmonth == "")
                    {
                        Challan.FeeMonth = Student_FeeMonth.Months;
                    }
                    else
                    {
                        Challan.FeeMonth = Nextmonth;
                    }
                    var duedate = Student_FeeMonth.DueDate.ToString().Split(' ');
                    Challan.DueDate = duedate[0];
                    var validdate = Student_FeeMonth.ValildityDate.ToString().Split(' ');
                    Challan.ValidDate = validdate[0];

                    Challan.PayableFee = Student_FeeMonth.FeePayable - Convert.ToDouble(Student_discount);
                    Challan.Arrears = Arrear;
                    var totalpayable = Arrear + Convert.ToDouble(Student_Penalty) + Student_FeeMonth.FeePayable;
                    Challan.TripCharges = trip;
                    Challan.TotalAmount = totalpayable+trip;

                    /////////////Challan No/////////////
                    var challan = db.StudentChallanForms.Where(x => x.StudentId == student.Id && x.StudentFeeMonthId == Student_FeeMonth.Id).FirstOrDefault();
                    if (challan == null)
                    {
                        try
                        {
                            No = (int)db.StudentChallanForms.Select(x => x.ChallanNo).Max();
                            No++;
                            StudentChallanForm stdchallan = new StudentChallanForm();
                            stdchallan.StudentId = student.Id;
                            stdchallan.ChallanNo = No;
                            stdchallan.StudentFeeMonthId = Student_FeeMonth.Id;
                            stdchallan.Status = "Created";
                            db.StudentChallanForms.Add(stdchallan);
                            db.SaveChanges();
                            ////////////// Accounting System///////////////
                            Ledger ledger_Assets = db.Ledgers.Where(x => x.Name == "Student Receivables").FirstOrDefault();
                            ledger_Assets.CurrentBalance += Convert.ToDecimal(Challan.TotalAmount);
                            db.SaveChanges();

                            Ledger ledger_Income = db.Ledgers.Where(x => x.Name == "Student Fee").FirstOrDefault();
                            ledger_Income.CurrentBalance += Convert.ToDecimal(Challan.TotalAmount);
                            db.SaveChanges();
                            ///////////////Send Text Message//////////////////

                            //AspNetParent_Child parentChild = db.AspNetParent_Child.Where(x => x.ChildID == student.AspNetUser.Id).FirstOrDefault();
                            //var number = db.AspNetUsers.Where(x => x.Id == parentChild.ParentID).Select(x => x.PhoneNumber).FirstOrDefault();
                            //var message = "Fee Challan of " + student.AspNetUser.Name + "is generated. Its due date is :" + Challan.DueDate + "and validity date:" + Challan.ValidDate;
                            //var url = "http://www.outreach.pk/api/sendsms.php/sendsms/url";
                            //String result = "";
                            //var newnum = "";
                            //if (number != null)
                            //{
                            //    var num = number.Substring(1);
                            //    newnum = "92" + num;
                            //}

                            //String messageer = HttpUtility.UrlEncode(message);

                            //String strPost = "id=ipcngsch&pass=ipc_ngs123&msg=" + messageer + "&to=" + newnum + "&mask=IPC-NGS&type=xml&lang=English";
                            //StreamWriter myWriter = null;
                            //HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                            //objRequest.Method = "POST";
                            //objRequest.ContentLength = Encoding.UTF8.GetByteCount(strPost);
                            //objRequest.ContentType = "application/x-www-form-urlencoded";
                            //try
                            //{
                            //    myWriter = new StreamWriter(objRequest.GetRequestStream());
                            //    myWriter.Write(strPost);
                            //}
                            //catch (Exception e)
                            //{

                            //}
                            //finally
                            //{
                            //    myWriter.Close();
                            //}
                            //HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                            //using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                            //{
                            //    result = sr.ReadToEnd();
                            //    // Close and clean up the StreamReader
                            //    sr.Close();
                            //}
                            //var messge = result;

                            //////////////end text message///////////

                        }
                        catch
                        {
                            No = 5050100;
                            StudentChallanForm stdchallan = new StudentChallanForm();
                            stdchallan.StudentId = student.Id;
                            stdchallan.ChallanNo = No;
                            stdchallan.StudentFeeMonthId = Student_FeeMonth.Id;
                            stdchallan.Status = "Created";
                            db.StudentChallanForms.Add(stdchallan);
                            db.SaveChanges();
                            /////////// Accounting System///////////
                            Ledger ledger_Assets = db.Ledgers.Where(x => x.Name == "Student Receivables").FirstOrDefault();
                            ledger_Assets.CurrentBalance += Convert.ToDecimal(Challan.TotalAmount);
                            db.SaveChanges();

                            Ledger ledger_Income = db.Ledgers.Where(x => x.Name == "Student Fee").FirstOrDefault();
                            ledger_Income.CurrentBalance += Convert.ToDecimal(Challan.TotalAmount);
                            db.SaveChanges();
                            ////////////////End///////////////
                        }
                    }
                    else
                    {
                        No = challan.ChallanNo;
                    }
                    Challan.ChallanID = No;
                 

                    ChallanList.Add(Challan);



              

                }


                return Json(ChallanList, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                var Error = "Error";
                return Json(Error, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AllResults(string Month, int StudentID)
        {

            //   double? RecurrenceFee, NonRecurrenceFee,GrandTotal;
            // decimal? Discount, PlentyFee;
            GrandTotal GT = new GrandTotal();
            if (Month != "" && StudentID != null)
            {
                //Recurrence Fee
                try
                {
                    double? fee = db.StudentFeeMonths.Where(x => x.StudentId == StudentID && x.Months == Month && x.Status == "Pending").FirstOrDefault().FeePayable;

                    if (fee == null)
                    {
                        fee = 0;
                    }

                    GT.RecurrenceFee = fee.ToString();
                }

                catch (Exception ex)
                {
                    return RedirectToAction("StudentFeeDetails");

                }

                //NonRecurrence Fee
                List<NonRecurringFeeMultiplier> result = db.NonRecurringFeeMultipliers.Where(x => x.StudentId == StudentID && x.Month == Month && x.Status == "Pending").ToList();
                double? totalfee = 0;

                for (int i = 0; i < result.Count(); i++)
                {
                    totalfee += result[i].TutionFee;
                }
                GT.NonRecurrenceFee = totalfee.ToString();

                //Discount 
                var result1 = (from std_dis in db.StudentDiscounts
                               join fee_dis in db.FeeDiscounts on std_dis.FeeDiscountId equals fee_dis.Id
                               where std_dis.StudentId == StudentID && std_dis.Month == Month
                               select new { fee_dis.Amount }).ToList();
                decimal? totalfee1 = 0;
                for (int i = 0; i < result1.Count(); i++)
                {
                    totalfee1 += result1[i].Amount;
                }
                GT.Discount = totalfee1.ToString();



                //arrears

                var StudentFeeMonthData = db.StudentFeeMonths.Where(x => x.StudentId == StudentID && x.Months == Month).FirstOrDefault();
                GT.StudentFeeMonthId = StudentFeeMonthData.Id;
                var _SessionId = StudentFeeMonthData.SessionId;
                int _Id = StudentFeeMonthData.Id - 1;
                bool Flag = true;
                double? Arrear = 0;
                while (Flag)
                {
                    try
                    {
                        double? payableamount = db.StudentFeeMonths.Where(x => x.StudentId == StudentID && x.SessionId == _SessionId && x.Status == "Pending" && x.Id == _Id).FirstOrDefault().FeePayable;

                        Arrear += payableamount;
                        _Id--;
                    }
                    catch (Exception ex)
                    {
                        Flag = false;
                    }
                }
                GT.Arrear = Arrear.ToString();



                //plentyfee

                var result3 = (from stdplenty in db.StudentPenalties
                               join plenty in db.PenaltyFees on stdplenty.PenaltyId equals plenty.Id
                               where stdplenty.StudentId == StudentID
                               select new { plenty.Amount }).ToList();
                decimal? totalfee3 = 0;
                for (int i = 0; i < result3.Count(); i++)
                {
                    totalfee3 += result3[i].Amount;
                }

                GT.PlentyFee = totalfee3.ToString();

                string Result = Newtonsoft.Json.JsonConvert.SerializeObject(GT);

                return Content(Result);
            }
            return Content("0");

        }
    }
}