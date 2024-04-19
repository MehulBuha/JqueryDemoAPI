using jQuerydemoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace jQuerydemoAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private readonly EmployeeDbContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public EmployeeController(EmployeeDbContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("list")]
        public IActionResult EmployeeList(DateTime? fromDate = null, DateTime? toDate = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "CreationDate", string sortDirection = "DESC", string searchTerm = null)
        {
            try
            {
                List<Employee> employees = _dbContext.Employees
                    .FromSqlRaw("EXEC GetEmployees @fromDate, @toDate, @pageSize, @pageNumber, @sortColumn, @sortDirection, @searchTerm",
                        new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value),
                        new SqlParameter("@toDate", toDate ?? (object)DBNull.Value),
                        new SqlParameter("@pageSize", pageSize),
                        new SqlParameter("@pageNumber", pageNumber),
                        new SqlParameter("@sortColumn", sortColumn),
                        new SqlParameter("@sortDirection", sortDirection),
                        new SqlParameter("@searchTerm", searchTerm ?? (object)DBNull.Value))
                    .ToList();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving employee data: " + ex.Message);
            }
        }


        [HttpPost("insert")]
        public async Task<IActionResult> InsertEmployee([FromForm] Employee employee, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                employee.Images = await SaveImageAsync(Image);
                await _dbContext.InsertEmployeeAsync(employee);
                return Ok(new { success = true, message = "Employee inserted successfully", data = employee });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while saving employee data: " + ex.Message);
            }
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Check if the image file is not null and has content
            if (imageFile == null || imageFile.Length == 0)
                return null; // Or throw an exception if you want to handle it differently

            try
            {
                // Get the uploads directory path
                var uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "Images");

                // Create the uploads directory if it doesn't exist
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate a unique file name for the image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Combine the uploads directory path with the file name
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save the image file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Return the relative path of the saved image (optional)
                return fileName;
            }
            catch (Exception ex)
            {
                // Handle exceptions here
                Console.WriteLine($"Error saving image: {ex.Message}");
                return null;
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> EditEmployee(int id)
        {
            try
            {
                Employee employee = await _dbContext.GetEmployeeByIdAsync(id);
                if (employee != null)
                    return Ok(new { success = true, data = employee });
                else
                    return NotFound(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while fetching employee data: " + ex.Message);
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateEmployee([FromForm] Employee employee, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (Image != null)
                {
                    employee.Images = await SaveImageAsync(Image);
                }

                await _dbContext.UpdateEmployeeAsync(employee);
                return Ok(new { success = true, message = "Employee updated successfully", data = employee });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while updating employee data: " + ex.Message);
            }
        }
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteEmployeeAsync(int id)
        {
            try
            {
                var result = await _dbContext.DeleteEmployeeAsync(id);
                if (result)
                {
                    return Ok(new { success = true, message = "Employee deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Employee not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting employee: " + ex.Message);
            }
        }

    }
}
