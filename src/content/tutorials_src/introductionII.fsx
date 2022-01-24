(***hide***)

(*
#frontmatter
---
title: F# Introduction II: Scripting in F#
category: fsharp
authors: Jonathan Ott
index: 2
---
*)
(**
# F# Introduction II: Scripting in F#

## Creating a .fsx file

### Visual Studio

* Open Visual Studio and navigate to the "File" tab, where you select to create a new file.
* Select the "F# Script File" option.  
    
    ![]({{root}}images/FsxVS.png)

* You now have a working script file. You can write code and execute it by selecting it and pressing `Alt + Enter`.

### Visual Studio Code

* Open Visual Studio Code and navigate to the "File" tab, where you select to create a new file.
* You will then be prompted to select a language. Choose F# there.  

    ![]({{root}}images/FsxVSCode.png)

* You now have a working script file. You can write code and execute it by selecting it and pressing `Alt + Enter`.
* When you are done with your file save it as .fsx.

## Referencing packages

* Packages on nuget can be referenced using '#r "nuget: PackageName"':
*)
// References the latest stable package
#r "nuget: FSharp.Stats"
// References a sepcific package version
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"
(**
* Alternatively, .dll files can be referenced directly with the following syntax:
*)
(***do-not-eval***)
#r @"Your\Path\To\Package\PackageName.dll"

(**
## Working with notebooks

* Visual Studio Code supports working with notebooks
* To work with notebooks, you need to install the [.NET Interactive Notebooks](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode) extension.  

    ![]({{root}}images/NotebooksExt.png)

* A new Notebook can be opened by pressing `Ctrl + Shift + P` and selecting ".NET Interactive: Create new blank notebook".
* You will then be prompted to create it either as .dib or .ipynb.
* When asked for the language, choose F#
* Notebooks contain Text- and Codeblocks:
* Adding a new Text- or Codeblock can be done by hovering at the upper or lower border of an existing block or upper part of the notebook and pressing `+Code` or `+Markdown`  

    ![]({{root}}images/NBBlock.png)

* Working with Textblocks:
    You can edit a Textblock by doubleklicking on it. Inside a Textblock you can write plain text or style it with [Markdown](https://en.wikipedia.org/wiki/Markdown).
    Once you are finished you can press the `Esc` button.
* Working with Codeblocks:
    You can start editing any Codeblock by clicking in it. In there you can start writing your own code or edit existing code. Once you are done you can execute the Codeblock by pressing `Ctrl + Alt + Enter`.
    If you want to execute all codeblocks at once, you can press on the two arrows in the upper left corner of the notebook.
*)

