using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TweetSharp;

namespace RotoMonster.Core.Libs
{
    public class TwitterLib
    {
        private readonly string consumerKey;
        private readonly string consumerSecret;
        private readonly string token;
        private readonly string tokenSecret;

        private TwitterService service;

        public TwitterLib(string consumerKey, string consumerSecret, string token, string tokenSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.token = token;
            this.tokenSecret = tokenSecret;

            service = new TwitterService(consumerKey, consumerSecret);
            service.AuthenticateWith(token, tokenSecret);
        }

        public long SendTweet(string tweet)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            SendTweetOptions options = new SendTweetOptions();
            options.Status = tweet;
            TwitterStatus status = service.SendTweet(options);

            return status.Id;
        }

        public void DeleteTweet(long tweetId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            DeleteTweetOptions options = new DeleteTweetOptions();
            options.Id = tweetId;
            service.DeleteTweet(options);
        }

        public void ReTweet(long tweetId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RetweetOptions options = new RetweetOptions();
            options.Id = tweetId;
            service.Retweet(options);
        }

        public void UnReTweet(long tweetId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            UnretweetOptions options = new UnretweetOptions();
            options.Id = tweetId;
            service.Unretweet(options);
        }

    }


}
