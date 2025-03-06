#r "nuget: Fornax.Core, 0.15.1"
#r "nuget: Markdig, 0.40.0"
#load "../generators/types.fsx"

open System.IO
open Types
open Markdig

let markdownPipeline =
    MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoIdentifiers()
        .UseAutoLinks()
        .UseCustomContainers()
        .UseDefinitionLists()
        .UseEmphasisExtras()
        .UseFigures()
        .UseFooters()
        .UseFootnotes()
        .UseGenericAttributes()
        .UseGridTables()
        .UseListExtras()
        .UseMathematics()
        .UseMediaLinks()
        .UsePipeTables()
        .UsePragmaLines()
        .UseSmartyPants()
        .UseTaskLists()
        .Build()

/// Extracts front matter from markdown content
let extractFrontMatter (content: string) =
    let lines = content.Split('\n')
    if lines.Length > 2 && lines.[0].Trim() = "---" then
        let separatorIndex = Array.findIndex (fun (line: string) -> line.Trim() = "---") lines.[1..]
        if separatorIndex > 0 then
            let frontMatter = lines.[1..separatorIndex]
            let markdown = String.concat "\n" lines.[separatorIndex + 2..]
            
            // Parse front matter into key-value pairs
            let parseFrontMatterLine (line: string) =
                let colonIndex = line.IndexOf(':')
                if colonIndex > 0 then
                    let key = line.[..colonIndex - 1].Trim().ToLower()
                    let value = line.[colonIndex + 1..].Trim().TrimStart('"').TrimEnd('"')
                    Some (key, value)
                else
                    None
                    
            let frontMatterMap = 
                frontMatter
                |> Array.choose parseFrontMatterLine
                |> Map.ofArray
                
            Some (frontMatterMap, markdown)
        else
            None
    else
        None

/// Loads a single page from a file
let loadPage (rootDir: string) (filePath: string) =
    try
        let text = File.ReadAllText filePath
        
        match extractFrontMatter text with
        | Some (frontMatter, markdownContent) ->
            let title = 
                frontMatter 
                |> Map.tryFind "title" 
                |> Option.defaultValue (Path.GetFileNameWithoutExtension filePath)
                
            let fileName = Path.GetFileNameWithoutExtension filePath
            let link = 
                if fileName.ToLower() = "index" then "/"
                else sprintf "/%s.html" fileName
                
            let htmlContent = Markdig.Markdown.ToHtml(markdownContent, markdownPipeline)
            
            // Calculate the relative path from the root directory
            let chopLength =
                if rootDir.EndsWith(Path.DirectorySeparatorChar) then rootDir.Length
                else rootDir.Length + 1
                
            let dirPart = 
                filePath
                |> Path.GetDirectoryName
                |> fun x -> x.[chopLength..] 
                
            let file = Path.Combine(dirPart, fileName + ".md").Replace("\\", "/")
            
            Some {
                title = title
                link = link
                content = htmlContent
                file = file
            }
        | None ->
            printfn "Warning: No front matter found in %s" filePath
            None
    with ex ->
        printfn "Error processing page %s: %s" filePath ex.Message
        None

/// The loader function called by Fornax
let loader (projectRoot: string) (siteContent: SiteContents) =
    printfn "Loading pages..."
    
    // Standard pages that should always exist
    let standardPages = [
        { 
            title = "Home"
            link = "/"
            content = ""
            file = "pages/index.md"
        }
        { 
            title = "Posts"
            link = "/posts/index.html"
            content = ""
            file = "pages/posts.md"
        }
        { 
            title = "About"
            link = "/about.html"
            content = ""
            file = "pages/about.md"
        }
        { 
            title = "Contact"
            link = "/contact.html"
            content = ""
            file = "pages/contact.md"
        }
    ]
    
    // Add standard pages as placeholders
    standardPages |> List.iter siteContent.Add
    
    // Find and load actual page content
    let pagesPath = Path.Combine(projectRoot, "pages")
    
    if Directory.Exists(pagesPath) then
        try
            Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
            |> Array.filter (fun p -> not (Path.GetFileName(p).StartsWith("_")))
            |> Array.choose (loadPage projectRoot)
            |> Array.iter (fun page -> 
                // Only add the page if it's not already in the standard pages
                let existingPages = siteContent.TryGetValues<Page>() |> Option.defaultValue Seq.empty
                let existing = existingPages |> Seq.tryFind (fun p -> p.file = page.file)
                
                match existing with
                | Some _ -> 
                    // Replace the existing page
                    siteContent.Remove<Page>() |> ignore
                    existingPages 
                    |> Seq.filter (fun p -> p.file <> page.file)
                    |> Seq.append [page]
                    |> Seq.iter siteContent.Add
                | None -> 
                    // Add as a new page
                    siteContent.Add(page)
            )
        with ex ->
            printfn "Error loading pages: %s" ex.Message
    else
        printfn "Warning: Pages directory not found at %s" pagesPath

    siteContent