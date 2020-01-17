## LibraryViewExtension(MSWebBrowser)

### what is this:
    A version of the LibraryViewExtension which displays the Librarie.JS component (Dynamo node library) using the System.Windows.Controls.WebBrowser control. This uses the Trident rendering engine.

### why is this:
    Some applications which integrate Dynamo have inflexible dependencies on CEF or CEFSharp of various versions. As an alternative solution to falling back to the older Dynamo 1.x WPF based library this extension gives users a similar experience and leverages the same librarie.js codebase with minimal changes.

### Architecture vs CEFSharp.

* unlike in CEFSharp there are no resourceHandlers registered which the browser control will call to resolve local requests. Instead all local resources are either resolved by calling c# methods directly or by embedding them as base64 or js/html strings into the html passed to the browser.
* similar to CEFSharp - an `ObjectForScripting` is registered with the browser which lets js code call c# methods.
* to allow data to be passed back and forth everything is serialized to strings. If data needs to move between c# to js - then the c# side must call `InvokeScript` and pass the data as an argument - this means that sometimes function calls are split into two halves. ie - The js side calls some c# method `firstHalf()` - c# context computes some result and then `invokeScript(("secondHalf", resultData)` which returns the result into the js context by calling a second function. Local variables are saved between the function calls on the js side by declaring them outside of either function scope.

### modifications necessary to librarie.js

* compiled with an import for `core-js`
* cache of originalUrl on the HTMLImage elements when they fail to load and .src is set to DefaultIcon.
* the version of librarie.min.js embedded into this repo has these two changes. (https://github.com/DynamoDS/librarie.js/pull/133)

### known issues

* shrink use of `core-js` to just replace the required polyfills or remove completely and use a standard polyfill if we only need to replace a few functions.
* no tests.
* pull duplicated classes out into libraryExtensionCore.
* minimal manual testing compared LibraryViewExtension which has been in production for 1.5 years.

### how to use this:

copy the extension manifest and bin directory of this project to the `Dynamo/viewExtensions/` folder of Dynamo. The manifest expects the extension binary to live in `./librarymsWebBrowser/LibraryViewExtensionMSWebBrowser.dll`.