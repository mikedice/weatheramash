using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using weatheramaSH.Utilities;
using System.Collections.Generic;

namespace weatheramaTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var timeStamp = TwitterOauthUtils.UnixTimeStamp();
            var nonce = TwitterOauthUtils.GenerateNonce();

            var noEncode = "0123ABCabc.-_~";
            var pe1 = TwitterOauthUtils.PercentEncode(noEncode);
            Assert.AreEqual(noEncode, pe1);

            var isEncode = "0123 abcd";
            var pe2 = TwitterOauthUtils.PercentEncode(isEncode);
            Assert.AreEqual(pe2, "0123%20abcd");

            var hexEncode = "/0123 abcd/";
            var pe3 = TwitterOauthUtils.PercentEncode(hexEncode);
            Assert.AreEqual(pe3, "%2F0123%20abcd%2F");

            // using test parameters from https://dev.twitter.com/oauth/overview/creating-signatures
            var requestParams = new Dictionary<string, string>()
            {
                {"include_entities", "true" }
            };
            var postParams = new Dictionary<string, string>()
            {
                {"status", "Hello Ladies + Gentlemen, a signed OAuth request!" }
            };
            var oauthParams = new Dictionary<string, string>()
            {
                { TwitterOauthUtils.OAuthConsumerKeyKey, "xvz1evFS4wEEPTGEFPHBog" },
                { TwitterOauthUtils.OAuthTokenKey, "370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb" },
                { TwitterOauthUtils.OAuthNonceKey, "kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg" },
                { TwitterOauthUtils.OAuthSignatureMethodKey, TwitterOauthUtils.OAuthSignatureMethod },
                { TwitterOauthUtils.OAuthTimestampKey, "1318622958" },
                { TwitterOauthUtils.OAuthVersionKey, TwitterOauthUtils.OAuthVersion }
            };
            var pe4 = TwitterOauthUtils.GenerateParameterString(requestParams, postParams, oauthParams);
            Assert.AreEqual(pe4, "include_entities=true&oauth_consumer_key=xvz1evFS4wEEPTGEFPHBog&oauth_nonce=kYjzVBB8Y0ZFabxSWbWovY3uYSQ2pTgmZeNu2VS4cg&oauth_signature_method=HMAC-SHA1&oauth_timestamp=1318622958&oauth_token=370773112-GmHxMAgYyLbNEtIKZeRNFsMKPR9EyMZeS9weJAEb&oauth_version=1.0&status=Hello%20Ladies%20%2B%20Gentlemen%2C%20a%20signed%20OAuth%20request%21");


            var signature = TwitterOauthUtils.CreateSignature("POST",
                "https://api.twitter.com/1/statuses/update.json",
                requestParams,
                postParams,
                oauthParams,
                "kAcSOqF21Fu85e7zjz7ZN2U4ZRhfV3WpwPAoE3Z7kBw",
                "LswwdoUaIvS8ltyTt5jkRh4J50vUPVVHtR2YPi5kE");
            Assert.AreEqual(signature, "tnnArxj06cWHq44gCs1OSKk/jLY=");




        }

        [TestMethod]
        public void TestMethod2()
        {
            var requestParams = new Dictionary<string, string>()
            {
                {"status", "hello world" }
            };
            var postParams = new Dictionary<string, string>();
            var oauthParams = new Dictionary<string, string>()
            {
                { TwitterOauthUtils.OAuthConsumerKeyKey, "IPR1vxhyueAMP5pmnLfWhpmC1" },
                { TwitterOauthUtils.OAuthTokenKey, "4326182834-ipjXPMLhihl567i9ytVxPdK9Ke82MPYAiIrE4tp" },
                { TwitterOauthUtils.OAuthNonceKey, "42539c9ff380a4042312cd1a10725d3c" },
                { TwitterOauthUtils.OAuthSignatureMethodKey, TwitterOauthUtils.OAuthSignatureMethod },
                { TwitterOauthUtils.OAuthTimestampKey, "1448755907" },
                { TwitterOauthUtils.OAuthVersionKey, TwitterOauthUtils.OAuthVersion }
            };
            var pe4 = TwitterOauthUtils.GenerateParameterString(requestParams, postParams, oauthParams);


            var signature = TwitterOauthUtils.CreateSignature("POST",
                "https://api.twitter.com/1.1/statuses/update.json",
                requestParams,
                postParams,
                oauthParams,
                "bGCTVpz3olFyjoOJP72vEVael345U6LOVBfQAqKQPoEZOIlJre",
                "IcMDfPnZJBcfCipr139RzlmXWxlWkTXJmA8i9Ezg7JDu2");
            Assert.AreEqual(TwitterOauthUtils.PercentEncode(signature), "vQgryM6FeUwMrdc4xCzCGlZYLHQ%3D");
         }
    }
}
