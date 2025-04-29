using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Enums;

namespace Project.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController: ControllerBase {
        private readonly AppDbContext context;

        public CustomerController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet("ShowAllCustomers")]
        [Authorize(Roles = "Admin")]
        public  IActionResult ShowAllCustomers() {
            var customers = context.Customers.ToList();
            var result = new List<ShowCustomerDTO>();
            foreach (var customer in customers) {
                result.Append(new ShowCustomerDTO {
                    CustomerId = customer.Id,
                    CustomerName = customer.UserName,
                    Status = customer.Status.ToString()
                });
            }
            return Ok(result);
        }

        [HttpGet("ShowCustomerwithUserName")]
        [Authorize(Roles = "Admin")]
        public IActionResult ShowCustomerwithUserName(string UserName) 
        {
            var customers = context.Customers.Where(e=>e.UserName== UserName).ToList();
            if (customers == null || customers.Count == 0)
            {
                return NotFound("No customers found with the given username.");
            }
            var result = new List<ShowCustomerDTO>();
            foreach (var customer in customers)
            {
                result.Append(new ShowCustomerDTO
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.UserName,
                    Status = customer.Status.ToString()
                });
            }
            return Ok(result);
        }

            [HttpPut("UpdateCustomerStatus")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateCustomerStatus(string customerId, string status) {
            var customer = context.Customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null) {
                return NotFound("Customer not found");
            }
            if (Enum.TryParse(status, out AccStatus customerStatus)) {
                customer.Status = customerStatus;
                context.SaveChanges();
                return Ok("Customer status updated successfully");
            }
            return BadRequest("Invalid status");
        }
    }
}
