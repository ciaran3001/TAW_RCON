using RCON_HLL_MVC.App_Start;
using RCON_HLL_MVC.Helpers;
using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace RCON_HLL_MVC
{
    [Authorize]
    [AllowCrossSite]
    public class RCONController : Controller
    {

        // GET: RCON
        public ActionResult Index()
        {
            string result = " "; 
            RconCommandsModel commands = new RconCommandsModel();
            HLLService.Connect("176.57.168.232", 28316, "TAWp0waofGr4ySkull",out result);
            return View(commands);


        }

        // GET: RCON/Analytics
        public ActionResult Analytics()
        {
            RconCommandsModel commands = new RconCommandsModel();
            return View();
        }
        // GET: RCON/Bans
        public ActionResult Bans()
        {
            RconCommandsModel commands = new RconCommandsModel();
            return View();
        }

        // GET: RCON/Audit
        public ActionResult Audit()
        {
            RconCommandsModel commands = new RconCommandsModel();
            return View();
        }

        // GET: RCON/GetBans
        /// <summary>
        /// This is an example test method to show how we can use the controller as the web api to get and serve data.
        /// This is also login secured.
        /// </summary>
        /// <returns>Return dummy JSON data.</returns>
        [HttpPost]
        public ActionResult GetBans()
        {
            return Json(new { foo = "bar", baz = "Blech" }, JsonRequestBehavior.AllowGet);
            //   return Json(ccService.Read().ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

      [HttpPost]
        public ActionResult SendCommand(string CommandJSON)
        {
            InputHelper helper = new InputHelper();
            RconCommand rconCommand;
            List<RconCommandParameter> populatedParams;
            string response = " ";

            if (helper.ConvertJSONToCommand(CommandJSON, out rconCommand, out populatedParams))
            {
                rconCommand.StartExecuting(populatedParams, out response);
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateNewSession()
        {
                string ip = "176.57.168.232";
                int port = 28316;
                string password = "TAWp0waofGr4ySkull";
            //ServerSession.m_communicationMutex.ReleaseMutex();
            string result = "";
                if (HLLService.Connect("176.57.168.232", 28316, "TAWp0waofGr4ySkull",out result))
                {
                    return Json("Connected: " + result, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json("Unable to connect: " + result, JsonRequestBehavior.AllowGet);
                }
            /* try { 
              ServerConnectionDetails details = new ServerConnectionDetails(ip, port, password);
                ServerSession _session = new ServerSession(details);
                if (RCONSetup.HLLConnection.Connect(ip,port,password))
                {
                    RCONSetup.RCONSession = _session;
                    ServerSession.m_communicationMutex.WaitOne();
                    _session.SendMessage(string.Format(ServerSession.s_rconLoginCommand, (object)RconCommand.QuoteString(details.ServerPassword)), true);
                    string receivedMessage;
                    var m_lastCommandSucceeded = _session.ReceiveMessage(out receivedMessage, true, true) && RconStaticLibrary.IsSuccessReply(receivedMessage);

                    return Json("Details Set: " + receivedMessage, JsonRequestBehavior.AllowGet);
                }
                return Json("Unable to connect", JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }*/

        }
        [HttpPost]
        [EnableCors("AllowSpecificOrigin")]
        public ActionResult SendGetter(string Getter)
        {
            InputHelper helper = new InputHelper();
            RconGetter rconGetter;
            List<RconCommandParameter> populatedParams;
            string[] response = new string[1]; 

            if(helper.FindGetter(Getter,out rconGetter)){
                if (rconGetter.GetData( out response))
                {
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }

            return Json( " Authenticated " + HLLService.Authenticated , JsonRequestBehavior.AllowGet);
        }


        #region Unused ActionResults
        /*  // GET: RCON/Details/5
          public ActionResult Details(int id)
          {
              return View();
          }

          // GET: RCON/Create
          public ActionResult Create()
          {
              return View();
          }

          // POST: RCON/Create
          [HttpPost]
          public ActionResult Create(FormCollection collection)
          {
              try
              {
                  // TODO: Add insert logic here

                  return RedirectToAction("Index");
              }
              catch
              {
                  return View();
              }
          }

          // GET: RCON/Edit/5
          public ActionResult Edit(int id)
          {
              return View();
          }

          // POST: RCON/Edit/5
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

          // GET: RCON/Delete/5
          public ActionResult Delete(int id)
          {
              return View();
          }

          // POST: RCON/Delete/5
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
          }*/
        #endregion
    }
}
