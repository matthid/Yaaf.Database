// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Test.Yaaf.Database.MySQL
open System.IO
open NUnit.Framework
open FsUnit
open Test.Yaaf.Database
open Yaaf.Database.MySQL
open System.Data.Entity
open MySql.Data.Entity

type MySQLTestDbContext () as x =
  inherit AbstractTestDbContext(MySQLTestDbContext.ConnectionName, false)
  do x.DoInit()
  
  override x.Init() = 
    DbConfiguration.SetConfiguration (new MySqlEFConfiguration ());
    System.Data.Entity.Database.SetInitializer(
      new MigrateDatabaseToLatestVersion<MySQLTestDbContext, MySQLConfiguration<MySQLTestDbContext>>())
 
  static member ConnectionName
    with get () =  
      let env = System.Environment.GetEnvironmentVariable ("connection_mysql")
      if System.String.IsNullOrWhiteSpace env then "Test_MySQL" else env

[<TestFixture>]
type ``MySQL Test``() =
  inherit RawSQLTests()

  override x.CreateContext () = new MySQLTestDbContext () :> _