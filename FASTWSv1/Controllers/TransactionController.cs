using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FASTWSv1.Controllers
{
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {
        [HttpGet]
        [Route("MIS/{employeeID}")]
        //EmployeeID is the MIS Employee ID
        public List<vwAssetAssignmentsForMI> GetMISTransactionsbyEmployeeID( int employeeID ) 
        {
            using( var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForMI> transactions = (from assigns in db.vwAssetAssignmentsForMIS
                                                              where assigns.MISEmployeeID == employeeID
                                                              select assigns).ToList();
                vwAssetAssignmentsForMI[] res = (from assigns in db.vwAssetAssignmentsForMIS
                          where assigns.MISEmployeeID == employeeID
                          select assigns).ToArray();
                return transactions;
            } 
        }

        [HttpGet]
        [Route("MISManager/{departmentID}")]
        //DepartmentID is the MIS Employee ID
        public List<vwAssetAssignmentsForMI> GetMISTransactionsbyDepartmentID(int departmentID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForMI> transactions = (from assigns in db.vwAssetAssignmentsForMIS
                                                              where assigns.DepartmentID == departmentID
                                                              select assigns).ToList();

                return transactions;
            }
        }

        [HttpGet]
        [Route("Admin/{employeeID}")]
        //EmployeeID is the Admin Employee ID
        public List<vwAssetAssignmentsForCustodian> GetAdminTransactionsbyEmployeeID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForCustodian> transactions = (from assigns in db.vwAssetAssignmentsForCustodians
                                                              where assigns.AdminID == employeeID
                                                              select assigns).ToList();

                return transactions;
            }
        }

        [HttpGet]
        [Route("AdminManager/{departmentID}")]
        //EmployeeID is the Admin Employee ID
        public List<vwAssetAssignmentsForCustodian> GetAdminTransactionsbyDepatmentID(int departmentID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForCustodian> transactions = (from assigns in db.vwAssetAssignmentsForCustodians
                                                                     where assigns.DepartmentID == departmentID
                                                                     select assigns).ToList();

                return transactions;
            }
        }

        [HttpGet]
        [Route("ManagerByID/{employeeID}")]
        //EmployeeID is the Admin Employee ID
        public List<vwAssetAssignmentsForManager> GetManagerTransactionsbyEmployeeID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForManager> transactions = (from assigns in db.vwAssetAssignmentsForManagers
                                                                     where assigns.ManagerID == employeeID
                                                                     select assigns).ToList();

                return transactions;
            }
        }

        [HttpGet]
        [Route("ManagerByDepartmentID/{departmentID}")]
        //EmployeeID is the Admin Employee ID
        public List<vwAssetAssignmentsForManager> GetManagerTransactionsbyDepartmentID(int departmentID)
        {
            using (var db = new FASTDBEntities())
            {
                List<vwAssetAssignmentsForManager> transactions = (from assigns in db.vwAssetAssignmentsForManagers
                                                                   where assigns.DepartmentID == departmentID
                                                                   select assigns).ToList();

                return transactions;
            }
        }

    }
}

