namespace MiniORM.App
{
    using System;
    using System.Collections.Generic;
    using Data.Entities;

    public class EmployeeViewModel
    {
        public string FullName { get; set; }

        public bool IsEmployed { get; set; }

        public virtual Department Department { get; set; }

        public ICollection<EmployeeProject> EmployeeProjects { get; }

        public override string ToString()
        {
            return $"{this.FullName}"
                   + Environment.NewLine
                   + $"{this.IsEmployed}"
                   + $"{this.Department?.Name}";
        }
    }
}