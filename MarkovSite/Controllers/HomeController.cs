using MarkovSharp.TokenisationStrategies;
using MarkovSite.Models;
using Newtonsoft.Json;
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
            ViewBag.Message = "About!";

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
            string requestData;
            using (Stream req = Request.InputStream)
            {
                req.Seek(0, SeekOrigin.Begin);
                requestData = new StreamReader(req).ReadToEnd();
            }

            var deserialisedRequestData = JsonConvert.DeserializeObject<TrainingRequest>(requestData);
            var model = new StringMarkov(deserialisedRequestData.ModelLevel ?? 1);

            var lines = deserialisedRequestData.TrainingData.Split(new char[] { '\n', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            model.Learn(lines);

            Session["Model"] = model;

            return new JsonResult()
            {
                Data = new
                {
                    Message = $"Learnt {lines.Count()} lines of training data using level {model.Level}",
                    Error = null as object
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpPost]
        public ActionResult GetPredictions()
        {
            if (Session["Model"] != null)
            {
                var model = (StringMarkov)Session["Model"];

                string json;
                using (Stream req = Request.InputStream)
                {
                    req.Seek(0, SeekOrigin.Begin);
                    json = new StreamReader(req).ReadToEnd();
                }
                var deserialisedResponse = JsonConvert.DeserializeObject<GetPredictionsRequest>(json);

                if (string.IsNullOrEmpty(deserialisedResponse.SeedText))
                {
                    deserialisedResponse.SeedText = model.GetPrepadGram();
                }

                try
                {
                    var suggestions = model.GetMatches(deserialisedResponse.SeedText.Trim())
                        .Where(a => 
                            a != model.GetPrepadGram()
                            && a != model.GetTerminatorGram()
                        )
                        .GroupBy(a => a)
                        .OrderByDescending(a => a.Count())
                        .Select(a => a.Key).ToArray();

                    return new JsonResult()
                    {
                        Data = new
                        {
                            Suggestions = suggestions,
                            Error = null as object
                        },
                    };
                }
                catch(Exception e)
                {
                    return new JsonResult()
                    {
                        Data = new
                        {
                            Suggestions = new string[] { },
                            Error = e.Message
                        },
                    };
                }
            }
            else
            {
                return new JsonResult()
                {
                    Data = new
                    {
                        Suggestions = new string[] { },
                        Error = "No Markov model was found in session, please try training data again"
                    },
                };
            }
        }
    }
}