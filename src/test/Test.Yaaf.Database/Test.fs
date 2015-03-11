// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Test.Yaaf.Database
open System.IO
open NUnit.Framework
open FsUnit
open Yaaf.Database
open Microsoft.AspNet.Identity.EntityFramework
open Microsoft.AspNet.Identity
open System.Data.Entity
open Swensen.Unquote

type MyUser () =
  inherit IdentityUser()

  member val Version : int = 0 with get, set

type AbstractTestDbContext (nameOrConnection, ?doInit) as x =
  inherit AbstractApplicationIdentityDbContext<MyUser>(nameOrConnection, false)
  let doInit = defaultArg doInit true
  do if doInit then x.DoInit()


type MSSQLTestDbContext () as x =
  inherit AbstractTestDbContext(MSSQLTestDbContext.ConnectionName, false)
  do x.DoInit()

  override x.Init() =
    System.Data.Entity.Database.SetInitializer(new NUnitInitializer<MSSQLTestDbContext>())
  //  System.Data.Entity.Database.SetInitializer(
  //    new MigrateDatabaseToLatestVersion<MSSQLTestDbContext, MSSQLConfiguration<MSSQLTestDbContext>>())
 
  static member ConnectionName
    with get () =  
      let env = System.Environment.GetEnvironmentVariable ("connection_mssql")
      if System.String.IsNullOrWhiteSpace env then "Test_MSSQL" else env

[<AbstractClass>]
type RawSQLTests () =
    
    abstract CreateContext : unit -> AbstractTestDbContext

    [<TestFixtureSetUp>]
    member x.FixtureSetup () =
      // Setup DataDirectory for databases
      System.AppDomain.CurrentDomain.SetData(
          "DataDirectory", 
          System.AppDomain.CurrentDomain.BaseDirectory)
      // Fix for some bug, see http://stackoverflow.com/questions/15693262/serialization-exception-in-net-4-5
      System.Configuration.ConfigurationManager.GetSection("dummy") |> ignore

    [<SetUp>]
    member x.Setup() =
      use c = x.CreateContext()
      c.Database.Delete() |> ignore
      c.SaveChanges() |> ignore

    [<Test>]
    member x.``can create and find user``() =
      (
        use c = x.CreateContext()
        c.Users.Add(new MyUser(UserName = "testuser", Version = 3)) |> ignore
        c.MySaveChanges() |> Async.RunSynchronously
      )
      
      (
        use c = x.CreateContext()
        let store = new UserStore<MyUser>(c)
        let manager = new UserManager<MyUser>(store)
        let user = manager.FindByName ("testuser")
        test <@ user.Version = 3 @>
      )
      

[<TestFixture>]
type ``MSSQL Test``() =
  inherit RawSQLTests()

  override x.CreateContext () = new MSSQLTestDbContext () :> _