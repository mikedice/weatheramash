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
    public class TwitterSignInController : ApiController
    {
        public const string consumer_key = "IPR1vxhyueAMP5pmnLfWhpmC1";
        public const string consumer_secret = "bGCTVpz3olFyjoOJP72vEVael345U6LOVBfQAqKQPoEZOIlJre";


        // App-only authentication 
        private const string AppOnlyAuthenticationUrl = "https://api.twitter.com/oauth2/token";
        
        // Request token URL 
        private const string RequestTokenUrl = "https://api.twitter.com/oauth/request_token";

        // OAuth autenticate URL
        private const string OauthAuthenticateUrl = "https://api.twitter.com/oauth/authenticate";

        // Authorize URL   
        private const string AuthorizeUrl = "https://api.twitter.com/oauth/authorize";

        // Access token URL 
        private const string AccessTokenUrl = "https://api.twitter.com/oauth/access_token";

        private const string AppCallbackUrl = "https://www.weatherama.com:8000/oauth/TwitterSignIn/callback";

        private const string AppHomeRoute = "https://www.weatherama.com:8000/";

        public const string UserAccessCookieName = "WeatheramaAccessCookie";

        public Task<HttpResponseMessage> Get()
        {
            return InitiateSignIn();
        }

        public Task<HttpResponseMessage> Get(string param)
        {
            switch (param)
            {
                case "callback":
                    return ProcessCallback();
                default:
                    return Error();
            }
        }

        public async Task<HttpResponseMessage> ProcessCallback()
        {
            var requestParams = Request.GetQueryNameValuePairs();
            var oauthToken = requestParams.Where(p => p.Key.Equals(TwitterOauthUtils.OAuthTokenKey)).FirstOrDefault();
            var oauthTokenVerifier = requestParams.Where(p => p.Key.Equals(TwitterOauthUtils.OAuthVerifierKey)).FirstOrDefault();


            // exchange the oauth token and oauth token verifier for a user access token
            var userAccessInfo = await AccessTokenExchange(oauthToken.Value, oauthTokenVerifier.Value);


            // compute a user access cookie and send the user back to the home page.
            var cookieValue = TwitterOauthUtils.ConstructUserAccessCookieValue(userAccessInfo);
            UserAuthInfo userAuthInfo;
            UserInfo userInfo;
            TwitterOauthUtils.ParseUserAccessCookie(cookieValue, out userInfo, out userAuthInfo);
            Program.SetUserAuthInfo(userAuthInfo);

            CookieHeaderValue userAccessCookie = new CookieHeaderValue(UserAccessCookieName, cookieValue);
            userAccessCookie.Path = "/";

            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.AddCookies(new CookieHeaderValue[] { userAccessCookie });
            response.Headers.Location = new System.Uri(AppHomeRoute);

            return response;
        }

        public Task<HttpResponseMessage> Error()
        {
            return null;
        }

        public async Task<string> AccessTokenExchange(string oauthToken, string oauthTokenVerifier)
        {
            var httpMethod = "POST";
            var baseUrl = AccessTokenUrl;

            var twitterOauthParams = new Dictionary<string, string>()
            {
                {TwitterOauthUtils.OAuthConsumerKeyKey,  consumer_key},
                {TwitterOauthUtils.OAuthNonceKey, TwitterOauthUtils.GenerateNonce() },
                {TwitterOauthUtils.OAuthSignatureMethodKey, TwitterOauthUtils.OAuthSignatureMethod },
                {TwitterOauthUtils.OAuthTimestampKey, TwitterOauthUtils.UnixTimeStamp() },
                {TwitterOauthUtils.OAuthTokenKey, oauthToken },
                {TwitterOauthUtils.OAuthVersionKey, TwitterOauthUtils.OAuthVersion }
            };
            var twitterQueryParams = new Dictionary<string, string>();
            var twitterPostParams = new Dictionary<string, string>();
            twitterPostParams.Add(TwitterOauthUtils.OAuthVerifierKey, oauthTokenVerifier);

            var oauthSignature = TwitterOauthUtils.CreateSignature(httpMethod,
                baseUrl,
                twitterQueryParams,
                twitterPostParams,
                twitterOauthParams,
                consumer_secret,
                null);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, baseUrl);
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

            var postBodyParamsList = new List<string>();
            foreach (var kvp in twitterPostParams)
            {
                postBodyParamsList.Add(string.Format("{0}={1}", kvp.Key, TwitterOauthUtils.PercentEncode(kvp.Value)));
            }

            var postBody = string.Join(",", postBodyParamsList);

            message.Content = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpClient httpClient = new HttpClient();
            var responseMessage = await httpClient.SendAsync(message);
            string responseContent = string.Empty;
            if (responseMessage.StatusCode == HttpStatusCode.OK &&
                responseMessage.Content != null)
            {
                responseContent = await responseMessage.Content.ReadAsStringAsync();
            }
            return responseContent;
        }


        public async Task<HttpResponseMessage> InitiateSignIn()
        { 
            var httpMethod = "POST";
            var baseUrl = RequestTokenUrl;
            var callbackUrl = AppCallbackUrl;
            var twitterOauthParams = new Dictionary<string, string>()
            {
                {TwitterOauthUtils.OAuthCallbackKey, callbackUrl},
                {TwitterOauthUtils.OAuthConsumerKeyKey,  consumer_key},
                {TwitterOauthUtils.OAuthNonceKey, TwitterOauthUtils.GenerateNonce() },
                {TwitterOauthUtils.OAuthSignatureMethodKey, TwitterOauthUtils.OAuthSignatureMethod },
                {TwitterOauthUtils.OAuthTimestampKey, TwitterOauthUtils.UnixTimeStamp() },
                {TwitterOauthUtils.OAuthVersionKey, TwitterOauthUtils.OAuthVersion }
            };
            var twitterQueryParams = new Dictionary<string, string>();
            var twitterPostParams = new Dictionary<string, string>();
            var oauthSignature = TwitterOauthUtils.CreateSignature(httpMethod,
                baseUrl,
                twitterQueryParams,
                twitterPostParams,
                twitterOauthParams,
                consumer_secret,
                null);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            message.Headers.Add("Accept", "*/*");
            message.Headers.Add("User-Agent", "weatherama-client-1.0");
            message.Headers.Add("Host", "api.twitter.com");

            var oauthParamsList = new List<string>();
            foreach(var kvp in twitterOauthParams)
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
                var responseTokens = TwitterOauthUtils.SplitOauthResponse(responseContent);
                // next leg of the journey
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                response.Headers.Location = new System.Uri(string.Format("{0}?{1}={2}", OauthAuthenticateUrl, TwitterOauthUtils.OAuthTokenKey, responseTokens[TwitterOauthUtils.OAuthTokenKey]));
                return response;
            }

            return Request.CreateResponse(HttpStatusCode.Unauthorized);


        }
    }
}
