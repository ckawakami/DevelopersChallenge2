namespace Nibo_Full_Stack_Developers_Challenge___Level_2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FileDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BankId = c.Int(nullable: false),
                        AccountNumber = c.Long(nullable: false),
                        AccountType = c.String(),
                        DateStart = c.DateTime(nullable: false),
                        DateEnd = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TransactionDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionType = c.Int(nullable: false),
                        DatePosted = c.DateTime(nullable: false),
                        TransactionAmmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Memo = c.String(),
                        FileDataId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FileDatas", t => t.FileDataId, cascadeDelete: true)
                .Index(t => t.FileDataId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionDetails", "FileDataId", "dbo.FileDatas");
            DropIndex("dbo.TransactionDetails", new[] { "FileDataId" });
            DropTable("dbo.TransactionDetails");
            DropTable("dbo.FileDatas");
        }
    }
}
