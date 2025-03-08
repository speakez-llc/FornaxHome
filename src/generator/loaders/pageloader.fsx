#r "nuget: Fornax.Core, 0.15.1"
#r "nuget: Markdig, 0.40.0"

open System.IO
open Markdig

// Define Page type directly here
type Page = {
    title: string
    link: string
    content: string
    file: string
}

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

let getConfig (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 // First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex (fun s -> s.StartsWith "---")
    let splitKey (line: string) =
        let seperatorIndex = line.IndexOf(':')
        if seperatorIndex > 0 then
            let key = line.[.. seperatorIndex - 1].Trim().ToLower()
            let value = line.[seperatorIndex + 1 ..].Trim()
            Some(key, value)
        else
            None
    fileContent
    |> Array.splitAt indexOfSeperator
    |> fst
    |> Seq.choose splitKey
    |> Map.ofSeq

let getContent (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 // First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex (fun s -> s.StartsWith "---")
    fileContent 
    |> Array.skip (indexOfSeperator + 1) 
    |> String.concat "\n"
    |> fun content -> Markdig.Markdown.ToHtml(content, markdownPipeline)

let trimString (str : string) =
    str.Trim().TrimEnd('"').TrimStart('"')

let isValidPage (filePath: string) =
    let ext = Path.GetExtension(filePath)
    let dir = Path.GetDirectoryName(filePath)
    ext = ".md" && 
    not (dir.Contains("_public")) && 
    not (Path.GetFileName(filePath).StartsWith("_")) &&
    not (dir.Contains("posts")) &&
    not (filePath.Contains("\\_public\\")) && 
    not (filePath.StartsWith("_public"))   

let loadFile (rootDir: string) (n: string) =
    try
        let text = File.ReadAllText n
        let config = getConfig text
        let content = getContent text

        let fileName = Path.GetFileNameWithoutExtension(n)
        let title = 
            config 
            |> Map.tryFind "title" 
            |> Option.map trimString 
            |> Option.defaultValue (if fileName = "index" then "Home" else fileName)

        let link = 
            if fileName = "index" then "/"
            else "/" + fileName + ".html"

        // Store the full path relative to project root
        let chopLength =
            if rootDir.EndsWith(Path.DirectorySeparatorChar) then rootDir.Length
            else rootDir.Length + 1

        let file = n.Substring(chopLength).Replace("\\", "/")

        printfn "Loaded page: %s (file: %s, link: %s)" title file link

        Some {
            title = title
            link = link
            content = content
            file = file  // Store full relative path
        }
    with ex ->
        printfn "Error processing %s: %s" n ex.Message
        None

let loader (projectRoot: string) (siteContent: SiteContents) =
    printfn "Loading pages from: %s" projectRoot

    
    // Load pages from files
    let pagesPath = Path.Combine(projectRoot, "pages")
    let loadedPages = 
        if Directory.Exists(pagesPath) then
            let pageFiles = Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
            pageFiles
            |> Array.filter (fun p -> not (Path.GetFileName(p).StartsWith("_")))
            |> Array.choose (loadFile projectRoot)
            |> Array.toList
        else
            []
    
    // Add pages to site content
    loadedPages |> List.iter (fun page -> siteContent.Add(page))
    
    // Critical error check - fail if no pages were loaded
    if List.isEmpty loadedPages then
        failwithf "CRITICAL ERROR: No pages found in %s. Site generation cannot continue." pagesPath
    
    printfn "Loaded %d pages" (List.length loadedPages)
    siteContent