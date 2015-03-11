namespace Yaaf.Database.MySQL

open System.Data.Entity

type MySqlHistoryContext(con, scheme) = 
  inherit System.Data.Entity.Migrations.History.HistoryContext(con, scheme)
  override x.OnModelCreating(modelBuilder) = 
    base.OnModelCreating(modelBuilder)
    modelBuilder.Entity<System.Data.Entity.Migrations.History.HistoryRow>().Property(fun h -> h.MigrationId)
      .HasMaxLength(System.Nullable(100)).IsRequired() |> ignore
    modelBuilder.Entity<System.Data.Entity.Migrations.History.HistoryRow>().Property(fun h -> h.ContextKey)
      .HasMaxLength(System.Nullable(200)).IsRequired() |> ignore

type MySQLConfiguration<'T when 'T :> DbContext>() as x = 
  inherit System.Data.Entity.Migrations.DbMigrationsConfiguration<'T>()
  
  do 
    x.CodeGenerator <- new MySql.Data.Entity.MySqlMigrationCodeGenerator()
    x.SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator())
    x.SetHistoryContextFactory("MySql.Data.MySqlClient", fun conn schema -> new MySqlHistoryContext(conn, schema) :> _)
    x.AutomaticMigrationsEnabled <- true
  
  override x.Seed(context) = ()
