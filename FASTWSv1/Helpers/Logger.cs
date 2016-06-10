using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FASTWSv1.Helpers
{
    public static class Logger
    {
        public enum UserAction
        {
            REGISTRATION,
            LOGIN,
            CHANGE_PASSWORD,
            RESET_PASSWORD,
            ADD_ASSET,
            ASSIGN_ASSET,
            TRANSFER_ASSET,
            ACCEPT,
            APPROVE,
            EMAIL,
            RELEASE
        };

      
        public static void AddToAuditTrail(UserAction action, int employeeID, string additionalData, string assetTag="", int assignmentID=0)
        {
            AuditTrail newLog = new AuditTrail();
            newLog.Date = DateTime.Now;
            newLog.EmployeeID = employeeID;
            newLog.AdditionalInformation = "RESULT : " + additionalData;
            newLog.AssetTag = assetTag;
            newLog.AssignmentID = assignmentID.ToString();

            switch (action)
            {
                case UserAction.RELEASE:
                    newLog.Action = "Asset Release";
                    break;
                case UserAction.REGISTRATION:
                    newLog.Action = "User Registration";
                    break;
                case UserAction.LOGIN:
                    newLog.Action = "User Login";
                    break;
                case UserAction.CHANGE_PASSWORD:
                    newLog.Action = "Change Password";
                    break;
                case UserAction.RESET_PASSWORD:
                    newLog.Action = "Reset Password";
                    break;
                case UserAction.ADD_ASSET:
                    newLog.Action = "New Asset";
                    break;
                case UserAction.ASSIGN_ASSET:
                    newLog.Action = "Assign Asset";
                    break;
                case UserAction.ACCEPT:
                    newLog.Action = "Asset Acceptance";
                    break;
                case UserAction.TRANSFER_ASSET:
                    newLog.Action = "Asset Transfer";
                    break;
                case UserAction.APPROVE:
                    newLog.Action = "Request Approval";
                    break;
                case UserAction.EMAIL:
                    newLog.Action = "Sending Email";
                    break;
                default:
                    newLog.Action = "Unknown Action";
                    break;
            }

            using (var db = new FASTDBLogEntities())
            {
                db.AuditTrails.Add(newLog);
                db.SaveChanges();
            }
        }


    }
}