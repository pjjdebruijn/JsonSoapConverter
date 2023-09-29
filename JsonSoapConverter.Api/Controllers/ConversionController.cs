
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace JsonSoapConverter.Api
{
    [ApiController]
    [Route("[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NumberToWordsResponse))]
    public class NumberConversionController : ControllerBase
    {

        private static HttpClient httpClient = new HttpClient( );

        [HttpPost]
        [Route("NumberToWords")]
        public async Task<IActionResult> NumberToWords([FromBody] NumberToWordsRequest request)
        {
            string soapRequestBody =
    $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <NumberToWords xmlns=""http://www.dataaccess.com/webservicesserver/"">
            <ubiNum>{request.ubiNum}</ubiNum>
        </NumberToWords>
    </soap:Body>
</soap:Envelope>";

            httpClient.DefaultRequestVersion = HttpVersion.Version20;

            using StringContent content = new StringContent(soapRequestBody, Encoding.UTF8, "text/xml");
            using HttpResponseMessage response = await httpClient.PostAsync("https://www.dataaccess.com/webservicesserver/NumberConversion.wso", content);
            string soapResponse = await response.Content.ReadAsStringAsync();

            string startTag = "<m:NumberToWordsResult>";
            string endTag = "</m:NumberToWordsResult>";
            int start = soapResponse.IndexOf(startTag);
            int end = soapResponse.IndexOf(endTag);

            //if (start != -1 && end != -1)
            //{
            //    start += startTag.Length;
            //    string result = soapResponse[start..end].Trim();
            //    return Ok(new NumberToWordsResponse { NumberToWordsResult = result });
            //}
            //else
            //{
            //    return BadRequest("Invalid SOAP Response");
            //}
            XNamespace ns = "http://www.dataaccess.com/webservicesserver/";
            XDocument doc = XDocument.Parse(soapResponse);
            var result = doc.Descendants(ns + "NumberToWordsResult").FirstOrDefault();
            if (result != null)
            {
                return Ok(new NumberToWordsResponse { NumberToWordsResult = result.Value });
            }
            else
            {
                return BadRequest("Invalid SOAP Response");
            }
        }

        [HttpPost]
        [Route("ListOfLanguagesByName")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListOfLanguagesByNameResponse))]
        public async Task<IActionResult> ListOfLanguagesByName([FromBody] ListOfLanguagesByNameRequest request)
        {
            string soapRequestBody =
        $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <ListOfLanguagesByName xmlns=""http://www.oorsprong.org/websamples.countryinfo"">
        </ListOfLanguagesByName>
    </soap:Body>
</soap:Envelope>";

            using HttpClient httpClient = new HttpClient();
            using StringContent content = new StringContent(soapRequestBody, Encoding.UTF8, "text/xml");
            using HttpResponseMessage response = await httpClient.PostAsync("http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso", content);
            string soapResponse = await response.Content.ReadAsStringAsync();

            var doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            var namespaces = new XmlNamespaceManager(doc.NameTable);
            namespaces.AddNamespace("m", "http://www.oorsprong.org/websamples.countryinfo");

            var languageNodes = doc.SelectNodes("//m:tLanguage", namespaces);

            var result = new ListOfLanguagesByNameResponse
            {
                Languages = new List<ListOfLanguagesByNameResponse.Language>()
            };

            foreach (XmlNode node in languageNodes)
            {
                var sISOCode = node.SelectSingleNode("m:sISOCode", namespaces)?.InnerText;
                var sName = node.SelectSingleNode("m:sName", namespaces)?.InnerText;

                result.Languages.Add(new ListOfLanguagesByNameResponse.Language
                {
                    sISOCode = sISOCode,
                    sName = sName
                });
            }

            return Ok(result);
        }


    }



    //STEP 1 -> TAKE XML / XSD AND GENERATE BELOW CLASSES. -> STEP 2 IMPLEMENT API ENDPOINT(LANGUAGES BY NAME). (ALSO GENERATED) --> NEXT BUILD AND RUN THE GENERATED PROGRAM USING SCRIPT OR SOMETHING?

    public class ListOfLanguagesByNameRequest
    {
        // This class can be empty since no input parameters are needed for the SOAP request
    }

    public class ListOfLanguagesByNameResponse
    {
        public List<Language> Languages { get; set; }

        public class Language
        {
            [Required]
            [MaxLength(9)]
            public string sISOCode { get; set; }
            public string sName { get; set; }
        }
    }


    public class NumberToWordsRequest
    {
        [Required]
        public int ubiNum { get; set; }
    }

    public class NumberToWordsResponse
    {
        [Required]
        public string? NumberToWordsResult { get; set; }
    }
}
