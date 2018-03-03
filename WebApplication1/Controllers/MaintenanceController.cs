using WebApplication1.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class MaintenanceController : Controller
    {

        #region EquipmentAudit
        public ActionResult Audit()
        {
            Users currentUser = new Users();
            currentUser.AddNewUser();
            currentUser.AccessUsersLDAP();
            ViewBag.Users = currentUser.GetAllUsers().ToList();
            ViewData["MainAdmin"] = currentUser.GetAdminState(System.Web.HttpContext.Current.Application["UserEmail"].ToString(), "Maintenance");

            Maintenance t = new Maintenance();
            ViewBag.Locations = t.GetLocations().ToList();
            ViewBag.IncompleteAuditReport = t.GetIncompleteAuditReport().ToList();
            //ViewBag.CompletedAuditReports = t.GetCompletedAuditReports().ToList();
            return View();
        }

        //partial view for map area pop up with safety equipments checklist
        public PartialViewResult AuditEquipment(int id, string reportid)
        {
            Maintenance t = new Maintenance();

            ViewBag.EquipmentType = t.GetEquipmentTypePerLocation(id, reportid).ToList();
            ViewData["Locations"] = t.getLocation(id);

            return PartialView("_EquipmentPopUp");
        }

        //start new report and return -1 if report already existed
        [HttpPost]
        public int StartReport(string reportID, DateTime startDate, string user)
        {

            int returnrows = 0;

            string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("spStartNewAuditReport", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@reportID", reportID);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@user", user);

                    returnrows = cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                string result = ex.Message;
            }
            return returnrows;
        }

        //db insert or update for each equipment checks
        public void AuditChecks(string ReportID, int SafetyEquipmentID, bool WasItChecked, string Notes, DateTime DateChecked, string User)
        {
            string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("spAuditEquipmentChecks", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ReportID", ReportID);
                    cmd.Parameters.AddWithValue("@SafetyEquipmentID", SafetyEquipmentID);
                    cmd.Parameters.AddWithValue("@WasItChecked", WasItChecked);
                    cmd.Parameters.AddWithValue("@Notes", Notes);
                    cmd.Parameters.AddWithValue("@DateChecked", DateChecked);
                    cmd.Parameters.AddWithValue("@User", User);
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                string result = ex.Message;
            }
        }

        //pull audit report into excel file in appdata
        [HttpPost]
        public string HistoricalAuditReport(string reportID, int typeOfReport)
        {
            SafetyReport sr = new SafetyReport();
            StringBuilder sb = new StringBuilder();

            var data1 = sr.GetHistoricalAuditReport(reportID, typeOfReport).ToList();

            sb.Append("ReportID, EquipmentType, Location, Notes, DateChecked, User \r\n");

            foreach (var row in data1)
            {

                sb.Append(row.ReportID + ",");
                sb.Append(row.EquipmentType + ",");
                sb.Append(row.Location + ",");
                sb.Append(row.Notes + ",");
                sb.Append(row.DateChecked + ",");
                sb.Append(row.User + ",");

                sb.Append("\r\n");
            }

            StreamWriter Excel = new StreamWriter(Server.MapPath("~/App_Data/Safety_Report_" + reportID + ".csv"));
            Excel.WriteLine(sb.ToString());
            Excel.Close();

            return "Safety_Report_" + reportID + ".csv";
        }

        //marks a audit recport completed in the database and prevents it from showing in continue menu
        [HttpPost]
        public void CompleteReport(string reportID, DateTime endDate)
        {

            string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("spCompleteReport", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@reportID", reportID);
                    cmd.Parameters.AddWithValue("@endDate", endDate);

                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                string result = ex.Message;
            }

        }

        //total mapcount for quipment check lists. Currently: 72
        public int GetMapCount(string reportid)
        {
            int mapcount = 0;
            string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    con.Open();
                    SqlCommand cmd;

                    cmd = new SqlCommand("select count(*) as mapcount from MaintenanceAuditTable where ReportID = @reportID and SafetyEquipmentID is not null", con);
                    cmd.Parameters.AddWithValue("@reportID", reportid);

                    mapcount = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            catch (Exception ex)
            {
                string result = ex.Message;
            }

            return mapcount;

        }

        #endregion




    }
}