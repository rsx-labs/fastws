using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

using FASTWSv1.Common;

namespace FASTWSv1.BO
{
    public class AssetProcess
    {
        public FixAsset GetAssetByID(int assetID)
        {
            using (var db = new FASTDBEntities())
            {
                FixAsset[] assets = (from asset in db.FixAssets
                                     where asset.FixAssetID == assetID
                                     select asset).ToArray();


                if (assets.Count() > 0)
                {
                    return assets[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public FixAsset GetAssetByAssetTag(string assetTag)
        {
            using (var db = new FASTDBEntities())
            {
                FixAsset[] assets = (from asset in db.FixAssets
                                     where (0 == String.Compare(assetTag, asset.AssetTag))
                                     select asset).ToArray();

                if (assets.Count() > 0)
                {
                    return assets[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public FixAsset GetAssetBySerialNumber(string serialNumber)
        {
            using (var db = new FASTDBEntities())
            {
                FixAsset[] assets = (from asset in db.FixAssets
                                     where (0 == String.Compare(serialNumber, asset.SerialNumber))
                                     select asset).ToArray();

                if ( assets.Count() > 0)
                {
                    return assets[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public FixAsset GetAssetbyIssuerID(int issuerID)
        {
            using ( var db = new FASTDBEntities())
            {
                FixAsset[] assets = (from asset in db.FixAssets
                                     where asset.IssuerID == issuerID
                                     select asset).ToArray();

                if ( assets.Count() > 0)
                {
                    return assets[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public int AddNewAsset(Models.ExternalAddAssetViewModel model)
        {
            using (var db = new FASTDBEntities())
            {
                FixAsset newAsset = new FixAsset();
                newAsset = model.GetNewFixAssetData();

               
                db.FixAssets.Add(newAsset);

                int result = db.SaveChanges();

                if ( result > 0 )
                {
                    return ReturnValues.SUCCESS;
                }
                else
                {
                    return ReturnValues.FAILED;
                }
                
            }
        }

        public int UpdateAssetStatus(int assetID, int newStatus)
        {
            using (var db = new FASTDBEntities())
            {
                FixAsset[] assets = (from asset in db.FixAssets
                                     where asset.FixAssetID == assetID
                                     select asset).ToArray();

                if (assets.Count() > 0)
                {
                    foreach (FixAsset asset in assets)
                    {
                        asset.AssetStatusID = newStatus;

                        db.SaveChanges();
                    }

                    return ReturnValues.SUCCESS;
                }

                return ReturnValues.FAILED;
            }
        }
        
        public vwFixAsset GetFixAssetViewByID(int fixAssetID)
        {
            using ( var db = new FASTDBEntities())
            {
                List<vwFixAsset> assets = (from asset in db.vwFixAssets
                                            where asset.FixAssetID == fixAssetID
                                            select asset).ToList();
                if (assets != null)
                {
                    return assets[0];
                }
            }
            return null;
        }


     }
}