#r "nuget: Fornax.Core, 0.15.1"

open Config
open System.IO

// Helper function to check if path contains node_modules
let isInNodeModules (path: string) =
    let normalizedPath = path.Replace('\\', '/').ToLower()
    normalizedPath.Contains("/node_modules/") || 
    normalizedPath.Contains("\\node_modules\\") ||
    normalizedPath.Contains("node_modules")

// Helper function to check if path is in _public folder
let isInPublicFolder (path: string) =
    path.Contains "_public" ||
    path.Contains("/_public/") ||
    path.Contains("\\_public\\") ||
    path.StartsWith("_public")

let postPredicate (projectRoot: string, page: string) =
    // Immediately reject any files in node_modules or _public
    if isInNodeModules(page) || isInPublicFolder(page) then
        false
    elif Path.GetExtension page = ".md" then
        let fileName = Path.Combine(projectRoot, page)
        let ctn = File.ReadAllText fileName
        ctn.Contains("layout: post")
    else
        false

let pagePredicate (projectRoot: string, page: string) =
    // Immediately reject any files in node_modules or _public
    if isInNodeModules(page) || isInPublicFolder(page) then
        false
    elif Path.GetExtension page = ".md" then
        let fileName = Path.Combine(projectRoot, page)
        let ctn = File.ReadAllText fileName
        ctn.Contains("layout: page")  // Check for page layout tag
    else
        false

let staticPredicate (projectRoot: string, page: string) =
    // Immediately reject any files in node_modules or _public
    if isInNodeModules(page) || isInPublicFolder(page) then
        false
    else
        // Then handle other exclusion cases
        let ext = Path.GetExtension page
        let fileShouldBeExcluded =
            ext = ".fsx" ||
            ext = ".md" ||
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
    // Only process CSS files that are not in node_modules and not in _public
    if isInNodeModules(page) || isInPublicFolder(page) then
        false
    else
        Path.GetExtension page = ".css"

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