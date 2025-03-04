#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "layout.fsx"

open System.IO
open System

// Use the Layout.Page type since that's the single source of truth
type Page = Layout.Page

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

        let file = Path.Combine(n |> Path.GetDirectoryName |> fun x -> x.[chopLength .. ], 
                                fileName + ".md").Replace("\\", "/")

        Some {
            Layout.Page.title = title
            link = link
            content = content
            file = file
        }
    with ex ->
        printfn "Error processing %s: %s" n ex.Message
        None

let loader (projectRoot: string) (siteContent: SiteContents) =
    // Always add standard pages
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
            file = "posts.md"
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
    standardPages |> List.iter siteContent.Add
    
    // Also load actual content from Markdown files
    let pagesPath = Path.Combine(projectRoot, "pages")
    
    if Directory.Exists(pagesPath) then
        try
            Directory.GetFiles(pagesPath, "*.md", SearchOption.AllDirectories)
            |> Array.filter (fun p -> not (Path.GetFileName(p).StartsWith("_")))
            |> Array.choose (loadFile pagesPath)
            |> Array.iter (fun page -> 
                // Update the page content but keep the standard page entry
                let existingPages = siteContent.TryGetValues<Page>() |> Option.defaultValue Seq.empty
                let existing = existingPages |> Seq.tryFind (fun p -> p.title = page.title)
                
                match existing with
                | Some _ -> 
                    // Replace the existing page
                    siteContent.Remove<Page>() |> ignore
                    existingPages 
                    |> Seq.filter (fun p -> p.title <> page.title)
                    |> Seq.append [page]
                    |> Seq.iter siteContent.Add
                | None -> 
                    // Add as a new page
                    siteContent.Add(page)
            )
        with ex ->
            printfn "Error loading pages: %s" ex.Message

    siteContent