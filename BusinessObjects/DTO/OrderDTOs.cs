using Newtonsoft.Json;
using System;
namespace BusinessObjects.DTO
{
    public class NewOrderDTO
    {
        //OrderId
        public Guid OrderId { get; set; }
        //CustomerId
        public Guid CustomerId { get; set; }
        //Status
        public string Status { get; set; } = null!;

        public string PaymentMethod { get; set; } = null!;

        public decimal? Price { get; set; }
        //Notes
        public string? Notes { get; set; }
        //PaymentId
        public Guid? TransactionId { get; set; }
        //AddressId
        public Guid AddressId { get; set; }
    }

    public class CheckoutDTO
    {
        public Guid AddressId { get; set; }
        public Guid CustomerId { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public List<ProductOptionDTO> Products { get; set; } = null!;
        public PaymentReturnDTO? PaymentReturnDTO { get; set; }
    }

    public class PreSubCheckoutDTO
    {
        public Guid ReferenceId { get; set; }
        public decimal Price { get; set; }
    }

    public class PreCheckoutDTO
    {
        public Guid ReferenceId { get; set; }
        public List<ProductOptionDTO> Products { get; set; } = null!;
    }

    public class ProductOptionDTO
    {
        public Guid AgencyId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } 
    }


    public class OrderHistoryItem
    {
        public Guid OrderId { get; set; }
        public decimal? TotalPrice { get; set; }
        public DateTime Date { get; set; }
        public List<OrderItem>? Items { get; set; }
    }

    public class OrderItem
    {
        public Guid BookId { get; set; }
        public string? BookName { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public string? Status { get; set; }
        public Guid? AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public string? BookDir { get; set; }
    }

    public class UserSpendInMonth //SONDB
    {
        public Guid CustomerId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
    }
    public class OrderedBookDTO //SONDB
    {
        public Guid BookId { get; set; }
        public string? BookName { get; set; }
        public string? BookDir { get; set; }

        public decimal Price { get; set; }
        public string? Status { get; set; }
        public Guid AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public int Quantity { get; set; }
    }
    public class UpdateProductStatusDTO //SONDB
    {
        public Guid OrderId { get; set; }
        public Guid BookId { get; set; }
        public Guid AgencyId { get; set; }
        public string? NewStatus { get; set; }
    }

    // -------------------------------------DATDQ----------------------------------------//

    public class OrderDetailsDTO
    {
        //OrderId
        public Guid? OrderId { get; set; }
        //CustomerId
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public List<string>? BookName { get; set; }
        public string? Status { get; set; } = null!;
        public string? PaymentMethod { get; set; } = null!;
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        //Notes
        public string? Notes { get; set; }
        //PaymentId
        public Guid? TransactionId { get; set; }
        //AddressId
        public string? Address { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class GhtkOrderResponseDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        public OrderResponseDTO Order { get; set; }
    }

    public class OrderResponseDTO
    {
        [JsonProperty("partner_id")]
        public string PartnerId { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("fee")]
        public string Fee { get; set; }

        [JsonProperty("insurance_fee")]
        public string InsuranceFee { get; set; }

        [JsonProperty("estimated_pick_time")]
        public string EstimatedPickTime { get; set; }

        [JsonProperty("estimated_deliverTime")]
        public string EstimatedDeliverTime { get; set; }
        public List<BookDTO> Products { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; }
    }

    public class GhtkOrderDTO
    {
        public List<ProductDTO> Products { get; set; }
        public OrderDTO Order { get; set; }
    }
    public class GhtkSettings
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string ApiVersion { get; set; }
    }
    public class ProductDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("product_code")]
        public int ProductCode { get; set; }
    }

    public class OrderDTO
    {
        public List<ProductDTO> Products { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pick_name")]
        public string PickName { get; set; }

        [JsonProperty("pick_address")]
        public string PickAddress { get; set; }

        [JsonProperty("pick_province")]
        public string PickProvince { get; set; }

        [JsonProperty("pick_district")]
        public string PickDistrict { get; set; }

        [JsonProperty("pick_ward")]
        public string PickWard { get; set; }

        [JsonProperty("pick_tel")]
        public string PickTel { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("ward")]
        public string Ward { get; set; }

        [JsonProperty("hamlet")]
        public string Hamlet { get; set; }

        [JsonProperty("is_freeship")]
        public bool IsFreeship { get; set; }

        [JsonProperty("pick_date")]
        public string PickDate { get; set; }

        [JsonProperty("pick_money")]
        public double PickMoney { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("transport")]
        public string Transport { get; set; }

        [JsonProperty("pick_option")]
        public string PickOption { get; set; }

        [JsonProperty("deliver_option")]
        public string DeliverOption { get; set; }

        [JsonProperty("pick_session")]
        public int PickSession { get; set; }

        [JsonProperty("tags")]
        public List<int> Tags { get; set; }
    }

    public class GhtkOrderStatusResponseDTO
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("order")]
        public GhtkOrderStatusDTO Order { get; set; }
    }

    public class GhtkOrderStatusDTO
    {
        [JsonProperty("label_id")]
        public string LabelId { get; set; }

        [JsonProperty("partner_id")]
        public string PartnerId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_text")]
        public string StatusText { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [JsonProperty("message")]
        public string OrderMessage { get; set; }

        [JsonProperty("pick_date")]
        public DateTime? PickDate { get; set; }

        [JsonProperty("deliver_date")]
        public DateTime? DeliverDate { get; set; }

        [JsonProperty("customer_fullname")]
        public string CustomerFullName { get; set; }

        [JsonProperty("customer_tel")]
        public string CustomerTel { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("storage_day")]
        public int StorageDay { get; set; }

        [JsonProperty("ship_money")]
        public decimal ShipMoney { get; set; }

        [JsonProperty("insurance")]
        public decimal Insurance { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("weight")]
        public int Weight { get; set; }

        [JsonProperty("pick_money")]
        public decimal PickMoney { get; set; }

        [JsonProperty("is_freeship")]
        public bool IsFreeship { get; set; }
    }
    // -----------------------------------END DATDQ--------------------------------------//

}

