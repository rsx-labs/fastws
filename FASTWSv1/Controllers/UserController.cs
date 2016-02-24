using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FASTWSv1.Models;
using FASTWSv1.Common;
using System.Web.Http.Description;

namespace FASTWSv1.Controllers
{
    
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        /// <summary>
        /// Initiates registration of new user
        /// </summary>
        /// <param name="model">This contains information about the new user to register</param>
        /// <returns>
        /// The method returns 1 if OK and 2 if not OK
        /// </returns>
        [HttpPost]
        [Route("Registration")]
        public HttpResponseMessage Registration(ExternalRegistrationViewModel model)
        {

            BO.UserProcess userProcess = new BO.UserProcess();

            int result = userProcess.RegisterUser(model.EmployeeID);

            if (result == ReturnValues.SUCCESS)
            {
                return ReturnMessages.RESPONSE_CREATED();
            }
            else if (result == ReturnValues.FAILED)
            {
                return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.EXISTS);
            }
            else
            {
                return ReturnMessages.RESPONSE_NOTFOUND();
            }

          

        }

        [HttpPost]
        [Route("Login")]
        public HttpResponseMessage Login(ExternalUserLoginViewModel model)
        {
            using (var db = new FASTDBEntities())
            {
                BO.UserProcess userProcess = new BO.UserProcess();

                int result = userProcess.LoginUser(model.EmployeeID, model.HashedPassword);

                if ( result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else if ( result == ReturnValues.FAILED )
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }
            }
            return ReturnMessages.RESPONSE_NOTFOUND();
        }

        [HttpPut]
        [Route("ChangePassword")]
        public HttpResponseMessage ChangePassword(ExternalChangePasswordViewModel model)
        {
            using (var db = new FASTDBEntities())
            {
                BO.UserProcess userProcess = new BO.UserProcess();

                int result = userProcess.ChangePassword(model.EmployeeID,
                                                        model.HashedOldPassword,
                                                        model.HashedNewPassword);

                if( result == ReturnValues.SUCCESS )
                {
                    return ReturnMessages.RESPONSE_OK();
                }
            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
        }

        [HttpPut]
        [Route("ResetPassword")]
        public HttpResponseMessage ResetPassword(ExternalResetPasswordViewModel model)
        {
            using(var db = new FASTDBEntities())
            {
                BO.UserProcess userProcess = new BO.UserProcess();

                int result = userProcess.ResetPassword(model.EmployeeID);

                if ( result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
            }
            return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
        }

        [HttpGet]
        [Route("Access/{employeeID}")]
        public List<UserAccessRights> GetAccessRights(int employeeID)
        {
            List<UserAccessRights> userAccess = new List<UserAccessRights>();

            using ( var db = new FASTDBEntities())
            {
                var rights = from right in db.AccessRights
                             where (( right.EmployeeID == employeeID) && ( right.Status == 1 ))
                             select right;

                foreach( AccessRight right in rights)
                {
                    UserAccessRights access = new UserAccessRights();
                    access.DepartmentID = right.DepartmentID;
                    access.AccessLevel = right.AccessLevel;

                    userAccess.Add(access);
                }
            }

            return userAccess;
        }

        //TODO : This is only for devs, remove in final build
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<Registration> GetAllRegistration()
        {
            List<Registration> registrants = new List<Registration>();

            using (var db = new FASTDBEntities())
            {
                return db.Registrations.ToList();
            }
        }

    }
}