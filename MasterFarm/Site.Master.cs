﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MasterFarm
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["Myconn"].ToString());
        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                getmenu();


                string MenuType = Request.QueryString["MID2"];
                string subMenutype = Request.QueryString["S2ID"];

                string MenuID = Request.QueryString["MID1"];
                string subMenuID = Request.QueryString["S1ID"];

                if (MenuType != null)
                {
                    SqlCommand cmd1 = new SqlCommand("Usp_GetAllRoles", con);

                    if (MenuType != null)
                    {
                        cmd1.Parameters.AddWithValue("@Menutype", MenuType);
                    }
                    else if (subMenutype != null)
                    {
                        cmd1.Parameters.AddWithValue("@Menutype", subMenutype);
                    }

                    if (MenuID != null)
                    {
                        cmd1.Parameters.AddWithValue("@ID", MenuID);
                    }
                    else if (subMenuID != null)
                    {
                        cmd1.Parameters.AddWithValue("@ID", subMenuID);
                    }

                    cmd1.Parameters.AddWithValue("@RID", Session["RoleID"]);
                    cmd1.Parameters.AddWithValue("@UID", Session["UserID"]);

                    cmd1.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da1 = new SqlDataAdapter();
                    da1.SelectCommand = cmd1;
                    DataSet ds1 = new DataSet();
                    da1.Fill(ds1);
                    if (ds1.Tables.Contains("Table") == true)
                    {
                        if (ds1.Tables[0].Rows.Count > 0)
                        {
                            if (ds1.Tables[0].Rows[0]["MRead"].ToString() == "1")
                            {
                                PnlMain.Enabled = false;
                            }
                            else if (ds1.Tables[0].Rows[0]["MBoth"].ToString() == "1")
                            {
                                PnlMain.Enabled = true;
                            }
                            else if (ds1.Tables[0].Rows[0]["MWrite"].ToString() == "1")
                            {
                                PnlMain.Enabled = true;
                            }

                            else
                            {
                                PnlMain.Enabled = false;
                            }

                        }
                    }
                }


            }
        }


        public void getmenu()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("Login.aspx");
            }
            else
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("Usp_GetAllMenuofRole", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID1", Session["UserID"]);
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(ds);
                StringBuilder ST = new StringBuilder();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    //ST.Append('<div class="left-menu">');
                    //ST.Append("<ul>");
                    ST.Append("<h1>" + Session["UserID"] + "</h1>");
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string xid1 = ds.Tables[0].Rows[i]["MenuID"].ToString();
                        string xid2 = ds.Tables[0].Rows[i]["MenuIDx"].ToString();
                        //string li = "<li class='has-sub'>" + "<a href =" + ds.Tables[0].Rows[i]["MenuURL"].ToString() + "?MID1=" + xid1 + "&MID2=" + "M" + ">" +"<div ></div>"+ ds.Tables[0].Rows[i]["MenuName"].ToString() + "</a>";
                        string li = "<li>" + "<a tabindex='-1' href =" + "#"+ "?MID1=" + xid1 + "&MID2=" + "M" + ">" + ds.Tables[0].Rows[i]["MenuName"].ToString() + "</a>";
                        ST.Append(li);

                        if (ds.Tables[0].Rows[i]["MenuID"].ToString() != null)
                        {
                            SqlCommand cmd1 = new SqlCommand("Usp_Submenu", con);
                            cmd1.CommandType = CommandType.StoredProcedure;
                            cmd1.Parameters.AddWithValue("@userID", ds.Tables[0].Rows[i]["UID"].ToString());
                            cmd1.Parameters.AddWithValue("@MenuID", ds.Tables[0].Rows[i]["MenuID"].ToString());
                            DataSet ds1 = new DataSet();
                            SqlDataAdapter da1 = new SqlDataAdapter();
                            da1.SelectCommand = cmd1;
                            da1.Fill(ds1);

                            ST.Append("<ul>");

                            for (int j = 0; j < ds1.Tables[0].Rows.Count; j++)
                            {
                                string yid1 = ds1.Tables[0].Rows[j]["MenuID"].ToString();
                                string yid2 = ds1.Tables[0].Rows[j]["subMenuIDx"].ToString();

                                if (ds1.Tables[0].Rows[j]["SubmenuName"].ToString() != null)
                                {
                                    string lix = "<li>" + "<a href =" + ds1.Tables[0].Rows[j]["SubMenuURL"].ToString() + "?S1ID=" + yid1 + "&S2ID=" + "S" + ">" + ds1.Tables[0].Rows[j]["SubmenuName"].ToString() + "</a>" + "</li>";
                                    ST.Append(lix);
                                }
                            }
                        }
                        ST.Append("</ul>");
                        ST.Append("</li>");


                    }
                    //ST.Append("</ul>");
                   // ST.Append("</div>");
                    Literal1.Text = ST.ToString();
                }
            }

        }




    }
}