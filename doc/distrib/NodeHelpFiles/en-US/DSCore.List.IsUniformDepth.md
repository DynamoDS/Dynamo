## In Depth
`List.IsUniformDepth` returns a Boolean value based on whether the list's depth is consistent, which means that each list has the same number of lists nested inside of it. 

In the example below, two lists are compared, one of uniform depth and one of non-uniform depth, to show the difference. The uniform list contains three lists with no nested lists. The non-uniform list contains two lists. The first list has no nested lists, but the second one has two nested lists. The lists at [0] and [1] are not equal in depth, so `List.IsUniformDepth` returns False.
___
## Example File

![List.IsUniformDepth](./DSCore.List.IsUniformDepth_img.jpg)