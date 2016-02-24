using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FASTWSv1.Models;
using FASTWSv1.Common;

namespace FASTWSv1.Controllers
{
    [RoutePrefix("api/Assignment")]
    public class AssignmentController : ApiController
    {

        [HttpPost]
        [Route("New/AssetTag")]
        public HttpResponseMessage AddNewAssignmentByAssetTag(ExternalAddAssignmentViewModel model)
        {
            BO.AssignmentProcess assignmentProcess = new BO.AssignmentProcess();
            //TODO : Add checking of duplicate assets
            if ( model.AssigningEmpID != 0 )
            {
                int result = assignmentProcess.AssignEquipmentByAssetTag(model);

                if (result > ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_CREATED();
                }
                else if ( result == ReturnValues.DUPLICATE)
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.EXISTS);
                }
                else if ( result == ReturnValues.NOT_AVAILABLE )
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.NOT_AVAILABLE);
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }

            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPost]
        [Route("New/SerialNumber")]
        public HttpResponseMessage AddNewAssignmentBySerialNumber(ExternalAddAssignmentViewModel model)
        {
            BO.AssignmentProcess assignmentProcess = new BO.AssignmentProcess();
            //TODO : Add checking of duplicate assets
            if (model.AssigningEmpID != 0)
            {
                int result = assignmentProcess.AssignEquipmentBySerialNumber(model);
                if ( result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_CREATED();
                }
                else if (result == ReturnValues.DUPLICATE)
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.EXISTS);
                }
                else
                {
                    ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }
            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPut]
        [Route("Approval")]
        public HttpResponseMessage ApproveOrDenyAssignmentRequest(ExternalApprovalViewModel model)
        {

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
        }

        [HttpPut]
        [Route("AcceptReject")]
        public HttpResponseMessage AcceptOrRejectAssignment(ExternalAcceptanceViewModel model)
        {
            BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();
            //Must have an EmployeeID and if Rejected, must have the optionalRemark
            if (model.acceptingEmployeeID != 0)
            {
                int result = ReturnValues.FAILED;

                if (model.accepted)
                {
                    result = assignProcess.AcceptAssignment(model.acceptingEmployeeID, model.assignmentID);
                  
                }
                else
                {
                    if (!String.IsNullOrEmpty(model.optionalRemarks))
                    {
                        result = assignProcess.RejectAssignment(model.acceptingEmployeeID, model.assignmentID, model.optionalRemarks);
                    }
                    else 
                    {
                        return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_CD);
                    }
                }

                if (result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else if (result == ReturnValues.NOT_FOUND)
                {
                    return ReturnMessages.RESPONSE_NOTFOUND();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }
            }
            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpGet]
        [Route("EmployeeID/{employeeID}")]
        public List<vwAssetAssignment> GetAssignmentsByEmployeeID(int employeeID)
        {
            using ( var db = new FASTDBEntities())
            {
                List<vwAssetAssignment> assignments = (from assigns in db.vwAssetAssignments
                                                       where assigns.EmployeeID == employeeID
                                                       select assigns).ToList();

                return assignments;
            }
        }

        [HttpGet]
        [Route("AssignmentID/{assignmentID}")]
        public List<vwAssetAssignment> GetAssignmentsByAssignmentID(int assignmentID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignment> assignments = (from assigns in db.vwAssetAssignments
                                                       where assigns.AssetAssignmentID == assignmentID
                                                       select assigns).ToList();

                return assignments;
            }
        }

        [HttpPost]
        [Route("TransferAsset")]
        public HttpResponseMessage TransferAsset(ExternalTransferAssignmentViewModel model)
        {
            int result = 0;
            BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

            if (model.RequestorID != 0)
            {
                if ( model.RequireApproval)
                {
                    result = assignProcess.TransferAssetWithApproval(model);
                }
                else
                {
                    result = assignProcess.TransferAssetWithoutApproval(model);
                }

                if ( result >= 0 )
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }
            }
            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }
        
        [HttpPut]
        [Route("Transfer/Approve")]
        public HttpResponseMessage ApproveTransfer(ExternalTransferAssignmentViewModel model)
        {
            if ( model.ApprovingID != 0)
            {
                BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();
                int result = assignProcess.TransferRequestApproved(model);

                if ( result > 0)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPut]
        [Route("Transfer/Deny")]
        public HttpResponseMessage DenyTransfer(ExternalTransferAssignmentViewModel model)
        {
            if (model.ApprovingID != 0)
            {
                if (model.OptionalRemarks.Trim().Length > 1)
                {
                    BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();
                    int result = assignProcess.TransferRequestDenied(model);

                    if (result == ReturnValues.SUCCESS)
                    {
                        return ReturnMessages.RESPONSE_OK();
                    }
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_CD);
                }
            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }


        [HttpPost]
        [Route("ReleaseAsset")]
        public HttpResponseMessage ReleaseAsset(ExternalReleaseAssignmentViewModel model)
        {
            if (model.RequestorID != 0)
            {
                if ( model.ReasonCode != 0 )
                {
                    BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();
                    int result = 0;

                    if ( model.RequireApproval )
                    {
                        result = assignProcess.ReleaseAssetWithApproval(model);
                    }
                    else
                    {
                        result = assignProcess.ReleaseAssetWithoutApproval(model);
                    }

                    if ( result == ReturnValues.SUCCESS)
                    {
                        return ReturnMessages.RESPONSE_OK();
                    }
                    else
                    {
                        return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                    }
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_CD);
                }

            }
            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);

        }

        [HttpPut]
        [Route("Release/Approve")]
        public HttpResponseMessage ApproveReleaseRequest(ExternalReleaseAssignmentViewModel model)
        {
            if (model.ApprovingID != 0)
            {
                BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

                int result = assignProcess.ApproveReleaseRequest(model);

                if (result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }
 
            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPut]
        [Route("Release/Deny")]
        public HttpResponseMessage DenyReleaseRequest(ExternalReleaseAssignmentViewModel model)
        {
            if (model.ApprovingID != 0)
            {
                BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

                int result = assignProcess.DenyReleaseRequest(model);

                if (result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }

            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPut]
        [Route("Release/Accept")]
        public HttpResponseMessage AcceptReleasedAsset(ExternalReleaseAssignmentViewModel model)
        {
            if (model.AcceptingID != 0)
            {
                BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

                int result = assignProcess.AcceptReleasedAsset(model);

                if (result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }

            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }

        [HttpPut]
        [Route("Release/Reject")]
        public HttpResponseMessage RejectReleasedAsset(ExternalReleaseAssignmentViewModel model)
        {
            if (model.ApprovingID != 0)
            {
                BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

                int result = assignProcess.RejectReleasedAsset(model);

                if (result == ReturnValues.SUCCESS)
                {
                    return ReturnMessages.RESPONSE_OK();
                }
                else
                {
                    return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
                }

            }

            return ReturnMessages.RESPONSE_NOTSUCCESSFUL(Constants.MISSING_ID);
        }
                
        
        //TODO : This is only for devs, remove in final build
        [HttpGet]
        public List<AssetAssignment> GetAllAssignments()
        {
            using (var db = new FASTDBEntities())
            {
                return db.AssetAssignments.ToList();
            }
        }
    }
}

