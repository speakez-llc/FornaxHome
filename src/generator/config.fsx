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
    let normalizedPage = page.Replace('\\', '/').ToLower()
    let ext = Path.GetExtension page
    
    // First, explicitly exclude all CSS files in node_modules
    if ext = ".css" && normalizedPage.Contains("node_modules") then
        false
    else
        let fileShouldBeExcluded =
            ext = ".fsx" ||
            ext = ".md" ||
            page.Contains "_public" ||
            page.Contains("/_public/") ||
            page.Contains("\\_public\\") ||
            page.StartsWith("_public") ||
            page.Contains "node_modules" ||
            page.Contains("/node_modules/") ||
            page.Contains("\\node_modules\\") ||
            page.Contains "_bin" ||
            page.Contains "_lib" ||
            page.Contains "_data" ||
            page.Contains "_settings" ||
            page.Contains "_config.yml" ||
            page.Contains ".sass-cache" ||
            page.Contains ".git" ||
            page.Contains ".ionide" ||
            page.Contains("package.json") ||
            page.Contains("package-lock.json") ||
            page.Contains("tailwind.config.js") ||
            page.Contains("postcss.config.js") ||
            page.Contains("style.css")
            
        fileShouldBeExcluded |> not

let tailwindPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let normalizedPage = page.Replace('\\', '/').ToLower()
    
    // Only process CSS files that are not in node_modules and not in _public
    ext = ".css" && 
    not (normalizedPage.Contains("node_modules")) &&
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
        {Script = "post.fsx"; Trigger = OnFilePredicate postPredicate; OutputFile = ChangeExtension "html" }
        {Script = "page.fsx"; Trigger = OnFilePredicate pagePredicate; OutputFile = Custom pageOutput }
        {Script = "staticfile.fsx"; Trigger = OnFilePredicate staticPredicate; OutputFile = SameFileName }
        {Script = "posts.fsx"; Trigger = Once; OutputFile = MultipleFiles id }
        {Script = "tailwind.fsx"; Trigger = OnFilePredicate tailwindPredicate; OutputFile = SameFileName }
    ]
}