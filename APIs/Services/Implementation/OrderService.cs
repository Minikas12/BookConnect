using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models.Ecom;
using DataAccess.DAO;
using DataAccess.DAO.Ecom;

namespace APIs.Services
{
	public class OrderService: IOrderService
	{
        private readonly OrderDAO _orderDAO;

        public OrderService()
        {
            _orderDAO = new OrderDAO();

        }
        public string TakeProductFromCart(Guid userId, Guid orderId) => new OrderDAO().TakeProductFromCart(userId, orderId);
        public string CreateNewOrder(NewOrderDTO data) => new OrderDAO().CreateNewOrder(data);
        public string TakeProductFromCartOptional (Guid userId, Guid orderId, List<ProductOptionDTO> products)
        => new OrderDAO().TakeProductFromCartOptional(userId, orderId, products);

        public decimal GetTotalAmount(List<ProductOptionDTO> productIds)
        => new OrderDAO().GetTotalAmount(productIds);

        public int GetCurrentStock(Guid productId) => new AgencyDAO().GetProductQuantity(productId);
      
        public decimal GetTotalSpendByCustomerIdInMonth(UserSpendInMonth userSpend) //SONDB 
         => new OrderDAO().GetTotalSpendByCustomerIdInMonth(userSpend);

        public string CompareTotalSpend(Guid customerId) //SONDB 
        => new OrderDAO().CompareTotalSpend(customerId);
        //public async Task<List<OrderedBookDTO>> GetUserOrderHistoryAsync(Guid userId) => await _orderDAO.GetUserOrderHistoryAsync(userId); //SONDB 
        public List<OrderHistoryItem> GetOrderHistory(Guid userId) => new OrderDAO().GetOrderHistory(userId); //SONDB 

        //public async Task UpdateProductStatusAsync(UpdateProductStatusDTO dto) => await _orderDAO.UpdateProductStatusAsync(dto);

        // ----------------------------DATDQ---------------------------------//

        public OrderDetailsDTO GetOrderDetailsById(Guid id) => new OrderDAO().GetOrderDetailsById(id);
        public PagedList<Order> GetAllOrder(PagingParams param)
        {
            return PagedList<Order>.ToPagedList(new OrderDAO().GetAllOrder().OrderBy(o => o.CreatedDate).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<Order> GetOrderByProductId(Guid id, PagingParams param)
        {
            return PagedList<Order>.ToPagedList(new OrderDAO().GetOrderByProductId(id).OrderBy(o => o.CreatedDate).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<Order> GetOrderByUserId(Guid id, PagingParams param)
        {
            return PagedList<Order>.ToPagedList(new OrderDAO().GetOrderByUserId(id).OrderBy(o => o.CreatedDate).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public PagedList<OrderDetailsDTO> SearchOrders(Guid userId, string? address, string? bookName, string? customerName, DateTime? startDate, DateTime? endDate, string status, PagingParams param)
        {
            return PagedList<OrderDetailsDTO>.ToPagedList(new OrderDAO().SearchOrders(userId, address, bookName, customerName, startDate, endDate, status).OrderBy(o => o.CreatedDate).AsQueryable(), param.PageNumber, param.PageSize); //New
        }

        // ----------------------------END DATDQ-----------------------------//

    }
}

