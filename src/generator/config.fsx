#r "nuget: Fornax.Core, 0.15.1"

open Config
open System.IO

/// Predicate for identifying post files
let postPredicate (projectRoot: string, page: string) =
    let fileName = Path.Combine(projectRoot, page)
    let ext = Path.GetExtension page
    if ext = ".md" then
        let ctn = File.ReadAllText fileName
        page.Contains("_public") |> not
        && ctn.Contains("layout: post")
    else
        false

/// Predicate for identifying page files
let pagePredicate (projectRoot: string, page: string) =
    let fileName = Path.Combine(projectRoot, page)
    let ext = Path.GetExtension page
    let dir = Path.GetDirectoryName page
    let isPage = 
        ext = ".md" &&
        not (page.Contains "_public") &&
        dir.Contains("pages") &&
        not (postPredicate(projectRoot, page))
    
    isPage

/// Predicate for identifying static files to copy
let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        ext = ".fsx" ||
        ext = ".md"  ||
        page.Contains "_public" ||
        page.Contains("/_public/") ||
        page.Contains "_bin" ||
        page.Contains "_lib" ||
        page.Contains "_data" ||
        page.Contains "_settings" ||
        page.Contains "_config.yml" ||
        page.Contains ".sass-cache" ||
        page.Contains ".git" ||
        page.Contains ".ionide"
    fileShouldBeExcluded |> not

/// Configuration for the site
let config = {
    Generators = [
        {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "page.fsx"; Trigger = OnFilePredicate pagePredicate; OutputFile = ChangeExtension "html" }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "posts.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "index.fsx"; Trigger = Once; OutputFile = NewFileName "index.html" }
        {Script = "tailwind.fsx"; Trigger = OnFileExt ".css"; OutputFile = SameFileName }
    ]
}