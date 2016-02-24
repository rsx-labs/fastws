using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

using FASTWSv1.Common;

namespace FASTWSv1.BO
{
    public class EmployeeProcess
    {
      
        public EmployeeProcess() { }
        
   
        public Employee GetEmployeeByID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                Employee[] employees = (from emp in db.Employees
                                        where emp.EmployeeID == employeeID
                                        select emp).ToArray();

                if (employees.Count() > 0)
                {
                    return employees[0];
                }
            }

            return null;
        }

        public vwEmployee GetEmployeeViewByID(int employeeID)
        {
            using (var db = new FASTDBEntities())
            {
                vwEmployee[] employees = (from emp in db.vwEmployees
                                        where emp.EmployeeID == employeeID
                                        select emp).ToArray();

                if (employees.Count() > 0)
                {
                    return employees[0];
                }
            }

            return null;
        }

        public Employee GetEmployeeByID(List<Employee> employeeSet, int employeeID)
        {
            Employee[] employees = (from emp in employeeSet
                                    where emp.EmployeeID == employeeID
                                    select emp).ToArray();


            if ( employees.Count() > 0 )
            {
                return employees[0];
            }

            return null;

        }
    }
}