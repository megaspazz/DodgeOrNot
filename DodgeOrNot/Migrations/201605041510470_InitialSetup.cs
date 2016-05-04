namespace DodgeOrNot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        Region = c.String(nullable: false, maxLength: 128),
                        MatchID = c.Long(nullable: false),
                        RawJSON = c.String(),
                    })
                .PrimaryKey(t => new { t.Region, t.MatchID });
            
            CreateTable(
                "dbo.Summoners",
                c => new
                    {
                        Region = c.String(nullable: false, maxLength: 128),
                        SummonerName = c.String(nullable: false, maxLength: 128),
                        RawJSON = c.String(),
                    })
                .PrimaryKey(t => new { t.Region, t.SummonerName });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Summoners");
            DropTable("dbo.Matches");
        }
    }
}
