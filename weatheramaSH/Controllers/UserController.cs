using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using weatheramaSH.Utilities;
using System.Net;
using System.Linq;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace weatheramaSH.Controllers
{
    public class UserController : ApiController
    {
        public Task<HttpResponseMessage> Get()
        {
            UserInfo userInfo = null;
            UserAuthInfo userAuthInfo = null;
            var cookie = Request.Headers.GetCookies(TwitterSignInController.UserAccessCookieName).FirstOrDefault();
            if (cookie != null)
            {

                TwitterOauthUtils.ParseUserAccessCookie(cookie[TwitterSignInController.UserAccessCookieName].Value, out userInfo, out userAuthInfo);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            if (userInfo != null)
            {
                response.Content = new StringContent(
                    JsonConvert.SerializeObject(userInfo),
                    Encoding.UTF8,
                    "application/json");
            }

            return Task.FromResult(response);

        }
    }
}
