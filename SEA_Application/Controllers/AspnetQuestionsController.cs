using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspnetQuestionsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetQuestions
        public ActionResult Index()
        {
            var aspnetQuestions = db.AspnetQuestions.Include(a => a.AspnetLesson).Include(a => a.AspnetOption);
            return View(aspnetQuestions.ToList());
        }

        // GET: AspnetQuestions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuestion aspnetQuestion = db.AspnetQuestions.Find(id);
            if (aspnetQuestion == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuestion);
        }

        // GET: AspnetQuestions/Create
        public ActionResult Create()
        {
            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name");
             ViewBag.TeacherID = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Teacher")), "Id", "Name");

            return View();
        }

        // POST: AspnetQuestions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QuestionAnswerViewModel QuestionAnswerViewModel)
        {

            var id = User.Identity.GetUserId();
            var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            AspnetQuestion Question = new AspnetQuestion();
            Question.Name = QuestionAnswerViewModel.QuestionName;
            Question.Is_Active = QuestionAnswerViewModel.QuestionIsActive;
            Question.Is_Quiz = QuestionAnswerViewModel.QuestionIsQuiz;
            Question.Type = QuestionAnswerViewModel.QuestionType;
            Question.LessonId = QuestionAnswerViewModel.LessonId;
            Question.AnswerId = null;
            Question.CreatedBy = username;
            Question.CreationDate = DateTime.Now;
            db.AspnetQuestions.Add(Question);
            db.SaveChanges();
             var QuestionType = QuestionAnswerViewModel.QuestionType;
            if(QuestionType == "MCQ" || QuestionType =="TF")
            {
                AspnetOption Op1 = new AspnetOption();
                Op1.Name = QuestionAnswerViewModel.OptionNameOne;
                Op1.QuestionId = Question.Id;
                Op1.CreationDate = DateTime.Now;
                db.AspnetOptions.Add(Op1);
                db.SaveChanges();
                AspnetOption Op2 = new AspnetOption();
                Op2.Name = QuestionAnswerViewModel.QuestionNameTwo;
                Op2.QuestionId = Question.Id;
                Op2.CreationDate = DateTime.Now;
                db.AspnetOptions.Add(Op2);
                db.SaveChanges();

                AspnetOption Op3 = new AspnetOption();
                Op3.Name = QuestionAnswerViewModel.QuestionNameThree;
                Op3.QuestionId = Question.Id;
                Op3.CreationDate = DateTime.Now;
                db.AspnetOptions.Add(Op3);
                db.SaveChanges();
                
                AspnetOption Op4 = new AspnetOption();
                Op4.Name = QuestionAnswerViewModel.QuesitonNameFour;
                Op4.QuestionId = Question.Id;
                Op4.CreationDate = DateTime.Now;
                db.AspnetOptions.Add(Op4);
                db.SaveChanges();

                int AnswerId;

                if(QuestionAnswerViewModel.Answer =="a" )
                {

                    AnswerId = Op1.Id;
                }

               else if (QuestionAnswerViewModel.Answer == "b")
                {
                    AnswerId = Op2.Id;

                }

              else  if (QuestionAnswerViewModel.Answer == "c")
                {
                    AnswerId = Op3.Id;

                }

                else

                {
                    AnswerId = Op4.Id;

                }

                Question.AnswerId = AnswerId;
                db.SaveChanges();

            }
            else
            {
                AspnetOption Op = new AspnetOption();
                Op.Name = QuestionAnswerViewModel.FillAnswer;
                Op.QuestionId = Question.Id;
                Op.CreationDate = DateTime.Now;
                db.AspnetOptions.Add(Op);
                db.SaveChanges();
                int AnswerId;
                AnswerId = Op.Id;
                Question.AnswerId = AnswerId;
                db.SaveChanges();
            }


            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name");
            return RedirectToAction("Index");


        }

        // GET: AspnetQuestions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuestion aspnetQuestion = db.AspnetQuestions.Find(id);
            if (aspnetQuestion == null)
            {
                return HttpNotFound();
            }
            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name", aspnetQuestion.LessonId);
            ViewBag.AnswerId = new SelectList(db.AspnetOptions, "Id", "Name", aspnetQuestion.AnswerId);
            return View(aspnetQuestion);
        }

        // POST: AspnetQuestions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,AnswerId,LessonId,CreatedBy,Is_Quiz,Is_Active,Type,CreationDate")] AspnetQuestion aspnetQuestion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspnetQuestion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name", aspnetQuestion.LessonId);
            ViewBag.AnswerId = new SelectList(db.AspnetOptions, "Id", "Name", aspnetQuestion.AnswerId);
            return View(aspnetQuestion);
        }

        // GET: AspnetQuestions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuestion aspnetQuestion = db.AspnetQuestions.Find(id);
            if (aspnetQuestion == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuestion);
        }

        // POST: AspnetQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetQuestion aspnetQuestion = db.AspnetQuestions.Find(id);
            db.AspnetQuestions.Remove(aspnetQuestion);
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
