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
    let siteInfo = ctx.TryGetValue<Globalloader.SiteInfo> ()
    let desc = siteInfo |> Option.map (fun si -> si.description) |> Option.defaultValue ""

    printfn "Generating page: %s" page
    let filePath = page

    let mutable finalHtml = "<p>Page content not found.</p>"
    if File.Exists(filePath) then
        let content = File.ReadAllText(filePath)
        let startIndex = content.IndexOf("---") + 3
        let endIndex = content.IndexOf("---", startIndex)
        if endIndex > startIndex then
            printfn "Found front matter. Extracting markdown content."
            let markdownOnly = content.Substring(endIndex + 3)
            let processed = processShortcodes markdownOnly
            let body = Markdig.Markdown.ToHtml(processed, markdownPipeline)
            finalHtml <- body

    Layout.layout ctx page [
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
                            !! finalHtml
                        ]
                    ]
                ]
            ]
        ]
    ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    try
        let rendered = generate' ctx page
        let fileName = Path.GetFileNameWithoutExtension(page)
        let outputFileName = if fileName = "index" then "index.html" else fileName + ".html"
        let outputPath = Path.Combine(projectRoot, "_public", outputFileName)
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)) |> ignore
        let html = Layout.render ctx rendered
        File.WriteAllText(outputPath, html)
        [outputPath]
    with ex ->
        printfn "Error generating page %s: %s" page ex.Message
        []