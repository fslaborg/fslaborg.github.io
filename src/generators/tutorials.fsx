#r "../_lib/Fornax.Core.dll"
#load "layout.fsx"

open System
open Html
open System.IO
open System.Diagnostics


let websocketScript ="""<script type="text/javascript">
    var wsUri = "ws://localhost:8080/websocket";
    function init() {
        websocket = new WebSocket(wsUri);
        websocket.onclose = function(evt) { onClose(evt) };
    }
    function onClose(evt) {
        console.log('closing');
        websocket.close();
        document.location.reload();
    }
    window.addEventListener("load", init, false);
</script>"""


let generate' (ctx : SiteContents) (_: string) =
    
    let tutorialsPath : Tutorialloader.TutorialDirectory = 
        ctx.TryGetValue<Tutorialloader.TutorialDirectory>()
        |> Option.defaultValue {Path=""}

        
    Layout.layout ctx "Tutorials" [
        div [] [!! tutorialsPath.Path]
        ]

let generate (ctx : SiteContents) (projectRoot: string) (page: string) =
     
    printfn "[Tutorials-Generator] Starting generate function ..."

    let tutorialsPath : Tutorialloader.TutorialDirectory = 
        ctx.TryGetValue<Tutorialloader.TutorialDirectory>()
        |> Option.defaultValue {Path=""}
        
    if tutorialsPath.Path <> "" && tutorialsPath.Path.Contains("tutorials_src") then

        let args = 
            sprintf 
                "fsdocs build --eval --input %s --output %s --noapidocs --clean --parameters root %s fsdocs-watch-script %s" 
                tutorialsPath.Path 
                (tutorialsPath.Path.Replace("_src","")) 
                "/content/tutorials/"
                websocketScript

        let psi = ProcessStartInfo()
        psi.FileName <- "dotnet"
        psi.Arguments <- args
        psi.CreateNoWindow <- true
        psi.WindowStyle <- ProcessWindowStyle.Hidden
        psi.UseShellExecute <- true
        try
            printfn "[Tutorials-Generator]: dotnet %s" args
            let proc = Process.Start psi
            proc.WaitForExit()
            printfn  "[Tutorials-Generator]: generated tutorial documents"
            generate' ctx page
            |> Layout.render ctx
        with
        | ex ->
            printfn "Error during fsdocs execution."
            printfn "EX: %s" ex.Message
            ""

    else 
        printfn "Error during fsdocs execution."
        ""

