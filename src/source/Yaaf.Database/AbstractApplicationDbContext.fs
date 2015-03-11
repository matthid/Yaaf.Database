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

type IUpgradeDatabaseProvider =
    abstract GetMigrator : unit -> DbMigrator

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

    abstract GetMigrator : unit -> DbMigrator
    default x.GetMigrator () =
        AbstractApplicationDbContext.MigratorNotImplemented()

    member x.MySaveChanges () =
        AbstractApplicationDbContext.MySaveChanges (x)    
 
    static member internal MigratorNotImplemented() =
        raise <| System.NotSupportedException("Migration is not supported by this type, please implement GetMigrator.")

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