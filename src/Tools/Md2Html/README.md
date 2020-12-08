# Md2Html

Md2Html.exe is a command line tool for converting markdown formatted text into html. It can also be used for sanitizing html.

## How to use

```
.\Md2Html.exe -h
  -h, -?, --help             Show help and exit

This tool converts Markdown to Html or Sanitize html
and reads from stdin and outputs to stdout

Converting Markdown to Html
---------------------------
Format markdown as follows:
<<<<<Convert>>>>>
Markdown formatted data here
<<<<<Eod>>>>>

Output will be:
Html formatted data
<<<<<Eod>>>>>

Sanitize Html
-------------
Format html data as follows:
<<<<<Sanitize>>>>>
Html data to sanitize here
<<<<<Eod>>>>>

Output will be:
Sanitized Html data or empty if no sanitization was needed
<<<<<Eod>>>>>
```

## Build process

### Windows

* Md2Html is compiled normally and the resulting binaries are created in a bin folder local to the project folder.
* Md2Html is merged into a single `exe`using `ILMerge`. 
* The single `exe`is created in the Md2Html folder in Dynamos shared bin folder

### Mono

* Md2Html is compiled normally and the resulting binaries are created in a bin folder local to the project folder.
* The resulting binaries are then copied to the Md2Html folder in Dynamos shared bin folder.
