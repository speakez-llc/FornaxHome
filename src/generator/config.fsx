#r "nuget: Fornax.Core, 0.15.1"

open Config
open System.IO

let postPredicate (projectRoot: string, page: string) =
    let fileName = Path.Combine(projectRoot,page)
    let ext = Path.GetExtension page
    if ext = ".md" then
        let ctn = File.ReadAllText fileName
        page.Contains("_public") |> not
        && ctn.Contains("layout: post")
    else
        false

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

// Function to output page files at root level
let pageOutput (page: string) =
    let fileName = Path.GetFileNameWithoutExtension(page)
    if fileName.ToLower() = "index" then
        "index.html"
    else
        fileName + ".html"

let config = {
    Generators = [
        {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "page.fsx"; Trigger = OnFilePredicate pagePredicate; OutputFile = Custom pageOutput }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "posts.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "tailwind.fsx"; Trigger = OnFileExt ".css"; OutputFile = SameFileName }
    ]
}