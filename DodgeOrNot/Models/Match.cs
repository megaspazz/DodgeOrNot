using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DodgeOrNot.Models
{
	public class Match : JSONObject
	{
		[Key]
		public long MatchID { get; set; }
	}
}