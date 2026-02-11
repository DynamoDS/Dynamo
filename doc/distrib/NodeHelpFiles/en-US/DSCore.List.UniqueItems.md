## In Depth
`List.UniqueItems` removes all duplicate items from an input list by creating a new list that only includes items that occur only once on the original list.

In the example below, we use `Math.RandomList` to first generate a list of random numbers between 0 and 1. We then multiply by 10 and use a `Math.Floor` operation to return a list of random integers between 0 and 9, with many of them repeated multiple times. We use `List.UniqueItems` to create a list in which each integer only occurs once. The order of the output list is based on the first found instance of an item
___
## Example File

![List.UniqueItems](./DSCore.List.UniqueItems_img.jpg)