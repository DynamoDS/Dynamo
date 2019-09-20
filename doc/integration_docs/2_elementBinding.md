## Element Binding and Trace

#### overview

*Trace* is a mechanism in Dynamo core, that is capable of serializing data into the .dyn (dynamo file). Crucially, this data is keyed to the call-sites of nodes within the dynamo graph.

When a Dynamo graph is opened from disk, the trace data saved therein is re-associated with the nodes of the graph.


#### what does it look like?
----

Trace data is serialized into the .dyn file inside a property called Bindings. This is an array of callsites-ids -> data. A callsite is the particular location/instance where a node is called in the designscript virtual machine. It's worth mentioning that nodes in a dynamo graph may be called multiple times and thus multiple callsites might be created for a single node instance.

```
"Bindings": [
    {
      "NodeId": "1e83cc25-7de6-4a7c-a702-600b79aa194d",
      "Binding": {
        "WrapperObject_InClassDecl-1_InFunctionScope-1_Instance0_1e83cc25-7de6-4a7c-a702-600b79aa194d":  "Base64 Encoded Data"
      }
    },
    {
      "NodeId": "c69c7bec-d54b-4ead-aea8-a3f45bea9ab2",
      "Binding": {
        "WrapperObject_InClassDecl-1_InFunctionScope-1_Instance0_c69c7bec-d54b-4ead-aea8-a3f45bea9ab2": "Base64 Encoded Data"
      }
    }
  ],

 
```

 it is *NOT* advisable to depend on the format of the serialized base64encoded data.


#### what problem are we trying to solve.
----


There are many reasons why one would want to save arbitrary data as a result of a function execution, but in this case trace was developed to solve a specific problem that users encounter frequently as they build and iterate on software programs that create elements in host applications.

The problem is one we have called `Element Binding` and the idea is this:

As a user develops and runs a Dynamo graph they will likely be generating new elements in the host application model. For our example
let's say the user has a small program that generates 100 doors in an architectural model. The number and location of these doors is controlled by their program.

The first time the user runs the program, it generates these 100 doors.

Later when the user modifies an input to their program, and re-executes it - their program will *(without element binding)* create 100 new doors, the old doors will still exist in the model along with the new ones.

----

Because Dynamo is a live programming environment and features an `"Automatic"` run mode where changes to the graph trigger a new execution this can quickly clutter a model with the results of many program runs.


We have found this is not usually what users expect, instead with element binding enabled the previous results of a graph execution are cleaned up and deleted or modified. Which one (*delete or modified*) depends on the flexibility of your host's API. With element binding enabled, after the second, third, or 50th run of the user's dynamo program - there are only 100 doors in the model.

This requires more than just being able to serialize data into the .dyn file - and as you will see below there are mechanisms in DynamoRevit built on top of trace to support these rebinding workflows.

----

This is an appropriate time to mention the other important use case of element binding for hosts like Revit. Because elements that were created when element binding was enabled will attempt to keep the existing element IDs (modify existing elements) - logic which was built on top of these elements in the host application will continue to exist after a dynamo program is run. For example:

Let's return to our architectural model example.

Let's run through an example first with element binding disabled - This time the user has a program that generates some architectural walls.

 They run their program, and it generates some walls in the host application. They then leave the dynamo graph, and use normal Revit tools to place some windows into those walls. The windows are bound to these specific walls as part of the Revit model.

The user starts Dynamo back up and runs the graph again - now, like in our last example, they have two sets of walls. The first set has the windows added to it, but the new walls do not.

If element binding had been enabled we can retain the existing work that was done manually in the host application without Dynamo. For example, if binding was enabled when the user ran their program the second time, the walls would be modified, not deleted, and the downstream changes made in the host application would persist. The model would contain walls with windows - instead of two sets of walls in various states.

-----

!!!Image!!!


#### element binding compared to trace
----

#### Trace APIs
----

