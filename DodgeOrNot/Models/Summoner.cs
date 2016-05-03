using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DodgeOrNot.Models
{
	public class Summoner : JSONObject
	{
		[Key, Column(Order = 1)]
		public string Region { get; set; }

		[Key, Column(Order = 1)]
		public string SummonerName { get; set; }

		public static Summoner[] FromRegionAndNames(string region, params string[] names)
		{
			//DodgeOrNotContext db = new DodgeOrNotContext();
			//Summoner summ = db.Summoners.Find(name);
			//if (summ != null)
			//{
			//	return summ;
			//}
			
			IEnumerable<string> safeNames = names.Select(x => FixName(x));
			string inputNames = string.Join(",", safeNames);
			string summURL = GetSummonerURL(region, names);
			WebClient wc = new WebClient();
			string json = wc.DownloadString(summURL);
			JObject jsObj = JObject.Parse(json);

			List<Summoner> summs = new List<Summoner>();
			foreach (var jp in jsObj) {
				summs.Add(new Summoner() {
					Region = region,
					SummonerName = FixName(jp.Key),
					RawJSON = jp.Value.ToString()
				});
			}
			return summs.ToArray();
		}

		// TODO: implement FixName method
		public static string FixName(string name)
		{
			return Regex.Replace(name, @"[^\w]", "").ToLowerInvariant();
		}

		public static string GetSummonerURL(string region, params string[] names)
		{
			IEnumerable<string> safeNames = names.Select(x => FixName(x));
			string inputNames = string.Join(",", safeNames);
			return string.Format("https://{0}.api.pvp.net/api/lol/{0}/v1.4/summoner/by-name/{1}?api_key={2}", region, inputNames, Global.API_KEY);
		}
	}
}