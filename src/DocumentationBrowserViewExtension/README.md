# Documentation Browser View Extension

## What is this?
A view extension that displays contextual information about errors reported by nodes.

When nodes report errors, info bubbles appear on top of them. Hovering the mouse over the bubble will display a message about the error and often a link that says `See more...`. Clicking that link will make the extension visible, if not already, and display the contextual help regarding the error.

## How does it work?
Internally, contextual help is achieved by the use of:
1. HTML documents that provides help content
2. Specially formatted error messages that link node errors to appropriate documents


## How can it be used in custom nodes?
Custom node developers can easily provide help content for the errors their nodes can raise. Here is an example:

### HTML document

```html
<!DOCTYPE html>
<html>
<body>
    <h3>Division by zero</h3>
    <div>In ordinary arithmetic, the divide operation is not defined when the divisor is zero. You may find more information <a href="https://en.wikipedia.org/wiki/Division_by_zero">here.</a>.</div>
</body>
</html>
```

This is a simple HTML document, named `DivisionByZero.html`, providing help content for a division by zero error. In order for it to be accesible to Dynamo, the only requirement is that you set its `Build Action` to `Embedded Resource`.


Note: For the sake of brevity, no styles are included in the HTML but you are encouraged to add styles to help documents.


### Error message format

```cs
namespace MyZeroTouchNode
{
    public static class MyMath
    {
        public static double Divide(double dividend, double divisor)
        {
            if (divisor == 0)
            {
                throw new Exception("You cannot divide by zero in math href=MyZeroTouchNode;DivisionByZero.html");
            }
            return dividend / divisor;
        }
    }
}
```

This a simple example of a Zero-Touch node that performs a division. Taking a look at the exception message, it ends with a special formatted URI, following `href=`. This part of the message is not actually displayed to the final user, but instead is picked up by Dynamo to trigger the display of help content. Dissecting the syntax, three parts can be distinguished:
1. `href=`: This marks where the message ends and the link information starts.
2. `MyZeroTouchNode;`: This provides the name of the assembly which contains the HTML document. This assembly must be available to Dynamo, which is automatically the case for Zero-Touch node assemblies. The semicolon is used to separate the assembly from the document.
3. `DivisionByZero.html`: This is the name of the HTML file that should be displayed for help. This file must be an `Embedded Resource` in the specified assembly.


### Contextual help best practices
1. Keep help content concise but useful. Provide links to additional online help resources.
2. Give style to HTML documents. Either use your own theme or base on the Dynamo theme used in [the built-in help documents](https://github.com/DynamoDS/Dynamo/tree/master/src/DocumentationBrowserViewExtension/Docs).
3. Avoid adding scripts to HTML documents. For security reasons we forbid their use.
