using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FASTWSv1.Common;
using FASTWSv1.Helpers;

namespace FASTWSv1.Controllers
{
    [RoutePrefix("api/Asset")]
    public class AssetController : ApiController
    {

        [HttpPost]
        [Route("AddAsset")]
        public HttpResponseMessage AddAsset(Models.ExternalAddAssetViewModel model)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();
            if (null != model)
            {
                //EmployeeID must be tracked
                if (model.EmployeeID != 0)
                {
                    if (assetProcess.AddNewAsset(model) == ReturnValues.SUCCESS)
                    {
                        Helpers.Logger.AddToAuditTrail(Logger.UserAction.ADD_ASSET, model.EmployeeID, String.Format("SUCCESSFUL. New Asset Tag {0}", model.AssetTag));
                        return ReturnMessages.RESPONSE_CREATED();
                    }
                }
            }

            Helpers.Logger.AddToAuditTrail(Logger.UserAction.ADD_ASSET, model.EmployeeID, String.Format("NOT SUCCESSFUL. Failed Asset Tag {0}",model.AssetTag));
                    
            return ReturnMessages.RESPONSE_NOTSUCCESSFUL();
        }

        [HttpGet]
        [Route("FixAssetID/{fixAssetID}")]
        public vwFixAssetList GetAssetByID(int fixAssetID)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();

            return assetProcess.GetAssetByID(fixAssetID);
        }

        [HttpGet]
        [Route("AssetTag/{assetTag}")]
        public vwFixAssetList GetAssetbyAssetTag(string assetTag)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();

            return assetProcess.GetAssetByAssetTag(assetTag);
        }

        [HttpGet]
        [Route("SerialNumber/{serialNumber}")]
        public vwFixAssetList GetAssetBySerialNumber(string serialNumber)
        {
            BO.AssetProcess assetProccess = new BO.AssetProcess();

            return assetProccess.GetAssetBySerialNumber(serialNumber);
        }

        [HttpGet]
        [Route("IssuerID/{issuerID}")]
        public vwFixAssetList GetAssetbyIssuerID(int issuerID)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();

            return assetProcess.GetAssetbyIssuerID(issuerID);
        }

        //TODO : This is only for devs, remove in final build
        [HttpGet]
        public List<FixAsset> GetAllAssets()
        {
            using (var db = new FASTDBEntities())
            {
                return db.FixAssets.ToList();
            }
        }


        [HttpGet]
        [Route("GetAssetTypes/{assetClass}")]
        public List<AssetType> GetAssetTypes(int assetClass)
        {
            BO.AssetProcess assetProcess = new BO.AssetProcess();

            return assetProcess.GetAssetTypes(assetClass);
        }

    }
}
