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
            new APIKey("186f8fec-2a95-407b-ac01-13d1e060796b", 10, 10),    // BE GOOD TO ME (NA)
            new APIKey("206eb44e-a927-4132-8586-42b50842f9c0", 10, 10),    // Lt is L (NA)
            new APIKey("42366f1f-f941-4121-b586-b635e695a42e", 10, 10),    // iLoveSchoolgirls (NA)
            new APIKey("5897411f-2bfb-4f22-a36b-e85f9e0f5003", 10, 10),    // Katherine Zhao (NA)
            new APIKey("43c95ae1-918b-41b7-8c7e-8be545bc7ac9", 10, 10),    // Celestial Beauty (EUW)
            new APIKey("e7335df9-be3b-4179-a9ed-00ebc1871a68", 10, 10),    // LOCA PEOPLE (LAN)
            new APIKey("4ff85aed-001e-43a8-9dbe-42c3cb1f1598", 10, 10),    // DonaId J Trump (LAN)
        };

        private static readonly object _API_LOCK = new object();
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
                lock (_API_LOCK)
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
            lock (_API_LOCK)
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