#r "nuget: Fornax.Core, 0.15.1"

open System
open System.IO
open System.Diagnostics

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    let generatorDir = projectRoot
    let outputPath = Path.Combine(generatorDir, "_public", Path.GetDirectoryName(page), Path.GetFileName(page))
    
    // Make sure output directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)) |> ignore
    
    try
        // Create process to run PostCSS with more explicit parameters
        let args = sprintf "-c postcss.config.js -i %s -o %s" page outputPath
        let psi = new ProcessStartInfo()
        psi.FileName <- "cmd"
        psi.Arguments <- sprintf "/c postcss -c postcss.config.js %s -o %s" page outputPath
        psi.WorkingDirectory <- generatorDir
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true
        
        // Log the command being executed
        printfn "Executing: npx postcss %s" args
        
        // Start the process
        use proc = Process.Start(psi)
        
        // Capture output for content and logging
        use stdOutReader = proc.StandardOutput
        let cssOutput = stdOutReader.ReadToEnd()
        
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
            printfn "CSS generated successfully to %s" outputPath
            
            if not(String.IsNullOrEmpty(cssOutput)) then
                printfn "PostCSS generated output (first 100 chars): %s" (cssOutput.Substring(0, min 100 cssOutput.Length))
            
            // Read the file we just created
            if File.Exists(outputPath) then
                File.ReadAllText(outputPath)
            else
                printfn "Warning: Output file not found at %s" outputPath
                ""
    with e ->
        printfn "Error generating CSS: %s" e.Message
        printfn "Stack trace: %s" e.StackTrace
        // Return empty CSS in case of error
        ""