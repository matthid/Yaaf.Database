// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the configuration of the Yaaf.AdvancedBuilding build script.

    The first step is handled in build.sh and build.cmd by restoring either paket dependencies or bootstrapping a NuGet.exe and 
    executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE).

    The secound step is executing build.fsx which loads this file (for configuration), builds the solution and executes all unit tests.
*)

#if FAKE
#else
// Support when file is opened in Visual Studio
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#endif

open BuildConfigDef
open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"

let buildConfig =
 // Read release notes document
 let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
 { BuildConfiguration.Defaults with
    ProjectName = "Yaaf.Database"
    CopyrightNotice = "Yaaf.Database Copyright © Matthias Dittrich 2015"
    ProjectSummary = "Some helpers for projects using the EntityFramework with multiple databases."
    ProjectDescription = "Some helpers for projects using the EntityFramework with multiple databases."
    ProjectAuthors = ["Matthias Dittrich"]
    NugetTags =  "fsharp csharp database"
    PageAuthor = "Matthias Dittrich"
    GithubUser = "matthid"
    Version = release.NugetVersion
    NugetPackages =
      [ "Yaaf.Database.nuspec", (fun config p ->
          { p with
              Version = config.Version
              ReleaseNotes = toLines release.Notes
              Dependencies = 
                [ "EntityFramework"
                  "Microsoft.AspNet.Identity.Core"
                  "Microsoft.AspNet.Identity.EntityFramework"
                  "FSharp.Core" ]
                  |> List.map (fun name -> name, (GetPackageVersion "packages" name)) })
        "Yaaf.Database.MySQL.nuspec", (fun config p ->
          { p with
              Version = config.Version
              ReleaseNotes = toLines release.Notes
              Project = "Yaaf.Database.MySQL"
              Summary = "MySQL helpers in addition to Yaaf.Database."
              Description = "MySQL helpers in addition to Yaaf.Database."
              Dependencies = 
                [ "EntityFramework"
                  "Microsoft.AspNet.Identity.Core"
                  "Microsoft.AspNet.Identity.EntityFramework"
                  "MySql.Data"
                  "MySQL.Data.Entities"
                  "FSharp.Core" ]
                  |> List.map (fun name -> name, (GetPackageVersion "packages" name))
                  |> List.append [ config.ProjectName, config.Version ] }) ]
    UseNuget = false
    SetAssemblyFileVersions = (fun config ->
      let info =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version config.Version
          Attribute.FileVersion config.Version
          Attribute.InformationalVersion config.Version]
      CreateFSharpAssemblyInfo "./src/SolutionInfo.fs" info
      //CreateCSharpAssemblyInfo "./src/SolutionInfo.cs" info
      )
    EnableProjectFileCreation = false
    GeneratedFileList =
      [ "Yaaf.Database.dll"; "Yaaf.Database.xml"
        "Yaaf.Database.MySQL.dll"; "Yaaf.Database.MySQL.xml" ]
    BuildTargets =
     [ { BuildParams.WithSolution with
          // The default build
          PlatformName = "Net45"
          AfterBuild = fun _ ->
            if isMono then
              // Fix MySql.Data.Entities
              File.Copy("build/test/net45/mysql.data.entity.EF6.dll", "build/test/net45/MySql.Data.Entity.EF6.dll", true)
              File.Copy("build/net45/mysql.data.entity.EF6.dll", "build/net45/MySql.Data.Entity.EF6.dll", true)
              // Delete Mono.Security.dll (use the gac version to prevent protocol errors)
              File.Delete "build/net45/Mono.Security.dll"
              File.Delete "build/test/net45/Mono.Security.dll"
          FindUnitTestDlls = fun (folder, config) -> 
            seq {
              if not isMono then
                yield folder @@ "Test.Yaaf.Database.dll" 
              yield folder @@ "Test.Yaaf.Database.MySQL.dll"
             } |> Seq.map Path.GetFullPath
          SimpleBuildName = "net45" }
       (*{ BuildParams.WithSolution with
          // The default build
          PlatformName = "Profile111"
          // Workaround FSharp.Compiler.Service not liking to have a FSharp.Core here: https://github.com/fsprojects/FSharpx.Reflection/issues/1
          AfterBuild = fun _ -> File.Delete "build/profile111/FSharp.Core.dll"
          SimpleBuildName = "profile111"
          FindUnitTestDlls =
            // Don't run on mono.
            if isMono then (fun _ -> Seq.empty) else BuildParams.Empty.FindUnitTestDlls }*)
       ]
  }