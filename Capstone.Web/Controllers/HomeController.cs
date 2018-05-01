using Capstone.Web.DAL;
using Capstone.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Capstone.Web.Controllers
{
    public class HomeController : Controller
    {
        //Dependency injection for Database Access Layer

        private INationalParkDAL _dal;

        public HomeController(INationalParkDAL dal)
        {
            _dal = dal;
        }


        // GET: Home, default View
        public ActionResult Index()
        {
            List<NationalPark> parks = _dal.GetAllParks();

            return View("ParkList", parks);
        }

        public ActionResult ParkList()
        {
            //Database call to receive list of all parks and pass through to View
            List<NationalPark> parks = _dal.GetAllParks();

            return View(parks);
        }


        public ActionResult Detail(string parkCode, string tempType)
        {
            /*Wanted to have a default Park Code for if the user attempts to reach this page without 
            going through Park List*/
            if (parkCode == null)
            {
                parkCode = "ENP";
            }

            /*This sets the temp choice as a Session value in order to persist for future View calls*/
            if (tempType == null)
            {
               tempType = Session["tempChoice"] as string;
            }

            //Persistence of temperature choice demonstrated below
            Session["tempChoice"] = tempType;

            //Database call to receive 5-Day Forecast
            List<WeatherReport> weatherReports = _dal.GetWeatherReports(parkCode, tempType);
            NationalPark park = _dal.GetOnePark(parkCode);
            DetailModel model = new DetailModel
            {
                NationalPark = park,
                WeatherReports = weatherReports
            };

            return View("Detail", model);
        }

        public ActionResult Survey()
        {
            //Populates the model for the survey View
            SurveyModel model = new SurveyModel();
            return View("Survey", model);
        }

        [HttpPost]
        public ActionResult FavoriteParks(SurveyModel model)
        {
           
            //Data validation to ensure user has entered an email address. Further validation occurs on View.
            if (model.Email == null)
            {
                TempData["errorStringEmail"] = "You must enter an email address";
                return View("Survey", model);
            }

            /*This database call checks to see if the user has already submitted a survey for this park by 
             * checking the provided email against the database. Error message is shown to user if they have already
             * posted a survey */
            bool doesSurveyExist = _dal.CheckForSurvey(model);

            if(doesSurveyExist == true)
            {
                TempData["errorString"] = "You cannot post more than one survey for this park";
                return RedirectToAction("Survey");
            }

            //Database method to add survey
            _dal.AddSurvey(model);

            return RedirectToAction("FavoriteParks");
        }

        public ActionResult FavoriteParks()
        {
            //Database call to get survey results for each park
            List<SurveyPark> surveyParks = _dal.GetSurveyParks();
            return View("FavoriteParks", surveyParks);
        }
    }
}