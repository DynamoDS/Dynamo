## In Depth
If acts as a conditional control node. The 'test' input takes a boolean value, while the 'true' and 'false' inputs can accept any data type. If the test value is 'true', the node will return the item from the 'true' input, if test is 'false', the node will return the item from the 'false' input. In the example below, we first generate a list of random numbers between zero and 99. The number of items in the list is controlled by an integer slider. We use a code block with the formula 'x%a==0' to test for divisibility by a second number, determined by a second number slider. This generates a list of boolean values corresponding to whether the items in the random list are divisible by the number determined by the second integer slider. This list of boolean values is used as the 'test' input for an If node. We use a default Sphere as the 'true' input, and a default Cuboid as the 'false' input. The result from the If node is a list of either spheres or cuboids. Finally, we use a Translate node to spread the list of geometries apart.

IF replicates on all nodes AS THOUGH SET TO SHORTEST. You can see the reason for this in the attached examples, especially when looking at what the results are when LONGEST is applied to a formula node and the "short" branch of the conditional is passes through. These changes were also made to allow predictable behavior when using single boolean inputs or a list of booleans.
___
## Example File

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

