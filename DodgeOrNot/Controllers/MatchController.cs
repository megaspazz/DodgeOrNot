using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DodgeOrNot.Models;

using Newtonsoft.Json;

namespace DodgeOrNot.Controllers
{
    public class MatchController : Controller
    {
        // GET: Match
        public ActionResult Index()
        {
            return View();
        }

        // GET: Match/Test
		public string Test()
        {
            return "this is THE match C0ntr0ll3r!!11!! ";
        }

        public string GetHistory(string region, long summonerID, string matchType, int beginIndex, int endIndex)
        {
            Match[] matches = Match.GetMatchesFor(region, summonerID, matchType, beginIndex, endIndex);
            return JsonConvert.SerializeObject(matches);
        }

        public string GetResults(string region, long summonerID, string matchType, int beginIndex, int endIndex)
        {
            MatchResult[] matchResults = MatchResult.GetMatchResultsFor(region, summonerID, matchType, beginIndex, endIndex);
            return JsonConvert.SerializeObject(matchResults);
        }
    }
}