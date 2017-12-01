using System;
using System.Linq;
using LiveBolt.Models;
using Newtonsoft.Json.Linq;

namespace LiveBolt.Services
{
    public class MLService : IMLService
    {
        private readonly IAPNSService _apns;

        public MLService(IAPNSService apns)
        {
            _apns = apns;
        }

        public void checkHomeStatus(Home home)
        {
            var predictedDoorLockPercentage = predictDoorLockPercentage(home);
            if (predictedDoorLockPercentage - (home.DLMs.Count(dlm => dlm.IsLocked) / home.DLMs.Count) > 0.25) {
                _apns.SendPushNotifications(home.Users.Where(user => user.DeviceToken != null).Select(user => user.DeviceToken), JObject.Parse("{'aps':{'alert':{'title': 'Home Alert','body': 'Home is in an unsafe state. Would you like to lock your doors?'},'badge':1,'sound':'default','category': 'ML_CATEGORY'}}"));
            }
        }

        private double predictDoorLockPercentage(Home home)
        {
            var date = DateTime.Now;
            var hour = date.Hour;
            int day;
            if (date.DayOfWeek == DayOfWeek.Sunday) {
                day = 6;
            }
            else {
                day = ((int)date.DayOfWeek) - 1;
            }
            var percentHome = percentUsersHome(home);

            return mlPrediction(hour, day, percentHome);
        }

        private double percentUsersHome(Home home)
        {
            return home.Users.Count(user => user.IsHome) / (double)home.Users.Count();
        }

        private double mlPrediction(int hour, int dayOfWeek, double usersHomePercent)
        {
            var syn0 = new [,] {
                {-5.33674485926,3.9656804362,7.67747512324,-2.94981029868},
                {7.83722640323,1.51991746848,-17.1982008467,-0.125585563158},
                {27.3282051396,1.35523463826,-20.9423295731,28.1231845701}
            };

            var syn1 = new[] {
                1.10473589441, -0.292766542878, 1.79943479126, 3.77375818356
            };

            double[] l1 = new double[4];
            for (int i = 0; i < 4; i++)
            {
                l1[i] = hour * syn0[0, i] + dayOfWeek * syn0[1, i] + usersHomePercent * syn0[2, i];
            }

            for (int i = 0; i < l1.Length; i++)
            {
                l1[i] = 1 / (1 + Math.Exp(-l1[i]));
            }

            double l2 = 0;
            for (int i = 0; i < l1.Length; i++)
            {
                l2 += l1[i] * syn1[i];
            }

            return 1 / (1 + Math.Exp(-l2));
        }
    }
}