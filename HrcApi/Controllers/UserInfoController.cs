using HrcApi.Filters;
using HrcApi.Models;
using HrcApi.Models.Auth;
using Microsoft.Ajax.Utilities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace HrcApi.Controllers
{
    
    public class ViewModelUserInfo : UserInfo
    {
        public string DepartmentName { get; set; }
        public int DepartmentID { get; set; }
        public string RoleName { get; set; }
        public int RoleID { get; set; }
    }
    [RoutePrefix("api/UserInfo")]
    public class UserInfoController : ApiController
    {
        private static string filePath;
        [HttpGet]
        [Route("login")]
        public IHttpActionResult Login(string LoginName, string LoginPwd)
        {
            try
            {
                var  db = DBhelp.GetInstance();
                UserInfo list = db.Queryable<UserInfo>().First(r => r.LoginName == LoginName && r.LoginPwd == LoginPwd);
                if (list == null)
                {

                    return Ok(new Models.Message() { MyProperty = 401, ErrorMessage = "账号或密码错误" });
                }
                else
                {
                    if (list.UserStatr == 1)
                    {
                        return Ok(new Models.Message() { data = JwtTools.Encoder(new Dictionary<string, object>() { { "UserID", list.UserID }, { "RoleID", list.RoleID } }) });
                    }
                    else
                    {
                        return Ok(new Models.Message() { MyProperty = 402, ErrorMessage = "此账号被锁定" });
                    }

                }
            }
            catch (Exception e)
            {
                return Ok(new Models.Message() { MyProperty = 403, ErrorMessage = e.Message });
            }
        }
        [HttpGet]
        [MyAuth]
        public IHttpActionResult GetUserInfoList(int page, int limit)
        {
            var db = DBhelp.GetInstance();
            try
            {
                int offset = (page - 1) * limit;
                var list = db.Queryable<UserInfo, Department>((a, b) => new JoinQueryInfos(
                       JoinType.Left, a.DepartmentID == b.DepartmentID
                       )).Where((a,b)=>a.isDel==0).Select<ViewModelUserInfo>().ToList();
                var data = list.OrderBy(r => r.UserID).Skip(offset).Take(limit);
                var json = new { code = 0, msg = "ok", data, count = list.Count() };
                return Json(json);
            }
            catch (Exception e)
            {
                var json = new { code = 0, msg = e.Message };
                return Json(json);
            }
        }
        [HttpGet]
        [MyAuth]
        public IHttpActionResult GetUserInfoList(int page, int limit, string DeparmentName, string UserName)
        {
            int offset = (page - 1) * limit;
            DeparmentName = DeparmentName ?? string.Empty;
            UserName = UserName ?? string.Empty;
            var db = DBhelp.GetInstance();
            try
            {
                var list = db.Queryable<UserInfo, Department>((a, b) => new JoinQueryInfos(
                     JoinType.Left, a.DepartmentID == b.DepartmentID
                     )).Where((a) => a.isDel == 0).Select<ViewModelUserInfo>().ToList();
                if (!string.IsNullOrWhiteSpace(DeparmentName) && !string.IsNullOrWhiteSpace(UserName))
                {
                    list = list.Where(r => r.DepartmentName.Contains(DeparmentName) && r.UserName.Contains(UserName)).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(UserName))
                {
                    list = list.Where(r => r.UserName.Contains(UserName)).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(DeparmentName))
                {
                    list = list.Where(r => r.DepartmentName.Contains(DeparmentName)).ToList();
                }
                var data = list.OrderBy(r => r.UserID).Skip(offset).Take(limit);
                var json = new { code = 0, msg = "ok", data, count = list.Count() };
                return Json(json);
            }
            catch (Exception e)
            {
                var json = new { code = 0, msg = e.Message };
                return Json(json);
            }
        }
        [HttpPost]
        [Route("UploadImage")]
        [MyAuth]
        public IHttpActionResult UploadImage()
        {
            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var file = HttpContext.Current.Request.Files[0];
                string fileName = file.FileName;
                filePath = HttpContext.Current.Server.MapPath("~/image/") + fileName;
                file.SaveAs(filePath);
                var json = new { mge = "ok" };
                return Json(json);
            }
            else
            {
                var json = new { mge = "no" };
                return Json(json);
            }
        }
        [HttpPost]
        [Route("UserInfoAdd")]
        [MyAuth]
        public string UserInfoAdd(UserInfo userInfo)
        {
            var db = DBhelp.GetInstance();
            try
            {
                UserInfo isRepeat = db.Queryable<UserInfo>().First(r => r.UserNumber == userInfo.UserNumber);
                UserInfo Repeat = db.Queryable<UserInfo>().First(r => r.UserTel == userInfo.UserTel);
                if (isRepeat != null)
                {
                    return "编号重复";
                }
                else if (Repeat != null)
                {
                    return "手机号重复";
                }
                else
                {
                    userInfo.LoginName = userInfo.UserTel;
                    userInfo.LoginPwd = "123456";
                    userInfo.UserIphone = filePath;
                    userInfo.EntryTime = DateTime.Now;
                    userInfo.UserStatr = 1;
                    if (userInfo.DepartmentID == 0)
                    {
                        userInfo.DepartmentID = 4;
                    }
                    int count = db.Insertable(userInfo).ExecuteCommand();
                    if (count > 0)
                    {
                        return "ok";
                    }
                    else
                    {
                        return "no";
                    }
                }
            }
            catch (Exception e)
            {
                return "错误信息:" + e.Message;
            }
        }
        [HttpPut]
        [MyAuth]
        public string UserInfoIsDel(string[] number)
        {
            string[] messageOK = new string[number.Length];
            string[] messageNO = new string[number.Length];
            int okindex = 0;
            int noindex = 0;
            string ok = string.Empty;
            string no = string.Empty;
            var db = DBhelp.GetInstance();
            try
            {
                foreach (var item in number)
                {
                    UserInfo userInfo = db.Queryable<UserInfo>().First(r => r.UserNumber == item);
                    if (userInfo != null)
                    {
                        userInfo.isDel = 1;
                        int count = db.Updateable(userInfo).ExecuteCommand();
                        if (count > 0)
                        {
                            messageOK[okindex] = item;
                            okindex++;
                        }
                        else
                        {
                            messageNO[noindex] = item;
                            noindex++;
                        }
                    }
                }
                if (okindex != 0)
                {
                    for (int i = 0; i < messageOK.Length; i++)
                    {
                        if (messageOK[i] != null)
                        {
                            ok = ok + messageOK[i].ToString() + " ";
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (noindex != 0)
                {
                    for (int i = 0; i < messageOK.Length; i++)
                    {
                        if (messageNO[i] != null)
                        {
                            no = no + messageNO[i].ToString() + " ";
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (ok != "" && no != "")
                {
                    return "编号为 " + ok + "的员工删除成功" + "ID为" + no + "的员工删除失败";
                }
                else if (ok != "" && no == "")
                {
                    return "编号为 " + ok + "的员工删除成功";
                }
                else if (ok == "" && no != "")
                {
                    return "编号为" + no + "的员工删除失败";
                }
                else
                {
                    return "未找到此员工";
                }
            }
            catch (Exception e)
            {
                return "错误信息:" + e.Message;
            }
        }
        [HttpPost]
        [MyAuth]
        public string UserInfoIsDel(int id)
        {
            var db = DBhelp.GetInstance();
            try
            {
                UserInfo userInfo = db.Queryable<UserInfo>().First(r => r.UserID == id);
                if (userInfo != null)
                {
                    userInfo.isDel = 1;
                    int count = db.Updateable(userInfo).ExecuteCommand();
                    if (count > 0)
                    {
                        return "ok";
                    }
                    else
                    {
                        return "no";
                    }
                }
                else
                {
                    return "未找到此员工";
                }
            }
            catch (Exception e)
            {

                return "错误信息:" + e.Message;
            }
        }
        [HttpPost]
        [Route("UserInfoUpdate")]
        [MyAuth]
        //更新用户信息
        public string UserInfoUpdate(UserInfo userInfo)
        {
            var db = DBhelp.GetInstance();
            try
            {
                //查询账号给传过来的实体赋值
                UserInfo NamePwd = db.Queryable<UserInfo>().First(r => r.UserID == userInfo.UserID);
                //查询是否有重复手机号
                UserInfo phone = db.Queryable<UserInfo>().First(r => r.UserTel == userInfo.UserTel && r.UserID != userInfo.UserID);
                if (phone != null)
                {
                    return "手机号不能重复";
                }
                if (userInfo.UserID != 1 && userInfo.RoleID == 1)
                {
                    return "你不能当总经理";
                }
                userInfo.UserIphone = filePath;
                userInfo.LoginName = NamePwd.LoginName;
                userInfo.LoginPwd = NamePwd.LoginPwd;
                userInfo.UserStatr = 1;
                int count = db.Updateable(userInfo).ExecuteCommand();
                if (count > 0)
                {
                    return "ok";
                }
                else
                {
                    return "no";
                }
            }
            catch (Exception e)
            {

                return "错误信息:" + e.Message;
            }
        }
        [HttpGet]
        [MyAuth]
        //查询个人信息
        public IHttpActionResult GetUserInfoList()
        {
            var db = DBhelp.GetInstance();
            try
            {
                int UserID = ((Useridentity)User.Identity).UserId;
                var list = db.Queryable<UserInfo, Department, Role>((a, b, c) => new JoinQueryInfos(
                      JoinType.Left, a.DepartmentID == b.DepartmentID,
                      JoinType.Left, a.RoleID == c.RoleID
                      )).Select<ViewModelUserInfo>().ToList();
                var json = new { msg = "ok", data = list };
                return Json(json);
            }
            catch (Exception e)
            {
                var json = new { msg = "错误信息:" + e.Message };
                return Json(json);
            }
        }
    }
}
