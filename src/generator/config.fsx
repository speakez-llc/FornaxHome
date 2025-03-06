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
    
    // Be more specific about which pages to process
    let isPage = 
        ext = ".md" &&
        not (page.Contains "_public") &&
        not (postPredicate(projectRoot, page)) &&
        (page.StartsWith("pages/") || page.StartsWith("pages\\")) &&
        not (Path.GetFileName(page).StartsWith("_")) &&
        // Exclude any paths containing HTML as those would be already processed
        not (page.EndsWith(".html"))

    isPage

let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        ext = ".fsx" ||
        ext = ".md"  ||
        ext = ".html"  ||
        page.EndsWith("tailwind.config.js") ||
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


let config = {
    Generators = [
        {Script = "less.fsx"; Trigger = OnFileExt ".less"; OutputFile = ChangeExtension "css" }
        {Script = "sass.fsx"; Trigger = OnFileExt ".scss"; OutputFile = ChangeExtension "css" }
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "page.fsx"; Trigger = OnFilePredicate pagePredicate; OutputFile = MultipleFiles id }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "posts.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "tailwind.fsx"; Trigger = OnFileExt ".css"; OutputFile = SameFileName }
    ]
}