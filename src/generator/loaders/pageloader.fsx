#r "../_lib/Fornax.Core.dll"

open System.IO
open System

type Page = {
    title: string
    link: string
    content: string
}

let isValidPage (filePath: string) =
    let ext = Path.GetExtension(filePath)
    let dir = Path.GetDirectoryName(filePath)
    ext = ".md" && 
    not (dir.Contains("_public")) && 
    not (Path.GetFileName(filePath).StartsWith("_"))

let titleFromPath (filePath: string) =
    let fileName = Path.GetFileNameWithoutExtension(filePath)
    if fileName = "index" then "Home"
    else fileName |> fun s -> Char.ToUpper(s.[0]).ToString() + s.Substring(1)

let loader (projectRoot: string) (siteContent: SiteContents) =
    let pagesPath = Path.Combine(projectRoot, "pages")
    
    // Create standard pages
    let standardPages = [
        { title = "Home"; link = "/"; content = "" }
        { title = "Posts"; link = "/posts/index.html"; content = "" }
        { title = "About"; link = "/about.html"; content = "" }
        { title = "Contact"; link = "/contact.html"; content = "" }
    ]
    
    // Add all standard pages to site content
    standardPages |> List.iter siteContent.Add
    
    // Also add any additional .md pages from the pages folder
    if Directory.Exists(pagesPath) then
        Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
        |> Array.filter isValidPage
        |> Array.map (fun filePath ->
            let fileName = Path.GetFileNameWithoutExtension(filePath)
            // Skip files that are already handled by standard pages
            if ["index"; "posts"; "about"; "contact"] |> List.contains fileName then
                None
            else
                let link = "/" + fileName + ".html"
                let content = File.ReadAllText filePath
                Some {
                    title = titleFromPath filePath
                    link = link
                    content = content
                }
        )
        |> Array.choose id
        |> Array.iter siteContent.Add

    siteContent