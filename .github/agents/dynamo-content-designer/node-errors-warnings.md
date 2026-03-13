# Node errors and warnings

Node warnings and errors alert the user to an issue with the graph. They notify the user of problems that interfere with normal graph operation by displaying an icon and expanded text bubble above the node. Node errors and warnings can vary in severity: Some graphs can run sufficiently with warnings, while others block expected results. In all cases, node errors and warnings are important tools to keep the user up to date on issues with their graph.

Follow these guidelines to ensure consistency and help save time when writing or updating node warning and error messages.

---

## Overview

Node warnings and errors appear above nodes, and users must hover over them to read the issue text. They are different from modal warnings and errors, which appear as separate dialogs and prevent the user from advancing until they've interacted with the dialog.

Since node warnings and errors indicate a problem with a graph, expect the user encountering them to feel blocked and frustrated, especially if the issue is unfamiliar or unexpected. Messaging should strive to help explain and resolve the issue as quickly and efficiently as possible.

Language should be as clear and simple as possible. It should help minimize the need to leave Dynamo to ask for help from a colleague, forum member, or customer support. Be sure to explain:

- What happened to cause the warning or error  
- How to get back on track  
- How to avoid the situation that triggered the warning or error in the future  

---

## Anatomy

A node error/warning message can include up to 4 distinct sections:

1. **Warning/error title**: Clearly state the warning. This should be an at-a-glance summary of the issue. Sentence case (capitalize only the first letter of the sentence and any proper nouns), 3-5 words, no end punctuation.  
2. **Briefly explain issue**: Explain what most likely caused the message and what the implications are. Sentence case, 30 words max, normal sentence punctuation.  
3. **Help resolve issue**: Provide advice on how to resolve the error or avoid it in the future. Sentence case, 35 words max, normal sentence punctuation.  
4. **Help link**: Link to help documentation, if available. You can provide a more in-depth explanation, examples, and guidance in the expanded documentation. This should specifically be "Learn more" instead of any other variations.  

---

## Guidelines

### Word choice

**Do**

- Use descriptive, clear, and concise language. As much as possible, imagine you are explaining the issue to a non-technical friend.
  - Example: Empty value received  
  - Example: Unable to create PolyCurve.  

**Don't**

- Don’t use jargon or highly advanced technical terms. More technical explanations can be provided in the expanded help.
  - Bad example: Dereferencing a non-pointer  
  - Bad example: PolyCurve.ByPoints operation failed.  

---

### References to node names and input types

**Do**

- Minimize use of node names in running text when possible. When including node names and full input types improves clarity, use formatting to improve readability and reduce visual clutter.
  - Example: The node received an input type it cannot use.
  Input expected: Autodesk.DesignScript.Geometry.Curve
    Input received: Function


**Don't**

- Avoid using node names and full input types in running text. It reduces scannability and can be confusing to read.
- Bad example: Surface.ByPatch expects argument type(s)(Autodesk.DesignScript.Geometry.Curve), but was called with (Function).  

---

### Tone

**Do**

- Messaging should seek to calmly help the user solve the issue. Be succinct to help the user proceed.
- Example: Save your work and reload the graph.  

- Focus on explaining and resolving the issue, not identifying who's at fault.
- Example: Make sure the upstream nodes have non-empty values.  
- Example: To create a PolyCurve, provide a non-empty list.  

**Don't**

- Don't deviate from a professional tone or be needlessly verbose.
- Bad example: Giving the Dynamo VM a new lease of life can potentially make it feel happier and behave better.  

- Don't blame the user. Don't say "sorry."
- Bad example: You didn't provide an input.  
- Bad example: Sorry, but we can't make a PolyCurve from an empty list.  

---

### Punctuation

**Do**

- To reinforce a calm, helpful tone, use periods at the end of sentences, or question marks for questions.
- Example: Unable to loft.  

**Don't**

- Never "yell" at the user by using exclamation points.
- Bad example: Unable to loft!  
