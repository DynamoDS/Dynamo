## In Depth
`List.AddItemToFront` inserts a given item to the beginning of a given list. The new item has index 0, while the original items are all shifted by an index of 1. Note that if the item to be added is a list, the list is added as a single object, producing a nested list. To combine two lists together into a single flat list, see `List.Join`. 

In the example below, we use a code block to generate a range of numbers from 0 to 5, stepping by 1. We then add a new item, the number 20, to the front of that list using `List.AddItemToFront.`
___
## Example File

![List.AddItemToFront](./DSCore.List.AddItemToFront_img.jpg)