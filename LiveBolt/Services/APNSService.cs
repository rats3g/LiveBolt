using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;

namespace LiveBolt.Services
{
    public class APNSService : IAPNSService
    {
        public void SendPushNotifications(IEnumerable<string> deviceTokens, JObject payload)
        {
            // Configuration (NOTE: .pfx can also be used here)
            var config = new ApnsConfiguration (ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                "Certificates.p12", "livebolt1896!");

            // Create a new broker
            var apnsBroker = new ApnsServiceBroker (config);

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) => {

                aggregateEx.Handle (ex => {

                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException) {
                        var notificationException = (ApnsNotificationException)ex;

                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine ($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                    } else {
                        // Inner exception might hold more useful information like an ApnsConnectionException
                        Console.WriteLine ($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                Console.WriteLine ("Apple Notification Sent!");
            };

            // Start the broker
            apnsBroker.Start ();

            foreach (var deviceToken in deviceTokens) {
                // Queue a notification to send
                apnsBroker.QueueNotification (new ApnsNotification {
                    DeviceToken = deviceToken,
                    Payload = payload
                });
            }

            // Stop the broker, wait for it to finish
            // This isn't done after every message, but after you're
            // done with the broker
            apnsBroker.Stop ();
        }
    }
}