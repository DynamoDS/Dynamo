# librarie.js
Reusable lightweight library component based on React.js

## Set up
Installing all dependencies

    $ npm install

Note: If you are having trouble running `npm install` and are outside the adsk network, please try removing the `.npmrc` file.

## Build and run librarie.js

### Development
- Build and serve development source scripts

    `$ npm run serve`
    
    Generates the development script `./dist/librarie.js` and and executes the file `./index.js` using node.

- Navigate to http://localhost:3456 in a web browser.

## Production

- Build production source scripts
    `npm run build`
  
    Generates the development script `./dist/librarie.min.js`, used as prodution artifact in Dynamo.

## Run Tests

####  Run all tests

    `$ npm run test`

    Test file is at folder `./__tests__/`
####  Run test for utility functions

    `$ npm run utiltest`

    Test file is at `./__tests__/mochatest/libraryUtilitiesTest.ts`

    Run all tests command does run all the tests, but it does not output libraryUtilitiesTest.ts console messages. When any test fail, it is unclear which one does and why. When that happens, use the command above to run just the utility tests.

- Example test  

 ```typescript
    // Importing the object to be tested
     import { LibraryItem } from '../src/components/LibraryItem';

    // A test case that tests object properties against their intended value 
     it("should create a libraryItem", function () {
     let libContainer = libController.createLibraryContainer(layoutSpecsJson, loadedTypesJson);
     let libraryItem = mount(<LibraryItem libraryContainer={libContainer} data={data} />); 
     expect(libraryItem.props().data.childItems[0].text).to.equal("Child0"); 
    }
 ```
- Instructions to write a tests are found at https://github.com/DynamoDS/librarie.js/wiki/Author-and-run-tests-for-librarie.js

### Generate Third Party License Info
* to generate about box html files use `npm run generate_license`, this will output alternative about box files to `license_output/` One will contain the full transitive production dep list, the other will contain the direct production deps.


## Usage
There are few ways in which library view (i.e. `LibraryContainer` object) can be constructed. Regardless of which method is used, the caller should first call `LibraryEntryPoint.CreateLibraryController` method to create `LibraryController` before obtaining an instance of `LibraryContainer` object.

```html
    <script>
        let libController = LibraryEntryPoint.CreateLibraryController();
    </script>
```

### Method 1
Constructing library view given the ID of an existing HTML element:

```html
    <div id="libraryContainerPlaceholder"></div>
    <script>
        let libController = LibraryEntryPoint.CreateLibraryController();
        let libContainer = libController.createLibraryByElementId("libraryContainerPlaceholder",
            loadedTypesJsonObject, layoutSpecsJsonObject);
    </script>
```

`LibraryController.createLibraryByElementId` function takes the following values as its arguments:

- `htmlElementId` - The ID of an HTML whose content is to be replaced with `LibraryContainer`.

- `loadedTypesJsonObject` - The JSON object to be used by library view as [Loaded Data Types](./docs/loaded-data-types.md). This argument is optional, but if it is supplied, the corresponding `layoutSpecsJsonObject` must also be supplied. If this argument is not supplied, see *Method 2* below for details on how it can be supplied at a later time.

- `layoutSpecsJsonObject` - The JSON object to be used by library view as [Layout Specification](./docs/layout-specs.md). This argument is optional, but if it is supplied, the corresponding `loadedTypesJsonObject` must also be supplied. If this argument is not supplied, see *Method 2* below for details on how it can be supplied at a later time.

### Method 2
Constructing a library view for consumption by other React.js components (e.g. hosting the library within a React.js tab control). This method creates a valid `JSX.Element` object so that it can be directly embedded under another React.js element. For details of `loadedTypesJsonObject` and `layoutSpecsJsonObject`, please refer to the above section.

```html
    <script>
        let libController = LibraryEntryPoint.CreateLibraryController();
        let libContainer = libController.createLibraryContainer();

        let aReactJsTabContainer = ...;
        aReactJsTabContainer.addTabPage(libContainer);

        // Supply loaded types and layout specs at a much later time.
        let append = false; // Replace existing contents instead of appending.
        libController.setLoadedTypesJson(loadedTypesJsonObject, append);
        libController.setLayoutSpecsJson(layoutSpecsJsonObject, append);
        libController.refreshLibraryView(); // Refresh library view.
    </script>
```

## Sample usage of librarie.js
The following simple HTML code illustrates the way to embed library view into an existing web page.

```html
<!DOCTYPE html>
<html>
    <head>
        <style>
            body {
                padding: 0;
                margin: 0;
                background-color: #353535;
            }
        </style>
    </head>
    <body>
        <!-- This is where the library view should appear -->
        <div id="libraryContainerPlaceholder"></div>

        <!-- The main library view compoment -->
        <script src = './dist/librarie.min.js'></script>

        <!-- Initialize the library view component -->
        <script>
            // Through client specific logic download json objects
            let loadedTypesJsonObject = getLoadedTypesJsonObject();
            let layoutSpecsJsonObject = getLayoutSpecsJsonObject();

            let libController = LibraryEntryPoint.CreateLibraryController();

            libController.on("itemClicked", function (item) {
                console.log(item); // Subscribed to click event
            })

            let libContainer = libController.createLibraryByElementId(
                "libraryContainerPlaceholder", // htmlElementId
                loadedTypesJsonObject,
                layoutSpecsJsonObject);

        </script>

    </body>
</html>
```

