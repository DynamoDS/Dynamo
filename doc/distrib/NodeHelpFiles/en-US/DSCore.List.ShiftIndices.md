## In Depth
`List.ShiftIndices` shifts the position of the items in a list by the `amount` input. A positive number in the `amount` input shifts the numbers up, while a negative number shifts the indices backwards. The items wrap around, causing items at the back of the list to wrap to the beginning. 

In the example below, we first generate a list with `Range`, then shift the indices forward by 3. The final 3 numbers from the original list wrap around to become the first 3 numbers on the new list.
___
## Example File

![List.ShiftIndices](./DSCore.List.ShiftIndices_img.jpg)