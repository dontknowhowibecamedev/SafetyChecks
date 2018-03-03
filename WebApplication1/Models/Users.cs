using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Models
{

    public class NoCache : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();

            base.OnResultExecuting(filterContext);
        }
    }
    public class Users
    {
        //User Properties
        public int ID { get; set; }

        public string LoginID {get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int EXT { get; set; }
        public string Department { get; set; }

        WindowsIdentity identity = HttpContext.Current.Request.LogonUserIdentity;
        //Connectionstring
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        public IEnumerable<Users> GetUsers()
        {
            List<Users> UserList = new List<Users>();

            UserList.Clear();

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("Select ID, Name from DB.dbo.Swift_Users;", con);
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Users details = new Users();
                    details.ID = Convert.ToInt32(rdr["ID"]);
                    details.FullName = rdr["Name"].ToString();
                    UserList.Add(details);
                }
            }
            return UserList;
        }



        public void AddNewUser()
        {
            string UserName = identity.Name.Remove(0, 4);
            //Adds user to the users table if the user doesn't already exist

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("spIsNewUser", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginID", UserName);
                con.Open();
                cmd.ExecuteNonQuery();
            }


        }

        public IEnumerable<Users> GetAllUsers()
        {

            List<Users> data = new List<Users>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                con.Open();
                SqlCommand cmd;

                cmd = new SqlCommand("SELECT  [LoginID] as LoginID ,[FirstName] + ' '+ [LastName] as FullName FROM [DB].[dbo].[Users];", con);
               // cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.AddWithValue("@requestID", requestID);

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Users Details = new Users();
                    Details.LoginID = rdr["LoginID"].ToString();
                    Details.FullName = rdr["FullName"].ToString();
                    data.Add(Details);
                }
            }
            return data;

        }
        public void AccessUsersLDAP()
        {
            string UserName = identity.Name.Remove(0, 4);
            //get AD information and populate it in the database if it doesn't exist.
           
                DirectoryEntry entry = new DirectoryEntry("LDAP://DB.local");
                // get a DirectorySearcher object
                DirectorySearcher search = new DirectorySearcher(entry);

                // specify the search filter
                search.Filter = "(&(objectClass=user)(anr=" + UserName + "))";

                // specify which property values to return in the search
                search.PropertiesToLoad.Add("givenName");   // first name
                search.PropertiesToLoad.Add("sn");          // last name
                search.PropertiesToLoad.Add("mail");        // smtp mail address
                // perform the search
                search.Filter = String.Format("(sAMAccountName={0})", UserName);
                SearchResult result = search.FindOne();


                foreach (SearchResult sResultSet in search.FindAll())
                {
                    //get AD full name and display on home
                    string firstname = GetProperty(sResultSet, "givenName");
                    string lastname = GetProperty(sResultSet, "sn");
                    string fullname = firstname + " " + lastname;

                    // title
                    string title = GetProperty(sResultSet, "title");
                    // telephonenumber
                    string phonenumber = GetProperty(sResultSet, "telephoneNumber");
                    //extention
                    string ext = GetProperty(sResultSet, "otherTelephone");
                    // email address
                    string emailaddress = GetProperty(sResultSet, "mail");

                    System.Web.HttpContext.Current.Application["UserFullName"] = fullname;

                    System.Web.HttpContext.Current.Application["UserEmail"] = emailaddress;

                    using (SqlConnection con = new SqlConnection(cs))
                    {
                        SqlCommand cmd = new SqlCommand("spUpdateUser", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@LoginID", UserName);
                        cmd.Parameters.AddWithValue("@FirstName", firstname);
                        cmd.Parameters.AddWithValue("@LastName", lastname);
                        cmd.Parameters.AddWithValue("@Email", emailaddress);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                }
               
            
         
        }

        public static string GetProperty(SearchResult searchResult, string PropertyName)
        {
            //member fuction for AD validation
            if (searchResult.Properties.Contains(PropertyName))
            {
                return searchResult.Properties[PropertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }


        public bool GetAdminState(string email, string system)
        {
            bool result = false;

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("spGetAdminState", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@System", system);
                con.Open();
                result = Convert.ToBoolean(cmd.ExecuteScalar());
            }

            return result;
        }


    }
}