using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectManageEntity;

namespace ProjectManagerAPI.Controllers
{
    public class User_infoController : ApiController
    {
        private FSD_AssignEntities db = new FSD_AssignEntities();
      

        // GET: api/User_info
        [Route("api/GetUsers")]  
        public IQueryable<User_info> GetUser_info()
        {
            HttpConfiguration config = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            
            return db.User_info;
        }

        // GET: api/User_info/5
        [ResponseType(typeof(User_info))]
        [Route("api/GetUser")]  
        public IHttpActionResult GetUser_info(int id)
        {
            User_info user_info = db.User_info.Find(id);
            if (user_info == null)
            {
                return NotFound();
            }

            return Ok(user_info);
        }

        // PUT: api/User_info/5
          [Route("api/PutUser")]  
        [ResponseType(typeof(void))]
        public IHttpActionResult PutUser_info(User_info user_info)
        {
            int id = user_info.User_ID;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user_info.User_ID)
            {
                return BadRequest();
            }

            db.Entry(user_info).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!User_infoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/User_info
        [ResponseType(typeof(User_info))]
        [Route("api/PostUser")]  
        public IHttpActionResult PostUser_info(User_info user_info)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.User_info.Add(user_info);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (User_infoExists(user_info.User_ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = user_info.User_ID }, user_info);
        }

        // DELETE: api/User_info/5
        [ResponseType(typeof(User_info))]
        [Route("api/DelUser")]  
        public IHttpActionResult DeleteUser_info(int id)
        {
            User_info user_info = db.User_info.Find(id);
            if (user_info == null)
            {
                return NotFound();
            }

            db.User_info.Remove(user_info);
            db.SaveChanges();

            return Ok(user_info);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool User_infoExists(int id)
        {
            return db.User_info.Count(e => e.User_ID == id) > 0;
        }
    }
}