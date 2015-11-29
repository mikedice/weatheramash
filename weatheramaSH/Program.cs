using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using weatheramaSH.Utilities;
namespace weatheramaSH
{
    class Program
    {
        public static Dictionary<string, UserAuthInfo> userIdToUserAuthInfo = new Dictionary<string, UserAuthInfo>();
        
        public static void SetUserAuthInfo(UserAuthInfo userAuthInfo)
        {
            userIdToUserAuthInfo[userAuthInfo.UserInfo.ScreenName] = userAuthInfo;
        }

        public static UserAuthInfo GetUserAuthInfo(string screenName)
        {
            if (userIdToUserAuthInfo.ContainsKey(screenName))
            {
                return userIdToUserAuthInfo[screenName];
            }
            return null;
        }

        static void Main(string[] args)
        {
            var startOptions = new StartOptions();
            startOptions.Urls.Add("http://+:8001/");
            startOptions.Urls.Add("https://+:8000/");

            // Start OWIN host 
            using (WebApp.Start<Startup>(startOptions))
            {
                Console.WriteLine("Weatherama server is running");
                Console.WriteLine("Press <enter> to exit...");
                Console.ReadLine();
            }
        }
    }
}
