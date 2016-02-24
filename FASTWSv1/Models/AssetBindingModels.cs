using System;
using System.Collections.Generic;

namespace FASTWSv1.Models
{
    //Models for using the AssetController
    /// <summary>
    /// 
    /// </summary>
    public class ExternalAddAssetViewModel
    {
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string AssetTag { get; set; }
        public string Brand { get; set; }
        public string Remarks { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int IssuerID {get;set;}
        public int LocationID {get;set;}
        public int AssetTypeID {get;set;}
        //The user adding the asset
        public int EmployeeID { get; set; }
        public int AssetClassID { get; set; }

        public FixAsset GetNewFixAssetData()
        {
            FixAsset asset = new FixAsset();

            asset.Model = this.Model;
            asset.SerialNumber = this.SerialNumber;
            asset.AssetTag = this.AssetTag;
            asset.Brand = this.Brand;
            asset.Remarks = this.Remarks;
            asset.AcquisitionDate = this.AcquisitionDate;
            asset.ExpiryDate = this.ExpiryDate;
            asset.IssuerID = this.IssuerID;
            asset.LocationID = this.LocationID;
            asset.AssetTypeID = this.AssetTypeID;
            asset.AssetClassID = this.AssetClassID;
            //new assets dont have FixAssetID
            //Status is autoatically set to For Assignment
            asset.AssetStatusID = Common.Constants.ASSET_STATUS_FORASSIGNMENT;

            return asset;
        }

    }

}