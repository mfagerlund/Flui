using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace FluiDemo.ListUi
{
    public class Office
    {
        public string Name { get; set; }
        public List<Employee> Employees { get; set; } = new();

        public float GetSalarySum() => Employees.Sum(x => x.Salary);

        private void DeleteEmployee(Employee employee) => Employees.Remove(employee);

        public void AddRandomEmployee()
        {
            Employees.Add(new Employee
            {
                Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6),
                Title = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 3),
                Salary = Random.Range(1, 6)
            });
        }

        public static List<Office> CreateOfficeList()
        {
            return new List<Office>
            {
                new Office
                {
                    Name = "Central",
                    Employees = new()
                    {
                        new Employee { Name = "Arne", Title = "Chimney Sweep", Salary = 7 },
                        new Employee { Name = "Benny", Title = "Chimney Sweep", Salary = 3 },
                        new Employee { Name = "Steve", Title = "Stevedore", Salary = 6 },
                        new Employee { Name = "John", Title = "Yeoman", Salary = 2 }
                    }
                },
                new Office
                {
                    Name = "South",
                    Employees = new()
                    {
                        new Employee { Name = "Schultz", Title = "Chimney Sweep", Salary = 7 },
                        new Employee { Name = "Frantz", Title = "Chimney Sweep", Salary = 3 },
                        new Employee { Name = "Hanz", Title = "Stevedore", Salary = 6 },
                        new Employee { Name = "Salander", Title = "Yeoman", Salary = 2 }
                    }
                }
            };
        }
    }
}