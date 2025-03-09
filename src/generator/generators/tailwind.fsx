#r "nuget: Fornax.Core, 0.15.1"

open System.IO
open System.Diagnostics
open System

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
    // Create a debug log file we can check later
    let logFile = Path.Combine(projectRoot, "tailwind-debug.log")
    File.WriteAllText(logFile, sprintf "Tailwind process started at %A\n" DateTime.Now)
    
    // Simple path setup
    let generatorDir = projectRoot
    let publicDir = Path.Combine(projectRoot, "_public")
    let baseStylePath = Path.Combine(generatorDir, "style", "style.css")
    let outputStylePath = Path.Combine(publicDir, "style", "style.css")
    let configPath = Path.Combine(generatorDir, "tailwind.config.js")
    
    File.AppendAllText(logFile, sprintf "Generator dir: %s\n" generatorDir)
    File.AppendAllText(logFile, sprintf "Config exists: %b\n" (File.Exists(configPath)))
    File.AppendAllText(logFile, sprintf "Base style exists: %b\n" (File.Exists(baseStylePath)))
    
    // Fail hard if config doesn't exist
    if not (File.Exists(configPath)) then
        let msg = sprintf "FATAL ERROR: tailwind.config.js not found at %s" configPath
        File.AppendAllText(logFile, msg + "\n")
        failwith msg
    
    // Check if base style exists
    if not (File.Exists(baseStylePath)) then
        let msg = sprintf "FATAL ERROR: Base style file not found at %s" baseStylePath
        File.AppendAllText(logFile, msg + "\n")
        failwith msg
    
    // Create output directory if it doesn't exist
    Directory.CreateDirectory(Path.GetDirectoryName(outputStylePath)) |> ignore
    
    try
        // Create a temporary Node.js script to process the CSS
        let nodeBuildScript = Path.Combine(generatorDir, "temp-build-css.js")
        
        // Use triple quotes for JavaScript code to avoid escaping issues
        let scriptContent = """
const fs = require('fs');
const path = require('path');
const postcss = require('postcss');
const tailwindcssPostcss = require('@tailwindcss/postcss');
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');

// Get paths from arguments
const inputPath = process.argv[2];
const outputPath = process.argv[3];
const configPath = process.argv[4];

// Read the input CSS file
const css = fs.readFileSync(inputPath, 'utf8');

// Process the CSS
postcss([
  tailwindcssPostcss(configPath),
  autoprefixer,
  cssnano({ preset: 'default' })
])
  .process(css, { from: inputPath, to: outputPath })
  .then(result => {
    // Write the processed CSS to the output file
    fs.writeFileSync(outputPath, result.css);
    console.log('CSS built successfully!');
    process.exit(0);
  })
  .catch(error => {
    console.error('Error processing CSS:', error);
    process.exit(1);
  });
"""
        File.WriteAllText(nodeBuildScript, scriptContent)
        File.AppendAllText(logFile, sprintf "Created temporary build script at %s\n" nodeBuildScript)

        // Run the Node.js script
        let psi = ProcessStartInfo()
        psi.FileName <- "node"
        
        // Use proper string concatenation for arguments
        let safeInputPath = baseStylePath.Replace(" ", "\\ ")
        let safeOutputPath = outputStylePath.Replace(" ", "\\ ")
        let safeConfigPath = configPath.Replace(" ", "\\ ")
        psi.Arguments <- sprintf "%s %s %s %s" nodeBuildScript safeInputPath safeOutputPath safeConfigPath
        
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true
        psi.RedirectStandardError <- true
        psi.CreateNoWindow <- true
        psi.WorkingDirectory <- generatorDir
        
        File.AppendAllText(logFile, sprintf "Running command: node %s\n" psi.Arguments)
        
        use proc = Process.Start(psi)
        let output = proc.StandardOutput.ReadToEnd()
        let error = proc.StandardError.ReadToEnd()
        proc.WaitForExit()
        
        File.AppendAllText(logFile, sprintf "Process exit code: %d\n" proc.ExitCode)
        File.AppendAllText(logFile, sprintf "Process output: %s\n" output)
        File.AppendAllText(logFile, sprintf "Process error: %s\n" error)
        
        // Clean up temporary script
        try
            File.Delete(nodeBuildScript)
            File.AppendAllText(logFile, "Deleted temporary script\n")
        with ex ->
            File.AppendAllText(logFile, sprintf "Failed to delete temporary script: %s\n" ex.Message)
        
        if proc.ExitCode <> 0 then
            let msg = sprintf "Tailwind CSS processing failed: %s\nOutput: %s" error output
            File.AppendAllText(logFile, sprintf "ERROR: %s\n" msg)
            failwith msg
        else
            File.AppendAllText(logFile, "Successfully processed CSS with Tailwind\n")
        
    with ex ->
        // Log the exception
        File.AppendAllText(logFile, sprintf "EXCEPTION: %s\n%s\n" ex.Message ex.StackTrace)
        failwith (sprintf "Exception running Tailwind CSS: %s" ex.Message)
    
    // Return an empty string to prevent Fornax from overwriting the file
    ""