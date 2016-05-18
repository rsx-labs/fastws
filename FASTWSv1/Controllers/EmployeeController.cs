using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using FASTWSv1.Models;

namespace FASTWSv1.Controllers
{
    [RoutePrefix("api/Employee")]
    public class EmployeeController : ApiController
    {
        [HttpGet]
        [Route("EmployeeID/{employeeID}")]
        public vwEmployeeList GetEmployeeByID(int employeeID)
        {
            BO.EmployeeProcess employeeProcess = new BO.EmployeeProcess();

            return employeeProcess.GetEmployeeViewByID(employeeID);
        }

        [HttpGet]
        [Route("Department/{departmentID}")]
        public List<Employee> GetEmployeeByDepartment(int departmentID)
        {
            BO.EmployeeProcess employeeProcess = new BO.EmployeeProcess();

            using (var db = new FASTDBEntities())
            {
                List<Employee> list = (from emp in db.Employees
                                        where emp.DepartmentID == departmentID
                                        select emp).ToList();

                return list;
            }
        }


        //TODO : This is only for devs, remove in final build
        [HttpGet]
        public List<Employee> GetAllEmployees()
        {
            using ( var db = new FASTDBEntities())
            {
                return db.Employees.ToList();
            }
        }

    }
}
