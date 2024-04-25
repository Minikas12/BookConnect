using System;
using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
    public class NewBookGroupDTO //DATDQ
    {
        public Guid? BookGroupId { get; set; }
        public string? BookGroupName { get; set; } = null!;
        public IFormFile? bookGroupImg { get; set; }
        public string? Description { get; set; }
    }

}

