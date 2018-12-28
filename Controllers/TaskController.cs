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
    public class TaskController : Controller
    {
        string Baseurl = System.Configuration.ConfigurationManager.AppSettings["WebAPIName"].ToString();
        // GET: Task
        public ActionResult Index()
        {
            PopulateTaskList();
            ViewBag.Message = "Hello ";
            return View();
        }

        // GET: Task/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Task/Create
        public ActionResult Create()
        {
            //this is the ultimate landing page for the Add task , so add all the logic here
            //This will populate the Field values

            Task oTempTask = new Task();
            oTempTask = PopulateTaskList();
            ViewBag.oListParentTask = ParentTaskList();
            ViewBag.oProjectList = ProjectList();
            return View(oTempTask);
        }

        public ActionResult SortByTask()
        {
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];

            ViewData["oList"] = oList.OrderBy(o => o.Task1).ToList();
            return View("Index");
        }
        public ActionResult SortByPTask()
        {
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];
            ViewData["oList"] = oList.OrderBy(o => o.ParentTask.Parent_Task).ToList(); 
            return View("Index");
        }
        public ActionResult SortByPR()
        {
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];

            ViewData["oList"] = oList.OrderBy(o => o.Priority).ToList();
            return View("Index");
        }
        public ActionResult SortBySS()
        {

            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];

            ViewData["oList"] = oList.OrderBy(o => o.Status).ToList();
            return View("Index");
        }
        public ActionResult SortByStartDate()
        {
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];

            ViewData["oList"] = oList.OrderBy(o => o.Start_Date).ToList();
            return View("Index");
        }
        public ActionResult SortByEndDate()
        {
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];

            ViewData["oList"] = oList.OrderBy(o => o.End_date).ToList();
            return View("Index");
        }


        public ActionResult SearchTask(string firstname)
        {
            List<Task> oList = new List<Task>();
            List<Task> oDisplay = new List<Task>();
            oList = (List<Task>)Session["TaskList"];
            foreach (Task oitem in oList)
            {
                if (oitem.Project.Project1.Contains(firstname) == true)
                {
                    oDisplay.Add(oitem);
                }
            }
            if (oDisplay == null || oDisplay.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No Project to Display.");
                ViewData["oList"] = oDisplay;
                return View("Index");
            }
            ViewData["oList"] = oDisplay;
            Session["TaskList"] = oDisplay;
            return View("Index");
        }
        public ActionResult Show(int id)
        {
            if (Session["TaskList"] == null) { PopulateTaskList(); }
            List<Task> oList = new List<Task>();
            List<Task> oDisplay = new List<Task>();
            oList = (List<Task>)Session["TaskList"];
            foreach (Task oitem in oList)
            {
                if (oitem.Project.Project_ID==id)
                {
                    oDisplay.Add(oitem);
                }
            }
            if (oDisplay == null || oDisplay.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "No Project to Display.");
                ViewData["oList"] = oDisplay;
                return View("Index");
            }
            Session["TaskList"] = oDisplay;
            ViewData["oList"] = oDisplay;
            return View("Index");
        }

        // POST: Task/Create
        [HttpPost]
        public ActionResult Create(Task collection, FormCollection form)
        {
           
            string PostString;
            try
            {
                int Priority = 1;
                string Prange = form["Prange"].ToString();
                int.TryParse(Prange, out Priority);
                string CheckParentTkt = form["hdnChk"].ToString();

                string ProjectVal = form["ProjectDDL"].ToString();

                //validate the Input
                //Parent task
                if (collection.Task1 == null || collection.Task1 == "")
                {
                    ModelState.AddModelError("ERROR", "Please add Task name");
                    return RedirectToAction("Create");
                }
                if (CheckParentTkt == "false")
                {
                    if (collection.Start_Date == null || collection.Start_Date < DateTime.Today || collection.End_date == null || collection.End_date < DateTime.Today.AddDays(1))
                    {
                        ModelState.AddModelError("ERROR", "Please enter all the subtask data and data should be valid.");
                        return RedirectToAction("Create");
                    }
                }



                if (CheckParentTkt == "true")//Parent Ticket
                {
                    ParentTask Ptask = new ParentTask();
                    Ptask.Parent_Task = collection.Task1;
                    //add the parent task , then call the userInfo table to update the Task ID based on the project ID
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        //HTTP POST
                        var postTask = client.PostAsJsonAsync<ParentTask>("PostParentTask", Ptask);
                        postTask.Wait();

                        var result = postTask.Result;
                        PostString = result.StatusCode.ToString();
                        if (PostString.Contains("InternalServerError") == true) { PostString = "OK"; }
                        ViewBag.Message = "Parent task added sucessfully";
                    }
                    UpdateUserTableforTask(ProjectVal, Ptask);
                }
                else//subtask
                {
                    string UserID = form["User"].ToString();
                    string ParentTaskval = form["taskName"].ToString();

                    int ProjectID;
                    int PtaskID;

                    int.TryParse(ProjectVal, out ProjectID);
                    int.TryParse(ParentTaskval, out PtaskID);

                    collection.Parent_ID = PtaskID;
                    collection.Project_ID = ProjectID;
                    collection.Status = "Active";
                    collection.Priority = Priority;
                    //call insert method
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        //HTTP POST
                        var postTask = client.PostAsJsonAsync<Task>("PostTask", collection);
                        postTask.Wait();

                        var result = postTask.Result;
                        PostString = result.StatusCode.ToString();
                        if (PostString.Contains("InternalServerError") == true) { PostString = "OK"; }
                        ViewBag.Message = "Subtask added sucessfully";
                    }

                }

                return RedirectToAction("Create");
            }
            catch
            {
                ModelState.AddModelError("ERROR", "Error in Add task");
                return View("Create");
            }
        }

        // GET: Task/Edit/5
        public ActionResult Edit(int id)
        {
            Session["EditTask"] = null;
            if (Session["TaskList"] == null) { PopulateTaskList(); }
            Task Info = new Task();
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];
            foreach (Task oTask in oList)
            {
                if (oTask.Task_ID == id)
                {
                    Info = oTask;
                    break;
                }
            }
            //choose the parent task of for the same project
            List<ParentTask> oParent = new List<ParentTask>();
            List<ParentTask> oParentDDL = new List<ParentTask>();
            oParent = ParentTaskList();
            foreach (ParentTask Opt in oParent)
            {
                foreach (Task oT in Opt.Tasks)
                {
                    if (oT.Project_ID == Info.Project_ID)
                    {
                        oParentDDL.Add(Opt);
                        break;
                    }
                }

            }
            Session["EditTask"] = Info;
            ViewBag.EditoListParentTask = oParentDDL;
            return View(Info);
        }

        // POST: Task/Edit/5
        [HttpPost]
        public ActionResult Edit(Task oTask, FormCollection form)
        {
            try
            {
                // TODO: Add update logic here
                int Priority = 1;
                int PtaskID;

                string Prange = form["Prange"].ToString();
                int.TryParse(Prange, out Priority);
                string Status = form["Status"].ToString();
                string ParentTaskval = form["taskName"].ToString();
                int.TryParse(ParentTaskval, out PtaskID);

                Task ToSave = new Task();
                ToSave = (Task)Session["EditTask"];
                ToSave.Start_Date = oTask.Start_Date;
                ToSave.End_date = oTask.End_date;
                ToSave.Priority = Priority;
                ToSave.Status = Status;
                ToSave.Parent_ID = PtaskID;
                ToSave.Task1 = oTask.Task1;
                ToSave.User_info = null;
                ToSave.Project = null;
                ToSave.ParentTask = null;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);

                    //HTTP POST
                    var postTask = client.PutAsJsonAsync<Task>("PutTask", ToSave);
                    postTask.Wait();

                    var result = postTask.Result;

                }
                Session["EditTask"] = null;
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Task/Delete/5
        public ActionResult Delete(int id)
        {
            //mark the task as Completed
            Task Info = new Task();
            List<Task> oList = new List<Task>();
            oList = (List<Task>)Session["TaskList"];
            foreach (Task oTask in oList)
            {
                if (oTask.Task_ID == id)
                {
                    Info = oTask;
                    Info.Status = "Completed";
                    break;
                }
            }

            Info.User_info = null;
            Info.Project = null;
            Info.ParentTask = null;
            string PostString = "";
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);

                //HTTP POST
                var postTask = client.PutAsJsonAsync<Task>("PutTask", Info);
                postTask.Wait();

                var result = postTask.Result;
                PostString = result.StatusCode.ToString();
                if (PostString.Contains("NoConte") == true) { PostString = "OK"; }
            }
            return RedirectToAction("Index");
        }

        // POST: Task/Delete/5
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


        private void UpdateUser(User_info oUser)
        {
            oUser.Project = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);

                //HTTP POST
                var postTask = client.PutAsJsonAsync<User_info>("PutUser", oUser);
                postTask.Wait();

                var result = postTask.Result;

            }
        }

        private void UpdateUserTableforTask(string ProjectID, ParentTask oTask)
        {
            int ParentTaskID = 0;

            List<ParentTask> oPTask = new List<ParentTask>();
            oPTask = ParentTaskList();

            foreach (ParentTask oPRT in oPTask)
            {
                if (oPRT.Parent_Task == oTask.Parent_Task)
                {
                    ParentTaskID = oPRT.Parent_ID;
                    break;
                }
            }

            List<User_info> oUser = new List<User_info>();
            oUser = UserList();
            foreach (User_info user in oUser)
            {
                if (user.Project_ID.ToString() == ProjectID)
                {
                    user.Task_ID = ParentTaskID;
                    UpdateUser(user);

                }
            }
        }

        private Task PopulateTaskList()
        {
            Session["TaskList"] = null;
            var TaskInfo = new List<Task>();
            Task oTempTask = new Task();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("Tasks");
                responseTask.Wait();

                //To store result of web api response.   
                var result = responseTask.Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (result.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = result.Content.ReadAsStringAsync().Result;

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    TaskInfo = JsonConvert.DeserializeObject<List<Task>>(Response);



                    oTempTask.Priority = 1;
                    oTempTask.User_info = UserList();
                    ViewData["Task"] = oTempTask;
                    ViewData["oList"] = TaskInfo;



                    Session["taskList"] = TaskInfo;
                    ViewBag.SubmitValue = "Add";
                }

            }
            return oTempTask;
        }
        //private class DataToView 
        //{
        //    public  List<User_info> User_info { get; set; }
        //    public List<ParentTask> User_info { get; set; }
        //    public List<Project> User_info { get; set; }
        //    public List<Task> User_info { get; set; }
        //}
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
        private List<Project> ProjectList()
        {
            var Info = new List<Project>();
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
                    Info = JsonConvert.DeserializeObject<List<Project>>(Response);
                }

            }


            return Info;
        }
        private List<ParentTask> ParentTaskList()
        {
            var ParentTaskInfo = new List<ParentTask>();
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();
                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var responseTask = client.GetAsync("ParentTasks");
                responseTask.Wait();

                //To store result of web api response.   
                var result = responseTask.Result;

                //Checking the response is successful or not which is sent using HttpClient  
                if (result.IsSuccessStatusCode)
                {
                    //Storing the response details recieved from web api   
                    var Response = result.Content.ReadAsStringAsync().Result;
                    ParentTaskInfo = JsonConvert.DeserializeObject<List<ParentTask>>(Response);
                }
            }
            return ParentTaskInfo;
        }
    }
}
