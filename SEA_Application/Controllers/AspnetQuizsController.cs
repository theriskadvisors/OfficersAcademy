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
    public class AspnetQuizsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetQuizs
        public ActionResult Index()
        {
            return View(db.AspnetQuizs.ToList());
        }

        // GET: AspnetQuizs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }

            var AllTopicIDs = db.Quiz_Topic_Questions.Where(x => x.QuizId == id).Select(x => x.TopicId).ToList();


            var AllTopic = from topic in db.AspnetSubjectTopics
                           where AllTopicIDs.Contains(topic.Id)
                           select topic;
            ViewBag.TopicId = new SelectList(AllTopic, "Id", "Name");


            //All Questions To Display

            var AllQuestionIDS = db.Quiz_Topic_Questions.Where(x => x.QuizId == id).Select(x => x.QuestionId).ToList();

            var AllQuestion = from Question in db.AspnetQuestions
                           where AllQuestionIDS.Contains(Question.Id)
                           select Question;
            ViewBag.QuestionID = new SelectList(AllQuestion, "Id", "Name");




            return View(aspnetQuiz);
        }

        // GET: AspnetQuizs/Create
        public ActionResult Create()
        {
            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics, "Id", "Name");



            return View();
        }

        // POST: AspnetQuizs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AspnetQuiz aspnetQuiz)
        {

            var id = User.Identity.GetUserId();
            var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            aspnetQuiz.CreationDate = DateTime.Now;
            aspnetQuiz.Created_By = username;
            db.AspnetQuizs.Add(aspnetQuiz);
            db.SaveChanges();
            string[] QuestionIDs = Request.Form["QuestionID"].Split(',');
            foreach (var a in QuestionIDs)
            {

                int Questionid = Convert.ToInt32(a);
                int SubjectTopicId = db.AspnetQuestions.Where(x => x.Id == Questionid).Select(x => x.AspnetLesson).Select(x => x.AspnetSubjectTopic.Id).FirstOrDefault();
                Quiz_Topic_Questions QuizTopicQuestions = new Quiz_Topic_Questions();
                QuizTopicQuestions.QuestionId = Questionid;
                QuizTopicQuestions.QuizId = aspnetQuiz.Id;
                QuizTopicQuestions.TopicId = SubjectTopicId;
                db.Quiz_Topic_Questions.Add(QuizTopicQuestions);
                db.SaveChanges();

            }

            return RedirectToAction("Index");
        }


        public ActionResult QuizAllQuestions(int QuizID)
        {

            var QuestionNames = from Quiz in db.AspnetQuizs
                                join QuizTopicQuestion in db.Quiz_Topic_Questions on Quiz.Id equals QuizTopicQuestion.QuizId
                                join Question in db.AspnetQuestions on QuizTopicQuestion.QuestionId equals Question.Id
                                where QuizTopicQuestion.QuizId == QuizID
                                select new
                                {
                                    Question.Name
                                };



            return Json(QuestionNames, JsonRequestBehavior.AllowGet);
        }

        public ActionResult QuestionsByTopics(int[] bdoIds)
        {

            List<AspnetSubjectTopic> AllTopics = (from topic in db.AspnetSubjectTopics
                                                  where bdoIds.Contains(topic.Id)
                                                  select topic).ToList();

            //var AllQuestions =   AllTopics.Select(x => x.AspnetLessons.Select(y => y.AspnetQuestions).Select(y => y)).ToList();


            var AllQuestion = (from topic in db.AspnetSubjectTopics
                               join lesson in db.AspnetLessons on topic.Id equals lesson.TopicId
                               join question in db.AspnetQuestions on lesson.Id equals question.LessonId

                               where bdoIds.Contains(topic.Id)
                               select new
                               {
                                   question.Id,
                                   question.Name

                               }).ToList();





            return Json(AllQuestion, JsonRequestBehavior.AllowGet);
        }

        // GET: AspnetQuizs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuiz);
        }

        // POST: AspnetQuizs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Start_Date,Due_Date,Created_By,CreationDate")] AspnetQuiz aspnetQuiz)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspnetQuiz).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspnetQuiz);
        }

        // GET: AspnetQuizs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            if (aspnetQuiz == null)
            {
                return HttpNotFound();
            }
            return View(aspnetQuiz);
        }

        // POST: AspnetQuizs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetQuiz aspnetQuiz = db.AspnetQuizs.Find(id);
            db.AspnetQuizs.Remove(aspnetQuiz);
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