#### simple Trace example from a node
----
An example of a Dynamo node which uses trace directly is provided here in the [DynamoSamples repo](https://github.com/DynamoDS/DynamoSamples/blob/master/src/SampleLibraryZeroTouch/Examples/TraceExample.cs)

The summary of the class there explains the gist of what trace is all about:

```
  /*
     * After a graph update, Dynamo typically disposes of all
     * objects created during the graph update. But what if there are 
     * objects which are expensive to re-create, or which have other
     * associations in a host application? You wouldn't want those those objects
     * re-created on every graph update. For example, you might 
     * have an external database whose records contain data which needs
     * to be re-applied to an object when it is created in Dynamo.
     * In this example, we use a wrapper class, TraceExampleWrapper, to create 
     * TraceExampleItem objects which are stored in a static dictionary 
     * (they could be stored in a database as well). On subsequent graph updates, 
     * the objects will be retrieved from the data store using a trace id stored 
     * in the trace cache.
     */
```

This example uses the trace apis in DynamoCore directly to store some data whenever a particular node executes. In this case a dictionary plays the part of the host application model - like Revit's model database.

The rough setup is:

A static util class `TraceExampleWrapper` is imported as a node into Dynamo. 
it contains a single method `ByString` which creates `TraceExampleItem` - These are regular .net objects which contain a `description` property.

Each `TraceExampleItem` is serialized into trace represented as a `TraceableId` - this is just a class containing an `IntId` which is marked `[Serializeable]` so it can be serialized with `SOAP` Formatter.
see [here for more info on the serializable attribute](https://docs.microsoft.com/en-us/dotnet/api/system.serializableattribute?view=netframework-4.8)



The `TraceableObjectManager` is similar to the `ElementBinder` in `DynamoRevit` - this manages the relationship between the host's document model and the data we have stored in dynamo trace.

The flow of two consecutive executions of graph that creates a single `TraceExampleItem` looks like this:

![first call](../images/Trace-first-call.png)

![second call](../images/Trace-second-call.png)




#### Element binding implementation example

-----
Let's quickly take a look at what a node that is using element binding looks like when implemented for DynamoRevit, this is analogous to the type of node used above in the given examples.


---


``` c#
    private void InitWall(Curve curve, Autodesk.Revit.DB.WallType wallType, Autodesk.Revit.DB.Level baseLevel, double height, double offset, bool flip, bool isStructural)
        {
            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(Document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var wallElem =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Wall>(Document);

            bool successfullyUsedExistingWall = false;
            //There was a modelcurve, try and set sketch plane
            // if you can't, rebuild 
            if (wallElem != null && wallElem.Location is Autodesk.Revit.DB.LocationCurve)
            {
                var wallLocation = wallElem.Location as Autodesk.Revit.DB.LocationCurve;
                <SNIP>

                    if(!CurveUtils.CurvesAreSimilar(wallLocation.Curve, curve))
                        wallLocation.Curve = curve;

                  <SNIP>
                
            }

            var wall = successfullyUsedExistingWall ? wallElem :
                     Autodesk.Revit.DB.Wall.Create(Document, curve, wallType.Id, baseLevel.Id, height, offset, flip, isStructural);
            InternalSetWall(wall);

            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalWall);
        }
```

The above code illustrates a sample constructor for a wall element - this constructor would be called from a node in dynamo like:
`Wall.byParams`

The important phases of the constructor's execution as they relate to element binding are:

1. use the `elementBinder` to check if there are any previously created objects which were bound to this callsite in a past run.
`ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Wall>`
2. if so ,then try to modify that wall instead of creating a new one.

```
 if(!CurveUtils.CurvesAreSimilar(wallLocation.Curve, curve))
                        wallLocation.Curve = curve;
```

3. else create a new a wall.
```
  var wall = successfullyUsedExistingWall ? wallElem :
                     Autodesk.Revit.DB.Wall.Create(Document, curve, wallType.Id, baseLevel.Id, height, offset, flip, isStructural);
                     
```

4. delete the old element we just retrieved from trace, and add our new one so we can look up this element in the future:
```
 ElementBinder.CleanupAndSetElementForTrace(Document, InternalWall);
 ```

