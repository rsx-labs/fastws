using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text;

using FASTWSv1.Helpers;
using System.Threading.Tasks;
namespace FASTWSv1.Providers
{
    public class EmailProvider
    {
        public enum EmailType{
            REGISTRATION,
            RESET_PASSWORD,
            CHANGE_PASSWORD,
            TRANSFER_REQUEST,
            TRANSFER_REQUEST_BY_MIS,
            ACCEPT_REQUEST,
            REQUEST_APPROVED,
            ACCEPTED,
            REJECTED,
            ASSIGNMENT_NEW,
            ASSIGNMENT_MIS,
            ASSIGNMENT_UPDATE,
            ASSIGNMENT_ACCEPTANCE,
            ASSIGNMENT_ACCEPTANCE_MIS,
            ASSIGNMENT_REJECTED,
            ASSIGNMENT_REJECTED_NOTIFICATION,
            LIST_ASSIGMENTS,
            TRANSFER_WDAPPROOVAL_DONE,
            TRANSFER_WOAPPROVAL_DONE,
            TRANSFER_RECEIVE,
            TRANSFER,
            TRANSFER_APPROVED,
            TRANSFER_DENIED,
            TRANSFER_COMPLETE,
            TRANSFER_MIS_ACCEPTANCE,
            TRANSFER_REQUEST_CONFIRMATION,
            TRANSFER_TO_MIS_WTAPPROVAL,
            RELEASE_REQUEST_WTAPPROVAL,
            RELEASE_REQUEST_CONFIRMATION,
            RELEASE_REQUEST_ACCEPTANCE,
            RELEASE_REQUEST_APPROVED,
            RELEASE_REQUEST_ACCEPTED,
            RELEASE_REQUEST_DENIED,
            RELEASE_REQUEST_REJECTED,
            GET_EMAIL_CSS_1,

        };

        private string _subject;
        private string _body;
        private List<string> _mailList;
        private List<string> _ccList;

        public void SendAsync(EmailData mailData)
        {
            _mailList = mailData.ToList;
            _ccList = mailData.CCList;
            _subject = mailData.Subject;
            _body = mailData.Body;
        
            System.Threading.Thread emailThread = new System.Threading.Thread(SMTPSend);

            emailThread.Start();
        }

        private void SMTPSend()
        {
            var message = new MailMessage();

            foreach(string address in _mailList)
            {
                    message.To.Add(new MailAddress(address));
            }

            foreach(string address in _ccList)
            {
                message.CC.Add(new MailAddress(address));
            }
            
            message.From = new MailAddress(ConfigurationHelper.MailFrom);
            message.Subject = _subject;
            message.Body = _body;
            message.IsBodyHtml = true;
            try
            {
                using (var smtp = new SmtpClient())
                {

                    var credentials = new NetworkCredential
                    {
                        UserName = ConfigurationHelper.MailFrom,
                        Password = ConfigurationHelper.MailPassword
                    };

                    smtp.Timeout = 180000; //Set timeout to 3 minutes
                    smtp.Credentials = credentials;
                    smtp.Host = ConfigurationHelper.MailServer;
                    smtp.Port = ConfigurationHelper.MailPort;
                    smtp.EnableSsl = ConfigurationHelper.MailEnableSSL;

                    try
                    {
                        smtp.Send(message);
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.EMAIL, 0, "SUCCESSFULL");
                    }
                    catch (Exception ex)
                    {
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.EMAIL, 0, String.Format("EMAIL ERROR : {0} {1}", message.To[0].Address, ex.Message.ToString()));
                        Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                    }
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                throw ex;
            }
        }
 
        public void SendUserRegistrationEmail(EmailType mailType, int employeeID, string employeeName, string employeeEmail,string password)
        {
            switch (mailType)
            {
                case EmailType.REGISTRATION:
                    //return GetTemplate("Registration.html");
                    string template = GetTemplate("Registration.html");
                    string emailBody = string.Format(template,employeeName, employeeID.ToString(), password);
                    EmailData emailData = new EmailData(employeeEmail, "FASTrack : User Registration", emailBody);
                    SendAsync(emailData);

                    break;
                case EmailType.CHANGE_PASSWORD:
                    //return GetTemplate("ChangePassword.html");
                    template = GetTemplate("ChangePassword.html");
                    emailBody = string.Format(template, employeeName);
                    emailData = new EmailData(employeeEmail, "FASTrack : Change Password", emailBody);
                    SendAsync(emailData);
                    break;
                    
                case EmailType.RESET_PASSWORD:
                    //return GetTemplate("ResetPassword.html");

                    template = GetTemplate("ResetPassword.html");
                    emailBody = string.Format(template,employeeName, employeeID.ToString(), password);
                    emailData = new EmailData(employeeEmail, "FASTrack : Reset Password", emailBody);
                    SendAsync(emailData);
                    break;
                default:
                    //do no thing
                    break;
            }
        }

