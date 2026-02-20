# Node Descriptions

Node descriptions briefly describe a node's function and output. In Dynamo, they appear in two places:

- In the node tooltip
- In the documentation browser

Follow these guidelines to ensure consistency and help save time when writing or updating node descriptions.

## Overview

Descriptions should be one to two sentences. If more info is needed, it can be included under In Depth in the Documentation Browser.

Sentence case (capitalize the first word of a sentence and any proper nouns). No period at the end if the description is one sentence. Use a period at the end if the description is two or more sentences.

Language should be as clear and simple as possible. Define acronyms at first mention unless they are known even to non-expert users.

Always prioritize clarity, even if that means deviating from these guidelines.

## Guidelines

| Do's | Don'ts |
|------|--------|
| Start the description with a third-person verb. Example: "Determines if one geometry object intersects with another" | Don't start with a second-person verb or with any noun. Bad example: "Determine if one geometry object intersects with another" |
| Use "Returns," "Creates," or another descriptive verb instead of "Gets." Example: "Returns a Nurbs representation of a surface" | Don't use "Get" or "Gets." It's less specific and has several possible translations. Bad example: "Gets a Nurbs representation of the surface" |
| When referring to inputs, use "given" or "input" instead of "specified." Example: "Deletes the given file." You may use "specified" when not directly referring to an input. Example: "Writes text content to a file specified by the given path" | Don't use "specified" when directly referring to inputs. Bad example: "Deletes the specified file" |
| Use "a" or "an" when first referring to an input. Use "the given" or "the input" for clarity. Example: "Sweeps a curve along the path curve" | Don't use "this" when first referring to an input. Bad example: "Sweeps this curve along the path curve" |
| Use "a" or "an" when first referring to an output. Use "the" only with "input" or "given." Example: "Copies a file. Copies the given file" | Don't use "the" on its own for outputs. Bad example: "Copies the file" |
| Capitalize the first word of a sentence and proper nouns. Example: "Returns the intersection of two BoundingBoxes" | Don't capitalize common geometry objects. Bad example: "Scales non-uniformly around the given Plane" |
| Capitalize Boolean, True, and False for output. Example: "Returns True if the two values are different" | Don't lowercase Boolean, True, or False. Bad example: "Returns true if the two values are different" |

---

# In-Depth Node Descriptions aka extended node help

Node in-depth descriptions dive deeper into a node's functionality, giving users an understanding of the node that allows them to successfully use it in a graph if they wish. The in-depth description lives in two places:

1. Documentation browser, below the node description
2. Dynamo dictionary

As opposed to a node description, which states a node's purpose in one to two sentences, an in-depth description delivers a comprehensive overview of the node, including things like use cases, do's and don'ts, differences and similarities to other nodes, and a detailed description of an accompanying example file.

Follow these guidelines to ensure consistency and help save time when writing or updating node in-depth descriptions.

---

## Overview

Since in-depth descriptions appear in contexts that are not space constrained, their length is not limited by their container. Provide as much guidance as is needed to clearly and comprehensively explain the node and its functionality to users from beginners to advanced.

"Comprehensively" here means that after reading the in-depth description, the user should be able to:

* understand what the node does
* if they wish, use it in their graph without running into issues resulting from insufficient information
* understand any limitations involving the node or its inputs/outputs, as well as any options they have regarding its usage, such as wiring a port vs. leaving it as default

However, most users have no need for the advanced technical details applicable to using the node in any other uncommon or immediately behind the nodes operational or graph. Unless there's a compelling reason to include information at this expert level, omit it.

---

## Content

In-depth help could include:

* A more detailed description of the node and how it works
* Thorough explanation of inputs and outputs
* Different ways of using the node, such as supplying an input vs. using the default
* Do's and Don'ts, such as any restrictions concerning input data
* Use cases and practical examples
* A description of the example file

Describe what the node does in sufficient detail. Don't simply restate or rephrase the node description. Recapping the node description is fine if you also provide additional information, but avoid repeating the node description verbatim to ensure that each part of the node documentation provides useful content to users. Avoid walls of text by using short paragraphs.

---

## Example file

Every node should ideally have an example file along with an in-depth description. The last section in the in-depth description should focus on describing and explaining the example file. Introduce the example with "In the example below..." and then explain how the node is used in the example.

Use example files to illustrate several use cases where applicable. This could mean using several instances of the node in the example file to showcase how it works with list levels, etc. At the same time, strive for compact and simplified node layout to avoid forcing the user to click and drag several times in the preview window to navigate the sample.

Balance the use of code blocks. While these can save a lot of space, newer users may benefit more from node-based examples.

Even if the node is simple and straightforward, you can create a small example graph to illustrate its use.

---

## Guidelines

In-depth help poses unique challenges for localization. Adhere to these guidelines to minimize negative impact on localization processes.

### ✅ Do's

* **Word choice:** Use descriptive, clear, and concise language. As much as possible, imagine you are explaining the issue to a non-technical friend.
  **Example:** ‘List.Flatten’ returns a one-dimensional list (a list with a single level) from a multi-dimensional list (a list with at least one nested list).

* **Formatting:** To minimize confusion for translators, format node names, inputs, and outputs as code by using Markdown backticks (‘). NOTE: These backticks aren't visible to the user in Dynamo, they are only used in this example in this document.
  **Example:** ‘List.TrueForAll’ returns a Boolean value showing if the condition in the ‘QueryFunction’ input is True for all items on the list.

* **Node names:** Begin the in-depth help with the node name, and state it in full, including library name, node name, and any parentheticals. On subsequent mentions, you may drop the parentheticals.
  **Example:** ‘Geometry.Rotate’ (origin, axis, degrees) rotates an input geometry around a base plane by a defined degree.

* **Tense:** Use present tense when describing node functionality.
  **Example:** ’List.Sublists’ takes an input list and returns a series of sublists based on the input range and offset.

* **Numbers:** When describing example files, numerals expressed as digits can improve clarity, especially when several numbers are used in close succession.
  **Example:** In the example below, a simple loop is created to add 10, starting with 1, until the result is larger than 100.

### ❌ Don'ts

* **Word choice:** Don't use jargon or highly advanced technical terms without explaining them. Technical details, where needed, should also be written with non-technical users in mind.
  **Bad example:** Knots: The knot vector should be a non-decreasing sequence. Interior knot multiplicity should be no larger than degree + 1 at the start/end knot and degree at an internal knot (this allows curves with G1 discontinuities to be represented).

* **Formatting:** Don't format node names, inputs, and outputs as regular text. Add single backticks ‘like this’ around node names to format them as code.
  **Bad example:** List.TrueForAll returns a Boolean value showing if the condition in the QueryFunction input is True for all items on the list.

* **Node names:** Don't add or remove spaces, periods, or any other punctuation to/from node names. Don't only use part of a node name.
  **Bad example:** ‘Geometry Rotate’ rotates an input geometry around a base plane by a defined degree.

* **Tense:** Don't use future tense (or any tense other than present, unless there's a reason to do so).
  **Bad example:** ‘List.Sublists’ will take an input list and return a series of sublists based on the input range and offset.

* **Numbers:** Don't spell out numbers when describing example files, unless the numbers are used to describe the graph layout, for example, "two nodes."
  **Bad example:** In the example below, a simple loop is created to add ten, starting with one, until the result is larger than one hundred.
