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

    public class UserController : Controller
    {
        string Baseurl = System.Configuration.ConfigurationManager.AppSettings["WebAPIName"].ToString();
        // GET: User
        public ActionResult Index()
        {
            Session["UserList"] = null;
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

                    //Deserializing the response recieved from web api and storing into the Employee list  
                    UserInfo = JsonConvert.DeserializeObject<List<User_info>>(Response);
                    ViewData["oList"] = UserInfo;
                    User_info oTempUser=new User_info();
                    ViewData["user"] = oTempUser;
                    Session["UserList"] = UserInfo;
                }

            }
            ViewBag.SubmitValue = "Add";
            return View(UserInfo);
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        
        public ActionResult SortByFName()
        {
            ViewBag.SubmitValue = "Add";
            List<User_info> oUserList = new List<User_info>();
                      oUserList = (List<User_info>)Session["UserList"];

          
            User_info oTempUser = new User_info();
            ViewData["user"] = oTempUser;
           ViewData["oList"] = oUserList.OrderBy(o=>o.FirstName).ToList();
            return View("Index");
        }
        public ActionResult SortByLName()
        {
            ViewBag.SubmitValue = "Add";
            List<User_info> oUserList = new List<User_info>();
            oUserList = (List<User_info>)Session["UserList"];


            User_info oTempUser = new User_info();
            ViewData["user"] = oTempUser;
            ViewData["oList"] = oUserList.OrderBy(o => o.LastName).ToList();
            return View("Index");
        }
        public ActionResult SortByEMPID()
        {
            ViewBag.SubmitValue = "Add";
            List<User_info> oUserList = new List<User_info>();
            oUserList = (List<User_info>)Session["UserList"];


            User_info oTempUser = new User_info();
            ViewData["user"] = oTempUser;
            ViewData["oList"] = oUserList.OrderBy(o => o.Employee_ID).ToList();
            return View("Index");
        }


        public ActionResult SearchUser(string firstname)
        {
            ViewBag.SubmitValue = "Add";
            List<User_info> oUserList = new List<User_info>();
            List<User_info> SearchList = new List<User_info>();
            oUserList=(List<User_info>) Session["UserList"] ;

            foreach (User_info ouser in oUserList)
            {
                if (ouser.FirstName.Contains(firstname) || ouser.LastName.Contains(firstname))
                {
                    SearchList.Add(ouser);
                }
            }
            if (SearchList == null)
            {
                ModelState.AddModelError(string.Empty, "No user found with this name.");
            }
            User_info oTempUser = new User_info();
            ViewData["user"] = oTempUser;
            ViewData["oList"] = SearchList;
            return View("Index");
        }
        // GET: User/Create
        public ActionResult Create()
        {
            return View("AddUser");
        }

        // POST: User/Create
        [HttpPost]
        public ActionResult Create(User_info collection)
        //public ActionResult Create(string firstname, string lastname, string empID)
        {

            if (collection.FirstName == "" || collection.FirstName == null || collection.LastName == "" || collection.LastName ==null|| collection.Employee_ID == 0)
            {
                ModelState.AddModelError(string.Empty, "Please enter all the fileds below");
                ViewData["oList"] = (List<User_info>)Session["UserList"];
               
                ViewData["user"] = collection;
                if (Session["UserToEdit"] == null) { ViewBag.SubmitValue = "Add"; }
                else { ViewBag.SubmitValue = "Update"; }
                                return View("Index");
            }
          


            try
            {
                // TODO: Add insert logic here
                if (Session["UserToEdit"]==null)
                {
                                       string PostString = "";
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(Baseurl);

                        //HTTP POST
                        var postTask = client.PostAsJsonAsync<User_info>("PostUser", collection);
                        postTask.Wait();

                        var result = postTask.Result;
                        PostString = result.StatusCode.ToString();
                        if (PostString.Contains("InternalServerError") == true) { PostString = "OK"; }
                    }
                }
                else
                {
                    if (Session["UserToEdit"] != null)
                    {
                        User_info UserToSave = new User_info();
                        UserToSave = (User_info)Session["UserToEdit"];
                        //Clear the session 
                        Session["UserToEdit"] = null;
                        UserToSave.FirstName = collection.FirstName;
                        UserToSave.LastName = collection.LastName;
                        UserToSave.Employee_ID = collection.Employee_ID;
                        //Call the Put Method
                        string PostString = "";
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(Baseurl);

                            //HTTP POST
                            var postTask = client.PutAsJsonAsync<User_info>("PutUser", UserToSave);
                            postTask.Wait();

                            var result = postTask.Result;
                            PostString = result.StatusCode.ToString();
                            if (PostString.Contains("NoConte") == true) { PostString = "OK"; }
                        }
                    }
                    //update
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            var Info = new User_info();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);
                //HTTP GET
                var responseTask = client.GetAsync("GetUser?id=" + id + "");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<User_info>();
                    readTask.Wait();

                    Info = readTask.Result;
                    Session["UserToEdit"] = Info;
                }
            }
            //use the session to capture the user for edit . then using the session in below just update the methods which needs update . then post in DB and call index which will refresh the list.
            ViewBag.SubmitValue = "Update";

            //firstname = "test";
            List<User_info> olist=(List<User_info>) Session["UserList"] ;
            ViewData["oList"] = olist;
            ViewData["user"] = Info;
            return View("Index");
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(string firstname, string lastname, string empID)
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

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Baseurl);

                //HTTP POST
                var postTask = client.DeleteAsync("DelUser?id=" + id.ToString() + "");
                postTask.Wait();

                var result = postTask.Result;

            }
            return RedirectToAction("Index");
        }

        // POST: User/Delete/5
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
    }
}
