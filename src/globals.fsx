#r "_lib/Fornax.Core.dll"
#r "_lib/Markdig.dll"

open Markdig
module MarkdownProcessing =

    let markdownPipeline =
        MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseGridTables()
            .UseGenericAttributes()
            .UseEmphasisExtras()
            .UseListExtras()
            .UseCitations()
            .UseCustomContainers()
            .UseFigures()
            .Build()

    let isFrontMatterSeparator (input : string) =
        input.StartsWith "---"

    ///`fileContent` - content of page to parse. Usually whole content of `.md` file
    ///returns content of config that should be used for the page
    let getFrontMatter (fileContent : string) =
        let fileContent = 
            fileContent.Split '\n'
            |> Array.skip 1 //First line must be ---

        let indexOfSeperator = fileContent |> Array.findIndex isFrontMatterSeparator

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

        
    ///`fileContent` - content of page to parse. Usually whole content of `.md` file
    ///returns HTML version of content of the page
    let getMarkdownContent (fileContent : string) =
        let fileContent = fileContent.Split '\n'
        let fileContent = fileContent |> Array.skip 1 //First line must be ---
        let indexOfSeperator = fileContent |> Array.findIndex isFrontMatterSeparator
        let content = 
            fileContent 
            |> Array.splitAt indexOfSeperator
            |> snd
            |> Array.skip 1 
            |> String.concat "\n"

        Markdown.ToHtml(content, markdownPipeline)