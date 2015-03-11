// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Database

open Microsoft.AspNet.Identity.EntityFramework
open System
open System.Collections.Generic
open System.Data.Entity
open System.Data.Entity.Core.Objects
open System.Data.Entity.Infrastructure
open System.Linq
open System.Text
open System.Threading
open System.Threading.Tasks
open Yaaf.Helper

type AbstractApplicationIdentityDbContext<'TUser when 'TUser :> IdentityUser >(nameOrConnectionString, ?doInit) as x =
    inherit IdentityDbContext<'TUser>(nameOrConnectionString : string)
    let doInit = defaultArg doInit true
           
    static do
        if (String.IsNullOrWhiteSpace (AppDomain.CurrentDomain.GetData ("DataDirectory") :?> string)) then
            System.AppDomain.CurrentDomain.SetData (
                "DataDirectory",
                System.AppDomain.CurrentDomain.BaseDirectory)
    do if doInit then x.DoInit()


    abstract Init : unit -> unit
    default x.Init () = ()
    
    member x.DoInit () =
        x.Init ()
        x.Database.Initialize (false)
        
    member x.MySaveChanges () =
        AbstractApplicationDbContext.MySaveChanges (x)    
