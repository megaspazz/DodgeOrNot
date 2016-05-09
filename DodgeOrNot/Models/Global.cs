using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace DodgeOrNot.Models
{
    public static class Global
    {
        public static Random RNG = new Random();

        public static readonly APIKey[] API_KEYS = {
            // Put your API keys here.
            // The app will crash if there are no keys!
        };

        private static readonly object _apiLock = new object();
        private static SortedSet<APIKeyUse> _API_KEY_PQ;
        static Global()
        {
            List<APIKeyUse> lst = new List<APIKeyUse>();
            foreach (APIKey key in API_KEYS)
            {
                for (int i = 0; i < key.MaxUses; i++)
                {
                    lst.Add(new APIKeyUse()
                    {
                        Key = key,
                        NextUse = DateTime.UtcNow
                    });
                }
            }
            APIKeyUse[] arr = lst.ToArray();
            Shuffle(arr);
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].KeyID = i;
            }
            _API_KEY_PQ = new SortedSet<APIKeyUse>(arr);
        }
        
        public static APIKeyUse CurrentKeyUse
        {
            get
            {
                lock (_apiLock)
                {
                    return _API_KEY_PQ.First();
                }
            }
        }

        public static APIKeyUse WaitCurrentKey()
        {
            APIKeyUse curr = Global.CurrentKeyUse;
            TimeSpan dt = curr.NextUse - DateTime.UtcNow;
            if (dt.Ticks > 0)
            {
                Thread.Sleep(dt);
            }
            return curr;
        }

        private static readonly double _RATE_LIMIT_WAIT_SECS = 5;
        public static string CallAPI(string urlFrag)
        {
            using (WebClient wc = new WebClient())
            {
                while (true)
                {
                    APIKeyUse curr = WaitCurrentKey();
                    string url = urlFrag + curr.Key.KeyString;
                    try
                    {
                        string response = wc.DownloadString(url);
                        CycleCurrentKey(curr, curr.Key.Cooldown);
                        return response;
                    }
                    catch (WebException ex)
                    {
                        HttpStatusCode statusCode = ((HttpWebResponse)ex.Response).StatusCode;
                        if ((int)statusCode == 429)
                        {
                            CycleCurrentKey(curr, _RATE_LIMIT_WAIT_SECS);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

        }

        public static void Swap<T>(T[] arr, int i, int j)
        {
            T tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        public static void Shuffle<T>(T[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                int idx = RNG.Next(i, arr.Length);
                Swap(arr, i, idx);
            }
        }

        private static void CycleCurrentKey(APIKeyUse curr, double seconds)
        {
            lock (_apiLock)
            {
                _API_KEY_PQ.Remove(curr);
                curr.NextUse = DateTime.UtcNow.AddSeconds(seconds);
                _API_KEY_PQ.Add(curr);
            }
        }

        private static void CycleCurrentKey(double seconds)
        {
            CycleCurrentKey(Global.CurrentKeyUse, seconds);
        }
    }

    public class APIKey
    {
        public string KeyString { get; set; }
        public int Cooldown { get; set; }
        public int MaxUses { get; set; }

        public APIKey(string key, int cd, int uses)
        {
            this.KeyString = key;
            this.Cooldown = cd;
            this.MaxUses = uses;
        }
    }

    //public class APIKeyCounter
    //{
    //    public APIKey Key { get; set; }
    //    public int Uses { get; set; }

    //    public APIKeyCounter(APIKey key)
    //    {
    //        this.Key = key;
    //    }
    //}

    //public class APIKeyCountdown
    //{
    //    public APIKey Key { get; set; }
    //    public DateTime RechargeTime { get; set; }

    //    public static APIKeyCountdown StartRecharge(APIKey key)
    //    {
    //        return new APIKeyCountdown()
    //        {
    //            Key = key,
    //            RechargeTime = DateTime.UtcNow.Add(new TimeSpan(0, 0, key.Cooldown))
    //        };
    //    }
    //}

    public class APIKeyUse : IComparable<APIKeyUse>
    {
        public int KeyID { get; set; }
        public APIKey Key { get; set; }
        public DateTime NextUse { get; set; }

        public int CompareTo(APIKeyUse other)
        {
            int dlu = this.NextUse.CompareTo(other.NextUse);
            if (dlu != 0)
            {
                return dlu;
            }
            return this.KeyID.CompareTo(other.KeyID);
        }
    }
}