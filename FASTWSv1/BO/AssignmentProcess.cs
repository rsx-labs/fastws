using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

using FASTWSv1.Common;
using FASTWSv1.Models;
using FASTWSv1.Providers;


namespace FASTWSv1.BO
{ 
    public class AssignmentProcess
    {

        public AssetAssignment GetAssignmentbyID(int assignmentID)
        {
            using ( var db = new FASTDBEntities())
            {
                List<AssetAssignment> assignment = (from assign in db.AssetAssignments
                                                    where assign.AssetAssignmentID == assignmentID
                                                    select assign).ToList();

                if ( assignment.Count() > 0 )
                {
                    return assignment[0];
                }

                return null;
            }
        }

        public List<vwAssetAssignment> GetAcceptedAssignmentsbyEmpID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignment> assignments = ( from assign in db.vwAssetAssignments
                                                        where assign.EmployeeID == employeeID && assign.AssignmentStatusID == Constants.ASSIGNMENT_STATUS_ACCEPTED
                                                       select assign).ToList();

                if (assignments.Count() > 0)
                {
                    return assignments;
                }
            }

            return null;
        }

        public vwAssetAssignment GetAssignmentViewByID(int assignmentID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignment> assignments = (from assign in db.vwAssetAssignments
                                                       where assign.AssetAssignmentID == assignmentID
                                                       select assign).ToList();

                if (assignments.Count() > 0)
                {
                    return assignments[0];
                }
            }

            return null;
        }

        public List<vwAssetAssignment> GetAssignmentsHistorybyEmpID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignment> assignments = (from assign in db.vwAssetAssignments
                                                       where assign.EmployeeID == employeeID
                                                       select assign).ToList();

                if (assignments.Count() > 0)
                {
                    return assignments;
                }
            }

            return null;
        }

        public int AssignEquipmentBySerialNumber(ExternalAddAssignmentViewModel model)
        {
            //Lets get the equipment info
            BO.AssetProcess assetProcess = new AssetProcess();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            FixAsset asset = assetProcess.GetAssetBySerialNumber(model.SerialNumber);
            Providers.EmailProvider email = new Providers.EmailProvider();
            int result = 0;
            bool isITAsset = false;

            if ( null != asset )
            {
                if ( asset.AssetClassID == Common.Constants.ASSET_CLASS_IT )
                {
                    isITAsset = true;
                    result = AssignNewITAssetToEmployee(asset.FixAssetID, model.ReceipientEmpID, model.OptionalRemarks);
                }
                else
                {
                    result = AssignNewNonITAssetToEmployee(asset.FixAssetID, model.ReceipientEmpID, model.OptionalRemarks);
                }

                //lets try sending the email confirmation
                if (Helpers.ConfigurationHelper.SendEmail)
                {
                    if (result > 0 )
                    {
                        email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_NEW, result);
                        email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_MIS, result);
                    }
                }
                //Lets add it to Audit trail
                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.ASSIGN_ASSET, model.AssigningEmpID,
                    String.Format("Assigned asset {0} to {1}.", asset.AssetTag, model.ReceipientEmpID.ToString()));

                return result;

            }

            return ReturnValues.NOT_FOUND;

        }

        public int AssignEquipmentByAssetTag(ExternalAddAssignmentViewModel model)
        {

            //Lets get the asset info
            BO.AssetProcess assetProcess = new AssetProcess();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            Providers.EmailProvider email = new Providers.EmailProvider();
            FixAsset asset = assetProcess.GetAssetByAssetTag(model.AssetTag);

            int result = 0;

            //Asset should exist and is for assignment
            if ( (null != asset) && ( asset.AssetStatusID == Constants.ASSET_STATUS_FORASSIGNMENT ))
            {
                if (asset.AssetClassID == Common.Constants.ASSET_CLASS_IT)
                {
                    result = AssignNewITAssetToEmployee(asset.FixAssetID, model.ReceipientEmpID, model.OptionalRemarks);
                }
                else
                {
                    result = AssignNewNonITAssetToEmployee(asset.FixAssetID, model.ReceipientEmpID, model.OptionalRemarks);
                }
                //lets try sending the email confirmation
                if (Helpers.ConfigurationHelper.SendEmail)
                {
                    if (result > 0 )
                    {
                        email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_NEW, result);
                        email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_MIS, result);
                    }
                }
                //Lets add it to Audit trail
                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.ASSIGN_ASSET, model.AssigningEmpID,
                    String.Format("Assigned asset {0} to {1}.", asset.AssetTag, model.ReceipientEmpID.ToString()));

                return result;
            }
            if (null == asset)
            {
                return ReturnValues.NOT_FOUND;
            }
            else
            {
                return ReturnValues.NOT_AVAILABLE;
            }
        }

        private int AssignNewITAssetToEmployee(int assetID, int employeeID, string optionalMessage ="")
        {
            BO.AssetProcess assetProcess = new AssetProcess();
            
            AssetAssignment result = null;

            using ( var db = new FASTDBEntities())
            {
                AssetAssignment newAssignment = new AssetAssignment();
                newAssignment.FixAssetID = assetID;
                newAssignment.EmployeeID = employeeID;
                newAssignment.DateAssigned = DateTime.Now;
                newAssignment.Remarks = optionalMessage;

                //Waiting for ACCEPTANCE
                Dictionary<string, int> nextStatus =
                            WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_MIS, assetID, 0);

                newAssignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                List<AssetAssignment> duplicates = (from dup in db.AssetAssignments
                                                    where ((dup.EmployeeID == employeeID) && (dup.FixAssetID == assetID) && (dup.AssignmentStatusID != Constants.ASSIGNMENT_STATUS_RELEASED) ) 
                                                    select dup).ToList();

                if (duplicates.Count() == 0)
                {

                    result = db.AssetAssignments.Add(newAssignment);
                    db.SaveChanges();

                    if (null != result)
                    {
                        //Seems like we have a successful add
                        //Lets change asset status to Assigned to MIS
                        
                        if (assetProcess.UpdateAssetStatus(assetID, nextStatus[Constants.ASSET_STATUS]) == ReturnValues.SUCCESS)
                        {
                            //return new assetassignmentID
                            return result.AssetAssignmentID;
                        }
                        //we had problem updating the status
                        return ReturnValues.FAILED;
                    }
                }
                else
                {
                    //Duplicate exist, not allowed
                    return ReturnValues.DUPLICATE;
                }

            }

            return ReturnValues.FAILED;
        }

        private int TransferITAssetToEmployee(int assetID, int employeeID, string optionalMessage = "")
        {
            BO.AssetProcess assetProcess = new AssetProcess();

            AssetAssignment result = null;

            using (var db = new FASTDBEntities())
            {
                AssetAssignment newAssignment = new AssetAssignment();
                newAssignment.FixAssetID = assetID;
                newAssignment.EmployeeID = employeeID;
                newAssignment.DateAssigned = DateTime.Now;
                newAssignment.Remarks = optionalMessage;

                //Waiting for ACCEPTANCE
                Dictionary<string, int> nextStatus =
                            WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER_APPROVED, assetID, 0);

                newAssignment.AssignmentStatusID = nextStatus["NAA"];

                List<AssetAssignment> duplicates = (from dup in db.AssetAssignments
                                                    where ((dup.EmployeeID == employeeID) && (dup.FixAssetID == assetID) && (dup.AssignmentStatusID != Constants.ASSIGNMENT_STATUS_RELEASED))
                                                    select dup).ToList();

                if (duplicates.Count() == 0)
                {

                    result = db.AssetAssignments.Add(newAssignment);
                    db.SaveChanges();

                    if (null != result)
                    {
                        //Seems like we have a successful add
                        //Lets change asset status to Assigned to MIS

                        if (assetProcess.UpdateAssetStatus(assetID, nextStatus[Constants.ASSET_STATUS]) == ReturnValues.SUCCESS)
                        {
                            //return new assetassignmentID
                            return result.AssetAssignmentID;
                        }
                        //we had problem updating the status
                        return ReturnValues.FAILED;
                    }
                }
                else
                {
                    //Duplicate exist, not allowed
                    return ReturnValues.DUPLICATE;
                }

            }

            return ReturnValues.FAILED;
        }

        private int AssignNewNonITAssetToEmployee(int assetID, int employeeID, string optionalMessage = "")
        {
            AssetAssignment result = null;
            BO.AssetProcess assetProcess = new AssetProcess();

            using (var db = new FASTDBEntities())
            {
                AssetAssignment newAssignment = new AssetAssignment();
                newAssignment.FixAssetID = assetID;
                newAssignment.EmployeeID = employeeID;
                newAssignment.DateAssigned = DateTime.Now;
                newAssignment.Remarks = optionalMessage;

                //Waiting for ACCEPTANCE
                Dictionary<string, int> nextStatus =
                            WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_EMPLOYEE, assetID, 0);

                newAssignment.AssignmentStatusID = nextStatus["NAA"];

                List<AssetAssignment> duplicates = (from dup in db.AssetAssignments
                                                    where ((dup.EmployeeID == employeeID) && (dup.FixAssetID == assetID))
                                                    select dup).ToList();

                if (duplicates.Count() > 0)
                {
                    result = db.AssetAssignments.Add(newAssignment);

                    if (null != result)
                    {
                        //Seems like we have a successful add
                        //Lets change asset status to Waiting for Acceptance
                        if (assetProcess.UpdateAssetStatus(assetID, nextStatus[Constants.ASSET_STATUS]) == ReturnValues.SUCCESS)
                        {
                            //return the new assetassignment ID
                            return result.AssetAssignmentID;
                        }
                        //we had problem updating the status
                        return ReturnValues.FAILED;
                    }
                }
                else
                {
                    return ReturnValues.DUPLICATE;
                }
            }

            return ReturnValues.FAILED;
        }

        private int TransferNonITAssetToEmployee(int assetID, int employeeID, string optionalMessage = "")
        {
            AssetAssignment result = null;
            BO.AssetProcess assetProcess = new AssetProcess();

            using (var db = new FASTDBEntities())
            {
                AssetAssignment newAssignment = new AssetAssignment();
                newAssignment.FixAssetID = assetID;
                newAssignment.EmployeeID = employeeID;
                newAssignment.DateAssigned = DateTime.Now;
                newAssignment.Remarks = optionalMessage;

                //Waiting for ACCEPTANCE
                Dictionary<string, int> nextStatus =
                            WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_EMPLOYEE, assetID, 0);

                newAssignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                List<AssetAssignment> duplicates = (from dup in db.AssetAssignments
                                                    where ((dup.EmployeeID == employeeID) && (dup.FixAssetID == assetID))
                                                    select dup).ToList();

                if (duplicates.Count() > 0)
                {
                    result = db.AssetAssignments.Add(newAssignment);

                    if (null != result)
                    {
                        //Seems like we have a successful add
                        //Lets change asset status to Waiting for Acceptance
                        if (assetProcess.UpdateAssetStatus(assetID, nextStatus[Constants.ASSET_STATUS]) == ReturnValues.SUCCESS)
                        {
                            //return the new assetassignment ID
                            return result.AssetAssignmentID;
                        }
                        //we had problem updating the status
                        return ReturnValues.FAILED;
                    }
                }
                else
                {
                    return ReturnValues.DUPLICATE;
                }
            }

            return ReturnValues.FAILED;
        }

        public int AcceptAssignment(int employeeID, int assignmentID)
        {
            BO.AssetProcess assetProcess = new AssetProcess();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            Providers.EmailProvider email = new Providers.EmailProvider();
            List<FixAsset> assetInQuestion;
            string optionalRemarks = string.Empty;
            vwEmployee employee = new vwEmployee();

            using ( var db = new FASTDBEntities())
            {
                AssetAssignment[] assignments = (from assign in db.AssetAssignments
                                                 where assign.AssetAssignmentID == assignmentID
                                                 select assign).ToArray();

                int assetID = assignments[0].FixAssetID;
                
                employee = employeeProcess.GetEmployeeViewByID(assignments[0].EmployeeID);

                if (assignments.Count() > 0)
                {
                    int prevStatus = assignments[0].AssignmentStatusID;

                    assetInQuestion = (from asset in db.FixAssets
                                       where asset.FixAssetID == assetID
                                       select asset).ToList();

                    Dictionary<string, int> nextStatus =
                            WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_ACCEPTED, assetID, assignments[0].AssetAssignmentID);

                    if (assignments[0].EmployeeID != employeeID)
                    {
                        //TODO : Needs additonal checking 
                        if (assetInQuestion[0].AssetStatusID == Constants.ASSET_STATUS_WITH_MIS)
                        {
                            optionalRemarks = String.Format("MIS Staff {0} accepted the assignment {1}.", employeeID.ToString(), assignmentID.ToString());
                        }
                        else
                        {
                            optionalRemarks = String.Format("Employee ID {0} not allowed to accept assignment {1}.", employeeID.ToString(), assignmentID.ToString());
                            Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.ACCEPT, employeeID,optionalRemarks);
                            return ReturnValues.FAILED;
                        }
                    }
                    else
                    {
                        optionalRemarks = String.Format("Employee {0} accepts Assignment {1}", employeeID.ToString(), assignmentID.ToString());
                    }

                    assignments[0].AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                    
                    if ( db.SaveChanges() > 0 )
                    {
                        //Acceptance is ok, need to send an email and log in the AuditTrail
                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.ACCEPT, employeeID, optionalRemarks);
                        
                        vwFixAsset asset = assetProcess.GetFixAssetViewByID(assetInQuestion[0].FixAssetID);

                        if ( Helpers.ConfigurationHelper.SendEmail )
                        {
                            email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_ACCEPTANCE, assignmentID);
                         
                            //TODO: Send updated liabilities to the employee and admin

                            email.SendAssignmentEmail(EmailProvider.EmailType.LIST_ASSIGMENTS, assignmentID);

                        }
                        return ReturnValues.SUCCESS;
                    }
                    return ReturnValues.FAILED;
                }
                return ReturnValues.NOT_FOUND;
            }

        }

        public int RejectAssignment(int employeeID, int assignmentID, string optionalRemarks = "")
        {
            BO.AssetProcess assetProcess = new AssetProcess();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            Providers.EmailProvider email = new Providers.EmailProvider();
            List<FixAsset> assetInQuestion;
            Employee employee = new Employee();
            List<string> toList = new List<string>();

            using (var db = new FASTDBEntities())
            {
                AssetAssignment[] assignments = (from assign in db.AssetAssignments
                                                 where assign.AssetAssignmentID == assignmentID
                                                 select assign).ToArray();

                int assetID = assignments[0].FixAssetID;

                employee = employeeProcess.GetEmployeeByID(assignments[0].EmployeeID);

                if (assignments.Count() > 0)
                {
                    int prevStatus = assignments[0].AssignmentStatusID;

                    assetInQuestion = (from asset in db.FixAssets
                                       where asset.FixAssetID == assetID
                                       select asset).ToList();

                    Dictionary<string, int> nextStatus = 
                        WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_REJECTED,assetID, assignments[0].AssetAssignmentID);

                    assignments[0].AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                    assetInQuestion[0].AssetStatusID = nextStatus[Constants.ASSET_STATUS];

                    if (db.SaveChanges() > 0)
                    {
                        //Acceptance is ok, need to send an email and log in the AuditTrail
                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.ACCEPT, employeeID, "REJECTED " +  optionalRemarks);

                        vwFixAsset asset = assetProcess.GetFixAssetViewByID(assetInQuestion[0].FixAssetID);

                        #region Send Email
                        if (Helpers.ConfigurationHelper.SendEmail)
                        {

                            email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_REJECTED, assignmentID);
                            email.SendAssignmentEmail(EmailProvider.EmailType.ASSIGNMENT_REJECTED_NOTIFICATION, assignmentID);
                        }
                        #endregion
                        return ReturnValues.SUCCESS;
                    }
                    return ReturnValues.FAILED;
                }
                return ReturnValues.NOT_FOUND;
            }
        }

        private int TransferToMIS(ExternalTransferAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            BO.AssetProcess assetProcess = new AssetProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSIGNMENT_MIS, model.FixAssetID, model.CurrentAssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);
                //Employee receipient = employeeProcess.GetEmployeeByID(model.ReceipientID);

                FixAsset asset = (from fasset in db.FixAssets
                                  where fasset.FixAssetID == model.FixAssetID
                                  select fasset).ToArray()[0];

                AssetAssignment assignment = (from assign in db.AssetAssignments
                                              where assign.AssetAssignmentID == model.CurrentAssignmentID
                                              select assign).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);

                if ( (asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];

                    if ( db.SaveChanges() > 0)
                    {
                        //Success, update the assignment
                        assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                        if ( db.SaveChanges() > 0)
                        {
                            Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET, 
                                model.RequestorID, String.Format("Asset {0} transferred to MIS.", model.FixAssetID.ToString()));

                            vwFixAsset assetView = assetProcess.GetFixAssetViewByID(asset.FixAssetID);

                            if ( Helpers.ConfigurationHelper.SendEmail )
                            {
                                //inform MIS
                                email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_MIS_ACCEPTANCE,assignment.AssetAssignmentID);
                                //inform owner
                                email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_WDAPPROOVAL_DONE, assignment.AssetAssignmentID);
                                //send updated list
                                email.SendAssignmentEmail(EmailProvider.EmailType.LIST_ASSIGMENTS, assignment.AssetAssignmentID);
                            }

                        }
                        else
                        {
                            //revert and return
                            asset.AssetStatusID = prevAssetStatus;
                            assignment.AssignmentStatusID = prevAssignStatus;
                            db.SaveChanges();
                            return ReturnValues.FAILED;
                        }
                    }
                    else
                    {
                        //revert and return failed
                        asset.AssetStatusID = prevAssetStatus;
                        db.SaveChanges();
                        return ReturnValues.FAILED;
                    }
                }
            }
            return ReturnValues.FAILED;
        }

        private int TransferFromMIS(ExternalTransferAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            BO.AssetProcess assetProcess = new AssetProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER_APPROVED, model.FixAssetID, model.CurrentAssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);

                FixAsset asset = (from fasset in db.FixAssets
                                  where fasset.FixAssetID == model.FixAssetID
                                  select fasset).ToArray()[0];

                AssetAssignment assignment = (from assign in db.AssetAssignments
                                              where assign.AssetAssignmentID == model.CurrentAssignmentID
                                              select assign).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                    if (db.SaveChanges() > 0)
                    {
                            Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                model.RequestorID, String.Format("Asset {0} transferred to Employee {1}.", model.FixAssetID.ToString(), model.ReceipientID));

                            vwFixAsset assetView = assetProcess.GetFixAssetViewByID(asset.FixAssetID);

                            if (Helpers.ConfigurationHelper.SendEmail)
                            {
                                //inform owner
                                email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_RECEIVE, assignment.AssetAssignmentID,"","",0,model.ReceipientID);
                                
                            }

                            return ReturnValues.SUCCESS;
                    }
                    else
                    {
                        //revert and return failed
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignStatus;
                        db.SaveChanges();
                        return ReturnValues.FAILED;
                    }
                }
            }
            return ReturnValues.FAILED;
        }

        public int TransferAssetWithoutApproval(ExternalTransferAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            int result = 0;

            using ( var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus = 
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER_WOAPPROVAL, model.FixAssetID, model.CurrentAssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);
                Employee receipient = employeeProcess.GetEmployeeByID(model.ReceipientID);

                FixAsset asset = (from fasset in db.FixAssets
                                  where fasset.FixAssetID == model.FixAssetID
                                  select fasset).ToArray()[0];

                AssetAssignment assignment = (from assign in db.AssetAssignments
                                              where assign.AssetAssignmentID == model.CurrentAssignmentID
                                              select assign).ToArray()[0];

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    //Check if the destination is MIS, if yes, exit and assign to MIS
                    if ( model.ToMIS )
                    {
                        result = TransferToMIS(model);
                        return result;
                    }

                    if ( asset.AssetStatusID == Constants.ASSET_STATUS_WITH_MIS)
                    {
                        result = TransferFromMIS(model);
                        return result;
                        
                    }
                    //Else continue

                    ExternalAddAssignmentViewModel newAssignment = new ExternalAddAssignmentViewModel();
                    newAssignment.AssetTag = asset.AssetTag;
                    newAssignment.AssigningEmpID = model.RequestorID;
                    newAssignment.ReceipientEmpID = model.ReceipientID;
                    newAssignment.OptionalRemarks = model.OptionalRemarks;
                    
                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    if (db.SaveChanges() > 0)
                    {
                        assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                        assignment.DateReleased = DateTime.Now;
        
                        if (db.SaveChanges() > 0)
                        {
                            if ( asset.AssetClassID == Constants.ASSET_CLASS_IT)
                            {
                                result = TransferITAssetToEmployee(asset.FixAssetID, model.ReceipientID, model.OptionalRemarks);
                            }
                            else
                            {
                                result = TransferNonITAssetToEmployee(asset.FixAssetID, model.ReceipientID, model.OptionalRemarks);
                            }

                            if (result > 0 )
                            {
                                //Inform the parties about the transfer via email
                                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                    model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} is almost done.",
                                    asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                                if (Helpers.ConfigurationHelper.SendEmail)
                                {
                                    //Send Email to Receipient
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_RECEIVE, assignment.AssetAssignmentID,"","",0,model.ReceipientID);

                                    //send email to requestor
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_WOAPPROVAL_DONE, assignment.AssetAssignmentID,"","",model.RequestorID,0);
                                }

                                return ReturnValues.SUCCESS;
                            }
                            else
                            {
                                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                    model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} FAILED.",
                                    asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                                asset.AssetStatusID = prevAssetStatus;
                                assignment.AssignmentStatusID = prevAssignStatus;
                                assignment.DateReleased = null;
                                db.SaveChanges();
                                return ReturnValues.FAILED;

                            }
                        }
                        else
                        {
                              Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                    model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} FAILED.",
                                    asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                            asset.AssetStatusID = prevAssetStatus;
                            db.SaveChanges();
                            return ReturnValues.FAILED;
                        }
                    }
                    else
                    {
                        return ReturnValues.FAILED;
                    }
                }

            }
            return ReturnValues.FAILED;
        }

        public int TransferAssetWithApproval(ExternalTransferAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            //int result = 0;

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus = 
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER, model.FixAssetID, model.CurrentAssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);
                Employee receipient = employeeProcess.GetEmployeeByID(model.ReceipientID);

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.CurrentAssignmentID
                                              select assigns).ToArray()[0];

                if ( ( asset!= null) && (assignment != null) )
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];

                    if ( db.SaveChanges() > 0)
                    {
                        assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                        assignment.Remarks = (model.ToMIS) ? "MIS" : model.ReceipientID.ToString();

                        if ( db.SaveChanges() > 0)
                        {
                            string receiver = (model.ToMIS) ? "MIS" : model.ReceipientID.ToString();

                            Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                model.RequestorID,String.Format("Transfer request of asset {0} by Employee {1} to Employee {2}",asset.FixAssetID,model.RequestorID,receiver));

                            if ( Helpers.ConfigurationHelper.SendEmail)
                            {
                                if ( model.ToMIS)
                                {
                                    //Inform MIS about the transfer request to them
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_TO_MIS_WTAPPROVAL, assignment.AssetAssignmentID);
                                    
                                    //Inform the manager about the request

                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER, assignment.AssetAssignmentID,
                                                      requestor.FirstName + " " + requestor.LastName, "MIS", requestor.EmployeeID);
                                }
                                else if (prevAssetStatus == Constants.ASSET_STATUS_WITH_MIS)
                                {
                                    //Inform the MIS
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_REQUEST_BY_MIS, assignment.AssetAssignmentID, "", "", requestor.EmployeeID, receipient.EmployeeID);

                                    //Inform MIS Manager for Approval.
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER, assignment.AssetAssignmentID,
                                                requestor.FirstName + " " + requestor.LastName, receipient.FirstName + " " + receipient.LastName, requestor.EmployeeID);
                                }
                                else
                                {
                                    //Inform the Requestor
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_REQUEST, assignment.AssetAssignmentID, "", "", 
                                                                requestor.EmployeeID, receipient.EmployeeID);

                                   //Inform the manager
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER, assignment.AssetAssignmentID,
                                        requestor.FirstName + " " + requestor.LastName, receipient.FirstName + " " + receipient.LastName,requestor.EmployeeID);
                                    
                                }

                                //Inform the requestor about that the request has been sent 
                                email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_REQUEST_CONFIRMATION, assignment.AssetAssignmentID, "", "", requestor.EmployeeID);      
                       
                            }

                            return ReturnValues.SUCCESS;
                        }
                        else
                        {
                            assignment.AssignmentStatusID = prevAssignStatus;
                            asset.AssetStatusID = prevAssetStatus;
                            db.SaveChanges();
                            return ReturnValues.FAILED;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        db.SaveChanges();
                        return ReturnValues.FAILED;
                    }
                }

                return ReturnValues.NOT_FOUND;
            }
        }
    
        public int TransferRequestApproved(ExternalTransferAssignmentViewModel model)
        {
            //return TransferAssetWithoutApproval(model);
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();
            int result = 0;

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER_APPROVED, model.FixAssetID, model.CurrentAssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);
                Employee receipient = employeeProcess.GetEmployeeByID(model.ReceipientID);

                FixAsset asset = (from fasset in db.FixAssets
                                  where fasset.FixAssetID == model.FixAssetID
                                  select fasset).ToArray()[0];

                AssetAssignment assignment = (from assign in db.AssetAssignments
                                              where assign.AssetAssignmentID == model.CurrentAssignmentID
                                              select assign).ToArray()[0];

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    //Check if the destination is MIS, if yes, exit and assign to MIS
                    if (model.ToMIS)
                    {
                        result = TransferToMIS(model);
                        return result;
                    }

                    if (asset.AssetStatusID == Constants.ASSET_STATUS_WITH_MIS)
                    {
                        result = TransferFromMIS(model);
                        return result;

                    }
                    //Else continue

                    ExternalAddAssignmentViewModel newAssignment = new ExternalAddAssignmentViewModel();
                    newAssignment.AssetTag = asset.AssetTag;
                    newAssignment.AssigningEmpID = model.RequestorID;
                    newAssignment.ReceipientEmpID = model.ReceipientID;
                    newAssignment.OptionalRemarks = model.OptionalRemarks;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    if (db.SaveChanges() > 0)
                    {
                        assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                        if (db.SaveChanges() > 0)
                        {
                            if (asset.AssetClassID == Constants.ASSET_CLASS_IT)
                            {
                                result = TransferITAssetToEmployee(asset.FixAssetID, model.ReceipientID, model.OptionalRemarks);
                            }
                            else
                            {
                                result = TransferNonITAssetToEmployee(asset.FixAssetID, model.ReceipientID, model.OptionalRemarks);
                            }

                            if (result > 0)
                            {
                                //Inform the parties about the transfer via email
                                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                    model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} is almost done.",
                                    asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                                if (Helpers.ConfigurationHelper.SendEmail)
                                {
                                    //Send Email to Receipient
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_RECEIVE, assignment.AssetAssignmentID, "", "", 0, model.ReceipientID);
                                    
                                    //send email to requestor
                                    email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_WOAPPROVAL_DONE, assignment.AssetAssignmentID, "", "", model.RequestorID, 0);
                                }

                                return ReturnValues.SUCCESS;
                            }
                            else
                            {
                                Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                    model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} FAILED.",
                                    asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                                asset.AssetStatusID = prevAssetStatus;
                                assignment.AssignmentStatusID = prevAssignStatus;
                                db.SaveChanges();
                                return ReturnValues.FAILED;

                            }
                        }
                        else
                        {
                            Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET,
                                  model.RequestorID, String.Format("Transfer of asset {0} to Employee {1} by Employee {2} FAILED.",
                                  asset.FixAssetID.ToString(), model.ReceipientID.ToString(), model.RequestorID.ToString()));

                            asset.AssetStatusID = prevAssetStatus;
                            db.SaveChanges();
                            return ReturnValues.FAILED;
                        }
                    }
                    else
                    {
                        return ReturnValues.FAILED;
                    }
                }

            }
            return ReturnValues.FAILED;



        }

        public int TransferRequestDenied(ExternalTransferAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using( var db = new FASTDBEntities())
            {
                Dictionary<string,int> nextStatus = 
                    WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_TRANSFER_DENIED,model.FixAssetID,model.CurrentAssignmentID);

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assign in db.AssetAssignments
                                              where assign.AssetAssignmentID == model.CurrentAssignmentID
                                              select assign).ToArray()[0];

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);

                if( (asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                    if ( db.SaveChanges()> 0)
                    {
                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.TRANSFER_ASSET, model.RequestorID, String.Format("TRANSFER DENIED : {0}", model.OptionalRemarks));

                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the requestor
                            email.SendAssignmentEmail(EmailProvider.EmailType.TRANSFER_DENIED, assignment.AssetAssignmentID);
                            
                        }

                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignStatus;
                        db.SaveChanges();

                        return ReturnValues.FAILED;
                    }
                }
            }

            return ReturnValues.FAILED;
        }

        public int ReleaseAssetWithoutApproval(ExternalReleaseAssignmentViewModel model)
        {

            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE_WOAPPROVAL, model.FixAssetID, model.AssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                    assignment.DateReleased = DateTime.Now;

                    if (db.SaveChanges() != 0)
                    {
                        List<string> adminList = email.GetAdminMailToList(requestor.DepartmentID);

                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.RequestorID, 
                            String.Format("Release of asset {0} without approval initiated.", asset.FixAssetID.ToString()));

                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the admin
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_ACCEPTANCE, assignment.AssetAssignmentID);

                            //inform the requestor that the request was sent.
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_CONFIRMATION, assignment.AssetAssignmentID, "", "", model.RequestorID, 0);
                          
                            return ReturnValues.SUCCESS;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;
                        assignment.DateReleased = null;
                        db.SaveChanges();
                    }
                }
            }


            return ReturnValues.FAILED;
        }

        public int ReleaseAssetWithApproval(ExternalReleaseAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE, model.FixAssetID, model.AssignmentID);

                Employee requestor = employeeProcess.GetEmployeeByID(model.RequestorID);
              
                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                    if (db.SaveChanges() != 0)
                    {
                        List<string> managersList = email.GetManagersMailList(requestor.DepartmentID);

                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.RequestorID,
                               String.Format("Release of asset {0} with approval initiated.", asset.FixAssetID.ToString()));


                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the managers about the request for approval.
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_WTAPPROVAL, assignment.AssetAssignmentID);
                            //inform the requestor that the request was sent.
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_CONFIRMATION, assignment.AssetAssignmentID, "", "", model.RequestorID, 0);
                        }

                        return ReturnValues.SUCCESS;
                    }
                    else 
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;

                        db.SaveChanges();
                    }
                }
            }

            return ReturnValues.FAILED;
        }

        public int ApproveReleaseRequest(ExternalReleaseAssignmentViewModel model)
        {

            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE_APPROVED, model.FixAssetID, model.AssignmentID);

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);
                Employee approver = employeeProcess.GetEmployeeByID(model.ApprovingID);

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
                    assignment.DateReleased = DateTime.Now;

                    if (db.SaveChanges() != 0)
                    {
                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.ApprovingID,
                                String.Format("Release of asset {0} approved.", asset.FixAssetID.ToString()));


                        List<string> adminList = email.GetAdminMailToList(owner.DepartmentID);
                        List<string> managersList = email.GetManagersMailList(owner.DepartmentID);

                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the admin.
                            email.SendAssignmentEmail(Providers.EmailProvider.EmailType.RELEASE_REQUEST_ACCEPTANCE, assignment.AssetAssignmentID);
                            
                            //inform the requestor that the request was sent.
                            email.SendAssignmentEmail(Providers.EmailProvider.EmailType.RELEASE_REQUEST_APPROVED,assignment.AssetAssignmentID);

                            return ReturnValues.SUCCESS;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;
                        assignment.DateReleased = null;
                        db.SaveChanges();
                    }
                }


                return ReturnValues.FAILED;
            }
        }

        public int DenyReleaseRequest(ExternalReleaseAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE_DENIED, model.FixAssetID, model.AssignmentID);

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);
                Employee approver = employeeProcess.GetEmployeeByID(model.ApprovingID);

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                    if (db.SaveChanges() != 0)
                    {

                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.ApprovingID,
                               String.Format("Release of asset {0} has been denied.", asset.FixAssetID.ToString()));

                        List<string> ccList = email.GetManagersMailList(owner.DepartmentID);
                        
                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the owner
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_DENIED, assignment.AssetAssignmentID, "", "", model.RequestorID, 0);

                            return ReturnValues.SUCCESS;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;
                        assignment.DateReleased = null;
                        db.SaveChanges();
                    }
                }
            }

            return ReturnValues.FAILED;
        }

        public int AcceptReleasedAsset(ExternalReleaseAssignmentViewModel model)
        {

            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE_ACCEPTED, model.FixAssetID, model.AssignmentID);         

                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);
                Employee acceptor = employeeProcess.GetEmployeeByID(model.AcceptingID);

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];
              
                    if (db.SaveChanges() != 0)
                    {

                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.AcceptingID,
                               String.Format("Released asset {0} has been accepted by the Admin", asset.FixAssetID.ToString()));

                        List<string> adminList = email.GetAdminMailToList(owner.DepartmentID);
                        List<string> ccList = email.GetManagersMailList(owner.DepartmentID);
                        ccList.Add(owner.EmailAddress);

                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the admin ( cc the managers and the owner)
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_ACCEPTED, 
                                    assignment.AssetAssignmentID, "", "", model.RequestorID, 0);

                            return ReturnValues.SUCCESS;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;
                        assignment.DateReleased = null;
                        db.SaveChanges();
                    }
                }


                return ReturnValues.FAILED;
            }

        }

        public int RejectReleasedAsset(ExternalReleaseAssignmentViewModel model)
        {
            EmailProvider email = new EmailProvider();
            BO.EmployeeProcess employeeProcess = new EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                Dictionary<string, int> nextStatus =
                   WorkflowProvider.GetNextStatus(WorkflowProvider.ActionType.ASSET_RELEASE_REJECTED, model.FixAssetID, model.AssignmentID);



                FixAsset asset = (from fixAsset in db.FixAssets
                                  where fixAsset.FixAssetID == model.FixAssetID
                                  select fixAsset).ToArray()[0];

                AssetAssignment assignment = (from assigns in db.AssetAssignments
                                              where assigns.AssetAssignmentID == model.AssignmentID
                                              select assigns).ToArray()[0];

                Employee owner = employeeProcess.GetEmployeeByID(assignment.EmployeeID);
                Employee acceptor = employeeProcess.GetEmployeeByID(model.AcceptingID);

                if ((asset != null) && (assignment != null))
                {
                    int prevAssetStatus = asset.AssetStatusID;
                    int prevAssignmentStatus = assignment.AssignmentStatusID;

                    asset.AssetStatusID = nextStatus[Constants.ASSET_STATUS];
                    assignment.AssignmentStatusID = nextStatus[Constants.ASSIGN_STATUS];

                    if (db.SaveChanges() != 0)
                    {

                        Helpers.Logger.AddToAuditTrail(Helpers.Logger.UserAction.RELEASE, model.AcceptingID,
                               String.Format("Released asset {0} has been rejected by the Admin", asset.FixAssetID.ToString()));

                        List<string> adminList = email.GetAdminMailToList(owner.DepartmentID);
                        List<string> ccList = email.GetManagersMailList(owner.DepartmentID);
                        ccList.Add(owner.EmailAddress);

                        if (Helpers.ConfigurationHelper.SendEmail)
                        {
                            //Inform the admin ( cc the managers and the owner)
                            email.SendAssignmentEmail(EmailProvider.EmailType.RELEASE_REQUEST_REJECTED,assignment.AssetAssignmentID);

                            return ReturnValues.SUCCESS;
                        }
                    }
                    else
                    {
                        asset.AssetStatusID = prevAssetStatus;
                        assignment.AssignmentStatusID = prevAssignmentStatus;
                        assignment.DateReleased = null;
                        db.SaveChanges();
                    }
                }


                return ReturnValues.FAILED;
            }
        }
    }
}