using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Web;

using Newtonsoft.Json.Linq;

namespace DodgeOrNot.Models
{
	public class Match : JSONObject
    {
        [Key, Column(Order = 0)]
        public string Region { get; set; }

        [Key, Column(Order = 1)]
        public long MatchID { get; set; }

        public static string GetMatchReferenceURL(string region, long summonerID, string matchType, int beginIndex, int endIndex)
        {
            return string.Format("https://{0}.api.pvp.net/api/lol/{0}/v2.2/matchlist/by-summoner/{1}?rankedQueues={2}&beginIndex={3}&endIndex={4}&api_key={5}", region, summonerID, matchType, beginIndex, endIndex, Global.API_KEY);
        }

        public static string GetMatchURL(string region, long matchID, bool includeTimeline)
        {
            return string.Format("https://{0}.api.pvp.net/api/lol/{0}/v2.2/match/{1}?includeTimeline={2}&api_key={3}", region, matchID, includeTimeline, Global.API_KEY);
        }

        public static Match GetMatch(string region, long matchID)
        {
            DodgeOrNotContext db = new DodgeOrNotContext();
            Match m = db.Matches.Find(region, matchID);
            if (m == null)
            {
                WebClient wc = new WebClient();
                string matchURL = GetMatchURL(region, matchID, false);
                string json = wc.DownloadString(matchURL);
                m = new Match()
                {
                    Region = region.ToLowerInvariant(),
                    MatchID = matchID,
                    RawJSON = json
                };
                db.Matches.Add(m);
                db.SaveChanges();
            }
            return m;
        }

        public static Match[] GetMatchesFor(string region, long summonerID, string matchType, int beginIndex, int endIndex)
        {
            WebClient wc = new WebClient();
            string matchRefURL = GetMatchReferenceURL(region, summonerID, matchType, beginIndex, endIndex);
            string json = wc.DownloadString(matchRefURL);
            JObject jsObj = JObject.Parse(json);
            List<Match> matches = new List<Match>();
            foreach (var matchObj in jsObj["matches"])
            {
                long matchID = matchObj["matchId"].ToObject<long>();
                Match m = GetMatch(region, matchID);
                matches.Add(m);
            }
            return matches.ToArray();
        }
	}

    public static class MatchType
    {
        public static readonly string TEAM_BUILDER_DRAFT_RANKED_5x5 = "TEAM_BUILDER_DRAFT_RANKED_5x5";
        public static readonly string RANKED_SOLO_5x5 = "RANKED_SOLO_5x5";
        public static readonly string RANKED_TEAM_3x3 = "RANKED_TEAM 3x3";
    }

    public class MatchResult
    {
        public long Kills { get; set; }
        public long Deaths { get; set; }
		public long Assists { get; set; }
		public double Participation { get; set; }
        public bool Victory { get; set; }

        public static MatchResult FromMatch(Match match, long summonerID)
        {
            JObject jsObj = JObject.Parse(match.RawJSON);
            int partID = 0;
            foreach (var identityObj in jsObj["participantIdentities"])
            {
                if (identityObj["player"]["summonerId"].ToObject<long>() == summonerID)
                {
                    partID = identityObj["participantId"].ToObject<int>();
                    break;
                }
            }
            if (partID == 0)
            {
                return null;
			}
			MatchResult mr = new MatchResult();
			int teamID = 0;
			Dictionary<int, long> teamKills = new Dictionary<int, long>();
            foreach (var partObj in jsObj["participants"])
			{
				int currTeam = partObj["teamId"].ToObject<int>();
				var statsObj = partObj["stats"];
				long playerKills = statsObj["kills"].ToObject<long>();
				long prev;
				teamKills.TryGetValue(currTeam, out prev);
				teamKills[currTeam] = prev + playerKills;
                if (partObj["participantId"].ToObject<int>() == partID)
				{
					teamID = currTeam;
                    mr.Kills = playerKills;
					mr.Deaths = statsObj["deaths"].ToObject<long>();
                    mr.Assists = statsObj["assists"].ToObject<long>();
                    mr.Victory = statsObj["winner"].ToObject<bool>();
                }
            }
			mr.Participation = (double)(mr.Kills + mr.Assists) / teamKills[teamID];
			return mr;
        }

        public static MatchResult[] GetMatchResultsFor(string region, long summonerID, string matchType, int beginIndex, int endIndex)
        {
            Match[] matches = Match.GetMatchesFor(region, summonerID, matchType, beginIndex, endIndex);
            return matches.Select(x => MatchResult.FromMatch(x, summonerID)).ToArray();
        }
    }

    //public class MatchHistory
    //{
    //    public Summoner SummonerInfo { get; set; }
    //    public MatchResult[] Matches { get; set; }

    //    public static MatchHistory FromSummonerID(string region, long summonerID, int recentGames)
    //    {
    //        return new MatchHistory
    //    }
    //}
}