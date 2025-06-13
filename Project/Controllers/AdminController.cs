using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Project.DTOs;
using Project.Enums;
using Project.Services.Implementations;
using Project.Services.Interfaces;
using Project.Tables;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<Person> _userManager;

        public AdminController(AppDbContext context , IEmailService emailService, UserManager<Person> userManager)
        {
            _context = context; 
            _emailService = emailService;
            _userManager = userManager;

        }




        //  Get All Active Orders for Delivery Person
        [HttpGet("ShowAllOrders")]
 
        public async Task<IActionResult> ShowAllOrders()
        {
            
            var orders = await _context.Orders
                .Include(o => o.customer)
                .Include(o => o.orderItems).
                Include(o => o.deliveryrep)
                .ToListAsync();

           if (orders.Count > 0)
                { 
            var orderDTOs = orders.Select(o => new OrderadminDTO
            {
                Id = o.Id,
                status = o.Status.ToString(),
                CustomerId = o.CustomerId,
                UserName = o.customer.UserName,
                address = o.address,
                phone = o.phone,
                DeliveryId = o.DeliveryId,
                DeliveryName = o.deliveryrep.UserName
            }).ToList();
            return Ok(orderDTOs);
            }
            return BadRequest(new { message = "No orders found." });

        }


        [HttpGet("ShowSpecificOrders")]

        public async Task<IActionResult> ShowSpecificOrders(int orderId)
        {

            var Order = await _context.Orders.FindAsync(orderId);
            if (Order == null)
                return NotFound(new { message = "Order not found." });


            var orders = await _context.Orders
                .Where(o => (o.Id == orderId))
                .Include(o => o.customer)
                .Include(o => o.orderItems).
                Include(o => o.deliveryrep)
                .ToListAsync();

            if (orders.Count > 0)
            {
                var orderDTOs = orders.Select(o => new OrderadminDTO
                {
                    Id = o.Id,
                    status = o.Status.ToString(),
                    CustomerId = o.CustomerId,
                    UserName = o.customer.UserName,

                    address = o.address,
                    phone = o.phone,
                    DeliveryId = o.DeliveryId,
                    DeliveryName = o.deliveryrep.UserName
                }).ToList();
                return Ok(orderDTOs);
            }
            return BadRequest(new { message = "No orders found." });
        }
               
           





        [HttpGet("GetOrderDetails")]

        public async Task<IActionResult> GetOrderDetails( int orderId)
        {                         
            var Order = await _context.Orders.FindAsync(orderId);
            if (Order == null)
                return NotFound(new { message = "Order not found." });

            var order = await _context.Orders
                  .Where(o => (o.Id == orderId ))
                  .Include(o => o.customer)
                  .Include(o => o.deliveryrep)
                  .Include(o => o.orderItems)
                      .ThenInclude(oi => oi.product)
                   .Include(o => o.orderItems)
                      .ThenInclude(oi => oi.color)
                   .Include(o => o.orderItems)
                      .ThenInclude(oi => oi.size)
                   .Include(o => o.orderItems)
                      .ThenInclude(o => o.product)
                          .ThenInclude(o => o.images)
                  .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = "Order not found." });

            var orderDTO = new specificOrderadminDto
            {
                Id = order.Id,                
                status = order.orderItems.Select(oi => oi.Status.ToString()).ToArray(),
                CustomerId = order.CustomerId,
                UserName = order.customer.UserName,
                address = order.address,              
                phone = order.phone,
                // All unique product IDs in this order
                ProductId = order.orderItems.Select(oi => oi.product.Id).ToArray(),
                ProductsName = order.orderItems.Select(oi => oi.product.Title).ToArray(),
                TotalPrice = order.TotalPrice,
                // All unique quantities in this order
                quantity = order.orderItems.Select(oi => oi.Quantity).ToArray(),
                DeliveryId = order.DeliveryId,
                DeliveryName = order.deliveryrep.UserName

            };

            return Ok(orderDTO);
        }


        [HttpPut("UpdateStatusOrder")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusOrder(int orderId, string Status)
        {

            if (!Enum.TryParse<OrdStatus>(Status, true, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            var order = await _context.Orders
                .Include(o => o.orderItems)
                    .ThenInclude(oi => oi.merchant) // optional if you need it
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound(new { message = "Order not found." });
            if (order.Status == newStatus)
                return BadRequest(new { message = "Order status is already set to the specified value." });

            if (order.orderItems == null)
                return BadRequest(new { message = "Order items not loaded." });

            foreach (var orderItem in order.orderItems)
            {
                orderItem.Status = newStatus;
            }





            //  Notify customer by email    
            var customer = await _context.Users.FindAsync(order.CustomerId);
                if (customer != null)
                {
                    await _emailService.SendEmailAsync(
                        customer.Email,
                        $"Your Order Status {order.Status}",
                        $"Dear {customer.UserName}, your order : {newStatus}."
                    );
                }

            //  Notify delivery by email
            var delivery = await _context.Users.FindAsync(order.DeliveryId);
                if (delivery != null)
                {
                    await _emailService.SendEmailAsync(
                        delivery.Email,
                        $"Your Order Status {order.Status}",
                        $"Dear {delivery.UserName},  order : {newStatus}."
                    );
                }
            //  Notify Merchant by email
            foreach (var x in order.orderItems)
            {
                var merchant = await _context.Users.FindAsync(x.merchant.Id);
                if (merchant != null)
                {
                    await _emailService.SendEmailAsync(
                        merchant.Email,
                        $"Your Order Status {order.Status}",
                        $"Dear {merchant.UserName},  order : {newStatus}."
                    );
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Order status updated successfully to {order.Status}." });


        }   
        


        [HttpPut("UpdatePhone")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePhone(int orderId, string newPhone)
        {

            var order = await _context.Orders
    .Include(o => o.orderItems)
        .ThenInclude(oi => oi.merchant) // optional if you need it
    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            if (string.IsNullOrEmpty(newPhone) || order.phone == newPhone)
                return BadRequest(new { message = "Please provide a new phone number." });
            else
            {
                if (!string.IsNullOrEmpty(newPhone))
                    order.phone = newPhone;            }                 

                   
            //  Notify customer by email
            var customer = await _context.Users.FindAsync(order.CustomerId);
            if (customer != null)
            {
                await _emailService.SendEmailAsync(
                    customer.Email,
                    $"Your Order  {order.Id}",
                    $"Dear {customer.UserName}, your Phone : {newPhone}."
                );
            }

            //  Notify delivery by email
            var delivery = await _context.Users.FindAsync(order.DeliveryId);
            if (delivery != null)
            {
                await _emailService.SendEmailAsync(
                    delivery.Email,
                    $"Update Order {order.Id}",
                    $"Dear {delivery.UserName}, Update Phone {newPhone} ."
                );
            }


            await _context.SaveChangesAsync();
            return Ok(new { message = "Phone updated successfully." });
        }



        [HttpPut("UpdateAdress")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAdress(int orderId, string newaddress)
        {

            var order = await _context.Orders
   .Include(o => o.orderItems)
       .ThenInclude(oi => oi.merchant) // optional if you need it
   .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { message = "Order not found." });

            if (string.IsNullOrEmpty(newaddress) || order.address == newaddress)
                return BadRequest(new { message = "Please provide a new address number." });
            else
            {
                if (!string.IsNullOrEmpty(newaddress))
                    order.address = newaddress;
            }


            //  Notify customer by email
            var customer = await _context.Users.FindAsync(order.CustomerId);
            if (customer != null)
            {
                await _emailService.SendEmailAsync(
                    customer.Email,
                    $"Your Order  {order.Id}",
                    $"Dear {customer.UserName}, your Phone : {newaddress}."
                );
            }

            //  Notify delivery by email
            var delivery = await _context.Users.FindAsync(order.DeliveryId);
            if (delivery != null)
            {
                await _emailService.SendEmailAsync(
                     delivery.Email,
                    $"Update Order {order.Id}",
                    $"Dear {delivery.UserName}, Update address {newaddress} ."
                );
            }


            await _context.SaveChangesAsync();
            return Ok(new { message = "address updated successfully." });
        }


        


        [HttpPost("create-delivery-rep")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDeliveryRep([FromBody] dtoNewDeliveryRep Rep)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (Rep == null)
                return BadRequest(new { message = "Invalid data." });

            var existingEmail = await _userManager.FindByEmailAsync(Rep.Email);
            if (existingEmail != null)
                return BadRequest(new { message = "Email already exists." });

            var existingUserName = await _userManager.FindByNameAsync(Rep.UserName);
            if (existingUserName != null)
                return BadRequest(new { message = "Username already exists." });

            // Check NationalId
            bool nationalExists = await _context.Admins.AnyAsync(a => a.NationalId == Rep.NationalId)
                                || await _context.DeliveryReps.AnyAsync(a => a.NationalId == Rep.NationalId)
                                || await _context.Merchants.AnyAsync(a => a.NationalId == Rep.NationalId);
            if (nationalExists)
                return BadRequest(new { message = "National ID already exists." });

            if(Rep.UserName == null ||Rep.Email == null || Rep.Password ==null ||
                Rep.Phone == null || Rep.State == null || Rep.Governorate == null ||
                Rep.Location == null || Rep.Gender == null || Rep.BirthDate == null )
                return BadRequest(new { message = "All fields are required." });

            if (!Enum.TryParse<GenderType>(Rep.Gender, true, out var newGender))
                return BadRequest(new { message = "Invalid status value." });

            // Create DeliveryRep
            var deliveryRep = new DeliveryRep
            {
                UserName = Rep.UserName,
                Email = Rep.Email,
                NationalId = Rep.NationalId,
                adminId = Rep.adminId,
                BirthDate = Rep.BirthDate,
                Gender = newGender,
                Governorate = Rep.Governorate,
                Location = Rep.Location,
                State = Rep.State,
                Type = Enums.PersonType.DeliveryRep,
                Status = Enums.AccStatus.Active,
                PhoneNumber = Rep.Phone,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(deliveryRep, Rep.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            

            // Generate and send verification code
            var verificationCode = GenerateVerificationCode();
            deliveryRep.VerificationCode = verificationCode;
            deliveryRep.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5);

            await _userManager.UpdateAsync(deliveryRep);

            await _emailService.SendEmailAsync(deliveryRep.Email, "Confirm Your Account",
                $"Welcome to our platform, please verify your email with this code: {verificationCode}");

            return Ok(new { message = "DeliveryRep created successfully." });
        }






        [HttpGet("GetAllDeliveryReps")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDeliveryReps()
        {
            var deliveryReps = await _context.DeliveryReps
                .Include(dr => dr.admin)
                .Include(dr => dr.Orders)
                 .ThenInclude(dr => dr.orderItems)
                .Select(dr => new
                {
                    id = dr.Id,
                    Name = dr.UserName,
                    //  HireAge = DateTime.Now.Year -dr.HireDate.Year,
                    HireAge = dr.HireAge,
                    Status = dr.Status.ToString(),
                    DeliveredOrdersCount = dr.Orders.Count(o => o.orderItems.Any(oi => (int)oi.Status == (int)OrdStatus.Recieved)),

                    CreatedByAdmin = dr.admin != null ? dr.admin.UserName : "Unknown"
                })
                .ToListAsync();

            if (deliveryReps == null || !deliveryReps.Any())
                return NotFound(new { message = "There are no delivery reps in the system." });

            return Ok(deliveryReps);
        }


        [HttpGet("GetDeliveryRepwithID")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeliveryRepwithID(string Id)
        {
            var deliveryReps = await _context.DeliveryReps
                .Where(e => e.Id == Id)
                .Include(dr => dr.admin)
                .Include(dr => dr.Orders)
                    .ThenInclude(dr => dr.orderItems)
                .Select(dr => new
                {
                    id =  dr.Id,
                    Name = dr.UserName,
                    //  HireAge = DateTime.Now.Year -dr.HireDate.Year,
                    HireAge = dr.HireDate.Year,
                    Status = dr.Status.ToString(),
                    DeliveredOrdersCount = dr.Orders.Count(o => o.orderItems.Any(oi => (int)oi.Status == (int)OrdStatus.Recieved) ),
                    CreatedByAdmin = dr.admin != null ? dr.admin.UserName : "Unknown"
                })
                .ToListAsync();

            if (deliveryReps == null || !deliveryReps.Any())
                return NotFound(new { message = "No Delivery Rep found with the given username." });

            return Ok(deliveryReps);
        }





        [HttpPut("UpdateDeliveryRepStatus")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDeliveryRepStatus([FromBody] UpdateDeliveryRepStatusDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeliveryRepId) || string.IsNullOrWhiteSpace(dto.NewStatus))
                return BadRequest(new { message = "Both DeliveryRepId and NewStatus are required." });

         
            var deliveryRep = await _context.DeliveryReps.FindAsync(dto.DeliveryRepId);
            if (deliveryRep == null)
                return NotFound(new { message = "Delivery representative not found." });


            if (!Enum.TryParse<AccStatus>(dto.NewStatus, true, out var newStatus))
                return BadRequest(new { message = "Invalid status. Allowed values: Active, Inactive, Banned" });

           
            if (deliveryRep.Status == newStatus)
                return BadRequest(new { message = $"DeliveryRep is already {newStatus}" });

           
            deliveryRep.Status = newStatus;
            _context.DeliveryReps.Update(deliveryRep);
            await _context.SaveChangesAsync();

            // send mail abount new status
            string statusMessage = newStatus switch
            {
                AccStatus.Active => "✅ Your account has been activated by the admin.",
                AccStatus.Banned => "⛔ Your account has been banned. Please contact support.",
                AccStatus.Inactive => "🟡 Your account has been set to inactive temporarily.",
                _ => "Your account status has been updated."
            };

            await _emailService.SendEmailAsync(
                deliveryRep.Email,
                "🚨 Account Status Update",
                $"Dear {deliveryRep.UserName},\n\n{statusMessage}\n\nThank you."
            );

            return Ok(new { message = $"✅ DeliveryRep status updated to {newStatus} and notification sent." });
        }










        [HttpGet("ShowAllProducts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ShowAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.category)
                .Include(p => p.merchant)
                .Include(p => p.images)
                .Include(p => p.ProductDetails)
                .ToListAsync();

            if (products == null || !products.Any())
                return NotFound(new { message = "No products found." });

            var productList = products.Select(p => new AdminProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Feedback = p.Feedback,
                Status = p.Status.ToString(), 
                Quantity = p.Quantity,
                CategoryName = p.category.Name,
                MerchantName = p.merchant?.UserName ?? "Unknown",
                MerchantId = p.merchantId,
                Image = $"//aston.runasp.net//Product_Image//{p.images.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();

            return Ok(productList);
        }


        [HttpGet("ShowProductWithId")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ShowProductWithId( int id)
        {
            var products = await _context.Products
                .Where(e => e.Id == id)
                .Include(p => p.merchant)
                .Include(p => p.images)
                .Include(p => p.ProductDetails)
                .ToListAsync();

            if (products == null || !products.Any())
                return NotFound(new { message = "No product found." });

            var productList = products.Select(p => new AdminProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Feedback = p.Feedback,
                Status = p.Status.ToString(),
                Quantity = p.Quantity,
                MerchantName = p.merchant?.UserName ?? "Unknown",
                MerchantId = p.merchantId,
                Image = $"//aston.runasp.net//Product_Image//{p.images.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();

            return Ok(productList);
        }







        [HttpPut("update-product-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductStatus(int productId, string Newstatus)
        {
            if (!Enum.TryParse<ProStatus>(Newstatus, true, out var newStatus))
                return BadRequest(new { message = "Invalid status value." });

            var product = await _context.Products
                .Include(p => p.merchant)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound(new { message = "Product not found." });

            if (product.Status == newStatus)
                return BadRequest(new { message = $"Product status is already set to {newStatus}." });

            bool isValidTransition =
                (product.Status == ProStatus.Pending && newStatus == ProStatus.Active) ||
                (newStatus == ProStatus.Banned) ||
                (product.Status == ProStatus.Banned && newStatus == ProStatus.Active);


            if (!isValidTransition)
                return BadRequest(new { message = "Invalid status transition. Allowed: From Pending To Active  or  Banned Product." });

            product.Status = newStatus;

            await _context.SaveChangesAsync();

            if (newStatus == ProStatus.Pending) {
                // ✅ send mail to the merchant 
                string message = $"Dear {product.merchant.UserName},\n\n" +
                                 $"Your product \"{product.Title}\" has been banned by the admin due to policy violations.\n\n" +
                                 $"If you believe this is a mistake, please contact support.";

                await _emailService.SendEmailAsync(
                    product.merchant.Email,
                    "🚫 Product Banned Notification",
                    message
                );
            }

            if (newStatus == ProStatus.Active)
            {
                // ✅ send mail to the merchant 
                string message = $"Dear {product.merchant.UserName},\n\n" +
                                 $"Your product \"{product.Title}\" has been Active Now .\n\n" +
                                 $"People can now Order it.";

                await _emailService.SendEmailAsync(
                    product.merchant.Email,
                    "✅ Product Activated Notification",
                    message
                );
            }
            return Ok(new { message = $"Product '{product.Title}' has been successfully {newStatus} and merchant has been notified."});
        }






        private string GenerateVerificationCode()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] number = new byte[4];
            rng.GetBytes(number);
            int code = Math.Abs(BitConverter.ToInt32(number, 0)) % 1000000;
            return code.ToString("D6");
        }
    }
}
