namespace DodgeOrNot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialDBSetup : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Matches",
                c => new
                    {
                        MatchID = c.Long(nullable: false, identity: true),
                        RawJSON = c.String(),
                    })
                .PrimaryKey(t => t.MatchID);
            
            CreateTable(
                "dbo.Summoners",
                c => new
                    {
                        SummonerName = c.String(nullable: false, maxLength: 128),
                        RawJSON = c.String(),
                    })
                .PrimaryKey(t => t.SummonerName);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Summoners");
            DropTable("dbo.Matches");
        }
    }
}
