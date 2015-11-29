using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace weatheramaSH.Utilities
{
    public class UserInfo
    {
        public string ScreenName { get; set; }
        public string UserId { get; set; }
    }

    public class UserAuthInfo
    {
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public static class TwitterOauthUtils
    {
        private static Random random = new Random();
        private const int NonceMax = 32;
        public const string OAuthConsumerKeyKey = "oauth_consumer_key";
        public const string OAuthNonceKey = "oauth_nonce";
        public const string OAuthSignatureMethodKey = "oauth_signature_method";
        public const string OAuthTimestampKey = "oauth_timestamp";
        public const string OAuthVersionKey = "oauth_version";
        public const string OAuthCallbackKey = "oauth_callback";
        public const string OAuthSignatureKey = "oauth_signature";
        public const string OAuthSignatureMethod = "HMAC-SHA1";
        public const string OAuthTokenKey = "oauth_token";
        public const string OAuthVerifierKey = "oauth_verifier";
        public const string OAuthTokenSecretKey = "oauth_token_secret";
        public const string OAuthCallbackConfirmedKey = "oauth_callback_confirmed";
        public const string OAuthUserIdKey = "user_id";
        public const string OAuthUserScreenNameKey = "screen_name";
        public const string OAuthUserExpiresKey = "x_auth_expires";
        public const string OAuthVersion = "1.0";
        public static char[] AuthResponseSplit = new char[] { '&' };
        public static char[] AuthResponseTokenSplit = new char[] { '=' };

        public static string ConstructUserAccessCookieValue(string userAccessInfo)
        {
            var bytes = Encoding.ASCII.GetBytes(userAccessInfo);
            return Convert.ToBase64String(bytes);
        }

        public static void ParseUserAccessCookie(string cookieValue, out UserInfo userInfo, out UserAuthInfo userAuthInfo)
        {
            var bytes = Convert.FromBase64String(cookieValue);
            var cookie = Encoding.ASCII.GetString(bytes);
            var tokens = cookie.Split(AuthResponseSplit);
            string oauthToken = string.Empty;
            string oauthTokenSecret = string.Empty;
            string screenName = string.Empty;
            string userId  = string.Empty;

            foreach(var token in tokens)
            {
                var kvp = token.Split(AuthResponseTokenSplit);
                if (kvp[0].Equals(OAuthTokenKey))
                {
                    oauthToken = kvp[1];
                }
                else if (kvp[0].Equals(OAuthTokenSecretKey))
                {
                    oauthTokenSecret = kvp[1];
                }
                else if (kvp[0].Equals(OAuthUserIdKey))
                {
                    userId = kvp[1];
                }
                else if (kvp[0].Equals(OAuthUserScreenNameKey))
                {
                    screenName = kvp[1];
                }
            }

            userInfo = new UserInfo
            {
                ScreenName = screenName,
                UserId = userId
            };

            userAuthInfo = new UserAuthInfo
            {
                UserInfo = userInfo,
                OAuthToken = oauthToken,
                OAuthTokenSecret = oauthTokenSecret
            };
        }

        public static string GenerateNonce()
        {
            var chars = new List<char>();
            int count = 0;
            // 32 bytes of random data with no non-word characters
            do
            {
                char val = (char)random.Next(0, 255);
                if ((val >= '0' && val <= '9') ||
                    (val >= 'A' && val <= 'Z') ||
                    (val >= 'a' && val <= 'z'))
                {
                    chars.Add(val);
                    count++;
                }

            }while (count < NonceMax);
            return new string(chars.ToArray());
        }

        public static string UnixTimeStamp()
        {
            var result = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            var longVal = Convert.ToInt64(result);
            return longVal.ToString("G");
        }

        public static string PercentEncode(string input)
        {
            // per https://dev.twitter.com/oauth/overview/percent-encoding-parameters
            var output = new List<char>();
            for(int i=0; i<input.Length; i++)
            {
                char c = input[i];
                if ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '-' ||
                    c == '.' ||
                    c == '_' ||
                    c == '~')
                {
                    output.Add(c);
                }
                else
                {
                    output.Add('%');
                    output.Add(HexChar(c / 16));
                    output.Add(HexChar(c % 16));
                }
            }
            return new string(output.ToArray());
        }

        public static char HexChar(int c)
        {
            if (c < 10)
            {
                return (char)('0' + c);
            }
            else
            {
                return (char)('A' + c - 10);
            }
        }

        public static string CreateSignature(
            string httpMethod, 
            string baseUrl,
            IDictionary<string, string> queryParameters,
            IDictionary<string, string> bodyParameters,
            IDictionary<string, string> oauthParams,
            string consumerSecret,
            string tokenSecret)
        {
            var parameterString = GenerateParameterString(queryParameters, bodyParameters, oauthParams);

            string signatureBaseString = string.Format("{0}&{1}&{2}",
                httpMethod.ToUpperInvariant(),
                PercentEncode(baseUrl),
                PercentEncode(parameterString));

            var signingKeyBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentException("consumer secret must be supplied");
            }
            signingKeyBuilder.Append(PercentEncode(consumerSecret));
            signingKeyBuilder.Append("&");
            if (!string.IsNullOrEmpty(tokenSecret))
            {
                signingKeyBuilder.Append(PercentEncode(tokenSecret));
            }

            var signature = HmacSha1Sign(signingKeyBuilder.ToString(), signatureBaseString);


            return signature;
        }

        public static string HmacSha1Sign(string key, string message)
        {
            string result = string.Empty;
            var keyBytes = Encoding.ASCII.GetBytes(key);
            var messageBytes = Encoding.ASCII.GetBytes(message);
            using (var hmac = new HMACSHA1())
            {
                hmac.Key = keyBytes;
                var hash = hmac.ComputeHash(messageBytes);
                result = Convert.ToBase64String(hash);
            }

            return result;
        }

        public static string GenerateParameterString(IDictionary<string, string> queryParams, IDictionary<string,string> postParams, IDictionary<string,string> oauthParams)
        {
            SortedList<string, string> signatureParameters = new SortedList<string, string>();

            foreach (var kvp in queryParams)
            {
                signatureParameters.Add(PercentEncode(kvp.Key), PercentEncode(kvp.Value));
            }
            foreach (var kvp in postParams)
            {
                signatureParameters.Add(PercentEncode(kvp.Key), PercentEncode(kvp.Value));
            }
            foreach(var kvp in oauthParams)
            {
                signatureParameters.Add(PercentEncode(kvp.Key), PercentEncode(kvp.Value));
            }
            List<string> kvps = new List<string>();
            foreach (var para in signatureParameters)
            {
                kvps.Add(string.Format("{0}={1}", para.Key, para.Value));
            }
            return string.Join("&", kvps);
        }

        public static IDictionary<string,string> SplitOauthResponse(string responseString)
        {
            var result = new Dictionary<string, string>();
            var authTokens = responseString.Split(AuthResponseSplit);
            foreach(var authToken in authTokens)
            {
                var kvp = authToken.Split(AuthResponseTokenSplit);
                result.Add(kvp[0], kvp[1]);
            }
            return result;
        }
    }
}
