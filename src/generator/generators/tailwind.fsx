#r "nuget: Fornax.Core, 0.15.1"

open System.IO
open System.Diagnostics
open System

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let generatorDir = projectRoot
    let baseStylePath = Path.Combine(generatorDir, page)
    
    try
        // Create process to run PostCSS and capture its output
        let psi = new ProcessStartInfo("cmd", $"/c npx postcss {baseStylePath}")
        psi.WorkingDirectory <- generatorDir
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true
        
        // Start the process
        use proc = Process.Start(psi)
        
        // Capture output for content and logging
        use stdOutReader = proc.StandardOutput
        let cssContent = stdOutReader.ReadToEnd()
        
        use stdErrReader = proc.StandardError
        let stdErr = stdErrReader.ReadToEnd()
        proc.WaitForExit()
        
        // Check process result
        if proc.ExitCode <> 0 then
            printfn "PostCSS process failed with exit code %d" proc.ExitCode
            printfn "Error output: %s" stdErr
            // Return empty CSS in case of error
            ""
        else
            printfn "CSS generated successfully (%d bytes)" cssContent.Length
            
            // Return the CSS content for Fornax to write
            cssContent
    with e ->
        printfn "Error generating CSS: %s" e.Message
        // Return empty CSS in case of error
        ""