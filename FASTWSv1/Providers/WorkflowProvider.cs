using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FASTWSv1.Common;

namespace FASTWSv1.Providers
{
    //This is poormans WWF . Using windows workflow maybe an overkill
    public static class WorkflowProvider
    {
        public enum ActionType{
            ASSIGNMENT_MIS,
            ASSIGNMENT_EMPLOYEE,
            ASSIGNMENT_ACCEPTED,
            ASSIGNMENT_REJECTED,
            ASSET_NEW,
            ASSET_TRANSFER_WOAPPROVAL,
            ASSET_TRANSFER,
            ASSET_TRANSFER_APPROVED,
            ASSET_TRANSFER_DENIED,
            ASSET_RELEASE,
            ASSET_RELEASE_WOAPPROVAL,
            ASSET_RELEASE_APPROVED,
            ASSET_RELEASE_DENIED,
            ASSET_RELEASE_ACCEPTED,
            ASSET_RELEASE_REJECTED
        };

        public static Dictionary<string,int> GetNextStatus( ActionType action, int assetID, int assignmentID = 0)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();
            BO.AssignmentProcess assignProcess = new BO.AssignmentProcess();

            Dictionary<string, int> result = new Dictionary<string, int>();

            vwFixAssetList asset = assetProcess.GetAssetByID(assetID);
            AssetAssignment assignment = new AssetAssignment();

            if ( assignmentID > 0 )
            {
                assignment = assignProcess.GetAssignmentbyID(assignmentID);
            }

            //FA - FixAsset Status;
            //AA - Current Assignment Status ( for transafer, this is for the requestor)
            //NAA - Next Assignment Status ( only for transfer, this is for the receipient)

            switch(action)
            {
                case ActionType.ASSET_NEW:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_FORASSIGNMENT);
                    return result;
                case ActionType.ASSET_TRANSFER:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_FORTRANSFER);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_APPROVAL);
                    return result;
                case ActionType.ASSET_TRANSFER_WOAPPROVAL:
                case ActionType.ASSET_TRANSFER_APPROVED:
                    result.Clear();
                    if (asset.AssetStatusID == Constants.ASSET_STATUS_WITH_MIS)
                    {
                        result.Add("FA", Constants.ASSET_STATUS_ASSIGNED);
                        result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_ACCEPTANCE);
                    }
                    else
                    {
                        result.Add("FA", Constants.ASSET_STATUS_ASSIGNED);
                        result.Add("AA", Constants.ASSIGNMENT_STATUS_RELEASED);
                        result.Add("NAA", Constants.ASSIGNMENT_STATUS_WT_ACCEPTANCE);
                    }
                    return result;
                case ActionType.ASSET_TRANSFER_DENIED:
                case ActionType.ASSET_RELEASE_DENIED:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_ASSIGNED);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_ACCEPTED);
                    return result;
                case ActionType.ASSET_RELEASE:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_FORRELEASE);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_APPROVAL);
                    return result;
                case ActionType.ASSET_RELEASE_WOAPPROVAL:
                case ActionType.ASSET_RELEASE_APPROVED:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_RELEASED);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_ACCEPTANCE);
                    return result;
                case ActionType.ASSIGNMENT_MIS:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_WITH_MIS);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_ACCEPTANCE);
                    return result;
                case ActionType.ASSIGNMENT_EMPLOYEE:
                    result.Clear();
                    result.Add("FA", Constants.ASSET_STATUS_ASSIGNED);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_ACCEPTANCE);
                    return result;
                case ActionType.ASSET_RELEASE_ACCEPTED:
                case ActionType.ASSIGNMENT_ACCEPTED:
                    result.Clear();
                    result.Add("FA", asset.AssetStatusID);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_ACCEPTED);
                    return result;
                case ActionType.ASSET_RELEASE_REJECTED:
                    result.Clear();
                    result.Add("FA", Constants.ASSIGNMENT_STATUS_RELEASED);
                    result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_APPROVAL);
                    return result;
                case ActionType.ASSIGNMENT_REJECTED:
                    result.Clear();
                    switch(asset.AssetStatusID)
                    {
                        case Constants.ASSET_STATUS_WITH_MIS:
                            result.Add("FA", Constants.ASSET_STATUS_FORRELEASE);
                            result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_APPROVAL);
                            break;
                        case Constants.ASSET_STATUS_ASSIGNED:
                            result.Add("FA", Constants.ASSET_STATUS_FORRELEASE);
                            result.Add("AA", Constants.ASSIGNMENT_STATUS_WT_APPROVAL);
                            break;
                        case Constants.ASSET_STATUS_FORRELEASE:
                            result.Add("FA", asset.AssetStatusID);
                            result.Add("AA", assignment.AssignmentStatusID);
                            break;
                    }
                    return result;
            }

            return result;
        }

     




    }
}