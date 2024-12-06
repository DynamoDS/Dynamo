## In Depth
`List.FilterByBoolMask ` takes two lists as inputs. The first list is split into two separate lists according to a corresponding list of Boolean (True or False) values. Items from the `list` input that correspond to True in the `mask` input are directed to the output labeled In, while those items that correspond to a False value are directed to the output labeled `out`. 

In the example below, `List.FilterByBoolMask ` is used to pick out wood and laminate from a list of materials. We first compare two lists to find matching items, then use an `Or` operator to check for True list items. Then, list items are filtered depending on whether they are wood or laminate, or something else.
___
## Example File

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)