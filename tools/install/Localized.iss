
; There needs to be a more generic way of doing this. In the interest of time 
; I repeatedly define "CurrentLanguage" three times for different languages.

#define CurrentLanguage "en-US"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore

;Revit 2014 / Vasari Beta 3
Source: temp\bin\Revit_2014\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3
Source: temp\bin\Revit_2014\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3

;Revit 2015 / Revit 2016
Source: temp\bin\Revit_2015\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016
Source: temp\bin\Revit_2015\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016

#define CurrentLanguage "de-DE"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore

;Revit 2014 / Vasari Beta 3
Source: temp\bin\Revit_2014\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3
Source: temp\bin\Revit_2014\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3

;Revit 2015 / Revit 2016
Source: temp\bin\Revit_2015\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016
Source: temp\bin\Revit_2015\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016

#define CurrentLanguage "ja-JP"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags: ignoreversion overwritereadonly; Components: DynamoCore

;Revit 2014 / Vasari Beta 3
Source: temp\bin\Revit_2014\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3
Source: temp\bin\Revit_2014\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2014\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2014 DynamoForVasariBeta3

;Revit 2015 / Revit 2016
Source: temp\bin\Revit_2015\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016
Source: temp\bin\Revit_2015\nodes\{#CurrentLanguage}\*; DestDir: {app}\Revit_2015\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoForRevit2015 DynamoForRevit2016
