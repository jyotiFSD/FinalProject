using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectManageEntity;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
namespace ProjectManager.Controllers
{
    public class ProjectController : Controller
    {
        string Baseurl = System.Configuration.ConfigurationManager.AppSettings["WebAPIName"].ToString();
        // GET: Project
        public ActionResult Index()
        {
            Session["ProjectList"] = null;
            var ProjectInfo = new List<Project>();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("Projects");
                responseTask.Wait();

                //To store result of web api response.   
                var result = responseTask.Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (result.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = result.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    ProjectInfo = JsonConvert.DeserializeObject<List<Project>>(Response);


                   
                }
                Project oTempproject = new Project();
                oTempproject.Priority = 1;
                oTempproject.User_info = UserList();
                ViewData["project"] = oTempproject;
                ViewData["oList"] = ProjectInfo;
                Session["ProjectList"] = ProjectInfo;
                ViewBag.SubmitValue = "Add";

            }


            //User_info
            return View("ProjectView");
        }

        public ActionResult SortByPName()
        {
            ViewBag.SubmitValue = "Add";
            List<Project> oList = new List<Project>();
            oList = (List<Project>)Session["ProjectList"];


            Project oTempproject = new Project();
            oTempproject.Priority = 1;
            oTempproject.User_info = UserList();
            ViewData["project"] = oTempproject;
            ViewData["oList"] = oList.OrderBy(o => o.Project1).ToList();
            return View("ProjectView");
        }
        public ActionResult SortByPriority()
        {
            ViewBag.SubmitValue = "Add";
            List<Project> oList = new List<Project>();
            oList = (List<Project>)Session["ProjectList"];


            Project oTempproject = new Project();
            oTempproject.Priority = 1;
            oTempproject.User_info = UserList();
            ViewData["project"] = oTempproject;
            ViewData["oList"] = oList.OrderBy(o => o.Priority).ToList();
            return View("ProjectView");
        }
        public ActionResult SortByStartDate()
        {
            ViewBag.SubmitValue = "Add";
            List<Project> oList = new List<Project>();
            oList = (List<Project>)Session["ProjectList"];


            Project oTempproject = new Project();
            oTempproject.Priority = 1;
            oTempproject.User_info = UserList();
            ViewData["project"] = oTempproject;
            ViewData["oList"] = oList.OrderBy(o => o.Start_Date).ToList();
            return View("ProjectView");
        }
        public ActionResult SortByEndDate()
        {
            ViewBag.SubmitValue = "Add";
            List<Project> oList = new List<Project>();
            oList = (List<Project>)Session["ProjectList"];


            Project oTempproject = new Project();
            oTempproject.Priority = 1;
            oTempproject.User_info = UserList();
            ViewData["project"] = oTempproject;
            ViewData["oList"] = oList.OrderBy(o => o.End_date).ToList();
            return View("ProjectView");
        }

        // POST: Project/Create
        [HttpPost]
        public ActionResult Create(Project collection, string Prange, FormCollection form)
        {
            try
            {
                int Priority = 1;
                int.TryParse(Prange, out Priority);
                string CheckDate = form["hdnChk"].ToString();
                string UserID = form["User"].ToString();
                //validate : 
                if (collection.Project1 == "")
                {
                    ModelState.AddModelError(string.Empty, "Please enter all the fileds below");
                    if (Session["ProjectToEdit"] == null) { ViewBag.SubmitValue = "Add"; }
                    else { ViewBag.SubmitValue = "Update"; }
                    collection.User_info = UserList();
                    collection.Priority = Priority;
                    ViewData["project"] = collection;
                    ViewData["oList"] = (List<Project>)Session["ProjectList"];
                    return View("ProjectView");
                }
                if (CheckDate == "true")
                {
                    if (collection.Start_Date < DateTime.Now || collection.Start_Date == null || collection.End_date <= DateTime.Now.Date || collection.End_date == null)
                    {
                        ModelState.AddModelError(string.Empty, "Please enter Correct start and end date");
                        if (Session["ProjectToEdit"] == null) { ViewBag.SubmitValue = "Add"; }
                        else { ViewBag.SubmitValue = "Update"; }
                        collection.User_info = UserList();
                        collection.Priority = Priority;
                        ViewData["project"] = collection;
                        ViewData["oList"] = (List<Project>)Session["ProjectList"];
                        return View("ProjectView");
                    }

                }


                string PostString;
                collection.Priority = Priority;

                if (Session["ProjectToEdit"] == null)
                {

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        //HTTP POST
                        var postTask = client.PostAsJsonAsync<Project>("PostProject", collection);
                        postTask.Wait();

                        var result = postTask.Result;
                        PostString = result.StatusCode.ToString();
                        if (PostString.Contains("InternalServerError") == true) { PostString = "OK"; }

                    }
                    //update the same in the User table
                    AddUserProject(collection, UserID);

                }
                else
                {
                    //update 
                    Project ProjectToSave = new Project();
                    ProjectToSave = (Project)Session["ProjectToEdit"];
                    //Clear the session 
                    Session["ProjectToEdit"] = null;
                    ProjectToSave.Project1 = collection.Project1;
                    ProjectToSave.Start_Date = collection.Start_Date;
                    ProjectToSave.End_date = collection.End_date;
                    ProjectToSave.Priority = Priority;
                    ProjectToSave.User_info = null;//this will not add any extra row in the User table.
                    ProjectToSave.Tasks = null;
                    
                    oUser(UserID, ProjectToSave.Project_ID);
                    //Call the Put Method

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        //HTTP POST
                        var postTask = client.PutAsJsonAsync<Project>("PutProject", ProjectToSave);
                        postTask.Wait();

                        var result = postTask.Result;
                        PostString = result.StatusCode.ToString();
                        if (PostString.Contains("NoConte") == true) { PostString = "OK"; }
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        // GET: Project/Edit/5
        public ActionResult Edit(int id)
        {
            Session["ProjectToEdit"] = null;

            var Info = new Project();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                //HTTP GET
                var responseTask = client.GetAsync("Project?id=" + id + "");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Project>();
                    readTask.Wait();

                    Info = readTask.Result;
                    Info.User_info = UserList();//this will populate the dropdown with all users.
                    Session["ProjectToEdit"] = Info;
                }
            }
            //use the session to capture the user for edit . then using the session in below just update the methods which needs update . then post in DB and call index which will refresh the list.
            ViewBag.SubmitValue = "Update";

            //firstname = "test";
            ViewData["project"] = Info;
            ViewData["oList"] = (List<Project>)Session["ProjectList"];
            return View("ProjectView");

        }

