using System;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models.Ecom;
using DataAccess.DAO;

namespace APIs.Services.Interfaces
{
	public interface IOrderService
	{
        public string TakeProductFromCart(Guid userId, Guid orderId);

        public string CreateNewOrder(NewOrderDTO data);

        public string TakeProductFromCartOptional (Guid userId, Guid orderId, List<ProductOptionDTO> products);

        public decimal GetTotalAmount(List<ProductOptionDTO> dto);

        public int GetCurrentStock(Guid productId);

        //Task<List<OrderedBookDTO>> GetUserOrderHistoryAsync(Guid userId);//SONDB 
        public decimal GetTotalSpendByCustomerIdInMonth(UserSpendInMonth userSpend);//SONDB 
        List<OrderHistoryItem> GetOrderHistory(Guid userId);
        public string CompareTotalSpend(Guid customerId);//SONDB 

        // ----------------------DATDQ-----------------------------//

        public PagedList<Order> GetAllOrder(PagingParams param);
        public PagedList<Order> GetOrderByProductId(Guid id, PagingParams param);
        public PagedList<Order> GetOrderByUserId(Guid id, PagingParams param);
        public OrderDetailsDTO GetOrderDetailsById(Guid id);
        public PagedList<OrderDetailsDTO> SearchOrders(Guid userId, string? address, string? bookName, string? customerName, DateTime? startDate, DateTime? endDate, string status, PagingParams param);

        // ---------------------END DATDQ--------------------------//
    }
}

