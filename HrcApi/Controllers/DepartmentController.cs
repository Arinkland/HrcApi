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
using System.Web.Http;

namespace HrcApi.Controllers
{
    [RoutePrefix("api/Department")]
    public class DepartmentController : ApiController
    {
        SqlSugarClient db=  DBhelp.GetInstance();
        [HttpGet]
        [MyAuth]
        //部门列表展示
        public IHttpActionResult GetList(int page, int limit)
        {

            int offset = (page - 1) * limit;
            try
            {
                //查询未删除的数据
                var list = db.Queryable<Department>().Where(r => r.isDel == 0).ToList();
                var list1 = list.OrderBy(r => r.DepartmentID).Skip(offset).Take(limit);
                var json = new { code = 0, msg = "", data = list1, count = list.Count() };
                return Json(json);
            }
            catch (Exception e)
            {
                var json = new { code = 0, msg = "错误信息:" + e.Message };
                return Json(json);
            }
        }
        [HttpPost]
        [MyAuth]
        //删除部门
        public string DeparmentISDel(int id)
        {
            int count = 0;
            Department a = db.Queryable<Department>().First(r => r.DepartmentID == id);
            try
            {
                db.Ado.BeginTran();
                if (id == 1 || id == 2 || id == 3 || id == 4 || id == 5 || id == 6)
                {
                    return "基础部门不能删除";
                }
                else
                {
                    if (a != null)
                    {
                        //把部门是否删除设置为1
                        a.isDel = 1;
                        count += db.Updateable(a).ExecuteCommand();
                        if (count > 0)
                        {
                            //查询当前部门的员工
                            var list = db.Queryable<UserInfo>().Where(r => r.DepartmentID == id).ToList();
                            if (list != null)
                            {
                                //把当前部门的员工全部放到临时部
                                foreach (var item in list)
                                {
                                    item.DepartmentID = 4;
                                }
                                count += db.Updateable(list).ExecuteCommand();
                            }
                            db.Ado.CommitTran();
                            if (count >= list.Count() + 1)
                            {
                                return "ok";
                            }
                            else
                            {
                                return "成功但失败,员工未转移至临时部门";
                            }
                        }
                        else
                        {
                            return "no";
                        }
                    }
                    else
                    {
                        return "未找到此部门";
                    }
                }
            }
            catch (Exception e)
            {
                db.Ado.RollbackTran();
                return "错误信息:" + e.Message;
            }
        }
        [HttpPut]
        [MyAuth]
        //批量删除部门
        public string DeparmentISDel([FromBody] int[] DepartmentID)
        {
            try
            {
                //存储成功删除的部门ID
                int[] messageOK = new int[DepartmentID.Length];
                //存储删除失败的部门ID
                int[] messageNO = new int[DepartmentID.Length];
                int okindex = 0;
                int noindex = 0;
                string ok = string.Empty;
                string no = string.Empty;
                foreach (var item in DepartmentID)
                {
                    int count = 0;
                    if (item == 1 || item == 2 || item == 3 || item == 4 || item == 5 || item == 6)
                    {
                        messageNO[noindex] = item;
                        noindex++;
                        continue;
                    }
                    else
                    {
                        Department department = db.Queryable<Department>().First(r => r.DepartmentID == item);
                        try
                        {
                            //开启事务,好像没用？
                            db.Ado.BeginTran();
                            if (department != null)
                            {
                                department.isDel = 1;
                                count+=  db.Updateable(department).ExecuteCommand();
                                if (count > 0)
                                {
                                    var list = db.Queryable<UserInfo>().Where(r => r.DepartmentID == item).ToList();
                                    if (list.Count>0)
                                    {
                                        foreach (var item1 in list)
                                        {
                                            item1.DepartmentID = 4;
                                        }
                                        count += db.Updateable(list).ExecuteCommand();
                                    }
                                    if (count >= list.Count() + 1)
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
                                else
                                {
                                    messageNO[noindex] = item;
                                    noindex++;
                                }
                            }
                            //提交事务
                            db.Ado.CommitTran();
                        }
                        catch (Exception e)
                        {
                            //回滚
                            db.Ado.RollbackTran();
                            messageNO[noindex] = item;
                            noindex++;
                            continue;
                        }
                    }
                }
                if (okindex != 0)
                {
                    for (int i = 0; i < messageOK.Length; i++)
                    {
                        if (messageOK[i] != 0)
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
                        if (messageNO[i] != 0)
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
                    return "ID为 " + ok + "的部门删除成功" + "ID为" + no + "的部门删除失败";
                }
                else if (ok != "" && no == "")
                {
                    return "ID为 " + ok + "的部门删除成功";
                }
                else if (ok == "" && no != "")
                {
                    return "ID为" + no + "的部门删除失败";
                }
                else
                {
                    return "基础部门不能删除";
                }
            }
            catch (Exception e)
            {
                return "错误信息:" + e.Message;
            }
        }
        [HttpPost]
        [Route("UpdateDeparment")]
        [MyAuth]
        //更新部门
        public string UpdateDeparment([FromBody] Department department)
        {
            try
            {
                //查询出要更新的实体
                var list = db.Queryable<Department>().First(r => r.DepartmentName == department.DepartmentName);
                if (list == null)
                {
                    //执行语句
                    int count= db.Updateable(list).ExecuteCommand();
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
                    return "部门名称重复";
                }
            }
            catch (Exception e)
            {
                return "错误信息:" + e.Message;
            }
        }
        [HttpGet]
        [MyAuth]
        //部门下拉框
        public IHttpActionResult GetList()
        {
            var list = db.Queryable<Department>().ToList();
            var json = new { list };
            return Json(list);
        }
        [HttpGet]
        [Route("GetListbyRole")]
        [MyAuth]
        //通过权限获取部门列表
        public IHttpActionResult GetListbyRole()
        {
            try
            {
                //获取用户权限
                int RoleId = ((Useridentity)User.Identity).RoleId;
                //获取用户ID
                int UserID = ((Useridentity)User.Identity).UserId;
                UserInfo userInfo = db.Queryable<UserInfo>().First(r => r.UserID == UserID);
                //int UserID = 7;
                if (RoleId == 1)
                {
                    var list = db.Queryable<Department>().Where(r => r.isDel == 0).ToList();
                    return Json(list);
                }
                else
                {
                    var list = db.Queryable<Department>().Where(r => r.isDel == 0&&r.DepartmentID==userInfo.DepartmentID).ToList();
                    return Json(list);
                }

            }
            catch (Exception e)
            {
                return Json("错误信息" + e.Message);
            }
        }
    }
}
