using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using static HrcApi.Models.Auth.Useridentity;

namespace HrcApi.Filters
{
    public class MyAuthAttribute : Attribute, IAuthorizationFilter
    {
        public bool AllowMultiple => throw new NotImplementedException();

        public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            IEnumerable<string> headers;
            if (actionContext.Request.Headers.TryGetValues(name: "token", out headers))
            {
                var UserID = Convert.ToInt32(JwtTools.Decode(jwtStr: headers.First())["UserID"]);
                var RoleID = Convert.ToInt32(JwtTools.Decode(jwtStr: headers.First())["RoleID"]);
                (actionContext.ControllerContext.Controller as ApiController).User = new ApplicationUser(UserID, RoleID);
                return await continuation();
            }
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }
    }
}