        // POST: Project/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int id)
        {

            //update user table Project ID =null then delete 
            SetUpdateUser(id.ToString());
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);

                //HTTP POST
                var postTask = client.DeleteAsync("DelProject?id=" + id.ToString() + "");
                postTask.Wait();

                var result = postTask.Result;

            }
            return RedirectToAction("Index");
        }

        // POST: Project/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        private List<User_info> UserList()
        {
            var UserInfo = new List<User_info>();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("GetUsers");
                responseTask.Wait();

                //To store result of web api response.   
                var result = responseTask.Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (result.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = result.Content.ReadAsStringAsync().Result;
                    UserInfo = JsonConvert.DeserializeObject<List<User_info>>(Response);
                }

            }


            return UserInfo;
        }
        private void AddUserProject(Project oProject, string UserID)
        {
            //get all Projects
            var ProjectInfo = new List<Project>();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("Projects");
                responseTask.Wait();

                //To store result of web api response.   
                var result = responseTask.Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (result.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = result.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    ProjectInfo = JsonConvert.DeserializeObject<List<Project>>(Response);

                }

            }

            //match the exactproject
            foreach (Project oPR in ProjectInfo)
            {
                if (oPR.Project1 == oProject.Project1 && oPR.Priority == oProject.Priority)
                {
                    oUser(UserID, oPR.Project_ID);
                    break;
                }
            }
            //call the ouser method to update the ProjectID.
        }
        private void oUser(string Userid, int ProjectID)
        {
            var Info = new User_info();


            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                //HTTP GET
                var responseTask = client.GetAsync("GetUser?id=" + Userid + "");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<User_info>();
                    readTask.Wait();

                    Info = readTask.Result;

                }
            }
            Info.Project_ID = ProjectID;
                        Info.Task = null;
            //Call the Put Method
            string PostString = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);

                //HTTP POST
                var postTask = client.PutAsJsonAsync<User_info>("PutUser", Info);
                postTask.Wait();

                var result = postTask.Result;
                PostString = result.StatusCode.ToString();
                if (PostString.Contains("NoConte") == true) { PostString = "OK"; }
            }


        }
        private void UpdateUser(string userID, string projectID)
        {
            //User_info Users
        }
        private void SetUpdateUser(string id)
        {
            var Info = new Project();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                //HTTP GET
                var responseTask = client.GetAsync("Project?id=" + id + "");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<Project>();
                    readTask.Wait();
                    Info = readTask.Result;

                }
            }

            //we have got the user info now update the project for the users
            foreach (User_info oUser in Info.User_info)
            {

                oUser.Project_ID = null;
                //Call the Put Method
                string PostString = "";
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);

                    //HTTP POST
                    var postTask = client.PutAsJsonAsync<User_info>("PutUser", oUser);
                    postTask.Wait();

                    var result = postTask.Result;
                    PostString = result.StatusCode.ToString();
                    if (PostString.Contains("NoConte") == true) { PostString = "OK"; }
                }
            }
        }


    }
}
