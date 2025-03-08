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
    if ext = ".md" then
        let ctn = File.ReadAllText fileName
        page.Contains("_public") |> not
        && ctn.Contains("layout: page")  // Check for page layout tag
    else
        false

let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        ext = ".fsx" ||
        ext = ".md"  ||
        page.Contains "_public" ||
        page.Contains("/_public/") ||
        page.Contains("\\_public\\") || // Add this for Windows paths
        page.StartsWith("_public") ||   // Add this for root-level paths
        page.Contains "_bin" ||
        page.Contains "_lib" ||
        page.Contains "_data" ||
        page.Contains "_settings" ||
        page.Contains "_config.yml" ||
        page.Contains ".sass-cache" ||
        page.Contains ".git" ||
        page.Contains ".ionide"
    fileShouldBeExcluded |> not

let tailwindPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    ext = ".css" &&
    not (page.Contains("_public"))

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
        {Script = "tailwind.fsx"; Trigger = OnFilePredicate tailwindPredicate; OutputFile = SameFileName }
    ]
}