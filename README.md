*Warning: The tool is not production-ready yet.
This repository serves to track the progress of research and implementation of the approach.*

# AskTheCode

**Program error cause finder for C#**

A common way to search for causes of program errors is to manually explore the code or use a debugger.
It is often a tedious process, especially if the code base is large and complicated.
*AskTheCode* wants to change that, being a Microsoft Visual Studio extension for C# to enable developers to gather crucial information about the code semantics.

Its main idea is pretty simple - the user selects an assertion in the code and *AskTheCode* attempts to find any inputs to the program that can break it.
The user can then see all the inputs violating the assertion and replay the particular error trace to see how it happened.
Due to the high complexity of the problem, it is not guaranteed to find the complete results every time.
Nevertheless, even partial information can save the user a lot of time (by not having to manually inspect the call sites that are "safe").
To grasp the basic principles and GUI of the tool, please see the [paper from InterAVT 2019](https://github.com/roberthusak/AskTheCode/raw/master/docs/InterAVT_2019_paper.pdf).

![Usage of AskTheCode inside Microsoft Visual Studio](https://github.com/roberthusak/AskTheCode/raw/master/docs/gui.png)

## Documentation

AskTheCode uses Microsoft Roslyn .NET compiler to analyse C# code and backward symbolic execution as its main code exploration engine.
Resulting graphs are displayed using MSAGL.
The most comprehensive documentation of the project are the theses and publications below:

- [Diploma thesis from Charles University](https://github.com/roberthusak/AskTheCode/raw/master/docs/CUNI_DiplomaThesis.pdf) - basic motivation and functionality of the tool
- [Diploma thesis from Czech Technical University](https://github.com/roberthusak/AskTheCode/raw/master/docs/CTU_DiplomaThesis.pdf) - handling of the heap objects
- [Paper from SCLIT 2018](https://github.com/roberthusak/AskTheCode/raw/master/docs/SCLIT_2018_paper.pdf) ([DOI](https://doi.org/10.1063/1.5114357)) - overview of the main backward symbolic execution algorithm
- [Paper from InterAVT 2019](https://github.com/roberthusak/AskTheCode/raw/master/docs/InterAVT_2019_paper.pdf) (accepted for publication) - high-level description of the tool and its recent GUI
- Paper from TAPAS 2019 ([working version on workshop's website](http://staticanalysis.org/tapas2019/talks/TAPAS_2019_paper_15.pdf)) - latest features implemented in the [F# branch](https://github.com/roberthusak/AskTheCode/tree/dev/fsharp)

## Future Plans

A rough overview of the current plans for the future:

- Convert the (most of the) implementation into F#, simplifying it and enhancing maintainability - see the F# branch.
- Extend the coverage of supported C# constructs to analyze.
- Implement various optmizations of the backward symbolic execution techniques - state merging, directed call graph symbolic execution etc.
- Combine backward symbolic execution with other techniques.

## Release

There are currently no official releases, as the usage and integration into Microsoft Visual Studio might still change a lot.
Compiling from the sources should work, feel free to contact me (robert@askthecode.net) in case of any problems.
