namespace BusinessObjects.DTO
{
	public class NewStatDTO
	{
        public Guid? PostId { get; set; }
        public Guid? BookId { get; set; }
        public int View { get; set; }
        public int? Interested { get; set; }
        public int? Purchase { get; set; }
        public int Search { get; set; }
        public int? Hearts { get; set; }
    }

    public class UpdateStatDTO
    {
        public Guid StatId { get; set; }
        public Guid? PostId { get; set; }
        public Guid? BookId { get; set; }
        public int View { get; set; }
        public int? Interested { get; set; }
        public int? Purchase { get; set; }
        public int Search { get; set; }
        public int? Hearts { get; set; }
    }

    public class RelavantFactorDTO
    {
        public List<Guid> CateIds { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public double Rating { get; set; }
    }

    public class Stats
    {
        public Dictionary<string, string>? Data { get; set; }
    }
}

