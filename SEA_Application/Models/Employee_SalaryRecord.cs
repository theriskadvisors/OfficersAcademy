//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEA_Application.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Employee_SalaryRecord
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee_SalaryRecord()
        {
            this.Employee_SalaryIncrement = new HashSet<Employee_SalaryIncrement>();
        }
    
        public int Id { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<double> StartingSalary { get; set; }
        public Nullable<double> CurrentSalary { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
    
        public virtual AspNetEmployee AspNetEmployee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee_SalaryIncrement> Employee_SalaryIncrement { get; set; }
    }
}
