using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Utils
{
	public class NIC_Data
    {
        [Key]
        public Guid NICId { get; set; }
        //"id": "042297018122",
        public string? Id { get; set; } 
        //"id_prob": "98.50",
        public string? Fullname { get; set; } 
        //"name": "PHAN HUY HOÀNG HIỆP",
        //"name_prob": "99.56",
        //"number_of_name_lines": "1",
        public DateTime? DateOfBirth { get; set; }
        //"dob": "16/05/1997",
        //"dob_prob": "97.99",
        public string? Sex { get; set; }
        //"sex": "NAM",
        //"sex_prob": "98.72",
        public string? Nationality { get; set; } 
        //"nationality": "VIỆT NAM",
        //"nationality_prob": "91.65",
        public string? Type_New { get; set; }
        //"type_new": "cccd_12_front",
        public DateTime? DateOfExpired { get; set; }
        //"doe": "16/05/2037",
        //"doe_prob": "86.12",
        public string? Home { get; set; }
        //"home": "THẠCH CHÂU, LỘC HÀ, HÀ TĨNH",
        //"home_prob": "95.08",
        public string? Address { get; set; }
        //"address": "THÔN NGOẠI ĐỘ, Đ BÌNH, ỨNG HÒA, HÀ NỘI",
        //"address_prob": "95.07",
        //"address_entities": {
        //  "province": "HÀ NỘI",
        //  "district": "ỨNG HÒA",
        //  "ward": "Đ BÌNH",
        //  "street": "THÔN NGOẠI ĐỘ"
        //},
        //"overall_score": "88.85",
        //"type": "new"
        public string? Religion { get; set; }
        //"religion_prob": "N/A",
        //"religion": "N/A",
        public string? Ethnicity { get; set; }
        //"ethnicity_prob": "N/A",
        //"ethnicity": "N/A",
        public string? Feature { get; set; }
        //"features": "NỐT RUỒI CÁCH 1CM DƯỚI TRƯỚC ĐUÔI MẮT PHẢI",
        //"features_prob": "99.29",
        public string? Issue_Date { get; set; }
        //"issue_date": "03/07/2020",
        //"issue_date_prob": "95.76",
        //"issue_loc_prob": "96.90",
        public string? Issue_Loc { get; set; }
        //"issue_loc": "CỤC TRƯỞNG CỤC CẢNH SÁT QUẢN LÝ HÀNH CHÍNH VỀ TRẬT TỰ XÃ HỘI",
        //"type": "new_back"
        public Guid OwnerId { get; set; }

        [ForeignKey("OwnerId"), JsonIgnore]
        public AppUser Owner { get; set; } = null!;
    }
}

