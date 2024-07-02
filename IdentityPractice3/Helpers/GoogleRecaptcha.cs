using Newtonsoft.Json.Linq;

namespace IdentityPractice3.Helpers
{
    public class GoogleRecaptcha
    {
        public bool Verify(string googleResponse)
        {
            string secKey = "6LflAu8pAAAAAJTc_AE2qwQprmYs8-vgHJXjfJtd";
            HttpClient httpClient = new HttpClient();
            var result = httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secKey}&response={googleResponse}", null).Result;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            string content = result.Content.ReadAsStringAsync().Result;
            dynamic jsonData = JObject.Parse(content);

            if (jsonData.success == "true")
            {
                return true;
            }
            return false;
        }
    }
}
