#r "../_lib/Fornax.Core.dll"
#r "../_lib/Markdig.dll"
#load "layout.fsx"

open Html
open System.IO
open System.Text.RegularExpressions

// Initialize Markdig pipeline
let markdownPipeline =
    let pipeline = new Markdig.MarkdownPipelineBuilder()
    pipeline.Build()

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