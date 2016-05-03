using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DodgeOrNot.Models
{
	public class DodgeOrNotContext : DbContext
	{
		public DbSet<Summoner> Summoners { get; set; }
		public DbSet<Match> Matches { get; set; }
	}
}