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
open System.Data.Entity.Migrations.Infrastructure
open System.Data.Entity.Core.Objects
open System.Data.Entity.Infrastructure
open System.Linq
open System.Text
open System.Threading
open System.Threading.Tasks
open Yaaf.Helper

/// This error indicates that the sending pipeline was already closed, so sending is not longer possible (IE the closing element </stream> was already sent!)
[<System.Serializable>]
type DatabaseUpgradeException =     
    inherit System.Exception
    val mutable private scriptErr :exn
    val mutable private upgradeScript :string
    new (msg : string) = { 
      inherit System.Exception(msg); scriptErr = null; upgradeScript = null }
    new (msg:string, inner:System.Exception) = { 
      inherit System.Exception(msg, inner); scriptErr = null; upgradeScript = null }
    new (info:System.Runtime.Serialization.SerializationInfo, context:System.Runtime.Serialization.StreamingContext) = {
        inherit System.Exception(info, context); scriptErr = null; upgradeScript = null
    }
    member x.ScriptGenerationException with get () = x.scriptErr and set v = x.scriptErr <- v
    member x.UpgradeScript with get () = x.upgradeScript and set v = x.upgradeScript <- v


type IUpgradeDatabaseProvider =
    abstract GetMigrator : unit -> DbMigrator
    abstract FixScript : string -> string

//[<System.Runtime.CompilerServices.Extension>]
module DatabaseUpgrade =
  let internal notImpl () =
        (raise <| System.NotSupportedException("Migration is not supported by this type, please implement GetMigrator."))
        : 'a
  let FixScript_MSSQL (script:string) = script
  let FixScript_MySQL (script:string) =
    script.Replace(
      "from information_schema.columns where", 
      "FROM information_schema.columns WHERE table_schema = SCHEMA() AND")
 
  //[<System.Runtime.CompilerServices.Extension>]
  let Upgrade (provider:IUpgradeDatabaseProvider) =
    let migrator =
      try provider.GetMigrator()
      with e -> raise <| new DatabaseUpgradeException(sprintf "Failed to ugprade database: %s" e.Message, e)
    try
      migrator.Update()
    with exn ->
      let f = new DatabaseUpgradeException(sprintf "Failed to ugprade database: %s" exn.Message, exn)
      try
        let scriptor = new MigratorScriptingDecorator(migrator)
        let script = scriptor.ScriptUpdate(sourceMigration = null, targetMigration = null)
        f.UpgradeScript <- provider.FixScript script
      with scriptErr ->
        f.ScriptGenerationException <- scriptErr
      raise f

[<AutoOpen>]
module UpgradeDatabaseProviderExtensions =
  type IUpgradeDatabaseProvider with
    member x.Upgrade() =
     DatabaseUpgrade.Upgrade x 

[<AbstractClass>]
type AbstractApplicationDbContext(nameOrConnectionString) =
    inherit DbContext(nameOrConnectionString : string)
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
    static member MySaveChanges (context: DbContext) =
      async {
        let saved = ref false
        while (not !saved) do
            let concurrentError = ref null;
            try
                do! context.SaveChangesAsync () |> Task.await |> Async.Ignore
                saved := true
            with 
            | :? DbUpdateConcurrencyException as e ->
                concurrentError := e
            //| :? DbUpdateException as e ->
            //    // Log error?
            //    raise e
            if (!concurrentError <> null) then
                let e = !concurrentError
                Console.Error.WriteLine ("DbUpdateConcurrencyException: {0}", e)
                for entry in e.Entries do
                    match (entry.State) with
                    | EntityState.Deleted ->
                        //deleted on client, modified in store
                        do! entry.ReloadAsync () |> Task.awaitPlain
                        entry.State <- EntityState.Deleted
                    | EntityState.Modified ->
                        let currentValues = entry.CurrentValues.Clone ();
                        do! entry.ReloadAsync () |> Task.awaitPlain
                        if (entry.State = EntityState.Detached) then
                            //Modified on client, deleted in store
                            context.Set(ObjectContext.GetObjectType(entry.Entity.GetType ())).Add(entry.Entity)
                            |> ignore
                        else
                        //Modified on client, Modified in store
                            do! entry.ReloadAsync () |> Task.awaitPlain
                            entry.CurrentValues.SetValues(currentValues)
                    | _ ->
                        //For good luck
                        do! entry.ReloadAsync () |> Task.awaitPlain
      } |> Async.StartAsTaskImmediate