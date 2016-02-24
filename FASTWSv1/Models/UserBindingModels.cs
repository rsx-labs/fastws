using System;
using System.Collections.Generic;

namespace FASTWSv1.Models
{
    //Models for using the UserController
    public class ExternalRegistrationViewModel
    {
        public int EmployeeID { get; set; }
    }

    public class ExternalUserLoginViewModel
    {
        public int EmployeeID { get; set; }
        public string HashedPassword { get; set; }
    }

    public class ExternalChangePasswordViewModel
    {
        public int EmployeeID { get; set; }
        public string HashedOldPassword { get; set; }
        public string HashedNewPassword { get; set; }
    }

    public class ExternalResetPasswordViewModel
    {
        public int EmployeeID { get; set; }
    }

    public class UserAccessRights
    {
        public int AccessLevel { get; set; }
        public int DepartmentID { get; set; }
    }

}