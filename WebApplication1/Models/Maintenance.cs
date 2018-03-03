using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebApplication1.Models
{
    public class Maintenance
    {
        public string Location { get; set; }
        public int LocationID { get; set; }
        public int EquipmentID { get; set; }
        public string EquipmentType { get; set; }
        public int MaintenanceAuditID { get; set; }
        public string ReportID { get; set; }
        public int SafetyLocationID { get; set; }
        public int SafetyEquipmentID { get; set; }
        public int? WasItChecked { get; set; }
        public string Notes { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateChecked { get; set; }
        public DateTime DateEnded { get; set; }
        public string User { get; set; }
        public byte Completed { get; set; }
        public string LocationName { get; set; }
        public int CheckCount { get; set; }

        WindowsIdentity identity = HttpContext.Current.Request.LogonUserIdentity;


        private string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        public IEnumerable<Maintenance> GetLocations()
        {

            List<Maintenance> tickets = new List<Maintenance>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("SELECT ID, Location FROM [DB].[dbo].[MaintenanceLocations];", con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Maintenance Details = new Maintenance();
                    Details.LocationID = Convert.ToInt32(rdr["ID"]);
                    Details.Location = rdr["Location"].ToString();


                    tickets.Add(Details);
                }
            }
            return tickets;

        }

        public IEnumerable<Maintenance> GetEquipmentTypePerLocation(int id, string reportid)
        {

            List<Maintenance> equipmentType = new List<Maintenance>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("spGetEquipmentListPerWarehouse", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@locationid", id);
                cmd.Parameters.AddWithValue("@reportid", reportid);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Maintenance Details = new Maintenance();
                    Details.EquipmentID = Convert.ToInt32(rdr["EquipmentID"]);
                    Details.EquipmentType = rdr["EquipmentType"].ToString();
                    Details.SafetyLocationID = Convert.ToInt32(rdr["LocationID"]);
                    Details.EquipmentID = Convert.ToInt32(rdr["EquipmentID"]);
                    Details.LocationName = rdr["LocationName"].ToString();
                    Details.ReportID = rdr["ReportID"].ToString();

                    if (!Convert.IsDBNull(rdr["WasItChecked"]))
                    {
                        Details.WasItChecked = Convert.ToByte(rdr["WasItChecked"]);
                    }
                    else
                    {
                        Details.WasItChecked = 0;
                    }

                    Details.Notes = rdr["Notes"].ToString();

                    Details.DateChecked = Convert.ToDateTime(rdr["DateChecked"] == DBNull.Value
                         ? (DateTime?)null
                         : (DateTime)rdr["DateChecked"]);

                    Details.User = rdr["User"].ToString();
                    //Details.CheckCount = Convert.ToInt32(rdr["checked"]);

                    equipmentType.Add(Details);
                }
            }
            return equipmentType;

        }



        public string getLocation(int id)
        {
            string location = "";
            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("Select Location from DB.dbo.MaintenanceLocations where ID = @id", con);
                cmd.Parameters.AddWithValue("@id", id);

                location = cmd.ExecuteScalar().ToString();

            }
            return location;
        }

        public IEnumerable<Maintenance> GetIncompleteAuditReport()
        {

            List<Maintenance> auditReport = new List<Maintenance>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("SELECT distinct reportID FROM [DB].[dbo].[MaintenanceAuditTable] where Completed = 0;", con);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Maintenance Details = new Maintenance();

                    Details.ReportID = rdr["ReportID"].ToString();


                    auditReport.Add(Details);
                }
            }
            return auditReport;
        }

    }
}