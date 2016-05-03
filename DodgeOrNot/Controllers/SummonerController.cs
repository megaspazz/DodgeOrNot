using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DodgeOrNot.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DodgeOrNot.Controllers
{
    public class SummonerController : Controller
    {
        // GET: Summoner
        public ActionResult Index()
        {
            return View();
        }

		public string Test()
		{
			return "sample text lorem ipsum whatever";
		}

		public string GetInfo(string region, string summonerNames)
		{
			string[] names = summonerNames.Split(',');
			Summoner[] summ = Summoner.FromRegionAndNames(region, names);
			return JsonConvert.SerializeObject(summ);
		}
    }
}