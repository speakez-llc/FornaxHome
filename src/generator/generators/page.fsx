#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "layout.fsx"

open Html
open System.Text.RegularExpressions
open System.IO
open System

// Add a comment about Markdig extensions
// Note: If you want to use more Markdig extensions, you can add them here:
let markdownPipeline =
    let pipeline = new Markdig.MarkdownPipelineBuilder()
    pipeline.Build()

// Process custom shortcodes in content
let processShortcodes (content: string) =
    // Contact form shortcode
    let contactFormPattern = "{{contact_form}}"


    // Alert shortcode
    let alertPattern = "{{alert (.*?)}}"
    let alertReplacer (m: Match) =
        let message = m.Groups.[1].Value
        sprintf """<div class="alert alert-info shadow-lg">
  <div>
    <span>%s</span>
  </div>
</div>""" message
    
    // Apply replacements
    let withContactForm = Regex.Replace(content, contactFormPattern, contactFormHtml)
    let withAlerts = Regex.Replace(withContactForm, alertPattern, alertReplacer)
    
    withAlerts

let getContent (fileContent : string) =
    let fileContent = fileContent.Split '\n'
    let fileContent = fileContent |> Array.skip 1 // First line must be ---
    let indexOfSeperator = fileContent |> Array.findIndex (fun s -> s.StartsWith "---")
    fileContent 
    |> Array.skip (indexOfSeperator + 1) 
    |> String.concat "\n"
    |> processShortcodes  // Process shortcodes before Markdown rendering
    |> fun content -> Markdig.Markdown.ToHtml(content, markdownPipeline)

let generate' (ctx : SiteContents) (page: string) =
    // Load page content directly from file if it exists
    let filePath = Path.Combine("pages", page)
    let pageTitle = 
        if page.EndsWith("index.md") then "Home"
        elif page.EndsWith("about.md") then "About"
        elif page.EndsWith("contact.md") then "Contact"
        else Path.GetFileNameWithoutExtension(page) |> fun s -> s.[0].ToString().ToUpper() + s.[1..]
    
    let pageContent = 
        if File.Exists(filePath) then
            try 
                let content = File.ReadAllText(filePath)
                let startIndex = content.IndexOf("---") + 3
                let endIndex = content.IndexOf("---", startIndex)
                if endIndex > startIndex then
                    let markdownContent = content.Substring(endIndex + 3)
                    processShortcodes markdownContent
                    |> fun content -> Markdig.Markdown.ToHtml(content, markdownPipeline)
                else
                    "<p>Error processing page content.</p>"
            with ex ->
                printfn "Error reading file %s: %s" filePath ex.Message
                "<p>Error reading page content.</p>"
        else
            printfn "File not found: %s" filePath
            "<p>Page content not found.</p>"
    
    // Get site info for the header
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let desc =
        siteInfo
        |> Option.map (fun si -> si.description)
        |> Option.defaultValue ""
    
    // Always render the page with the standard layout
    Layout.layout ctx pageTitle [
        section [Class "hero bg-primary text-primary-content py-24"] [
            div [Class "hero-content text-center"] [
                div [Class "max-w-md"] [
                    h1 [Class "text-4xl font-bold accent"] [!!desc]
                ]
            ]
        ]
        div [Class "container mx-auto px-4"] [
            section [Class "py-8"] [
                div [Class "max-w-3xl mx-auto"] [
                    div [Class "card bg-base-100 shadow-xl"] [
                        div [Class "card-body"] [
                            div [Class "prose lg:prose-lg"] [
                                !! pageContent
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "Page generator called for: %s" page
    try
        let rendered = generate' ctx page
        Layout.render ctx rendered
    with ex ->
        printfn "Error in page generator: %s" ex.Message
        
        // Fallback to a simple error page
        let errorPage = Layout.layout ctx "Error" [
            div [Class "container mx-auto px-4 py-8"] [
                div [Class "card bg-error text-error-content max-w-md mx-auto"] [
                    div [Class "card-body"] [
                        h2 [Class "card-title"] [!!"Error Generating Page"]
                        p [] [!!"There was an error generating this page. Please check the console for details."]
                    ]
                ]
            ]
        ]
        
        Layout.render ctx errorPage