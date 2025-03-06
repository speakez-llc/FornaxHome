#r "nuget: Fornax.Core, 0.15.1"
#r "nuget: Markdig, 0.40.0"

open System.IO
open System

// Define Page type directly here
type Page = {
    title: string
    link: string
    content: string
    file: string
}

let markdownPipeline =
    let pipeline = new Markdig.MarkdownPipelineBuilder()
    pipeline.Build()

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
    not (dir.Contains("posts")) // Exclude post files from pages

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

        let chopLength =
            if rootDir.EndsWith(Path.DirectorySeparatorChar) then rootDir.Length
            else rootDir.Length + 1

        // Store the full path to the file in the pages directory
        let file = Path.Combine(n |> Path.GetDirectoryName |> fun x -> x.[chopLength .. ], 
                                fileName + ".md").Replace("\\", "/")

        printfn "Loaded page: %s (file: %s, link: %s)" title file link

        Some {
            title = title
            link = link
            content = content
            file = file
        }
    with ex ->
        printfn "Error processing %s: %s" n ex.Message
        None

let loader (projectRoot: string) (siteContent: SiteContents) =
    printfn "Loading pages from: %s" projectRoot
    
    // First, clear any existing pages
    siteContent.Remove<Page>() |> ignore
    
    // Add standard pages
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
    
    // Add standard pages to content
    printfn "Adding standard pages:"
    standardPages |> List.iter (fun p -> 
        printfn "  - %s (%s)" p.title p.file
        siteContent.Add(p)
    )
    
    // Load actual content from Markdown files
    let pagesPath = Path.Combine(projectRoot, "pages")
    
    if Directory.Exists(pagesPath) then
        printfn "Found pages directory: %s" pagesPath
        try
            let pageFiles = Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
            printfn "Found %d page files" pageFiles.Length
            
            pageFiles
            |> Array.filter (fun p -> not (Path.GetFileName(p).StartsWith("_")))
            |> Array.choose (loadFile projectRoot)
            |> Array.iter (fun page -> 
                // Always add the page content
                siteContent.Add(page)
                printfn "Added page %s with content" page.title
            )
        with ex ->
            printfn "Error loading pages: %s" ex.Message
    else
        printfn "Warning: Pages directory not found at %s" pagesPath
    
    // Debug: show what's in siteContent
    let loadedPages = siteContent.TryGetValues<Page>() |> Option.defaultValue Seq.empty
    printfn "Loaded %d pages in total:" (Seq.length loadedPages)
    loadedPages |> Seq.iter (fun p -> printfn "  - %s (%s)" p.title p.file)

    siteContent