        public void SendAssignmentEmail(EmailType mailType, int assignmentID, string optVal1="", string optVal2="",int requestorID=0, int receipientID=0)
        {
            BO.AssignmentProcess assProcess = new BO.AssignmentProcess();
            BO.AssetProcess assetProcess= new BO.AssetProcess();
            BO.EmployeeProcess empProcess = new BO.EmployeeProcess();
            vwAssetAssignment assignment = assProcess.GetAssignmentViewByID(assignmentID);
            vwFixAsset asset = new vwFixAsset();
            vwEmployeeList employee = new vwEmployeeList();

            List<string> receipients = new List<string>();
            List<string> ccReceipients = new List<string>();
            string receiver = string.Empty;

            if ( assignment != null)
            {
                asset = assetProcess.GetFixAssetViewByID(assignment.FixAssetID.Value);
                employee = empProcess.GetEmployeeViewByID(assignment.EmployeeID.Value);
            }
            try
            {

                switch (mailType)
                {
                    case EmailType.ASSIGNMENT_ACCEPTANCE:

                        string emailTemplate = GetTemplate("Assignment_Accepted.html");
                        string emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        string emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);

                        receipients = GetAdminMailToList(employee.DepartmentID);
                        EmailData emailData = new EmailData(employee.EmailAddress, receipients, emailSubj, emailBody);

                        SendAsync(emailData);
                        break;

                    case EmailType.ASSIGNMENT_UPDATE:

                        emailTemplate = GetTemplate("Assignment_Update.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);

                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);

                        SendAsync(emailData);
                        break;

                    case EmailType.ASSIGNMENT_ACCEPTANCE_MIS:

                        receipients = GetMISMailToList(employee.DepartmentID);
                        emailTemplate = GetTemplate("Assignment_Accepted.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "MIS");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);

                        emailData = new EmailData(receipients, emailSubj, emailBody);

                        SendAsync(emailData);
                        break;
                    case EmailType.LIST_ASSIGMENTS:

                        receipients = GetAdminMailToList(employee.DepartmentID);
                        emailTemplate = GetTemplate("Employee_Accountability_Form.html");

                        List<vwAssetAssignment> assignmentsList = new List<vwAssetAssignment>();
                        assignmentsList = assProcess.GetAcceptedAssignmentsbyEmpID(employee.EmployeeID);
                        System.Text.StringBuilder list = new System.Text.StringBuilder();
                        if (assignmentsList != null)
                        {
                            foreach (vwAssetAssignment assign in assignmentsList)
                            {
                                list.Append("<tr>");
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.AssetAssignmentID));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.AssetTag));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.SerialNumber));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.TypeDescription));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.Brand));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.Model));
                                list.Append(String.Format("<td style=\"padding:15px\">{0}</td>", assign.DateAssigned.Value.ToShortDateString()));
                                list.Append("</tr>");
                            }
                        }

                        emailBody = String.Format(emailTemplate, employee.EmployeeID, employee.FirstName,
                                                        employee.LastName, employee.Description, employee.GroupName, list.ToString());
                        emailSubj = String.Format("FASTrack : Accountability Form for {0} {1}", employee.FirstName, employee.LastName);
                        SendAsync(new Providers.EmailData(employee.EmailAddress, receipients, emailSubj, emailBody));

                        break;
                    case EmailType.ASSIGNMENT_NEW:

                        emailTemplate = GetTemplate("Assignment_New.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.ASSIGNMENT_MIS:

                        emailTemplate = GetTemplate("Assignment_MIS.html");
                        receipients = GetMISMailToList(employee.DepartmentID);
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "MIS");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.ASSIGNMENT_REJECTED:

                        emailTemplate = GetTemplate("Assignment_Rejected.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.ASSIGNMENT_REJECTED_NOTIFICATION:

                        switch (assignment.AssetStatusID)
                        {
                            case Common.Constants.ASSET_STATUS_FORASSIGNMENT:
                                receipients = GetAdminMailToList(employee.DepartmentID);
                                receiver = "Property Custodians";
                                break;
                            case Common.Constants.ASSET_STATUS_FORRELEASE:
                                receipients = GetManagersMailList(employee.DepartmentID);
                                receiver = "Managers";
                                break;
                            case Common.Constants.ASSET_STATUS_WITH_MIS:
                                receipients = GetMISMailToList(employee.DepartmentID);
                                receiver = "MIS";
                                break;
                            default:
                                break;

                        }

                        emailTemplate = GetTemplate("Assignment_Rejected_Notifications.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, receiver);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);
                        break;
                    case EmailType.TRANSFER_MIS_ACCEPTANCE:

                        emailTemplate = GetTemplate("Transfer_MIS_Acceptance.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "MIS");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);

                        receipients = GetMISMailToList(employee.DepartmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;

                    case EmailType.TRANSFER_WDAPPROOVAL_DONE:

                        emailTemplate = GetTemplate("Transfer_Done.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER_WOAPPROVAL_DONE:

                        vwEmployeeList requestor = empProcess.GetEmployeeViewByID(requestorID);
                        emailTemplate = GetTemplate("Transfer_Done.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_REJECTED:

                        //Inform the admin ( cc the managers and the owner)
                        receipients = GetAdminMailToList(employee.DepartmentID);
                        ccReceipients = GetManagersMailList(employee.DepartmentID);
                        ccReceipients.Add(employee.EmailAddress);

                        emailTemplate = GetTemplate("Release_Rejected.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "Property Custodians");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(receipients, ccReceipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER_TO_MIS_WTAPPROVAL:

                        //Inform MIS about the transfer request to them
                        receipients = GetMISMailToList(employee.DepartmentID);
                        emailTemplate = GetTemplate("Transfer_To_MIS_WithApproval.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "MIS");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER:

                        //Inform the manager about the request
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        receipients = GetManagersMailList(requestor.DepartmentID);
                        emailTemplate = GetTemplate("Transfer_Approval.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "Managers", optVal1, optVal2);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER_REQUEST:

                        //Inform the requestor
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        vwEmployeeList receivingEmployee = empProcess.GetEmployeeViewByID(receipientID);

                        emailTemplate = GetTemplate("Transfer_Request.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName,
                                           receivingEmployee.FirstName + " " + receivingEmployee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignmentID);
                        emailData = new EmailData(requestor.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;

                    case EmailType.TRANSFER_REQUEST_CONFIRMATION:

                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        emailTemplate = GetTemplate("Transfer_Confirmation.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(requestor.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER_REQUEST_BY_MIS:

                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        receivingEmployee = empProcess.GetEmployeeViewByID(receipientID);
                        receipients = GetMISMailToList(requestor.DepartmentID);
                        emailTemplate = GetTemplate("Transfer_Request_By_MIS.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "MIS", receivingEmployee.FirstName + " " + receivingEmployee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;

                    case EmailType.TRANSFER_RECEIVE:

                        //send email to receipient
                        receivingEmployee = empProcess.GetEmployeeViewByID(receipientID);
                        emailTemplate = GetTemplate("Transferred_Receive.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, receivingEmployee.FirstName + " " + receivingEmployee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(receivingEmployee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_ACCEPTANCE:

                        //Inform the admin.
                        receipients = GetAdminMailToList(employee.DepartmentID);
                        emailTemplate = GetTemplate("Release_Request_Acceptance.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "Property Custodians");
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_APPROVED:

                        //inform owner
                        emailTemplate = GetTemplate("Release_Approved.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);
                        break;

                    case EmailType.TRANSFER_DENIED:

                        //inform the requestor
                        emailTemplate = GetTemplate("ransfer_Denied.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, employee.FirstName + " " + employee.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(employee.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);
                        break;
                    case EmailType.RELEASE_REQUEST_CONFIRMATION:

                        //inform the requestor
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        emailTemplate = GetTemplate("Release_Confirmation.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(requestor.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_WTAPPROVAL:

                        //Inform the managers about the request for approval.
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        receipients = GetManagersMailList(requestor.DepartmentID);
                        emailTemplate = GetTemplate("Release_WithApproval.html");
                        emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, "Managers", requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_DENIED:

                        //Inform the requestor
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        receipients = GetManagersMailList(requestor.DepartmentID);

                        emailTemplate = GetTemplate("Release_Denied.html");
                        emailBody = emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(requestor.EmailAddress, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.RELEASE_REQUEST_ACCEPTED:

                        //Inform the Admin
                        requestor = empProcess.GetEmployeeViewByID(requestorID);
                        receipients = GetAdminMailToList(requestor.DepartmentID);
                        emailTemplate = GetTemplate("Release_Accepted.html");
                        emailBody = emailBody = BuildAssignmentEmailBody(assignment, emailTemplate, requestor.FirstName + " " + requestor.LastName);
                        emailSubj = String.Format("FASTrack : Asset Assignment REF#{0}", assignment.AssetAssignmentID);
                        emailData = new EmailData(receipients, emailSubj, emailBody);
                        SendAsync(emailData);

                        break;
                    case EmailType.TRANSFER_COMPLETE:

                        //TODO : Is this needed?
                        //return GetTemplate("Asset_Transfer_Complete.txt");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
            }
        }
        
        private string BuildAssignmentEmailBody(vwAssetAssignment assignment,string emailTemplate, 
                                                    string receiver, string optVal1="",string optVal2="", string optVal3="")
        {
            string dateReleased = (assignment.DateReleased != null) ? assignment.DateReleased.Value.ToShortDateString() : String.Empty;

            string email = String.Format(emailTemplate, receiver, assignment.TypeDescription, assignment.Brand, assignment.Model,
                                            assignment.AssetTag, assignment.SerialNumber, "", assignment.StatusDescription,
                                            assignment.AssignmentStatus, assignment.DateAssigned.Value.ToShortDateString(),
                                            dateReleased,"",optVal1,optVal2, optVal3);

            return email;
            
        }
 
        private string GetTemplate(string fileName)
        {
            string completeFile = HttpContext.Current.Server.MapPath("\\App_Data\\EmailTemplates\\") + fileName;

            using ( FileStream fs= new FileStream(completeFile,FileMode.Open))
            {
                using ( StreamReader reader = new StreamReader(fs))
                {
                    return reader.ReadToEnd();
                }
            }

        }

        public List<string> GetMISMailToList(int optionalDepartmentID = 0)
        {
            using( var db = new FASTDBEntities())
            {
                List<string> toList = new List<string>();

                if ( optionalDepartmentID != 0 )
                {
                    toList = (from list in db.vwMISLists
                              where list.DepartmentID == optionalDepartmentID
                              select list.EmailAddress).ToList();
                }
                else
                {
                    toList = (from list in db.vwMISLists
                              select list.EmailAddress).ToList();
                }

                return toList;
            }
        }

        public List<string> GetAdminMailToList(int optionalDepartmentID = 0)
        {
            using (var db = new FASTDBEntities())
            {
                List<string> toList = new List<string>();

                if (optionalDepartmentID != 0)
                {
                    toList = (from list in db.vwCustodiansLists
                              where list.DepartmentID == optionalDepartmentID
                              select list.EmailAddress).ToList();
                }
                else
                {
                    toList = (from list in db.vwCustodiansLists
                              select list.EmailAddress).ToList();
                }

                return toList;
            }
        }

        public List<string> GetManagersMailList(int optionalDepartmentID = 0)
        {
            using (var db = new FASTDBEntities())
            {
                List<string> toList = new List<string>();

                if (optionalDepartmentID != 0)
                {
                    toList = (from list in db.vwManagersLists
                              where list.DepartmentID == optionalDepartmentID
                              select list.EmailAddress).ToList();
                }
                else
                {
                    toList = (from list in db.vwManagersLists
                              select list.EmailAddress).ToList();
                }

                return toList;
            }
        }

    }

    public class EmailData
    {
        public List<string> ToList { get; set; }
        public List<string> CCList { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
   
        public EmailData( List<string> toListParam, List<string> ccListParam, string subjectParam, string bodyParam)
        {
            ToList = toListParam;
            CCList = ccListParam;
            Subject = subjectParam;
            Body = bodyParam;
            }

        public EmailData(string mailTo, List<string> ccListParam, string subjectParam, string bodyParam)
        {
            ToList = new List<string>() { mailTo };
            CCList = ccListParam;
            Subject = subjectParam;
            Body = bodyParam;
        }

        public EmailData( string mailTo, string subjectParam, string bodyParam)
        {
            ToList = new List<string>() { mailTo };
            CCList = new List<string>();
            Subject = subjectParam;
            Body = bodyParam;
        }

        public EmailData( List<string> toListParam, string subjectParam, string bodyParam)
        {
            ToList = toListParam;
            CCList = new List<string>();
            Subject = subjectParam;
            Body = bodyParam;
        }
    }
}