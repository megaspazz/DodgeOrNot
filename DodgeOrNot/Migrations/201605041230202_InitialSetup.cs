namespace DodgeOrNot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSetup : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Matches");
            DropPrimaryKey("dbo.Summoners");
            AddColumn("dbo.Matches", "Region", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Summoners", "Region", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Matches", "MatchID", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.Matches", new[] { "Region", "MatchID" });
            AddPrimaryKey("dbo.Summoners", new[] { "Region", "SummonerName" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Summoners");
            DropPrimaryKey("dbo.Matches");
            AlterColumn("dbo.Matches", "MatchID", c => c.Long(nullable: false, identity: true));
            DropColumn("dbo.Summoners", "Region");
            DropColumn("dbo.Matches", "Region");
            AddPrimaryKey("dbo.Summoners", "SummonerName");
            AddPrimaryKey("dbo.Matches", "MatchID");
        }
    }
}
