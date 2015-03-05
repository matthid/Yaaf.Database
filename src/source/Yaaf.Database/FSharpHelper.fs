// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------
namespace Yaaf.Database

open Microsoft.FSharp.Core
open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.Threading.Tasks

type FSharpHelper () =

    static member ToFSharpS<'T when 'T : (new : unit -> 'T) and 'T : struct and 'T :> ValueType> ( data:Nullable<'T>) =
        if (data.HasValue) then
            Some (data.Value)
        else
            None
      
    static member ToFSharp<'T when 'T : null> (data:'T) = 
        if obj.ReferenceEquals((data:'T), (null : 'T)) then
            None
        else
            Some (data)

    static member FromFSharpS<'T when 'T : (new : unit -> 'T) and 'T : struct and 'T :> ValueType> (data:'T option) =
        match data with
        | Some d -> Nullable(d)
        | None -> Nullable()
        
    static member FromFSharp<'T when 'T : null> (data: 'T option) : 'T =
        FSharpHelper.FromFSharp<'T>((data : 'T option), (null : 'T)) : 'T

    static member FromFSharp<'T> (data : 'T option, def : 'T) : 'T =
        match data with
        | Some d -> d
        | None -> def