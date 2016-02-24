using System;
using System.Collections.Generic;

namespace FASTWSv1.Models
{
    //Models for using the AssignmentController
    /// <summary>
    /// 
    /// </summary>
    public class ExternalAddAssignmentViewModel
    {
        public int ReceipientEmpID { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public int AssigningEmpID { get; set; }
        public string OptionalRemarks { get; set; }
        
    }

    public class ExternalAcceptanceViewModel
    {
        public int assignmentID { get; set; }
        public bool accepted { get; set; }
        public int acceptingEmployeeID { get; set; }
        public string optionalRemarks { get; set; }
    }

    public class ExternalApprovalViewModel
    {
        public int AssignmentID { get; set; }
        public bool Accepted { get; set; }
        public int ApprovingEmployeeID { get; set; }
        public string OptionalRemarks { get; set; }
    }

    public class ExternalTransferAssignmentViewModel
    {
        //Requestor ID is the one initiating the transfer
        public bool ToMIS { get; set; }
        public int RequestorID { get; set; }
        public int ReceipientID { get; set; }
        public int CurrentAssignmentID { get; set; }
        public int FixAssetID { get; set; }
        public bool RequireApproval { get; set; }
        public string OptionalRemarks { get; set; }
        public int ApprovingID { get; set; }
        //public int AssignmentID { get; set; }
    }

    public class ExternalReleaseAssignmentViewModel
    {
        public int RequestorID { get; set; }
        public int AssignmentID { get; set; }
        public int FixAssetID { get; set; }
        public bool RequireApproval { get; set; }
        public int ReasonCode { get; set; }
        public string OptionalRemarks { get; set; }
        public int ApprovingID { get; set; }
        public int AcceptingID { get; set; }

    }

}