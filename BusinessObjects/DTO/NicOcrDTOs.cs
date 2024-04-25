using Microsoft.AspNetCore.Http;

namespace BusinessObjects.DTO
{
    public enum NicSide
    {
        Front,
        Back
    }

    public class OcrApiRequestDTO
    {
        public IFormFile NicImage { get; set; } = null!;
        public NicSide NidSide { get; set; }
    }

    public class OcrApiStatus
    {
        public string? errorCode { get; set; }
        public string? errorMessage { get; set; }
    }
    public class OcrApiFrontResponseDTO : OcrApiStatus
    {
        public List<OcrApiResponseFrontData> data { get; set; } = null!;
    }

    public class OcrApiBackResponseDTO : OcrApiStatus
    {
        public List<OcrApiResponseBackData> data { get; set; } = null!;
    }

    public class OcrApiResponseFrontData
    {
        //"id":"",
        public string id { get; set; } = null!;
        //"id_prob":"",
        public string id_prob { get; set; } = null!;
        //"name":"",
        public string name { get; set; } = null!;
        //"name_prob":"",
        public string name_prob { get; set; } = null!;
        //"number_of_name_lines":"",
        public string number_of_name_lines { get; set; } = null!;
        //"dob":"",
        public string dob { get; set; } = null!;
        //"dob_prob":"",
        public string dob_prob { get; set; } = null!;
        //"sex":"",
        public string sex { get; set; } = null!;
        //"sex_prob":"",
        public string sex_prob { get; set; } = null!;
        //"nationality":"",
        public string nationality { get; set; } = null!;
        //"nationality_prob":"",
        public string nationality_prob { get; set; } = null!;
        //"type_new":"",
        public string type_new { get; set; } = null!;
        //"doe":"",
        public string doe { get; set; } = null!;
        //"doe_prob":"",
        public string doe_prob { get; set; } = null!;
        //"home":"",
        public string home { get; set; } = null!;
        //"home_prob":"",
        public string home_prob { get; set; } = null!;
        //"address":"",
        public string address { get; set; } = null!;
        //"address_prob":"",
        public string address_prob { get; set; } = null!;
        //"address_entities":{"province":"","district":"","ward":"","street":""},
        public AddressEntities address_entities { get; set; } = null!;
        //"overall_score":"",
        public string overall_score { get; set; } = null!;
        //"type":""
        public string type { get; set; } = null!;
    }

    public class AddressEntities
    {
        public string province { get; set; } = null!;
        public string district { get; set; } = null!;
        public string ward { get; set; } = null!;
        public string street { get; set; } = null!;
    }

    public class OcrApiResponseBackData
    {
        //"religion_prob": "xxxx",
        public string religion_prob { get; set; } = null!;
        //"religion": "xxxx",
        public string religion { get; set; } = null!;
        //"ethnicity_prob": "xxxx",
        public string ethnicity_prob { get; set; } = null!;
        //"ethnicity": "xxxx",
        public string ethnicity { get; set; } = null!;
        //"features": "xxxx",
        public string features { get; set; } = null!;
        //"features_prob": "xxxx",
        public string features_prob { get; set; } = null!;
        //"issue_date": "xxxx",
        public string issue_date { get; set; } = null!;
        //"issue_date_prob": "xxxx",
        public string issue_date_prob { get; set; } = null!;
        //"issue_loc_prob": "xxxx",
        public string issue_loc_prob { get; set; } = null!;
        //"issue_loc": "xxxx",
        public string issue_loc { get; set; } = null!;
        //"type": "xxxx"
        public string type { get; set; } = null!;
    }
}

