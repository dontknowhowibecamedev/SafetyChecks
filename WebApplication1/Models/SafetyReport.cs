using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class SafetyReport
    {

        public string ReportID { get; set; }
        public string Notes { get; set; }
        public string DateStarted { get; set; }
        public string DateChecked { get; set; }
        public string DateEnded { get; set; }
        public string User { get; set; }
        public string EquipmentType { get; set; }
        public string Location { get; set; }
        


        private string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;


        public IEnumerable<SafetyReport> GetHistoricalAuditReport(string reportID, int typeOfReport)
        {
            List<SafetyReport> report = new List<SafetyReport>();

            using (SqlConnection con = new SqlConnection(cs))
            {

                SqlCommand cmd = new SqlCommand("spGetHistoricAuditReports", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@reportID", reportID);
                cmd.Parameters.AddWithValue("@typeOfReport", typeOfReport);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    SafetyReport Details = new SafetyReport();
                    Details.ReportID = rdr["ReportID"].ToString();
                    Details.Notes = rdr["Notes"].ToString();
                    Details.DateChecked = rdr["DateChecked"].ToString();
                    Details.User = rdr["User"].ToString();
                    Details.EquipmentType = rdr["EquipmentType"].ToString();
                    Details.Location = rdr["Location"].ToString();
                    

                    report.Add(Details);
                }
            }
            return report;
        }
        
        }
    }
