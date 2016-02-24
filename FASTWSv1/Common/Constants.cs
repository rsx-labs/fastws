using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FASTWSv1.Common
{
    public static class Constants
    {
        public const string AUDIT_ACTION_REG = "Registration";
        public const string AUDIT_ACTION_LOGIN = "Login";
        public const string AUDIT_ACTION_RESET = "Reset Password";
        public const string AUDIT_ACTION_CHANGE = "Change Password";

        public const int ASSET_CLASS_NON_IT = 1;
        public const int ASSET_CLASS_IT = 2;
        public const int ASSET_CLASS_OTHERS = 3;

        public const int ASSIGNMENT_STATUS_WT_APPROVAL = 1;
        public const int ASSIGNMENT_STATUS_WT_ACCEPTANCE = 2;
        public const int ASSIGNMENT_STATUS_ACCEPTED = 3;
        public const int ASSIGNMENT_STATUS_FOR_TRANSFER = 4;
        public const int ASSIGNMENT_STATUS_REJECTED = 5;
        public const int ASSIGNMENT_STATUS_RELEASED = 6;
        public const int ASSIGNMENT_STATUS_DENIED = 7;

        public const int ASSET_STATUS_NEW = 1;
        public const int ASSET_STATUS_FORASSIGNMENT = 2;
        public const int ASSET_STATUS_WITH_MIS = 3;
        public const int ASSET_STATUS_ASSIGNED = 4;
        public const int ASSET_STATUS_DECOMMISSIONED = 5;
        public const int ASSET_STATUS_FORTRANSFER = 6;
        public const int ASSET_STATUS_FORRELEASE = 7;
        public const int ASSET_STATUS_FORREPAIR = 8;
        public const int ASSET_STATUS_RELEASED = 9;

        public const int RELEASE_REASON_FOR_REASSIGNMENT = 1;
        public const int RELEASE_REASON_FOR_REPAIR = 2;
        public const int RELEASE_REASON_FOR_DISPOSAL = 3;
        public const int RELEASE_REASON_FOR_CORRECTION = 4;
        public const int RELEASE_REASON_OTHERS = 5;

        public const string ASSET_STATUS = "FA";
        public const string ASSIGN_STATUS = "AA";

        public const string EXISTS = "EXISTS";
        public const string MISSING_ID = "MISSING ID";
        public const string MISSING_CD = "MISSING CODE";
        public const string NOT_AVAILABLE = "NOT AVAILABLE";

        public static string GetAssetClassDesc(int assetClassID)
        {
            //Since the items are small, we can use this rather than querying the DB
            switch(assetClassID)
            {
                case ASSET_CLASS_IT:
                    return "IT Equipment";
                case ASSET_CLASS_NON_IT:
                    return "Non-IT Equipment";
                default:
                    return "Others";
            }
        }

        public static string GetAssetStatusDesc(int assetStatusID)
        {
            //Since the items are small, we can use this rather than querying the DB
            switch (assetStatusID)
            {
                case ASSET_STATUS_NEW:
                    return "New";
                case ASSET_STATUS_FORASSIGNMENT:
                    return "For Assignment";
                case ASSET_STATUS_ASSIGNED:
                    return "Assigned";
                case ASSET_STATUS_DECOMMISSIONED:
                    return "Decommissioned";
                case ASSET_STATUS_WITH_MIS:
                    return "Assigned to MIS";
                case ASSET_STATUS_FORRELEASE:
                    return "For Release";
                case ASSET_STATUS_FORTRANSFER:
                    return "For Transfer";
                case ASSET_STATUS_FORREPAIR:
                    return "For Repair";
                default:
                    return "Others";
            }
        }

        public static string GetAssignmentStatusDesc(int assignmentStatusID)
        {
            //Since the items are small, we can use this rather than querying the DB
            switch (assignmentStatusID)
            {
                case ASSIGNMENT_STATUS_ACCEPTED:
                    return "Accepted";
                case ASSIGNMENT_STATUS_FOR_TRANSFER:
                    return "For Transfer";
                case ASSIGNMENT_STATUS_WT_ACCEPTANCE:
                    return "Waiting for Acceptance";
                case ASSIGNMENT_STATUS_WT_APPROVAL:
                    return "Waiting for Approval";
                case ASSIGNMENT_STATUS_REJECTED:
                    return "Rejected";
                case ASSIGNMENT_STATUS_RELEASED:
                    return "Released";
                default:
                    return "Others";
            }
        }

        public static string GetReasonCodeDesc(int reasonCode)
        {
            //Since the items are small, we can use this rather than querying the DB
            switch (reasonCode)
            {
                case RELEASE_REASON_FOR_REASSIGNMENT:
                    return "For Reassignment";
                case RELEASE_REASON_FOR_REPAIR:
                    return "For Repair";
                case RELEASE_REASON_FOR_DISPOSAL:
                    return "For Disposal";
                case RELEASE_REASON_FOR_CORRECTION:
                    return "For Correction";
                default:
                    return "Others";
            }
        }

    }
}