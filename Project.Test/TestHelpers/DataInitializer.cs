using Project.Core.Common.Enum;
using Project.Core.Entities;
using System;
using System.Collections.Generic;

namespace Project.Test.TestHelpers
{
    public class DataInitializer
    {
        public static List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>()
            {
                new Employee {
                Id = "7C39DD64-4643-B552-1213-1243D9F5D643",
                FirstName = "Colleen",
                LastName = "Wyatt",
                Phone = "1-587-268-3228",
                Email = "urna.et.arcu@yahoo.edu",
                DateOfBirth = Convert.ToDateTime("1932-12-24")
                },
                new Employee {
                    Id = "EA2E7616-1428-2E6E-9858-136E89E456C9",
                    FirstName = "Galvin",
                    LastName = "Carlson",
                    Phone = "120-2165",
                    Email = "convallis.est@protonmail.couk",
                    DateOfBirth = DateTime.Parse("1934-08-25")
                },
                new Employee {
                    Id = "18DF809D-4833-669B-EF81-EAD4B48EB1A9",
                    FirstName = "Jenna",
                    LastName = "Mullins",
                    Phone = "1-142-253-5067",
                    Email = "ut.molestie@aol.edu",
                    DateOfBirth = DateTime.Parse("1970-11-15")
                },
                new Employee {
                    Id = "CACB9AA9-D434-8CEE-DD96-B4C20191ED94",
                    FirstName = "Adam",
                    LastName = "Marks",
                    Phone = "674-1489",
                    Email = "adipiscing@hotmail.org",
                    DateOfBirth = DateTime.Parse("1957-05-26")
                },
                new Employee {
                    Id = "99EBC81B-E423-D78A-B833-2CFDD8C28DA1",
                    FirstName = "Lacey",
                    LastName = "Hendrix",
                    Phone = "1-294-259-6844",
                    Email = "nulla.cras@yahoo.com",
                    DateOfBirth = DateTime.Parse("1997-08-30")
                }
            };
            return employees;
        }

        public static List<LostProperty> GetAllLostProperties()
        {
            var lostProperties = new List<LostProperty>
            {
                new LostProperty {
                    Id = 0,
                    Name = "Asus charger",
                    Description = "risus quis diam luctus lobortis. Class",
                    Status = PropertyStatus.Lost,
                    Location = "est ac mattis",
                    FoundTime = DateTime.Parse("2021-12-09"),
                    EmployeeId = "7C39DD64-4643-B552-1213-1243D9F5D643"
                },
                new LostProperty {
                    Id = 1,
                    Name = "KMS employee mug",
                    Description = "semper cursus. Integer mollis. Integer tincidunt aliquam",
                    Status = PropertyStatus.Lost,
                    Location = "Pellentesque ut",
                    FoundTime = DateTime.Parse("2023-03-01"),
                    EmployeeId = null
                },
                new LostProperty {
                    Id = 2,
                    Name = "Airpod 2",
                    Description = "dis parturient montes, nascetur ridiculus mus.",
                    Status = PropertyStatus.Found,
                    Location = "aliquet libero.",
                    FoundTime = DateTime.Parse("2021-06-08"),
                    EmployeeId = null
                },
                new LostProperty {
                    Id = 3,
                    Name = "Logitech mouse G102",
                    Description = "velit eu sem. Pellentesque ut ipsum",
                    Status = PropertyStatus.Found,
                    Location = "et arcu imperdiet",
                    FoundTime = DateTime.Parse("2022-02-25"),
                    EmployeeId = "7C39DD64-4643-B552-1213-1243D9F5D643"
                },
                new LostProperty {
                    Id = 4,
                    Name = "KMS ID Badge",
                    Description = "Nulla facilisis. Suspendisse commodo tincidunt nibh. Phasellus",
                    Status = PropertyStatus.Return,
                    Location = "elit, pellentesque a,",
                    FoundTime = DateTime.Parse("2022-06-06"),
                    EmployeeId = "99EBC81B-E423-D78A-B833-2CFDD8C28DA1"
                }
            };
            return lostProperties;
        }
    }
}
