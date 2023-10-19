## In Depth
`List.RemoveIfNot` returns a list that retains items that match the given element type and removes all other items on the original list.   

You may need to use the full node path, such as `Autodesk.DesignScript.Geometry.Surface`, in the `type` input to remove items. To retrieve the paths for list items, you can input your list into an `Object.Type` node.

In the example below, `List.RemoveIfNot` returns a list with one line, removing the point elements from the original list because they do not match the specified type.
___
## Example File

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)