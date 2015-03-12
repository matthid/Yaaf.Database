// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Database
open Microsoft.AspNet.Identity.EntityFramework
open System
open System.Collections.Generic
open System.Data.Entity
open System.Data.Entity.Migrations
open System.Data.Entity.Core.Objects
open System.Data.Entity.Infrastructure
open System.Linq
open System.Text
open System.Threading
open System.Threading.Tasks
open Yaaf.Helper

[<AbstractClass>]
type AbstractApplicationIdentityDbContext<'TUser when 'TUser :> IdentityUser >(nameOrConnectionString) as x =
    inherit IdentityDbContext<'TUser>(nameOrConnectionString : string)           
    static do
        if (String.IsNullOrWhiteSpace (AppDomain.CurrentDomain.GetData ("DataDirectory") :?> string)) then
            System.AppDomain.CurrentDomain.SetData (
                "DataDirectory",
                System.AppDomain.CurrentDomain.BaseDirectory)
    interface IUpgradeDatabaseProvider with
      member x.GetMigrator() = x.GetMigrator()
      
      member x.FixScript s = x.FixScript s
      
    abstract FixScript : string -> string
    default x.FixScript s = s

    abstract GetMigrator : unit -> DbMigrator
    default x.GetMigrator () = DatabaseUpgrade.notImpl ()

    member x.MySaveChanges () =
        AbstractApplicationDbContext.MySaveChanges (x)    
