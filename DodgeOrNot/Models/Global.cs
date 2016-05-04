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
        
        public static readonly APIKeyCounter[] API_KEYS = new APIKey [] {
            // Paste all your keys here!  The app will not work without at least one key!
        }.Select(x => new APIKeyCounter(x)).ToArray();
        
        static Global()
        {
            Shuffle(API_KEYS);
        }

        private static int _API_IDX = 0;
        public static APIKeyCounter CurrentKeyCounter
        {
            get
            {
                return API_KEYS[_API_IDX];
            }
        }

        public static string UseCurrentKey()
        {
            APIKeyCounter curr = Global.CurrentKeyCounter;
            curr.Uses++;
            if (curr.Uses >= curr.Key.MaxUses)
            {
                RecycleCurrentKey();
            }
            return curr.Key.KeyString;
        }

        public static string CallAPI(string urlFrag)
        {
            using (WebClient wc = new WebClient())
            {
                while (true)
                {
                    try
                    {
                        var url = urlFrag + UseCurrentKey();
                        return wc.DownloadString(url);
                    }
                    catch
                    {
                        RecycleCurrentKey();
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

        private static void RecycleCurrentKey()
        {
            API_KEYS[_API_IDX].Uses = 0;
            _API_IDX = (_API_IDX + 1) % API_KEYS.Length;
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

    public class APIKeyCounter
    {
        public APIKey Key { get; set; }
        public int Uses { get; set; }

        public APIKeyCounter(APIKey key)
        {
            this.Key = key;
        }
    }

    public class APIKeyCountdown
    {
        public APIKey Key { get; set; }
        public DateTime RechargeTime { get; set; }

        public static APIKeyCountdown StartRecharge(APIKey key)
        {
            return new APIKeyCountdown()
            {
                Key = key,
                RechargeTime = DateTime.UtcNow.Add(new TimeSpan(0, 0, key.Cooldown))
            };
        }
    }
}