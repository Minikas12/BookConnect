using BusinessObjects.DTO;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NicOcrController : ControllerBase
    {
        private readonly IConfiguration _config;
        public NicOcrController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost("ocr-request")]
        public async Task<IActionResult> Post([FromForm] OcrApiRequestDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid!");
                }
                using (var client = new HttpClient())
                {
                    var formContent = new MultipartFormDataContent();

                    var memoryStream = new MemoryStream();
                    await dto.NicImage.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset the position of the stream

                    formContent.Add(new StreamContent(memoryStream), "image", dto.NicImage.FileName);

                    client.DefaultRequestHeaders.Add("api_key", "t833lvdQ2FHs1Mu4X6PYKdNu1t9vXwgH");

                    var response = await client.PostAsync("https://api.fpt.ai/vision/idr/vnm", formContent);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    //return Ok(new OcrApiFrontResponseDTO());

                    if (response.IsSuccessStatusCode)
                    {
                        if (dto.NidSide == NicSide.Front)
                        {
                            OcrApiFrontResponseDTO? resFrontData = Newtonsoft.Json.JsonConvert.DeserializeObject<OcrApiFrontResponseDTO>(responseContent);
                            return CreateResponseData(resFrontData);
                        }
                        OcrApiBackResponseDTO? resBackData = Newtonsoft.Json.JsonConvert.DeserializeObject<OcrApiBackResponseDTO>(responseContent);
                        return CreateResponseData(resBackData);
                        //return Ok(responseContent);
                    }
                    else
                    {
                        return BadRequest(responseContent);
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [NonAction]
        public IActionResult CreateResponseData(object? data)
        {
            if (data != null)
            {
                return Ok(new { message = "OK", data });
            }
            else
            {
                return Ok(new { message = "Not OK" });
            }
        }
    }
}

