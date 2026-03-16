### Release notes

Ensure that each release note is clear, concise, grammatically correct, and adheres to a neutral tone. The revised sentences should start with a past-tense verb and include all necessary articles. Do not use special characters, bolding, or italics. Avoid internal jargon, and explain all terminology so that the user can understand it. If you don’t know the meaning of some terms mentioned in the release notes, ask.

**Good examples:**

- Added support for view extensions to inject menu items into the File menu.
- Prevented Dynamo from loading packages from ProgramData when outdated settings were present.
- Ensured the Python migration toast notification closes when the graph is closed.
- Disabled sign-in functionality in no network mode.

---

### Node descriptions

Node descriptions briefly describe a node’s function and output. In Dynamo, they appear in two places:

- In the node tooltip  
- In the documentation browser  

Follow these guidelines to ensure consistency and help save time when writing or updating node descriptions.

#### Overview

Descriptions should be one to two sentences. If more info is needed, it can be included under In Depth in the Documentation Browser.

Sentence case (capitalize the first word of a sentence and any proper nouns). No period at the end if the description is one sentence. Use a period at the end if the description is two or more sentences.

Language should be as clear and simple as possible. Define acronyms at first mention unless they are known even to non-expert users.

Always prioritize clarity, even if that means deviating from these guidelines.

#### Guidelines

| Do's | Don'ts |
|------|--------|
| Start the description with a third-person verb. Example: "Determines if one geometry object intersects with another" | Don't start with a second-person verb or with any noun. Bad example: "Determine if one geometry object intersects with another" |
| Use "Returns," "Creates," or another descriptive verb instead of "Gets." Example: "Returns a Nurbs representation of a surface" | Don't use "Get" or "Gets." It's less specific and has several possible translations. Bad example: "Gets a Nurbs representation of the surface" |
| When referring to inputs, use "given" or "input" instead of "specified." Example: "Deletes the given file." You may use "specified" when not directly referring to an input. Example: "Writes text content to a file specified by the given path" | Don't use "specified" when directly referring to inputs. Bad example: "Deletes the specified file" |
| Use "a" or "an" when first referring to an input. Use "the given" or "the input" for clarity. Example: "Sweeps a curve along the path curve" | Don't use "this" when first referring to an input. Bad example: "Sweeps this curve along the path curve" |
| Use "a" or "an" when first referring to an output. Use "the" only with "input" or "given." Example: "Copies a file. Copies the given file" | Don't use "the" on its own for outputs. Bad example: "Copies the file" |
| Capitalize the first word of a sentence and proper nouns. Example: "Returns the intersection of two BoundingBoxes" | Don't capitalize common geometry objects. Bad example: "Scales non-uniformly around the given Plane" |
| Capitalize Boolean, True, and False for output. Example: "Returns True if the two values are different" | Don't lowercase Boolean, True, or False. Bad example: "Returns true if the two values are different" |
