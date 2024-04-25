using System;
using System.Runtime.Serialization;
using Azure.Core;
using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Ecom;
using BusinessObjects.Models.Ecom.Payment;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
    public class OrderDAO
    {
        //Cart to Order
        public string TakeProductFromCart(Guid userId, Guid orderId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Cart? cart = context.Carts.Where(c => c.CustomerId == userId).FirstOrDefault();

                    if (cart != null)
                    {
                        var basketItems = context.Baskets.Where(b => b.CartId == cart.CartId).ToList();

                        foreach (var item in basketItems)
                        {
                            item.CartId = null;
                            item.OrderId = orderId;
                            Book? book = context.Books.Where(b => b.ProductId == item.ProductId).FirstOrDefault();

                            if (book != null)
                            {
                                item.Stored_Price = book.Price;
                            }
                            else return "Error! Product doesn't exist, productId: " + item.ProductId;
                            context.Database.ExecuteSqlRaw("UPDATE Baskets SET CartId = NULL, OrderId = {0}, Stored_Price = {1} WHERE ProductId = {2} AND CartId is not null AND OrderId is null", orderId, item.Stored_Price, item.ProductId);
                        }
                        int result = context.Baskets.Where(o => o.OrderId == orderId).Count();
                        if (result == 1)
                        {
                            return "Successfully!";
                        }
                        else return "Add Fail!";
                    }
                    else return "Cart doesn't existed!!!";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string TakeProductFromCartOptional(Guid userId, Guid orderId, List<ProductOptionDTO> productIds)
        {
            string hihi = "";
            try
            {
                using (var context = new AppDbContext())
                {
                    Cart? cart = context.Carts.Where(c => c.CustomerId == userId).FirstOrDefault();

                    if (cart != null)
                    {
                        //List<Basket> basketItems = new List<Basket>();
                        foreach (ProductOptionDTO dto in productIds)
                        {
                            Basket? temp = context.Baskets.Where(b => b.ProductId == dto.ProductId && b.CartId == cart.CartId).FirstOrDefault();
                            if (temp != null)
                            {
                                temp.CartId = null;
                                temp.OrderId = orderId;
                                Book? book = context.Books.Where(b => b.ProductId == temp.ProductId).FirstOrDefault();

                                if (book != null)
                                {
                                    temp.Stored_Price = book.Price;
                                }
                                else { return "Error! Product doesn't exist, productId: " + temp.ProductId; }

                                int clear = temp.Quantity - dto.Quantity;

                                hihi = "IN CART:" + temp.Quantity.ToString() + " | CHECKOUT: " + dto.Quantity.ToString();

                                if (clear <= 0)
                                {
                                    context.Database.ExecuteSqlRaw("UPDATE Baskets SET CartId = NULL, OrderId = {0}, Stored_Price = {1} WHERE ProductId = {2} AND CartId is not null AND OrderId is null", orderId, temp.Stored_Price, temp.ProductId);
                                }
                                else
                                {
                                    context.Database.ExecuteSqlRaw("UPDATE Baskets SET Quantity = Quantity - {0} WHERE ProductId = {1} AND CartId is not null AND OrderId is null", dto.Quantity, dto.ProductId);
                                    context.Database.ExecuteSqlRaw("insert into Baskets(ProductId, OrderId, Quantity, AddedDate, Stored_Price) values ({0}, {1}, {2}, {3}, {4})", dto.ProductId, orderId, dto.Quantity, DateTime.Now, temp.Stored_Price);
                                }
                            }
                        }
                        //int result = context.Baskets.Where(o => o.OrderId == orderId).Count();
                        //if (result == 1)
                        //{
                        //return "Successfully!";
                        return hihi;
                        //}
                        //else return "Add Fail!";
                    }
                    return "Cart doesn't existed!!!";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string CreateNewOrder(NewOrderDTO data)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    decimal? total_Price = 0;

                    Order newOrder = new Order
                    {
                        OrderId = data.OrderId,
                        CustomerId = data.CustomerId,
                        Total_Price = data.Price,
                        Status = data.Status,
                        Quantity = 0,
                        Notes = data.Notes,
                        CreatedDate = DateTime.Now,
                        PaymentMethod = data.PaymentMethod,
                        TransactionId = data.TransactionId,
                        AddressId = data.AddressId
                    };
                    context.Orders.Add(newOrder);
                    int result = context.SaveChanges();
                    if (result == 1)
                    {
                        return "Successfully!";
                    }
                    else return "Fail to create new order!!!";
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public decimal GetTotalAmount(List<ProductOptionDTO> dto)
        {
            try
            {
                decimal result = 0;
                using (var context = new AppDbContext())
                {
                    foreach (ProductOptionDTO c in dto)
                    {
                        Book? temp = context.Books.Where(b => b.ProductId == c.ProductId).FirstOrDefault();
                        if (temp != null)
                        {
                            result += temp.Price * c.Quantity;
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Order> GetAllOrder()
        {

            List<Order> orderList = new List<Order>();
            try
            {
                using (var context = new AppDbContext())
                {
                    orderList = context.Orders.ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return orderList;
        }

        public List<Basket> GetBasketByProduct(Guid id)
        {
            List<Basket> result = new List<Basket>();

            try
            {
                using (var context = new AppDbContext())
                {
                    result = context.Baskets.Where(b => b.ProductId == id).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }

        public int GetNumberOfBooksSoldByProductId(Guid id)
        {
            try
            {
                List<Basket> baskets = GetBasketByProduct(id);
                int totalQuantitySold = 0;
                if (baskets.Count > 0)
                {
                    int count = baskets.Sum(b => b.Quantity);
                    totalQuantitySold += count;
                }
                return totalQuantitySold;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting the number of books sold.", e);
            }
        }

        public int GetNumberOfUnitSoldByProductId(Guid Id)
        {
            try
            {
                int numberOfBaskets = 0;
                using (var context = new AppDbContext())
                {
                    // Count the number of baskets containing the specified product ID
                    numberOfBaskets = context.Baskets
                        .Count(b => b.ProductId == Id);
                }
                return numberOfBaskets;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the number of baskets containing the product ID.", e);
            }
        }

        public decimal GetRevenueByProductId(Guid id)
        {
            try
            {
                List<Basket> baskets = GetBasketByProduct(id);
                decimal totalRevenue = 0;
                if (baskets.Count > 0)
                {
                    decimal count = (decimal)baskets.Sum(b => b.Stored_Price);
                    totalRevenue += count;
                }
                return totalRevenue;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting the revenue by productId.", e);
            }
        }

        public decimal GetRevenueByProductIdAndTime(Guid id, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Basket> baskets = GetBasketByProduct(id).Where(b => b.AddedDate >= startDate && b.AddedDate <= endDate).ToList();
                decimal totalRevenue = 0;

                if (baskets.Count > 0)
                {
                    totalRevenue = baskets.Sum(b => b.Stored_Price ?? 0);
                }

                return totalRevenue;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting the revenue by productId and DateTime range.", e);
            }
        }



        public List<TransactionRecord> GetTransactionByUser(Guid id)
        {
            List<TransactionRecord> trans = new List<TransactionRecord>();

            try
            {
                using (var context = new AppDbContext())
                {
                    trans = context.Transactions.Where(t => t.UserId == id).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return trans;
        }


        public List<Order> GetOrderByProductId(Guid id)
        {
            List<Order> result = new List<Order>();

            try
            {
                using (var context = new AppDbContext())
                {
                    List<Basket> baskets = GetBasketByProduct(id);
                    List<Guid?> orderIds = baskets.Select(b => b.OrderId).ToList();

                    result = context.Orders.Where(o => orderIds.Contains(o.OrderId)).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }


        public List<Order> GetOrderByUserId(Guid id)
        {
            List<Order> result = new List<Order>();

            try
            {
                using (var context = new AppDbContext())
                {
                    List<TransactionRecord> trans = GetTransactionByUser(id);
                    List<Guid?> transIds = trans.Select(t => t.TransactionId).ToList();

                    result = context.Orders.Where(o => transIds.Contains(o.TransactionId)).ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }


        public OrderDetailsDTO GetOrderDetailsById(Guid orderId) //Fix book name to list
        {
            Order? order = new Order();
            List<Basket>? basket = new List<Basket>();
            AppUser? appUser = new AppUser();
            Address? address = new Address();
            List<string>? books = new List<string>();
            OrderDetailsDTO result = new OrderDetailsDTO();
            try
            {
                using (var context = new AppDbContext())
                {
                    order = context.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                    appUser = context.AppUsers.Where(a => a.UserId == order.CustomerId).FirstOrDefault();
                    basket = context.Baskets.Where(bk => bk.OrderId == orderId).ToList();
                    foreach (var bk in basket)
                    {

                        Book book = context.Books.Where(b => b.ProductId == bk.ProductId).FirstOrDefault();
                        books.Add(book.Name);
                    }
                    address = context.Addresses.Where(ad => ad.AddressId == order.AddressId).FirstOrDefault();
                }
                if (order != null)
                {
                    result = new OrderDetailsDTO()
                    {
                        OrderId = order.OrderId,
                        CustomerId = order.CustomerId,
                        CustomerName = appUser.Username,
                        BookName = books,
                        Status = order.Status,
                        PaymentMethod = order.PaymentMethod,
                        Price = order.Total_Price,
                        Quantity = order.Quantity,
                        Notes = order.Notes,
                        TransactionId = order.TransactionId,
                        CreatedDate = order.CreatedDate,
                        Address = address.SubDistrict + "," + address.District + "," + address.City_Province
                    };
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int GetTotalNumberOfBookSold(Guid agencyId)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInvent(agencyId);
                int total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        int num = GetNumberOfBooksSoldByProductId(book.ProductId);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of book sold.", e);
            }
        }

        public decimal GetTotalRevenue(Guid agencyId)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInvent(agencyId);
                decimal total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        decimal num = GetRevenueByProductId(book.ProductId);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of book sold.", e);
            }
        }

        public decimal GetTotalRevenueByTime(Guid agencyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInvent(agencyId);
                decimal total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        decimal num = GetRevenueByProductIdAndTime(book.ProductId, startDate, endDate);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total revenue by DateTime.", e);
            }
        }

        public Dictionary<string, decimal> GetRevenueByMonth(Guid agencyId) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var agency = context.Agencies.FirstOrDefault(a => a.AgencyId == agencyId);
                    if (agency == null)
                    {
                        throw new Exception("Agency not found.");
                    }

                    var user = context.AppUsers.FirstOrDefault(app => app.UserId == agency.OwnerId);
                    if (user == null)
                    {
                        throw new Exception("User not found for the agency.");
                    }

                    var transaction = context.Transactions.FirstOrDefault(t => t.UserId == user.UserId);
                    if (transaction == null)
                    {
                        throw new Exception("Transaction not found for the user.");
                    }

                    var order = context.Orders
                                        .OrderBy(o => o.CreatedDate)
                                        .FirstOrDefault(o => o.TransactionId == transaction.TransactionId);
                    if (order == null)
                    {
                        throw new Exception("Order not found for the transaction.");
                    }

                    var basket = context.Baskets
                                        .OrderBy(b => b.AddedDate)
                                        .FirstOrDefault(b => b.OrderId == order.OrderId);
                    if (basket == null)
                    {
                        throw new Exception("Basket not found for the order.");
                    }

                    DateTime agencyAddedDate = basket.AddedDate;
                    if (agencyAddedDate == DateTime.MinValue)
                    {
                        throw new Exception("Agency added date not found.");
                    }

                    // Initialize variables for iteration
                    DateTime currentDate = agencyAddedDate.Date;
                    DateTime endDate = DateTime.Today.Date;

                    Dictionary<string, decimal> revenueByMonth = new Dictionary<string, decimal>();

                    // Iterate through each month starting from the added date until the end of the current month
                    while (currentDate.Year < endDate.Year || (currentDate.Year == endDate.Year && currentDate.Month <= endDate.Month))
                    {
                        // Calculate the start and end dates for the current month
                        DateTime monthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                        string monthYear = monthStartDate.ToString("MMMM yyyy");

                        DateTime monthEndDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

                        // Get revenue for the current month
                        decimal revenue = GetTotalRevenueByTime(agencyId, monthStartDate, monthEndDate);
                        revenueByMonth.Add(monthYear, revenue);

                        // Move to the next month
                        currentDate = currentDate.AddMonths(1);
                    }

                    return revenueByMonth;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by month.", e);
            }
        }


        //New --------------------------------------------------------------------------------
        public Dictionary<string, decimal> GetRevenueByDayInput(Guid agencyId,DateTime currentDate, DateTime endDate) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Dictionary<string, decimal> revenueByDay = new Dictionary<string, decimal>();

                    // Iterate through each day starting from the agency's added date until the current date
                    while (currentDate <= endDate)
                    {
                        // Get revenue for the current day
                        decimal revenue = GetTotalRevenueByTime(agencyId, currentDate, currentDate);
                        string dayKey = currentDate.ToString("dd-MM-yyyy");
                        revenueByDay.Add(dayKey, revenue);

                        // Move to the next day
                        currentDate = currentDate.AddDays(1);
                    }

                    return revenueByDay;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by day.", e);
            }
        }

        public Dictionary<string, decimal> GetRevenueByDay(Guid agencyId) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var agency = context.Agencies.FirstOrDefault(a => a.AgencyId == agencyId);
                    if (agency == null)
                    {
                        throw new Exception("Agency not found.");
                    }

                    var user = context.AppUsers.FirstOrDefault(app => app.UserId == agency.OwnerId);
                    if (user == null)
                    {
                        throw new Exception("User not found for the agency.");
                    }

                    var transaction = context.Transactions.FirstOrDefault(t => t.UserId == user.UserId);
                    if (transaction == null)
                    {
                        throw new Exception("Transaction not found for the user.");
                    }

                    var order = context.Orders
                                        .OrderBy(o => o.CreatedDate)
                                        .FirstOrDefault(o => o.TransactionId == transaction.TransactionId);
                    if (order == null)
                    {
                        throw new Exception("Order not found for the transaction.");
                    }

                    var basket = context.Baskets
                                        .OrderBy(b => b.AddedDate)
                                        .FirstOrDefault(b => b.OrderId == order.OrderId);
                    if (basket == null)
                    {
                        throw new Exception("Basket not found for the order.");
                    }

                    DateTime agencyAddedDate = basket.AddedDate;
                    if (agencyAddedDate == DateTime.MinValue)
                    {
                        throw new Exception("Agency added date not found.");
                    }

                    // Initialize variables for iteration
                    DateTime currentDate = agencyAddedDate.Date;
                    DateTime endDate = DateTime.Today.Date;

                    Dictionary<string, decimal> revenueByDay = new Dictionary<string, decimal>();

                    // Iterate through each day starting from the agency's added date until the current date
                    while (currentDate <= endDate)
                    {
                        // Get revenue for the current day
                        decimal revenue = GetTotalRevenueByTime(agencyId, currentDate, currentDate);
                        string dayKey = currentDate.ToString("dd-MM-yyyy");
                        revenueByDay.Add(dayKey, revenue);

                        // Move to the next day
                        currentDate = currentDate.AddDays(1);
                    }

                    return revenueByDay;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by day.", e);
            }
        }



        public int GetNumberOfBookSoldByProductIdAndTime(Guid id, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Basket> baskets = GetBasketByProduct(id).Where(b => b.AddedDate >= startDate && b.AddedDate <= endDate).ToList();
                int total = 0;

                if (baskets.Count > 0)
                {
                    total = baskets.Sum(b => b.Quantity);
                }

                return total;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting the revenue by productId and DateTime range.", e);
            }
        }

        public int GetTotalNumberOfBookSoldByTime(Guid agencyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInvent(agencyId);
                int total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        int num = GetNumberOfBookSoldByProductIdAndTime(book.ProductId, startDate, endDate);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total revenue by DateTime.", e);
            }
        }

        public Dictionary<string, SoldInfo> GetNumberOfBookSoldAndUnitByMonth(Guid userId, Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var agency = context.Agencies.FirstOrDefault(a => a.AgencyId == agencyId);
                    if (agency == null)
                    {
                        throw new Exception("Agency not found.");
                    }

                    var user = context.AppUsers.FirstOrDefault(app => app.UserId == agency.OwnerId);
                    if (user == null)
                    {
                        throw new Exception("User not found for the agency.");
                    }

                    var transaction = context.Transactions.FirstOrDefault(t => t.UserId == user.UserId);
                    if (transaction == null)
                    {
                        throw new Exception("Transaction not found for the user.");
                    }

                    var order = context.Orders
                                        .OrderBy(o => o.CreatedDate)
                                        .FirstOrDefault(o => o.TransactionId == transaction.TransactionId);
                    if (order == null)
                    {
                        throw new Exception("Order not found for the transaction.");
                    }

                    var basket = context.Baskets
                                        .OrderBy(b => b.AddedDate)
                                        .FirstOrDefault(b => b.OrderId == order.OrderId);
                    if (basket == null)
                    {
                        throw new Exception("Basket not found for the order.");
                    }

                    DateTime agencyAddedDate = basket.AddedDate;
                    if (agencyAddedDate == DateTime.MinValue)
                    {
                        throw new Exception("Agency added date not found.");
                    }

                    // Initialize variables for iteration
                    DateTime currentDate = agencyAddedDate.Date;
                    DateTime endDate = DateTime.Today.Date;

                    Dictionary<string, SoldInfo> numberOfBookSoldByMonth = new Dictionary<string, SoldInfo>();

                    // Iterate through each month starting from the added date
                    while (currentDate <= endDate || currentDate.Month == DateTime.Today.Month)
                    {
                        // Calculate the start and end dates for the current month
                        DateTime monthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                        string monthYear = monthStartDate.ToString("MMMM yyyy");

                        DateTime monthEndDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));

                        // Get revenue for the current month
                        int numberOfBookSold = GetTotalNumberOfBookSoldByTime(agencyId, monthStartDate, monthEndDate);
                        int numberOfUnitSold = GetNumberOfUnitSoldByTime(userId, monthStartDate, monthEndDate);
                        numberOfBookSoldByMonth.Add(monthYear, (new SoldInfo { NumberOfBookSold=numberOfBookSold,NumberOfUnitSold=numberOfUnitSold}));

                        // Move to the next month
                        currentDate = currentDate.AddMonths(1);
                    }

                    return numberOfBookSoldByMonth;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by month.", e);
            }
        }

        public Dictionary<string, SoldInfo> GetNumberOfBookSoldAndUnitByDay(Guid userId, Guid agencyId) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var agency = context.Agencies.FirstOrDefault(a => a.AgencyId == agencyId);
                    if (agency == null)
                    {
                        throw new Exception("Agency not found.");
                    }

                    var user = context.AppUsers.FirstOrDefault(app => app.UserId == agency.OwnerId);
                    if (user == null)
                    {
                        throw new Exception("User not found for the agency.");
                    }

                    var transaction = context.Transactions.FirstOrDefault(t => t.UserId == user.UserId);
                    if (transaction == null)
                    {
                        throw new Exception("Transaction not found for the user.");
                    }

                    var order = context.Orders
                                        .OrderBy(o => o.CreatedDate)
                                        .FirstOrDefault(o => o.TransactionId == transaction.TransactionId);
                    if (order == null)
                    {
                        throw new Exception("Order not found for the transaction.");
                    }

                    var basket = context.Baskets
                                        .OrderBy(b => b.AddedDate)
                                        .FirstOrDefault(b => b.OrderId == order.OrderId);
                    if (basket == null)
                    {
                        throw new Exception("Basket not found for the order.");
                    }

                    DateTime agencyAddedDate = basket.AddedDate;
                    if (agencyAddedDate == DateTime.MinValue)
                    {
                        throw new Exception("Agency added date not found.");
                    }

                    // Initialize variables for iteration
                    DateTime currentDate = agencyAddedDate.Date;
                    DateTime endDate = DateTime.Today.Date;

                    Dictionary<string, SoldInfo> numberOfBookSoldByDay = new Dictionary<string, SoldInfo>();

                    // Iterate through each day starting from the agency's added date until the current date
                    while (currentDate <= endDate)
                    {
                        // Get sold info for the current day
                        int numberOfBookSold = GetTotalNumberOfBookSoldByTime(agencyId, currentDate, currentDate);
                        int numberOfUnitSold = GetNumberOfUnitSoldByTime(userId, currentDate, currentDate);
                        string dayKey = currentDate.ToString("dd-MM-yyyy");
                        numberOfBookSoldByDay.Add(dayKey, new SoldInfo { NumberOfBookSold = numberOfBookSold, NumberOfUnitSold = numberOfUnitSold });

                        // Move to the next day
                        currentDate = currentDate.AddDays(1);
                    }

                    return numberOfBookSoldByDay;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting data by day.", e);
            }
        }

        public Dictionary<string, SoldInfo> GetNumberOfBookSoldAndUnitByDayInput(Guid userId, Guid agencyId,DateTime currentDate,DateTime endDate) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Dictionary<string, SoldInfo> numberOfBookSoldByDay = new Dictionary<string, SoldInfo>();

                    // Iterate through each day starting from the agency's added date until the current date
                    while (currentDate <= endDate)
                    {
                        // Get sold info for the current day
                        int numberOfBookSold = GetTotalNumberOfBookSoldByTime(agencyId, currentDate, currentDate);
                        int numberOfUnitSold = GetNumberOfUnitSoldByTime(userId, currentDate, currentDate);
                        string dayKey = currentDate.ToString("dd-MM-yyyy");
                        numberOfBookSoldByDay.Add(dayKey, new SoldInfo { NumberOfBookSold = numberOfBookSold, NumberOfUnitSold = numberOfUnitSold });

                        // Move to the next day
                        currentDate = currentDate.AddDays(1);
                    }

                    return numberOfBookSoldByDay;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting data by day.", e);
            }
        }

        public decimal GetTotalRevenueByType(Guid agencyId, string type)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInventoryByType(agencyId, type);
                decimal total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        decimal num = GetRevenueByProductId(book.ProductId);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of book sold.", e);
            }
        }



        public Dictionary<string, RevenueInfo> GetTotalRevenueByType(Guid agencyId)
        {
            try
            {
                // Get total revenue for all types
                decimal totalRevenue = GetTotalRevenue(agencyId);

                // Calculate revenue by type and percentage
                Dictionary<string, RevenueInfo> revenueByType = new Dictionary<string, RevenueInfo>();

                // Get revenue for the "old" type
                decimal oldTypeRevenue = GetTotalRevenueByType(agencyId, "Old");
                revenueByType.Add("Old", new RevenueInfo { Revenue = oldTypeRevenue, Percentage = (oldTypeRevenue / totalRevenue) * 100 });

                // Get revenue for the "new" type
                decimal newTypeRevenue = GetTotalRevenueByType(agencyId, "New");
                revenueByType.Add("New", new RevenueInfo { Revenue = newTypeRevenue, Percentage = (newTypeRevenue / totalRevenue) * 100 });

                // Add total revenue
                revenueByType.Add("Total", new RevenueInfo { Revenue = totalRevenue, Percentage = 100 });

                return revenueByType;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by type.", e);
            }
        }



        public decimal GetTotalRevenueByCategory(Guid agencyId, string cate)
        {
            try
            {
                BookDAO b = new BookDAO();
                List<Book> books = b.GetAllBookInInventoryByCategory(agencyId, cate);
                decimal total = 0;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        decimal num = GetRevenueByProductId(book.ProductId);
                        total += num;
                    }
                }

                return total;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of book sold.", e);
            }
        }
        CategoryDAO cateDAO = new CategoryDAO();

        public Dictionary<string, RevenueInfo> GetTotalRevenueByCategory(Guid agencyId)
        {
            try
            {
                var c = cateDAO.GetAllCategory();
                // Get total revenue for all types
                decimal totalRevenue = GetTotalRevenue(agencyId);
                Dictionary<string, RevenueInfo> revenueByCate = new Dictionary<string, RevenueInfo>();
                foreach (var category in c)
                {
                    // Get revenue for the each category
                    decimal cateRevenue = GetTotalRevenueByCategory(agencyId, category.CateName);
                    revenueByCate.Add(category.CateName, (new RevenueInfo { Revenue = cateRevenue, Percentage = (cateRevenue / totalRevenue) * 100 }));
                }
                revenueByCate.Add("Total", (new RevenueInfo { Revenue = totalRevenue, Percentage = 100 }));
                return revenueByCate;
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue by category.", e);
            }
        }

        public (decimal thisMonthRevenue,decimal thisDayRevenue, decimal avgMonthRevenue, 
            decimal avgDayRevenue, string highestMonth, string highestDay, decimal avgMonthPercentage, 
            decimal avgDayPercentage, decimal highestMonthPercentage, 
            decimal highestDayPercentage) GetRevenueStats(Guid agencyId) //Update
        {
            try
            {
                // Get revenue by month
                Dictionary<string, decimal> revenueByMonth = GetRevenueByMonth(agencyId);
                Dictionary<string, decimal> revenueByDay = GetRevenueByDay(agencyId);
                // Calculate total revenue
                decimal totalRevenue = revenueByMonth.Sum(x => x.Value);

                // Calculate number of months
                int numberOfMonths = revenueByMonth.Count;
                int numberOfDays = revenueByDay.Count;
                // Calculate revenue of this month
                string thisMonth = DateTime.Now.ToString("MMMM yyyy");
                decimal thisMonthRevenue = revenueByMonth.ContainsKey(thisMonth) ? revenueByMonth[thisMonth] : 0;
                string thisDay = DateTime.Now.ToString("dd-MM-yyyy");
                decimal thisDayRevenue = revenueByDay.ContainsKey(thisDay) ? revenueByDay[thisDay] : 0;
                // Calculate average revenue
                decimal avgMonthRevenue = totalRevenue / numberOfMonths;
                decimal avgDayRevenue = totalRevenue/ numberOfDays;

                // Find the month with the highest revenue
                string highestMonth = revenueByMonth.OrderByDescending(x => x.Value).FirstOrDefault().Key;
                string highestDay = revenueByDay.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                // Calculate percentage of revenue of this month compared to the average revenue
                decimal avgMonthPercentage = (thisMonthRevenue / avgMonthRevenue) * 100;
                decimal avgDayPercentage = (thisDayRevenue / avgDayRevenue) * 100;

                // Calculate percentage of this month revenue compared to the highest month revenue
                decimal highestMonthRevenue = revenueByMonth[highestMonth];
                decimal highestMonthPercentage = (thisMonthRevenue / highestMonthRevenue) * 100;
                decimal highestDayRevenue = revenueByDay[highestDay];
                decimal highestDayPercentage = (thisDayRevenue / highestDayRevenue) * 100;

                return (thisMonthRevenue, thisDayRevenue, avgMonthRevenue, 
                    avgDayRevenue, highestMonth, highestDay, avgMonthPercentage, 
                    avgDayPercentage, highestMonthPercentage, highestDayPercentage);
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while getting revenue statistics.", e);
            }
        }

        public int GetTotalNumberOfUnitSold(Guid userId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Fetch the transaction ID for the given user
                    var transactionId = context.Transactions
                                                .Where(t => t.UserId == userId)
                                                .Select(t => t.TransactionId)
                                                .FirstOrDefault();

                    if (transactionId == Guid.Empty)
                    {
                        // Handle scenario where no transaction is found for the user
                        return 0;
                    }

                    // Fetch all the baskets associated with the transaction ID
                    var baskets = context.Baskets
                                         .Where(b => b.OrderId != null &&
                                                     context.Orders.Any(o => o.TransactionId == transactionId && o.OrderId == b.OrderId))
                                         .ToList();

                    // Calculate the total number of units sold
                    int total = baskets.Count();
                    return total;
                }
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of units sold.", e);
            }
        }

        public int GetNumberOfUnitSoldByTime(Guid userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Fetch the transaction ID for the given user
                    var transactionId = context.Transactions
                                                .Where(t => t.UserId == userId)
                                                .Select(t => t.TransactionId)
                                                .FirstOrDefault();

                    if (transactionId == Guid.Empty)
                    {
                        // Handle scenario where no transaction is found for the user
                        return 0;
                    }

                    // Fetch all the baskets associated with the transaction ID
                    var baskets = context.Baskets
                                         .Where(b => b.OrderId != null &&
                                                     context.Orders.Any(o => o.TransactionId == transactionId && o.OrderId == b.OrderId) &&
                                                     b.AddedDate >= startDate && b.AddedDate <= endDate)
                                         .ToList();

                    // Calculate the total number of units sold
                    int total = baskets.Count;
                    return total;
                }
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the total number of units sold by time.", e);
            }
        }

        /*------------------------------------------Son-------------------------------------*/

        public decimal GetTotalSpendByCustomerIdInMonth(UserSpendInMonth userSpend) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    DateTime startDate = new DateTime(userSpend.Year, userSpend.Month, 1);
                    DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                    decimal totalSpend = context.Orders
                        .Where(o => o.CustomerId == userSpend.CustomerId &&
                                    o.CreatedDate >= startDate && o.CreatedDate <= endDate)
                        .Sum(o => o.Total_Price ?? 0);

                    return totalSpend;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public string CompareTotalSpend(Guid customerId) //SONDB 
        {
            // Get current month and year
            DateTime currentDate = DateTime.Now;
            int currentYear = currentDate.Year;
            int currentMonth = currentDate.Month;

            // Get previous month and year
            DateTime previousDate = currentDate.AddMonths(-1);
            int previousYear = previousDate.Year;
            int previousMonth = previousDate.Month;

            decimal currentMonthSpend = GetTotalSpendByCustomerIdInMonth(new UserSpendInMonth { CustomerId = customerId, Year = currentYear, Month = currentMonth });
            decimal lastMonthSpend = GetTotalSpendByCustomerIdInMonth(new UserSpendInMonth { CustomerId = customerId, Year = previousYear, Month = previousMonth });

            if (currentMonthSpend > lastMonthSpend)
            {
                decimal percentageIncrease = ((currentMonthSpend - lastMonthSpend) / lastMonthSpend) * 100;
                return $"! You've spent {percentageIncrease}% more this month compared to last month.";
            }
            else if (currentMonthSpend < lastMonthSpend)
            {
                decimal percentageDecrease = ((lastMonthSpend - currentMonthSpend) / lastMonthSpend) * 100;
                return $"You've spent {percentageDecrease}% less this month compared to last month.";
            }
            else
            {
                return "Your spending remains the same compared to last month.";
            }
        }


        public List<OrderHistoryItem> GetOrderHistory(Guid userId) //SONDB 
        {
            using (var context = new AppDbContext())
            {
                var orderHistory = new List<OrderHistoryItem>();

                // Retrieve orders for the given user
                var orders = context.Orders.Where(o => o.CustomerId == userId).ToList();

                foreach (var order in orders)
                {
                    var orderItem = new OrderHistoryItem
                    {
                        OrderId = order.OrderId,
                        TotalPrice = order.Total_Price,
                        Date = order.CreatedDate,
                        Items = new List<OrderItem>() // Ensure the Items property holds a list of OrderItem objects
                    };
                    // Retrieve baskets associated with the order
                    var baskets = context.Baskets.Where(b => b.OrderId == order.OrderId).ToList();

                    foreach (var basket in baskets)
                    {
                        var book = context.Books.FirstOrDefault(b => b.ProductId == basket.ProductId);
                        var inventory = context.Inventories.FirstOrDefault(i => i.ProductId == basket.ProductId);
                        var agency = context.Agencies.FirstOrDefault(a => a.AgencyId == inventory.AgencyId);

                        if (book != null && inventory != null) // Ensure book and inventory are found
                        {
                            var orderDetail = new OrderItem
                            {
                                BookId = basket.ProductId,
                                BookName = book.Name,
                                BookDir = book.BookDir,
                                Price = basket.Stored_Price,
                                Quantity = basket.Quantity,
                                Status = basket.ItemStatus,
                                AgencyId = inventory.AgencyId,
                                AgencyName = agency.AgencyName,
                            };

                            orderItem.Items.Add(orderDetail);
                        }
                    }

                    orderHistory.Add(orderItem);
                }

                return orderHistory;
            }
        }
        public List<OrderDetailsDTO> SearchOrders(Guid userId, string? address, string? bookName, string? customerName, DateTime? startDate, DateTime? endDate, string status)
        {
            List<OrderDetailsDTO> results = new List<OrderDetailsDTO>();

            try
            {
                // Validate the status parameter
                if (!string.IsNullOrEmpty(status) && !status.Equals("OnGoing", StringComparison.OrdinalIgnoreCase) && !status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Invalid status. Status must be either 'OnGoing' or 'Done'.");
                }

                // Get all order details
                List<OrderDetailsDTO> allOrders = GetAllOrderDetails(userId);

                // Apply filtering based on the provided parameters
                results = allOrders.Where(order =>
                {
                    bool addressMatches = string.IsNullOrEmpty(address) || order.Address.Contains(address);

                    bool bookNameMatches = string.IsNullOrEmpty(bookName) || order.BookName.Any(b => b.Contains(bookName));

                    bool customerNameMatches = string.IsNullOrEmpty(customerName) || order.CustomerName.Contains(customerName);

                    bool startDateMatches = startDate == null || order.CreatedDate >= startDate;

                    bool endDateMatches = endDate == null || order.CreatedDate <= endDate;

                    // If any of the parameters is not null, apply the filtering logic
                    return addressMatches && bookNameMatches && customerNameMatches && startDateMatches && endDateMatches;
                }).ToList();

                // Additional check for "OnGoing" status
                if (!string.IsNullOrEmpty(status) && status.Equals("OnGoing", StringComparison.OrdinalIgnoreCase))
                {
                    using (var context = new AppDbContext())
                    {
                        // Filter out orders that are present in the Basket table
                        results = results.Where(order => !context.Baskets.Any(b => b.OrderId == order.OrderId)).ToList();
                    }
                }

                // Additional check for "Done" status
                if (!string.IsNullOrEmpty(status) && status.Equals("Done", StringComparison.OrdinalIgnoreCase))
                {
                    using (var context = new AppDbContext())
                    {
                        // Filter out orders that are not present in the Basket table
                        results = results.Where(order => context.Baskets.Any(b => b.OrderId == order.OrderId)).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return results;
        }
        private List<OrderDetailsDTO> GetAllOrderDetails(Guid userId)
        {
            List<OrderDetailsDTO> orderDetails = new List<OrderDetailsDTO>();

            try
            {
                // Retrieve all order details for the specified user
                List<Order> orders = GetOrderByUserId(userId);

                // Convert each order to OrderDetailsDTO
                foreach (var order in orders)
                {
                    OrderDetailsDTO orderDetail = GetOrderDetailsById(order.OrderId);
                    orderDetails.Add(orderDetail);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return orderDetails;
        }
    }
}

