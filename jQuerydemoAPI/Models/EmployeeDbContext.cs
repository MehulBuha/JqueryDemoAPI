using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace jQuerydemoAPI.Models
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasNoKey().ToView("GetEmployees");
        }
        public async Task<bool> InsertEmployeeAsync(Employee employee)
        {
            
                await Database.ExecuteSqlInterpolatedAsync($@"EXEC InsertEmployee 
                    @Name={employee.Name}, 
                    @Gender={employee.Gender}, 
                    @Email={employee.Email}, 
                    @Password={employee.Password}, 
                    @PhoneNo={employee.PhoneNo}, 
                    @Address={employee.Address}, 
                    @Occupation={employee.Occupation}, 
                    @DateOfBirth={employee.DateOfBirth}, 
                    @Images={employee.Images}
                   ");
                return true;
           
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            var employees = await Employees.FromSqlRaw("EXEC GetEmployeeById @EmployeeID",
                new SqlParameter("@EmployeeID", id)).ToListAsync();

            return employees.FirstOrDefault();
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            
                await Database.ExecuteSqlInterpolatedAsync($@"EXEC UpdateEmployee 
                    @Id={employee.Id}, 
                    @Name={employee.Name}, 
                    @Gender={employee.Gender}, 
                    @Email={employee.Email}, 
                    @Password={employee.Password}, 
                    @PhoneNo={employee.PhoneNo}, 
                    @Address={employee.Address}, 
                    @Occupation={employee.Occupation}, 
                    @DateOfBirth={employee.DateOfBirth}, 
                    @Images={employee.Images}
                   ");
                return true;
          
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
           
            
                await Database.ExecuteSqlInterpolatedAsync($"EXEC DeleteEmployee @Id={id}");
                return true;
            
            
        }
        public async Task<bool> ValidateLogin(string email, string password)
        {
            
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Email", SqlDbType.VarChar, 255) { Value = email },
                    new SqlParameter("@Password", SqlDbType.VarChar, 255) { Value = password },
                    new SqlParameter("@IsAuthenticated", SqlDbType.Int) { Direction = ParameterDirection.Output }
                };

                await Database.ExecuteSqlRawAsync("EXEC VerifyLogin @Email, @Password, @IsAuthenticated OUTPUT", parameters);

                int isAuthenticated = (int)parameters[2].Value;
                return isAuthenticated == 1;
            
          
        }


    }
}
