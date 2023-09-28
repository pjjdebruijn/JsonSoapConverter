
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace JsonSoapConverter.Api
{
    [ApiController]
    [Route("[controller]")]
    public class NumberConversionController : ControllerBase
    {
        /// <summary>
        ///  takes json and maps it into request of existing soap service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("NumberToWords")]
        public async Task<IActionResult> NumberToWords([FromBody] NumberToWordsRequest request)
        {
            var soapRequestBody = 
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <NumberToWords xmlns=""http://www.dataaccess.com/webservicesserver/"">
            <ubiNum>{request.ubiNum}</ubiNum>
        </NumberToWords>
    </soap:Body>
</soap:Envelope>";

            using HttpClient httpClient = new HttpClient();
            using var content = new StringContent(soapRequestBody, Encoding.UTF8, "text/xml");
            using var response = await httpClient.PostAsync("https://www.dataaccess.com/webservicesserver/NumberConversion.wso", content);
            var soapResponse = await response.Content.ReadAsStringAsync();

            var startTag = "<m:NumberToWordsResult>";
            var endTag = "</m:NumberToWordsResult>";
            var start = soapResponse.IndexOf(startTag);
            var end = soapResponse.IndexOf(endTag);

            if (start != -1 && end != -1)
            {
                start += startTag.Length;
                var result = soapResponse[start..end].Trim();
                return Ok(new NumberToWordsResponse { NumberToWordsResult = result });
            }
            else
            {
                return BadRequest("Invalid SOAP Response");
            }
        }
    }

    public class NumberToWordsRequest
    {
        public int ubiNum { get; set; }
    }

    public class NumberToWordsResponse
    {
        public string NumberToWordsResult { get; set; }
    }
}