## Registering event handlers

`LibraryController` object supports several events. So subscribe to an event of interest, do the following:

```js
// 'libController' is an instance of 'LibraryController' previously constructed. 
libController.on("someEventName", function(data) {
    // Handle 'someEventName' here, the argument 'data` is event dependent.
});
```

The event names are also defined as string properties in the controller.

### Event 'itemClicked'

This event is raised when a library item is clicked. The registered event handler will be called with the following argument:

- `contextData`: This is the value of `contextData` passed through [Loaded Data Types](./docs/loaded-data-types.md) JSON data for the corresponding item.

```js
libController.on("itemClicked", function(contextData) {
    console.log(contextData);
})
```

The string property for the event name is: ItemClickedEventName. So the following achieves the same:

```js
libController.on(libController.ItemClickedEventName, function(contextData) {
    console.log(contextData);
})
```

### Event 'searchTextUpdated'

This event is raised when user starts typing on the search bar, and the display mode of SearchView is `list`. In this event, it should call a search algorithm from some other components, and return a list of [Search Result Items](./docs/search-items.md) in JSON format to the caller.

- `searchText`: This is the value of state `searchText` in `SearchView` component, which is a string value that user has entered in the search bar.

 ```js
libController.on("searchTextUpdated", function (searchText) {
    console.log(searchText);
    return null;
});
```

The string property for the event name is: SearchTextUpdatedEventName. So the following achieves the same:

```js
libController.on(libController.SearchTextUpdatedEventName, function(contextData) {
    console.log(contextData);
})
```

### Event 'itemMouseEnter'

This event is raised when the mouse enters the range of one library item.

 ```js
libController.on("itemMouseEnter", function (arg) {
    console.log("Data: " + arg.data);
    console.log("Rect(top, left, bottom, right): " + arg.rect.top + "," + arg.rect.left + "," + arg.rect.bottom + "," + arg.rect.right);
});
```

The string property for the event name is: ItemMouseEnterEventName. So the following achieves the same:

```js
libController.on(libController.ItemMouseEnterEventName, function(arg) {
    console.log("Data: " + arg.data);
    console.log("Rect(top, left, bottom, right): " + arg.rect.top + "," + arg.rect.left + "," + arg.rect.bottom + "," + arg.rect.right);
})
```

### Event 'itemMouseLeave'

This event is raised when the mouse leaves the range of one library item.

```js
libController.on("itemMouseLeave", function (arg) {
    console.log("Data: " + arg.data);
    console.log("Rect(top, left, bottom, right): " + arg.rect.top + "," + arg.rect.left + "," + arg.rect.bottom + "," + arg.rect.right);
});
```

The string property for the event name is: ItemMouseLeaveEventName. So the following achieves the same:

```js
libController.on(libController.ItemMouseLeaveEventName, function(arg) {
    console.log("Data: " + arg.data);
    console.log("Rect(top, left, bottom, right): " + arg.rect.top + "," + arg.rect.left + "," + arg.rect.bottom + "," + arg.rect.right);
})
```

### Event 'itemSummaryExpanded'

This event is raised when user clicks on the expand icon displayed on the right of a leaf library item or search result item. This event should return data for showing summary, which contains `inputParameters`, `outputParameters` and `description`, follwing the format in the example below.

- `arg`: This contains a callback function `setDataCallback` and `contextData` of the item. The function `setDataCallback` will send the data back to librarie.js for displaying ItemSummary.

```js
libController.on(libController.ItemSummaryExpandedEventName, function (arg) {
    var data = arg.contextData;

    // The final data sent back to librarie.js should follow the format in the example.
    var finalData = {
        "inputParameters": [
            {
                "name": "c1",
                "type": "Color"
            },
            {
                "name": "c2",
                "type": "Color"
            }
        ],
        "outputParameters": [
            "Color"
        ],
        "description": "Construct a Color by combining two input Colors."
    };

    // Send data back to librarie.js
    arg.setDataCallback(finalData);
});
```

### Event 'sectionIconClicked'

This event is raised when used clicks on the icon displayed on the right of a section header.

- `sectionText`: This is the `text` property of the section that is clicked, which is defined in the [Layout Specification](./docs/v0.0.1/layout-specs.md).

```js
libController.on(libController.SectionIconClickedEventName, function (sectionText) {
     console.log(sectionText, "icon clicked");
     return null;
});
```

## Overriding the default search function

The default search function can be overriden by setting searchLibraryItemsHandler of the controller.


 ```js
libController.searchLibraryItemsHandler = function (text, callback) {
}
```

- `text`: This is the input string for searching.
- `callback`: This is defined as SearchLibraryItemsCallbackFunc. It need to be called with the search result in a json format as defined
in https://github.com/DynamoDS/librarie.js/blob/master/docs/loaded-data-types.md.
