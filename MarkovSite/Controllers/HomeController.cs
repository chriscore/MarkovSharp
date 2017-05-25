using MarkovSharp.TokenisationStrategies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MarkovSite.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Get in touch";

            return View();
        }

        [HttpPost]
        public ActionResult Train()
        {
            var model = new StringMarkov(1);

            string text;
            using (Stream req = Request.InputStream)
            {
                req.Seek(0, SeekOrigin.Begin);
                text = new StreamReader(req).ReadToEnd();
            }

            var lines = text.Split(new char[] { '\n', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            model.Learn(lines);

            Session["Model"] = model;

            return new JsonResult()
            {
                Data = $"Learnt {lines.Count()} lines of training data",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult GetPredictions()
        {
            // this method is just a placeholder example

            // TODO: use markovsharp nuget.
            if (Session["Model"] != null)
            {
                var model = (StringMarkov)Session["Model"];

                string text;
                using (Stream req = Request.InputStream)
                {
                    req.Seek(0, SeekOrigin.Begin);
                    text = new StreamReader(req).ReadToEnd();
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = model.GetPrepadGram();
                }

                try
                {
                    var suggestions = model.GetMatches(text.Trim())
                        .Where(a => 
                            !string.IsNullOrWhiteSpace(a)
                            && a != model.GetTerminatorGram()
                        )
                        .GroupBy(a => a)
                        .OrderByDescending(a => a.Count())
                        .Select(a => a.Key);

                    return new JsonResult()
                    {
                        Data = suggestions,
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                catch(Exception)
                {
                    return new JsonResult()
                    {
                        Data = new string[] { "-", "-", "-" }
                    };
                }
            }
            else
            {
                return new JsonResult()
                {
                    Data = new string[] { "-", "-", "-" }
                };
            }
        }
    }
}