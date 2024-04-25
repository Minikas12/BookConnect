namespace APIs.DTO.Ecom
{
	public class ProductToCartDTO
	{
		public Guid ProductId { get; set; }
		public Guid CartId { get; set; }
		public int Quantity { get; set; }
	}

	public class CartDetailsDTO
	{
        public Guid? ProductId { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string? Name { get; set; } = null!;
        public int? Quantity { get; set; }
		public Guid? CartId { get; set;}

	}

}

