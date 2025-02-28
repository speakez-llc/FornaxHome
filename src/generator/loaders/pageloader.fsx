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
    
    Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
    |> Array.filter isValidPage
    |> Array.map (fun filePath ->
        let link = 
            filePath.Substring(pagesPath.Length)
                .Replace("\\", "/")
                .Replace(".md", ".html")
                .Replace("index.html", "/")
        let content = File.ReadAllText filePath
        {
            title = titleFromPath filePath
            link = link
            content = content
        })
    |> Array.iter siteContent.Add

    siteContent