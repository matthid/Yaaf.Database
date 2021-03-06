﻿// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Database
open System
open System.Collections.Generic
open System.Data.Entity
open System.Linq
open System.Text
open System.Threading.Tasks

type MyDropCreateDatabaseIfModelChanges<'TContext when 'TContext :> DbContext > (seedMethod : 'TContext -> unit) =
    inherit DropCreateDatabaseIfModelChanges<'TContext>()
    
    default x.Seed (context:'TContext) =
        base.Seed (context);
        seedMethod(context)

type MyCreateDatabaseIfNotExists<'TContext when 'TContext :> DbContext > (seedMethod : 'TContext -> unit) =
    inherit CreateDatabaseIfNotExists<'TContext>()
    
    override x.Seed (context:'TContext) =
        base.Seed (context)
        seedMethod(context)

type NUnitInitializer<'TContext when 'TContext :> DbContext > () as x =
    let changer = new MyDropCreateDatabaseIfModelChanges<'TContext> (x.Seed);
    let creator = new MyCreateDatabaseIfNotExists<'TContext> (x.Seed);

    abstract Seed : 'TContext -> unit
    default x.Seed (context:'TContext) = ()

    abstract InitializeDatabase : 'TContext -> unit
    default x.InitializeDatabase (context:'TContext) =
        try
            changer.InitializeDatabase (context);
        with
        | :? NotSupportedException ->
            // Database not existent
            creator.InitializeDatabase (context)

    interface IDatabaseInitializer<'TContext> with
      member x.InitializeDatabase c = x.InitializeDatabase c