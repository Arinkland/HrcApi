using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace HrcApi.Models.Auth
{
    public class Useridentity: IIdentity
    {
        public Useridentity(int UserID, int RoleID)
        {
            UserId = UserID;
            RoleId = RoleID;
        }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Name => throw new NotImplementedException();

        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();
        public class ApplicationUser : IPrincipal
        {
            public ApplicationUser(int UserID, int RoleID)
            {
                Identity = new Useridentity(UserID, RoleID);
            }
            public IIdentity Identity { get; set; }

            public bool IsInRole(string role)
            {
                throw new NotImplementedException();
            }
        }
    }
}