using DataBase.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using Project.DTOs;
using Project.Enums;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {        
            public ProductController(AppDbContext userManager)
            {
                _userManager = userManager;
            }
            private readonly AppDbContext _userManager;


        // GET: api/Show Random Product 
        [HttpGet("ShowRandomProduct")]
        //[Authorize (Roles = "Customer")]
        public async Task<IActionResult> ShowRandomProduct(int count)
        {
            var activeProducts = await _userManager.Products
                .Where(p => p.Status == ProStatus.Active || p.Status == ProStatus.OutOfStock)
                .Include(p => p.images)
                .Include(p => p.category)
                .ToListAsync();

            if (activeProducts.Count < 1)
                return NotFound(new { message = "No active products found" });

            var random = new Random();

            var selectedProducts = activeProducts
                .OrderBy(x => random.Next())
                .Take(count)
                .ToList();

            var result = selectedProducts.Select(p => new dtoProductHome
            {
                Id = p.Id,
                Title = p.Title,
                SellPrice = p.SellPrice,
                Status = p.Status.ToString(),
                Category = p.category?.Name ?? "No Category",
                Feedback = p.Feedback,
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                ImageUrl = $"//aston.runasp.net//Product_Image//{p.images?.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();

            return Ok(result);

        }


        // GET: api/Show High Rate Product
        [HttpGet("ShowHighFeedbackProduct")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowHighFeedbackProduct(int count)
        {
            var activeProducts = await _userManager.Products
                .Where(p => p.Status == ProStatus.Active || p.Status == ProStatus.OutOfStock)
                .Include(p => p.images)
                .Include(p => p.category)
                .Include(p => p.feedbacks)
                .ToListAsync();

            if (activeProducts.Count < 1)
                return NotFound(new { message = "No active products found" });

            // Sort products by the highest average feedback score
            var productsWithHighestFeedback = activeProducts
                .OrderByDescending(p => p.Feedback)  // Sort by feedback in descending order
                .Take(count)                             // Take the top 5 products
                .ToList();                           // Convert to a list


            var result = productsWithHighestFeedback.Select(p => new dtoProductHome
            {
                Id = p.Id,
                Title = p.Title,
                SellPrice = p.SellPrice,
                Status = p.Status.ToString(),
                Category = p.category?.Name ?? "No Category",
                Feedback = p.Feedback,
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                ImageUrl = $"//aston.runasp.net//Product_Image//{p.images?.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();

            return Ok(result);
        }

        [HttpGet("ShowAllProduct")]//Title,SalePrice, Category ,Image
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowAllProduct ()
        {
            var products = await _userManager.Products.
                Include(p => p.images)
                .Include(p => p.merchant)
                .Include(p => p.category)
                .Include(p => p.category)
                .ToListAsync();
            if(products == null || !products.Any())
                return NotFound(new { message = "No products found" });
            var productList = products.Select(p => new AllProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Status = p.Status.ToString(),
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                SellPrice = p.SellPrice,
                CategoryName = p.category?.Name ?? "Unknown",
                MerchantName = p.merchant?.UserName ?? "Unknown",
                MerchantId = p.merchantId,
                Image = $"//aston.runasp.net//Product_Image//{p.images.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();
            return Ok(productList);

        }


        [HttpGet("ShowAllProducts")]//Title,SalePrice, Category ,Image
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowAllProducts()
        {
            var products = await _userManager.Products.
                Where(p => (p.Status == ProStatus.Active|| p.Status == ProStatus.OutOfStock)).
                Include(p => p.images)
                .Include(p => p.merchant)
                .Include(p => p.category)
                .ToListAsync();
            if (products == null || !products.Any())
                return NotFound(new { message = "No products found" });
            var productList = products.Select(p => new AllProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Status = p.Status.ToString(),
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                SellPrice = p.SellPrice,
                CategoryName = p.category?.Name ?? "Unknown",
                MerchantName = p.merchant?.UserName ?? "Unknown",
                MerchantId = p.merchantId,
                Image = $"//aston.runasp.net//Product_Image//{p.images.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();
            return Ok(productList);

        }








        //----------------------------------------------------------------------------------


        // GET: api/Show Specific Product
        [HttpGet("ShowSpecificProduct")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowSpecificProduct(int id , String? customerId )
        {
            // Check if the product exists
            if (id <= 0)
                return BadRequest(new { message = "Invalid product ID" });

            // Fetch the product with its related data
            var product = await _userManager.Products
                 .Where(p => p.Id == id)
                 .Include(p => p.category)
                 .Include(p => p.merchant)
                 .Include(p => p.feedbacks)
                    .ThenInclude(p => p.customer)
                 .Include(p => p.feedbackcmments)                     
                 .Include(p => p.images)
                     .ThenInclude(img => img.color)
                 .Include(p => p.ProductDetails)
                     .ThenInclude(pd => pd.color)
                 .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Product not found" });

            // Get the first color (if any) based on productId
            var firstColor = product.ProductDetails?
                                .Where(pd => pd.color != null && pd.productId == id)
                                .Select(pd => pd.color)
                                .FirstOrDefault();

            // Get the random 3 images related to the first color and productId, if they exist
            var imagesForColor = product.images?
                     .Where(img => img.color != null
                                && img.colorId == firstColor?.Id
                                && img.productId == id)
                     .OrderBy(_ => Guid.NewGuid()) // Shuffle randomly
                     .Select(img => img.ImageData)
                     .Take(3)
                     .ToList() ?? new List<string>();

            try { 
            // Prepare the response DTO
            var pro = new dtoSpecificProduct
            {
                Id = product.Id,
                Title = product.Title,
                Status = product.Status.ToString(),
                Description = product.Description,
                personStar = product.feedbacks
                    .Where(f => f.customerId == customerId)
                    .Select(f => f.Star)
                    .FirstOrDefault(),
                averageStar = product.feedbacks != null && product.feedbacks.Any()
                    ? product.feedbacks.Average(f => f.Star)
                    : 0,
                UnitPrice = product.UnitPrice,
                Discount = product.Discount,                
                SellPrice = product.SellPrice,                
                Color = product.ProductDetails?
                            .Where(pd => pd.colorId > 0
                            && pd.Quantity > 0)
                            .Select(pd => pd.color.Name)
                            .Distinct()
                            .ToArray() ?? new string[] { },
                ColorId = product.ProductDetails?
                            .Where(pd => pd.color != null
                            && pd.Quantity > 0)
                            .Select(pd => pd.color.Id)
                            .Distinct()
                            .ToArray() ?? new int[] { },
                Type = product.category.Type ?? "Unknown"
                ,
                Category = product.category?.Name ?? "Unknown",
                CategoriesId = product.categoryId,

                // Return the list of image URLs for the first color
                ImageUrls = imagesForColor.Select(img => $"//aston.runasp.net//Product_Image//{img ?? "unknownProduct.jpg"}").ToList(),
                MerchantName = product.merchant?.UserName ?? "Unknown",
                MerchantId = product.merchantId,
                MerchantFeedbak = product.merchant.Feedback,

                UserName = product.feedbackcmments
                                    .Select(s => s.customer.UserName)
                                    .ToArray() ?? new string[] { },

                CommentRate = product.feedbackcmments?
                                    .Select(fc => fc.CommentRate)
                                    .ToArray() ?? new double[] { },

                DateCreate = product.feedbackcmments?.OrderByDescending(fc => fc.DateCreate)
                                .Select(dt => dt.DateCreate).ToArray() ?? new DateTime[] { },

                OriginalComment = product.feedbackcmments?
                                    .Select(fc => fc.OriginalComment)
                                    .ToArray() ?? new string[] { },
                TranslateComment = product.feedbackcmments?
                                    .Select(fc => fc.TranslateComment)
                                    .ToArray() ?? new string[] { },
                AllStars=product.feedbacks?.
                    Select(s => s.Star)
                    .ToArray() ?? new int[] { }
            };                    
            return Ok(pro);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while processing the request", error = ex.Message });
            }
        }



        // GET: api/Show Size Image for Color
        [HttpGet("showsizeImageforColor")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> showsizeImageforColor(int productId, int colorId)
        {
            // Fetch the product with its related data
            var product = await _userManager.Products
                .Where(p => p.Id == productId)
                .Include(p => p.ProductDetails)
                    .ThenInclude(pd => pd.size)  // Include sizes for the product
                .Include(p => p.images)  // Include images for the product
                    .ThenInclude(img => img.color)  // Include color data for images
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (product.Quantity == 0 || product.Status == ProStatus.OutOfStock)
                return NotFound(new { message = "Product OutOfStock" });
            // Fetch the images related to the specific product and color
            var imagesForColor = product.images?
                .Where(img => img.color != null && img.color.Id == colorId)  // Filter images by colorId
                .Select(img => img.ImageData)  // Get the ImageData for the filtered images
                .ToList() ?? new List<string>(); // Default to an empty list if no images found

            // Fetch the sizes related to the specific product and color
            var sizesForColor = product.ProductDetails?
                .Where(pd => pd.color != null && pd.color.Id == colorId && pd.Quantity > 0)  // Filter sizes by colorId
                .Select(pd => pd.size)                                      // Get the size for the filtered product details
                .ToList();                             // Default to an empty list if no sizes found

            // Prepare the response model
            var result = new
            {
                ProductId = product.Id,
                Images = imagesForColor.Select(img => $"//aston.runasp.net//Product_Image//{img ?? "unknownProduct.jpg"}").ToList(),
                sizeId = sizesForColor.Select(s => s.Id).ToList(),
                Sizes = sizesForColor.Select(s => s.Gradient).ToList()  // Select both Id and Gradient for sizes
            };

            return Ok(result);
        }








        [HttpGet("ShowProductByCategory")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowProductByCategory(int categoryId, int count)
        {
            // Fetch the products by category
            var activeProducts = await _userManager.Products
                .Where(p => p.categoryId == categoryId && (p.Status == ProStatus.Active || p.Status == ProStatus.OutOfStock))
                .Include(p => p.images)
                .Include(p => p.category)
                .ToListAsync();
            if (activeProducts.Count < 1)
                return NotFound(new { message = "No active products found" });

            var random = new Random();

            var selectedProducts = activeProducts
                .OrderBy(x => random.Next())
                .Take(count)
                .ToList();

            var result = selectedProducts.Select(p => new dtoProductHome
            {
                Id = p.Id,
                Title = p.Title,
                Status = p.Status.ToString(),
                SellPrice = p.SellPrice,
                Category = p.category?.Name ?? "No Category",
                Feedback = p.Feedback,
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                ImageUrl = $"//aston.runasp.net//Product_Image//{p.images?.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();
            
            return Ok(result);
        }



        // GET: api/Show Product By Merchantid
        [HttpGet("ShowProductByMerchantId")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> ShowProductByMerchantId(string merchantId)
        {
            // Fetch the products by merchant ID
            var activeProducts = await _userManager.Products
                .Where(p => p.merchantId == merchantId &&( p.Status == ProStatus.Active || p.Status == ProStatus.OutOfStock))
                .Include(p => p.images)
                .Include(p => p.category)
                .ToListAsync();
            if (activeProducts == null)
                return NotFound(new { message = "No products found" });
            if (activeProducts.Count < 1)
                return NotFound(new { message = "No active products found" });


            var random = new Random();

            var selectedProducts = activeProducts          
                .ToList();

            var result = selectedProducts.Select(p => new dtoProductHome
            {
                Id = p.Id,
                Title = p.Title,
                SellPrice = p.SellPrice,
                Status = p.Status.ToString(),
                Category = p.category?.Name ?? "No Category",
                Feedback = p.Feedback,
                Discount = p.Discount,
                UnitPrice = p.UnitPrice,
                ImageUrl = $"//aston.runasp.net//Product_Image//{p.images?.FirstOrDefault()?.ImageData ?? "unknownProduct.jpg"}"
            }).ToList();

            return Ok(result);
        }







        // GET: api/Show All Favourites
        [HttpGet("ShowAllFavProduct")]
        [Authorize(Roles = "Customer")]
        public IActionResult ShowAllFavProduct(string customerId)
        {
            var products = _userManager.FavProducts
                .Where(p => p.customerId == customerId)
                .Include(p => p.product)
                    .ThenInclude(p => p.images);

            if (products.Count() < 1)
                return NotFound(new { message = "No FavProduct found" });


            var FavItem = products.Select(p => new FavProductDTO
            {
                Id = p.Id,
                ProductId = p.productId,
                Title = p.product.Title,
                Status = p.product.Status.ToString(),
                Discount = p.product.Discount,
                UnitPrice = p.product.UnitPrice,
                SellPrice = p.product.SellPrice,
                Image = $"//aston.runasp.net//Product_Image//{p.product.images.FirstOrDefault().ImageData ?? "unknownProduct.jpg"}"

            }).ToList();
            return Ok(FavItem);
        }

        // GET: api/Show All Favourites
        [HttpGet("ShowAllFavMerchant")]
        [Authorize(Roles = "Customer")]
        public  IActionResult ShowAllFavMerchant(string customerId)
        {
            var products = _userManager.FavMerchants
                .Where(p => p.customerId == customerId)
                .Include(p => p.merchant);

            if (products.Count() < 1)
                return NotFound(new { message = "No Fav Merchant found" });


            var FavMer = products.Select(p => new FavMerchantDTO
            {
                Id = p.Id,
                MerchantId = p.merchantId,
                MerchantName = p.merchant.UserName,
                Image = $"//aston.runasp.net//Product_Image//{p.merchant.IMG.FirstOrDefault().ToString() ?? "unknownProduct.jpg"}",
                Rate = p.merchant.Feedback

            }).ToList();
            return Ok(FavMer);
        }




        // GET: api/Show All Carts
        [HttpGet("ShowAllCarts")]
        [Authorize(Roles = "Customer")]
        public  IActionResult GetAllCarts(string customerId)
        {
            var products = _userManager.Carts.Where(f => f.customerId == customerId)
                .Include(f => f.color)
                .Include(f => f.product).
                    ThenInclude(f => f.images)
                .Include(f => f.size);


            if (products.Count() < 1)
                return NotFound(new { message = "No products found in Cart" });


            var CartItems = new CartDTO
            {
                Id = products.Select(p => p.Id).ToArray(),
                ProductIds = products.Select(p => p.productId).ToArray(),
                ProductsNames = products.Select(p => p.product.Title).ToArray(),
                ProductDescribtions = products.Select(p => p.product.Description).ToArray(),
                ProductPrice = products.Select(p => p.product.SellPrice).ToArray(),
                quantity = products.Select(p => p.Quantity).ToArray(),
                color = products.Select(p => p.color.Name).ToArray(),
                size = products.Select(p => p.size.Gradient).ToArray(),
                image = products.Select(p => p.product.images.FirstOrDefault().ImageData).ToArray(),
                TotalPrice = products.Sum(p => p.product.SellPrice * p.Quantity)

            };

            for (var item = 0; item< CartItems.image.Length; item++)
            {
                CartItems.image[item] = $"//aston.runasp.net//Product_Image//{CartItems.image[item] ?? "unknownProduct.jpg"}";

            }

            return Ok(CartItems);
        }








        [HttpPost("AddToFavProduct")]
            [Authorize(Roles = "Customer")]
            public IActionResult AddProductToFav(string cusId , int ProdID)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Check if the product exists
                var product = _userManager.Products.FirstOrDefault(f => f.Id == ProdID);
                var customer = _userManager.Customers.FirstOrDefault(f => f.Id == cusId);
            var existingfav = _userManager.FavProducts.FirstOrDefault(f => f.productId == ProdID && f.customerId == cusId);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }
            if (existingfav != null)
            {
                return NotFound(new { message = "Item exist before inside favourite" });
            }
            var favProduct = new FavProduct
                {
                    productId = ProdID,
                    customerId = cusId
                };
                _userManager.FavProducts.Add(favProduct);
                _userManager.SaveChanges();
                return Ok(new { message = "Product stored in favourite" });
            }

        [HttpPost("AddToFavMerchant")]
        [Authorize(Roles = "Customer")]
        public IActionResult AddToMerchant(string cusId, string MerID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the product exists
            var product = _userManager.Merchants.FirstOrDefault(f => f.Id == MerID);
            var customer = _userManager.Customers.FirstOrDefault(f => f.Id == cusId);
            var existingfav = _userManager.FavMerchants.FirstOrDefault(f => f.customerId == cusId && f.merchantId == MerID);

            if (product == null)
            {
                return NotFound(new { message = "Merchant not found" });
            }
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }
            if (existingfav != null)
            {
                return NotFound(new { message = "Item exist before inside favourite" });
            }
            var favMerchant = new FavMerchant
            {
                merchantId = MerID,
                customerId = cusId
            };
            _userManager.FavMerchants.Add(favMerchant);
            _userManager.SaveChanges();
            return Ok(new { message = "Merchant stored in favourite" });
        }



        [HttpDelete("DeleteProductFromFav")]
        [Authorize(Roles = "Customer")]
        public IActionResult DeleteProductFromFav(int favId)
            {
                var favProduct = _userManager.FavProducts.FirstOrDefault(f => f.Id == favId);
                if (favProduct == null)
                {
                    return NotFound(new { message = "Product not found in favourite" });
                }
                _userManager.FavProducts.Remove(favProduct);
                _userManager.SaveChanges();
                return Ok(new { message = "Product removed from favourite" });
            }


        [HttpDelete("DeleteFromFavMerchant")]
        [Authorize(Roles = "Customer")]
        public IActionResult DeleteFromFavMerchant(int favMerId)
        {
            var FavMerchant = _userManager.FavMerchants.FirstOrDefault(f => f.Id == favMerId);
            if (FavMerchant == null)
            {
                return NotFound(new { message = "Merchant not found in favourite" });
            }
            _userManager.FavMerchants.Remove(FavMerchant);
            _userManager.SaveChanges();
            return Ok(new { message = "Merchant removed from favourite" });
        }



        [HttpPost("AddToCart")]
        [Authorize(Roles = "Customer")]
        public IActionResult AddProductToCart(AddCartDTO cartDTO)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Check if the product exists
                var product = _userManager.Products.FirstOrDefault(f => f.Id == cartDTO.ProductId);
                var customer = _userManager.Customers.FirstOrDefault(f => f.Id == cartDTO.CustomerId);
                var color = _userManager.Colors.FirstOrDefault(f => f.Id == cartDTO.colorId);
                var size = _userManager.Sizes.FirstOrDefault(f => f.Id == cartDTO.sizeId);
                var existingCart = _userManager.Carts.FirstOrDefault(f => (f.productId == cartDTO.ProductId && f.customerId == cartDTO.CustomerId
                    && f.colorId == cartDTO.colorId && f.sizeId == cartDTO.sizeId)); 
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
                if (customer == null)
                {
                    return NotFound(new { message = "Customer not found" });
                }
                if (color == null)
                {
                    return NotFound(new { message = "Color not found" });
                }
                if (size == null)
                {
                    return NotFound(new { message = "Size not found" });
                }
                if (existingCart != null)
                {
                return NotFound(new { message = "Item exist before inside cart" });
            }
                var cart = new Cart
                {
                    productId = cartDTO.ProductId,
                    customerId = cartDTO.CustomerId,
                    colorId = cartDTO.colorId,
                    sizeId = cartDTO.sizeId,
                    Quantity = 1
                };
                _userManager.Carts.Add(cart);
                _userManager.SaveChanges();
                return Ok(new { message = "Product stored in cart" });
            }

            [HttpDelete("DeleteFromCart")]
            public IActionResult DeleteProductFromCart(int cartId)
            {
                var cart = _userManager.Carts.FirstOrDefault(f => f.Id == cartId);
                if (cart == null)
                {
                    return NotFound(new { message = "Product not found in cart" });
                }
                _userManager.Carts.Remove(cart);
                _userManager.SaveChanges();
                return Ok(new { message = "Product removed from cart" });
            }

            [HttpPut("UpdateQuantity")]
            public IActionResult UpdateProductQuantity(int cartId, int quantity)
            {
                var cart = _userManager.Carts.FirstOrDefault(f => f.Id == cartId);
                if (cart == null)
                {
                    return NotFound(new { message = "Product not found in cart" });
                }
                cart.Quantity = quantity;
                _userManager.Carts.Update(cart);
                _userManager.SaveChanges();
                return Ok(new { message = "Product quantity updated" });
            }



        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(CommentDTO com)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if the product exists
            var product = _userManager.Products.FirstOrDefault(f => f.Id == com.productId);
            var customer = _userManager.Customers.FirstOrDefault(f => f.Id == com.customerId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }
            // Check if the customer has any order with the product and status = Recieved (e.g., status = 3)
            var hasReceivedProduct = await _userManager.Orders
                .Where(o => o.CustomerId == com.customerId)
                .SelectMany(o => o.orderItems)
                .AnyAsync(oi => oi.productId == com.productId && oi.Status == OrdStatus.Recieved);

            
            if (!hasReceivedProduct)
            {
                return BadRequest(new { message = "Customer has not Ordered this product yet" });
            }

            var existcomment = _userManager.Feedbacks.
                FirstOrDefault(f => f.productId == com.productId && f.customerId == com.customerId );

            if (existcomment == null)
            {
                var comment = new Feedback
                {
                    productId = com.productId,
                    customerId = com.customerId,
                    Star = com.Star,
                };
                _userManager.Feedbacks.Add(comment);
                _userManager.SaveChanges();
            }
            else
            {
                existcomment.Star = com.Star;
                _userManager.Feedbacks.Update(existcomment);
                _userManager.SaveChanges();


            }
            var existfeedbackcomment = _userManager.FeedbackComments.
                Where(f => f.productId == com.productId && f.customerId == com.customerId && f.OriginalComment == com.OriginalComment && f.TranslateComment == com.TranslateComment && f.CommentRate == com.CommentRate);


            if (!existfeedbackcomment.Any())
            {
                var feedbackComment = new FeedbackComments
            {
                productId = com.productId,
                customerId = com.customerId,
                OriginalComment = com.OriginalComment,
                TranslateComment = com.TranslateComment,
                CommentRate = com.CommentRate,
                DateCreate = DateTime.Now,               
            };
            _userManager.FeedbackComments.Add(feedbackComment);
            _userManager.SaveChanges();
            return Ok(new { message = "Comment added successfully" });
            }
            return BadRequest(new { message = "Comment already exists" });
        }



        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment(string customerId, int productId, string origincomment)
        {
            var feedbackexist = await _userManager.FeedbackComments
                .FirstOrDefaultAsync(f => f.customerId == customerId && f.productId == productId && f.OriginalComment == origincomment);

            if (feedbackexist == null)
            {
                return NotFound(new { message = "Comment not found" });
            }
            _userManager.FeedbackComments.Remove(feedbackexist);
            _userManager.SaveChanges();

            feedbackexist = await _userManager.FeedbackComments
                .FirstOrDefaultAsync(f => f.customerId == customerId && f.productId == productId);
            if (feedbackexist == null)
            {
                // If no more comments exist for this product, remove the feedback entry
                var feedback = await _userManager.Feedbacks
                    .FirstOrDefaultAsync(f => f.customerId == customerId && f.productId == productId);
                if (feedback != null)
                {
                    _userManager.Feedbacks.Remove(feedback);
                    _userManager.SaveChanges();

                }


                return Ok(new { message = "Comment deleted successfully" });
            }
            return Ok(new { message = "Comment deleted successfully" });
        }











            [HttpGet("CheckBuyingProduct")]
        public async Task<IActionResult> CheckBuyingProduct(string customerId, int productId)
        {
            // Check if the product exists
            var product = await _userManager.Products.FirstOrDefaultAsync(f => f.Id == productId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Check if the customer exists
            var customer = await _userManager.Customers.FirstOrDefaultAsync(f => f.Id == customerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            // Check if the customer has any order with the product and status = Recieved (e.g., status = 3)
            var hasReceivedProduct = await _userManager.Orders
                .Where(o => o.CustomerId == customerId)
                .SelectMany(o => o.orderItems)
                .AnyAsync(oi => oi.productId == productId && oi.Status == OrdStatus.Recieved);

            if (!hasReceivedProduct)
            {
                return Ok(new { BuyIt = false });
            }

            return Ok(new { BuyIt = true });
        }




        [HttpGet("TrainModel")]
        public async Task<IActionResult> TrainModel()
        {
            var activeProducts = await _userManager.Products
                .Where(p => p.Status == ProStatus.Active || p.Status == ProStatus.OutOfStock)
                .Include(p => p.images)
                .Include(p => p.category)
                .ToListAsync();

            if (activeProducts.Count < 1)
                return NotFound(new { message = "No active products found" });

            var selectedProducts = activeProducts
                .Select(p => p)
                .ToList();

            var result = selectedProducts.Select(p => new ProductModelDTO
            {
                Id = p.Id,
                Name = p.Title,
                Category = p.category?.Name ?? "No Category",
                Description = p.Description,
            }).ToList();

            return Ok(result);

        }




    }
    }

