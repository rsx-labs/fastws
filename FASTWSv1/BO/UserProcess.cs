using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FASTWSv1.Common;
using FASTWSv1.Helpers;
using FASTWSv1.Providers;
using System.Text.RegularExpressions;

namespace FASTWSv1.BO
{
    public class UserProcess
    {
        Providers.EmailProvider email = new Providers.EmailProvider();

        public UserProcess(){}

        public int RegisterUser(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {

                EmployeeProcess employeeProcess = new EmployeeProcess();

                Registration[] registrants = (from reg in db.Registrations
                                              where reg.EmployeeID == employeeID
                                              select reg).ToArray();

                Employee employee = employeeProcess.GetEmployeeByID(employeeID);
      
                if (employee != null)
                {
                    if (registrants.Count() > 0)
                    {
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.REGISTRATION, employeeID, "FAILED : USER EXISTS");
                        return ReturnValues.FAILED;
                    }
                    else
                    {
                        Registration newReg = new Registration();

                        newReg.EmployeeID = employeeID;

                        string password;

                        if (ConfigurationHelper.SendEmail)
                        {
                            password = System.Web.Security.Membership.GeneratePassword(8, 0);
                            password = Regex.Replace(password, @"[^a-zA-Z0-9]", m => "$");
                        }
                        else
                        {
                            //Use the current user employee ID if email is out
                            password = employeeID.ToString();
                        }

                        newReg.Password = Providers.MD5HashProvider.CreateMD5Hash(password);
                        newReg.DateStamp = DateTime.Now;
                        newReg.Status = 0;

                        db.Registrations.Add(newReg);

                        db.SaveChanges();

                        if(ConfigurationHelper.SendEmail)
                        {
                            email.SendUserRegistrationEmail(EmailProvider.EmailType.REGISTRATION, employeeID, 
                                        employee.FirstName + " " + employee.LastName, employee.EmailAddress, password);
                        }

                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.REGISTRATION, employeeID, "SUCCESSFUL");

                        return ReturnValues.SUCCESS;
                    }
                }

            }

            Helpers.Logger.AddToAuditTrail(Logger.UserAction.REGISTRATION, employeeID, "NOT FOUND");
            return ReturnValues.NOT_FOUND;
        }

        public int LoginUser(int employeeID, string hashedPassword)
        {
            using (var db = new FASTDBEntities())
            {
                EmployeeProcess employeeProcess = new EmployeeProcess();

                if ( employeeProcess.GetEmployeeByID(employeeID) != null)
                {
                    var registrations = from reg in db.Registrations
                                        where reg.EmployeeID == employeeID
                                        && reg.Password == hashedPassword
                                        select reg;

                    if (registrations.Count() > 0 )
                    {
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.LOGIN, employeeID, "SUCCESSFUL");
                        return ReturnValues.SUCCESS;
                    }
                    else
                    {
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.LOGIN, employeeID, "FAILED");
                        return ReturnValues.FAILED;
                    }
                }
            }

            Helpers.Logger.AddToAuditTrail(Logger.UserAction.LOGIN, employeeID, "NOT FOUND");
            return ReturnValues.NOT_FOUND;
        }

        public int ChangePassword(int employeeID, string oldPassword , string newPassword)
        {
            using (var db = new FASTDBEntities())
            {
                EmployeeProcess employeeProcess = new EmployeeProcess();
                Employee employee = employeeProcess.GetEmployeeByID(employeeID);

                if (employeeProcess.GetEmployeeByID(employeeID) != null)
                {
                    var registrations = from reg in db.Registrations
                                        where reg.EmployeeID == employeeID
                                        && reg.Password == oldPassword
                                        select reg;

                    if ( registrations.Count() > 0 )
                    {
                        foreach(Registration userReg in registrations)
                        {
                            userReg.Password = newPassword;
                        }

                        db.SaveChanges();

                        //TODO : Email employee about the successful change
                        if (ConfigurationHelper.SendEmail)
                        {
                            email.SendUserRegistrationEmail(EmailProvider.EmailType.CHANGE_PASSWORD, 0, 
                                employee.FirstName + " " + employee.LastName, employee.EmailAddress, "");
                        }
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.CHANGE_PASSWORD, employeeID, "SUCCESSFUL");
                        return ReturnValues.SUCCESS;
                    }
                }
            }
            Helpers.Logger.AddToAuditTrail(Logger.UserAction.CHANGE_PASSWORD, employeeID, "FAILED");
            return ReturnValues.FAILED;
        }

        public int ResetPassword(int employeeID)
        {

            using(var db = new FASTDBEntities())
            {
                EmployeeProcess employeeProcess = new EmployeeProcess();

                Employee employee = employeeProcess.GetEmployeeByID(employeeID);

                if (employee != null)
                {
                    var registrations = from reg in db.Registrations
                                        where reg.EmployeeID == employeeID
                                        select reg;

                    if( registrations.Count() > 0)
                    {
                        foreach(Registration regUser in registrations)
                        {
                            string password;

                            if (ConfigurationHelper.SendEmail)
                            {
                                password = System.Web.Security.Membership.GeneratePassword(6, 0);
                                password = Regex.Replace(password, @"[^a-zA-Z0-9]", m => "$");
                            }
                            else
                            {
                                //Use the current user employee ID if email is out
                                password = employeeID.ToString();
                            }                          
                            
                            regUser.Password = Providers.MD5HashProvider.CreateMD5Hash(password);
                            regUser.DateStamp = DateTime.Now;

                            db.SaveChanges();

                            if (ConfigurationHelper.SendEmail)
                            {
                                email.SendUserRegistrationEmail(EmailProvider.EmailType.RESET_PASSWORD, employeeID, 
                                        employee.FirstName + " " + employee.LastName, employee.EmailAddress, password);
                            }
                            Helpers.Logger.AddToAuditTrail(Logger.UserAction.RESET_PASSWORD, employeeID, "SUCCESSFUL");
                            return ReturnValues.SUCCESS;
                        }
                    }
                }
            }
            Helpers.Logger.AddToAuditTrail(Logger.UserAction.RESET_PASSWORD, employeeID, "FAILED : NOT FOUND");
            return ReturnValues.NOT_FOUND;
        }
    }
}