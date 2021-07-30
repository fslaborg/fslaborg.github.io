#r "_lib/Fornax.Core.dll"
#r "_lib/Markdig.dll"

open System.IO
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

    let trimString (str : string) =
        str.Trim().TrimEnd('"').TrimStart('"')

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
                Some(key, trimString value)
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

module ScriptProcessing =

    let isFrontMatterIndicator (input : string) =
        input.StartsWith "#frontmatter"

    let getFrontMatter (fileContent : string) =

        let fileContent = 
            fileContent.Split '\n'
       
        let indexOfFrontmatter = fileContent |> Array.findIndex isFrontMatterIndicator

        let fileContent =
            fileContent
            |> Array.skip (indexOfFrontmatter + 2)

        let indexOfSeperator = fileContent |> Array.findIndex MarkdownProcessing.isFrontMatterSeparator

        let splitKey (line: string) = 
            let seperatorIndex = line.IndexOf(':')
            if seperatorIndex > 0 then
                let key = line.[.. seperatorIndex - 1].Trim().ToLower()
                let value = line.[seperatorIndex + 1 ..].Trim() 
                Some(key, MarkdownProcessing.trimString value)
            else 
                None

        fileContent
        |> Array.splitAt indexOfSeperator
        |> fst
        |> Seq.choose splitKey
        |> Map.ofSeq        


module Predicates =
        
    let markdownPredicate (projectRoot: string, page: string) =
        let ext = Path.GetExtension page
        let fileName = Path.GetFileNameWithoutExtension page
        fileName.ToUpperInvariant() <> "README"
        && ext = ".md"

    let isMarkdownFile f = markdownPredicate ("",f)

    let tutorialPredicate (projectRoot: string, page: string) = 
        let ext = Path.GetExtension page
        page.Contains("tutorials")
        && ext = ".fsx"
        || ext = ".md"

    let isTutorialFile f = tutorialPredicate("",f)

    let staticPredicate (projectRoot: string, page: string) =
        let ext = Path.GetExtension page

        if page.Contains "tutorials/" && ext = ".fsx" then true
        elif page.Contains "_public" ||
           page.Contains "_bin" ||
           page.Contains "_lib" ||
           page.Contains "_data" ||
           page.Contains "_settings" ||
           page.Contains "_config.yml" ||
           page.Contains ".sass-cache" ||
           page.Contains ".git" ||
           page.Contains ".ionide" ||
           ext = ".fsx"
        then
            false
        else
            true
