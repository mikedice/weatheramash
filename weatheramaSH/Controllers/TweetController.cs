using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using weatheramaSH.Utilities;
using System.Net;
using System.Linq;
using System.Net.Http.Headers;

namespace weatheramaSH.Controllers
{
    public class TweetController : ApiController
    {
        public class UserTweet
        {
            public string screenName { get; set; }
            public string message { get; set; }
        }
        public async Task<HttpResponseMessage> Post([FromBody] UserTweet tweet)
        {
            var userAuthInfo = Program.GetUserAuthInfo(tweet.screenName);
            if (userAuthInfo == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);

            }


            var httpMethod = "POST";
            var baseUrl = "https://api.twitter.com/1.1/statuses/update.json";
            var twitterOauthParams = new Dictionary<string, string>()
            {
                {TwitterOauthUtils.OAuthConsumerKeyKey,  TwitterSignInController.consumer_key},
                {TwitterOauthUtils.OAuthNonceKey, TwitterOauthUtils.GenerateNonce() },
                {TwitterOauthUtils.OAuthSignatureMethodKey, TwitterOauthUtils.OAuthSignatureMethod },
                {TwitterOauthUtils.OAuthTimestampKey, TwitterOauthUtils.UnixTimeStamp() },
                {TwitterOauthUtils.OAuthVersionKey, TwitterOauthUtils.OAuthVersion },
                {TwitterOauthUtils.OAuthTokenKey, userAuthInfo.OAuthToken }
            };
            var twitterQueryParams = new Dictionary<string, string>();
            twitterQueryParams.Add("status", tweet.message);
            var twitterPostParams = new Dictionary<string, string>();
            var oauthSignature = TwitterOauthUtils.CreateSignature(httpMethod,
                baseUrl,
                twitterQueryParams,
                twitterPostParams,
                twitterOauthParams,
                TwitterSignInController.consumer_secret,
                userAuthInfo.OAuthTokenSecret);

            List<string> queryParamsList = new List<string>();
            foreach(var kvp in twitterQueryParams)
            {
                queryParamsList.Add(string.Format("{0}={1}", kvp.Key, kvp.Value));
            }

            var url = string.Format("{0}?{1}", baseUrl, string.Join("&", queryParamsList));


            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, url);
            message.Headers.Add("Accept", "*/*");
            message.Headers.Add("User-Agent", "weatherama-client-1.0");
            message.Headers.Add("Host", "api.twitter.com");

            var oauthParamsList = new List<string>();
            foreach (var kvp in twitterOauthParams)
            {
                oauthParamsList.Add(string.Format("{0}=\"{1}\"", kvp.Key, TwitterOauthUtils.PercentEncode(kvp.Value)));
            }
            oauthParamsList.Add(string.Format("{0}=\"{1}\"", TwitterOauthUtils.OAuthSignatureKey, TwitterOauthUtils.PercentEncode(oauthSignature)));
            var oauthHeader = string.Format("OAuth {0}", string.Join(",", oauthParamsList));
            message.Headers.Add("Authorization", oauthHeader);

            

            HttpClient httpClient = new HttpClient();
            var responseMessage = await httpClient.SendAsync(message);
            string responseContent = string.Empty;
            if (responseMessage.Content != null)
            {
                responseContent = await responseMessage.Content.ReadAsStringAsync();
            }

            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                System.Diagnostics.Debug.WriteLine(responseContent);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
