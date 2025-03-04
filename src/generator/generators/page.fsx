#r "nuget: Fornax.Core, 0.15.1"
#r "nuget: Markdig, 0.40.0"
#load "layout.fsx"

open Html
open System.IO
open System.Text.RegularExpressions
open Markdig

// Initialize Markdig pipeline
let markdownPipeline =
    MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoIdentifiers()
        .UseAutoLinks()
        .UseCitations()
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
        .UseYamlFrontMatter()
        .Build()

// Process custom shortcodes in content
let processShortcodes (content: string) =
    // Contact form shortcode
    let contactFormPattern = "{{contact_form}}"
    let contactFormHtml = """
<div class="card bg-base-100 shadow-xl">
  <div class="card-body">
    <form action="/api/contact" method="post">
      <div class="form-control w-full">
        <label class="label">
          <span class="label-text">Name</span>
        </label>
        <input type="text" name="name" class="input input-bordered w-full" required />
      </div>
      
      <div class="form-control w-full mt-4">
        <label class="label">
          <span class="label-text">Email</span>
        </label>
        <input type="email" name="email" class="input input-bordered w-full" required />
      </div>
      
      <div class="form-control w-full mt-4">
        <label class="label">
          <span class="label-text">Message</span>
        </label>
        <textarea name="message" class="textarea textarea-bordered h-24" required></textarea>
      </div>
      
      <div class="form-control mt-6">
        <button type="submit" class="btn btn-primary w-full">Send Message</button>
      </div>
    </form>
  </div>
</div>
"""

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

let generate' (ctx : SiteContents) (page: string) =
    // Print debugging information
    printfn "Generating page: %s" page
    
    // Fix the file path - don't prepend "pages/" as it's already in the path
    let filePath = page
    let pageTitle = 
        if page.EndsWith("index.md") then "Home"
        elif page.EndsWith("about.md") then "About"
        elif page.EndsWith("contact.md") then "Contact"
        else Path.GetFileNameWithoutExtension(page) |> fun s -> s.[0].ToString().ToUpper() + s.[1..]
    
    printfn "Looking for file at path: %s" filePath
    
    let pageContent = 
        if File.Exists(filePath) then
            try 
                printfn "Found file: %s" filePath
                let content = File.ReadAllText(filePath)
                printfn "Content length: %d" content.Length
                
                let startIndex = content.IndexOf("---") + 3
                let endIndex = content.IndexOf("---", startIndex)
                if endIndex > startIndex then
                    printfn "Found front matter. Extracting markdown content."
                    let markdownContent = content.Substring(endIndex + 3)
                    printfn "Markdown content length: %d" markdownContent.Length
                    
                    // Process the markdown content 
                    let html = Markdown.ToHtml(markdownContent, markdownPipeline)
                    printfn "Generated HTML length: %d" html.Length
                    
                    // Process shortcodes after markdown rendering
                    processShortcodes html
                else
                    "<p>Error processing page content: Could not find front matter.</p>"
            with ex ->
                printfn "Error reading file %s: %s" filePath ex.Message
                "<p>Error reading page content.</p>"
        else
            printfn "File not found: %s" filePath
            "<p>Page content not found.</p>"
    
    // Use Layout.layout which now includes the navigation bar
    Layout.layout ctx pageTitle [
        section [Class "hero bg-primary text-primary-content py-24"] [
            div [Class "hero-content text-center"] [
                div [Class "max-w-md"] [
                    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
                    let desc =
                        siteInfo
                        |> Option.map (fun si -> si.description)
                        |> Option.defaultValue ""
                    h1 [Class "text-4xl font-bold accent"] [!!desc]
                ]
            ]
        ]
        div [Class "container mx-auto px-4"] [
            section [Class "py-8"] [
                div [Class "max-w-3xl mx-auto"] [
                    div [Class "card bg-base-100 shadow-xl"] [
                        div [Class "card-body"] [
                            div [Class "prose prose-lg prose-headings:font-bold prose-h1:text-2xl prose-h2:text-xl prose-a:text-blue-600 prose-ul:list-disc prose-ul:ml-4 !max-w-none"] [
                                !!pageContent
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    printfn "Page generator called for: %s (projectRoot: %s)" page projectRoot
    try
        let rendered = generate' ctx page
        Layout.render ctx rendered
    with ex ->
        printfn "Error in page generator: %s" ex.Message
        
        // Use Layout.layout for error page too
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