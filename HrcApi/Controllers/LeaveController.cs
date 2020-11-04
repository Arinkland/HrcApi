using HrcApi.Filters;
using HrcApi.Models;
using HrcApi.Models.Auth;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HrcApi.Controllers
{
    public class ViewModelLeave : Leave
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string CI_Name { get; set; }
        public string UserTel { get; set; }
    }

    [RoutePrefix("api/Leave")]
    public class LeaveController : ApiController
    {
        SqlSugarClient db = DBhelp.GetInstance();
        [MyAuth]
        //查询自己的请假申请
        public IHttpActionResult GetList()
        {
            try
            {
                int id = ((Useridentity)User.Identity).UserId;
                var list = db.Queryable<UserInfo, Leave, CategoryItems>((a, b, c) => new JoinQueryInfos(
                      JoinType.Left, b.UserID == a.UserID,
                      JoinType.Left, b.LeaveState == c.CI_ID
                      )).Where((a, b, c) => c.C_Category == "LeaveStart" && a.UserID == id)
                    .Select<ViewModelLeave>().ToList();
                var json = new { code = "0", msg = "", data = list };
                return Json(json);
            }
            catch (Exception e)
            {
                var json = new { code = "0", msg = e.Message };
                return Json(json);
            }
        }
        [MyAuth]
        //查询自己的请假申请
        public IHttpActionResult GetList(string LeaveStartTime, string LeaveEndTime)
        {
            try
            {
                DateTime StartTime;
                DateTime EndTime;
                int id = ((Useridentity)User.Identity).UserId;
                var list = db.Queryable<UserInfo, Leave, CategoryItems>((a, b, c) => new JoinQueryInfos(
                      JoinType.Left, b.LeaveState == c.CI_ID,
                      JoinType.Left, b.UserID == a.UserID
                      )).Where((a, b, c) => c.C_Category == "LeaveStart" && a.UserID == id);
                if (LeaveStartTime != null && LeaveEndTime != null)
                {
                    StartTime = Convert.ToDateTime(LeaveStartTime);
                    EndTime = Convert.ToDateTime(LeaveEndTime);
                    EndTime = Convert.ToDateTime(EndTime.ToString("yyyy-MM-dd 23:59:59"));
                    list = list.Where((a, b) => b.LeaveTime >= StartTime && b.LeaveTime <= EndTime);
                    var json = new { code = "0", msg = "", data = list };
                    return Json(json);
                }
                else if (LeaveStartTime != null && LeaveEndTime == null)
                {
                    StartTime = Convert.ToDateTime(LeaveStartTime);
                    list = list.Where((a, b) => b.LeaveTime >= StartTime);
                    var json = new { code = "0", msg = "", data = list };
                    return Json(json);
                }
                else if (LeaveStartTime == null && LeaveEndTime != null)
                {
                    EndTime = Convert.ToDateTime(LeaveEndTime);
                    EndTime = Convert.ToDateTime(EndTime.ToString("yyyy-MM-dd 23:59:59"));
                    list = list.Where((a, b) => b.LeaveTime <= EndTime);
                    var json = new { code = "0", msg = "", data = list };
                    return Json(json);
                }
                else
                {
                    var json = new { code = 0, msg = "ok", data = list };
                    return Json(json);
                }
            }
            catch (Exception e)
            {
                var json = new { code = "0", msg = e.Message };
                return Json(json);
            }
        }
        [HttpPut]
        [MyAuth]
        //批量删除请假
        public string DelLeaveAll(int[] id)
        {
            try
            {
                int count = 0;
                //Leave leave = db.Queryable<Leave>().First(r => r.LeaveID == item);
                //根据主键ID删除
                count = db.Deleteable<Leave>().In(id).ExecuteCommand();

                if (count > id.Length)
                {
                    return "全部删除成功";
                }
                else if (count > 0 && count < id.Length)
                {
                    return "部分删除成功,部分删除失败";
                }
                else
                {
                    return "删除失败";
                }
            }
            catch (Exception e)
            {

                return "错误信息:" + e.Message;
            }
        }
        [HttpPost]
        [Route("LeaveAdd")]
        [MyAuth]
        //申请请假
        public string LeaveAdd(Leave leave)
        {
            try
            {
                int id = ((Useridentity)User.Identity).UserId;
                List<Leave> list = db.Queryable<Leave>().Where(r => r.UserID == id && r.LeaveState == 3).ToList();
                //查询是否存在重复请假
                foreach (var item in list)
                {
                    if (leave.LeaveStartTime < item.LeaveStartTime && leave.LeaveEndTime < item.LeaveStartTime)
                    {

                    }
                    else if (leave.LeaveStartTime > item.LeaveEndTime)
                    {

                    }
                    else
                    {
                        return "该时段已存在请假";
                    }
                }
                if (leave.LeaveHalfDay == "上午")
                {
                    leave.LeaveStartTime = Convert.ToDateTime(Convert.ToDateTime(leave.LeaveStartTime).ToString("yyyy-MM-dd 08:00:00"));
                    leave.LeaveEndTime = Convert.ToDateTime(Convert.ToDateTime(leave.LeaveEndTime).ToString("yyyy-MM-dd 12:00:00"));
                }
                if (leave.LeaveHalfDay == "下午")
                {
                    leave.LeaveStartTime = Convert.ToDateTime(Convert.ToDateTime(leave.LeaveStartTime).ToString("yyyy-MM-dd 14:00:00"));
                    leave.LeaveEndTime = Convert.ToDateTime(Convert.ToDateTime(leave.LeaveEndTime).ToString("yyyy-MM-dd 18:00:00"));
                }
                if (leave.LeaveHalfDay == "全天")
                {
                    leave.LeaveEndTime = Convert.ToDateTime(Convert.ToDateTime(leave.LeaveEndTime).ToString("yyyy-MM-dd 23:59:59"));
                }
                leave.LeaveState = 3;
                leave.LeaveTime = DateTime.Now;
                //leave.UserID = Convert.ToInt32(HttpContext.Current.Session["id"].ToString());
                leave.UserID = id;
                int count = db.Insertable(leave).ExecuteCommand();
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
        [Route("GetLeave")]
        [MyAuth]
        //获取请假审批列表
        public IHttpActionResult GetLeave()
        {
            try
            {
                int UserID = ((Useridentity)User.Identity).UserId;
                //int UserID = 1023;
                //查询当前用户权限
                UserInfo userInfo = db.Queryable<UserInfo>().First(r => r.UserID == UserID);
                //总经理可以查看所有部门的请假
                if (userInfo.RoleID == 1)
                {
                    var list = from a in db.UserInfo
                               join b in db.Department on a.DepartmentID equals b.DepartmentID into c
                               from d in c.DefaultIfEmpty()
                               join e in db.Leave on a.UserID equals e.UserID
                               where e.LeaveState == 3
                               select new
                               {
                                   a.UserID,
                                   a.UserName,
                                   a.UserTel,
                                   d.DepartmentName,
                                   d.DepartmentID,
                                   e.LeaveID,
                                   e.LeaveStartTime,
                                   e.LeaveEndTime,
                                   e.LeaveHalfDay,
                                   e.LeaveTime,
                                   e.LeaveReason
                               };
                    var json = new { code = 0, msg = "", data = list };
                    return Json(json);
                }
                else
                {
                    //部门经理只能看本部门的
                    var list = from a in db.UserInfo
                               join b in db.Department on a.DepartmentID equals b.DepartmentID into c
                               from d in c.DefaultIfEmpty()
                               join e in db.Leave on a.UserID equals e.UserID
                               where d.DepartmentID == userInfo.DepartmentID && e.LeaveState == 3 && a.RoleID == 5
                               select new
                               {
                                   a.UserID,
                                   a.UserName,
                                   a.UserTel,
                                   d.DepartmentName,
                                   d.DepartmentID,
                                   e.LeaveID,
                                   e.LeaveStartTime,
                                   e.LeaveEndTime,
                                   e.LeaveHalfDay,
                                   e.LeaveTime,
                                   e.LeaveReason
                               };
                    var json = new { code = 0, msg = "", data = list };
                    return Json(json);
                }
            }
            catch (Exception e)
            {
                var json = new { code = 0, msg = "错误信息:" + e.Message };
                return Json(json);
            }
        }
    }
}
