// ----------------------------------------------------------------------------
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

type EmptyConfiguration() =
    inherit DbConfiguration()
    do
        ()
        //SetDefaultConnectionFactory (new LocalDbConnectionFactory ("v11.0"));


type MSSQLConfiguration<'T when 'T :> DbContext>() as x =
  inherit System.Data.Entity.Migrations.DbMigrationsConfiguration<'T>()

  do
    x.AutomaticMigrationsEnabled <- true

  override x.Seed(context) = ()
