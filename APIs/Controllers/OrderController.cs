using APIs.Services.Implementation;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Ecom.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITransactionService _transactionService;
        private readonly IBookService _bookService;
        private readonly IAccountService _accService;

        public OrderController(IOrderService orderService, ITransactionService transactionService, IBookService bookService, IAccountService accountService)
        {
            _transactionService = transactionService;
            _orderService = orderService;
            _bookService = bookService;
            _accService = accountService;

        }

        [HttpPost]
        [Route("create-order")]
        public IActionResult CreateOrder([FromBody] CheckoutDTO request)
        {

            decimal totalAmount = _orderService.GetTotalAmount(request.Products);

            Guid orderId = Guid.NewGuid();
            NewOrderDTO newOrder = new NewOrderDTO();
           


            switch (request.PaymentMethod)
            {
                case "VnPay":
                    {
                        newOrder.OrderId = orderId;
                        newOrder.CustomerId = request.CustomerId;
                        newOrder.Price = request.PaymentReturnDTO.Amount;
                        newOrder.Status = request.PaymentReturnDTO?.PaymentStatus;
                        newOrder.Notes = request.PaymentReturnDTO.PaymentMessage;
                        newOrder.TransactionId = Guid.Parse(request.PaymentReturnDTO.PaymentRefId);
                        newOrder.PaymentMethod = request.PaymentMethod;
                        newOrder.AddressId = request.AddressId;
                    }
                    break;
                case "COD":
                    {
                        newOrder.OrderId = orderId;
                        newOrder.CustomerId = request.CustomerId;
                        newOrder.Price = totalAmount;
                        newOrder.Status = "It's a COD, what do u think ?";
                        newOrder.Notes = "Told u, it's a COD ?";
                        newOrder.PaymentMethod = request.PaymentMethod;
                        newOrder.AddressId = request.AddressId;
                        newOrder.TransactionId = null;
                    }
                    break;
            }

            string result = _orderService.CreateNewOrder(newOrder);

            if (result == "Successfully!")
            {
                string result2 = _orderService.TakeProductFromCartOptional(request.CustomerId, orderId, request.Products);

                return Ok(result2);
            }
            return BadRequest(result);
        }


        [HttpPost]
        [Route("check-out")]
        public async Task<IActionResult> CheckoutAsync([FromBody] PreCheckoutDTO dto)
        {

            foreach (ProductOptionDTO p in dto.Products)
            {
                if (_orderService.GetCurrentStock(p.ProductId) < p.Quantity)
                {
                    return BadRequest("Can't purchase more product than inside stock!");
                }
            }

            decimal totalAmount = _orderService.GetTotalAmount(dto.Products);
            int truncatedAmount = (int)Math.Round(totalAmount, MidpointRounding.AwayFromZero);



            NewTransactionDTO newTransDTO = new NewTransactionDTO()
            {
                PaymentContent = "Ecommerce cart checkout" + Guid.NewGuid(),
                PaymentCurrency = "vnd",
                RequiredAmount = truncatedAmount,
                ReferenceId = dto.ReferenceId.ToString()
            };

            using (HttpClient client = new HttpClient())
            {
                // Convert the request data to JSON
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(newTransDTO);

                // Prepare the HTTP request content
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Make the POST request and include the request content in the body
                HttpResponseMessage response = await client.PostAsync("https://localhost:7138/api/Payment/vnpay/create-vnpay-link", content);
                ;
                // Check the response status code
                if (response.IsSuccessStatusCode)
                {
                    // Request successful
                    string? responseJson = await response.Content.ReadAsStringAsync();

                    PaymentLinkDTO? link = JsonConvert.DeserializeObject<PaymentLinkDTO>(responseJson);

                    return Ok(link?.PaymentUrl);
                }
                else
                {
                    // Request failed
                    return BadRequest("API request failed with status code: " + response.StatusCode);
                }
            }


            //Guid orderId = Guid.NewGuid();
            //NewOrderDTO dto = new NewOrderDTO()
            //{
            //    OrderId = orderId,
            //    CustomerId = request.CustomerId,
            //    Status = data?.PaymentStatus,
            //    Notes = data?.PaymentMessage,
            //    PaymentId = data != null ? Guid.Parse(data.PaymentId) : Guid.Empty,
            //    AddressId = request.AddressId,
            //};
            //string result = _orderService.CreateNewOrder(dto);

            //if (result == "Successfully!")
            //{
            //    string result2 = _orderService.TakeProductFromCartOptional(request.CustomerId, orderId, request.Products);

            //    if (result2 == "Successfully!")
            //    {
            //        return Ok("Successfully!");
            //    }
            //    return BadRequest(result2);
            //}
            //return BadRequest(result);
        }

        [HttpGet("get-transaction-by-id")]
        public IActionResult GetTransactionById(Guid refId)
        {
            try
            {
                TransactionRecord? record = _transactionService.GetTransactionById(refId);
                if (record != null) return Ok(record);
                else return BadRequest("Not found!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //[HttpGet("get-user-order-hisotry")]

        //public IActionResult GetUserHistoryOrder(Guid userId, [FromQuery] PagingParams @params)
        //{
        //    try
        //    {
        //        var orderHistory = _orderService.GetOrderHistory(userId, @params);


        //        if (orderHistory != null)
        //        {
        //            var metadata = new
        //            {
        //                orderHistory.TotalCount,
        //                orderHistory.PageSize,
        //                orderHistory.CurrentPage,
        //                orderHistory.TotalPages,
        //                orderHistory.HasNext,
        //                orderHistory.HasPrevious
        //            };
        //            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
        //            return Ok(orderHistory);
        //        }
        //        else return BadRequest("No chapter!!!");
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}

  

        [HttpGet("total-spend")] //SONDB 
        public IActionResult GetTotalSpendInMonth([FromQuery] UserSpendInMonth userSpend)
        {
            decimal totalSpend = _orderService.GetTotalSpendByCustomerIdInMonth(userSpend);

            string message = $"This month, you have spent a total of {totalSpend}.";

            return Ok(message);
        }
        [HttpGet("compare-spend-this-month-to-last-month")] //SONDB 
        public IActionResult CompareSpend(Guid customerId)
        {
            string comparisonMessage = _orderService.CompareTotalSpend(customerId);
            return Ok(comparisonMessage);
        }
        [HttpGet("order-history")] //SONDB 
        public  ActionResult<List<OrderedBookDTO>> GetUserOrderHistoryAsync(Guid userId)
        {
            try
            {
                var orderHistory =  _orderService.GetOrderHistory(userId);
                return Ok(orderHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //----------------------DATDQ----------------------------------------//
        [HttpGet, Authorize]
        [Route("GetAllOrder")]
        public IActionResult GetAllOrder([FromQuery] PagingParams @params)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }
                var chapters = _orderService.GetAllOrder(@params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet, Authorize]
        [Route("GetAllOrderByProductId")]
        public IActionResult GetAllOrderByProductId(Guid id, [FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id or Inventory Is not found for the given owner");
                }

                // Check if the book belongs to the same agency
                List<Book> b = _bookService.GetAllBookInInvent(agencyId);
                int check = 0;
                foreach (var x in b)
                {
                    if (id == x.ProductId)
                    {
                        check++;
                    }
                }
                if (check == 0)
                {
                    return BadRequest("You do not have permission to view list order by this book.");
                }
                var chapters = _orderService.GetOrderByProductId(id, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }



        [HttpGet, Authorize]
        [Route("GetAllOrderOfAgency")]
        public IActionResult GetAllOrderOfAgency([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _orderService.GetOrderByUserId(userId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }



        [HttpGet, Authorize]  //New
        [Route("SearchOrders")]
        public IActionResult SearchOrders(string? address, string? bookName, string? customerName, DateTime? startDate, DateTime? endDate, string status, [FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _orderService.SearchOrders(userId, address, bookName, customerName, startDate, endDate, status, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet, Authorize]
        [Route("GetOrderDetail")]
        public IActionResult GetOrderDetail(Guid id)
        {
            try
            {

                OrderDetailsDTO profile = _orderService.GetOrderDetailsById(id);
                return Ok(profile);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // ----------------------END DATDQ-----------------------------------//

    }
}