﻿using System;
using System.Net.Http;
using System.Threading;
using Reddit;

namespace channelbot_2
{
    /* TODO
     * [] - Add HMAC to pubsubhubbub
     * [X] - Add reddit "listing" support
     * [X] - GET oauth.reddit.com/message/unread endpoint for getting all unread messages
     * [X] - POST oauth.reddit.com/api/read_message 
     * [] - Convert channel name to channel id in reddit PMs, atm only channel_id field is supported
     * [] - Pubsubhubbub listen to OnAdd and OnRemove events from reddit and add subscription and remove subscription
     * [] - Reddit listen to Pubsubhubbub for incoming notification (and post it to reddit)
     */
    internal class Program
    {
        public static ManualResetEvent QuitEvent = new ManualResetEvent(false);
        public static HttpClient HttpClient = new HttpClient();
        public static Reddit reddit;

        private static void Main(string[] args)
        {
            //Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Starting ChannelBot2");
            Console.CancelKeyPress += (sender, eArgs) => {
                QuitEvent.Set();
                eArgs.Cancel = true;
            };
            Console.ForegroundColor = ConsoleColor.White;

            // Rainbows!!
            Console.ForegroundColor = ConsoleColor.Cyan;
            // Setup RedditToken for use in polling etc.
            var redditTokenManager = new RedditTokenManager();
            redditTokenManager.Start();
            Console.WriteLine("Initialized reddit token manager..");

            // Setup Reddit Client
            var redditAPI = new RedditAPI(accessToken:RedditTokenManager.CurrentToken);
            using (var reddit = new Reddit(redditAPI))
            {
                Program.reddit = reddit;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Initialized reddit logic..");
                reddit.MonitorUnreadPMs();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Monitoring reddit PMs every 30sec..");

                //Start polling
                var pollManager = new PollManager();
                pollManager.Start();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Initialized backup poller for sending out posts to subreddits");

                // Start pubsubhubbub, call dispose on it to remove listeners from event
                using (var hubbub = new PubSubHubBub())
                {
                    hubbub.Start();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[SUPERDEBUG] Setup pubsubhubbub TCP listener..\r\n");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    QuitEvent.WaitOne();
                }
            } 
        }
    }
}
