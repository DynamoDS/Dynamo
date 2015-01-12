
; There needs to be a more generic way of doing this. In the interest of time 
; I repeatedly define "CurrentLanguage" three times for different languages.

#define CurrentLanguage "en-US"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore

#define CurrentLanguage "de-DE"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore

#define CurrentLanguage "ja-JP"

;Localized Resource Files - DynamoCore
Source: temp\bin\{#CurrentLanguage}\*; DestDir: {app}\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore
Source: temp\bin\nodes\{#CurrentLanguage}\*; DestDir: {app}\nodes\{#CurrentLanguage}; Flags:skipifsourcedoesntexist ignoreversion overwritereadonly; Components: DynamoCore
