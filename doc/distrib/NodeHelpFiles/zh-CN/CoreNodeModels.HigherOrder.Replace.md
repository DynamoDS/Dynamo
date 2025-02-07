## 详细
“Replace By Condition”将获取给定列表，并使用给定条件对每个项目求值。如果条件计算结果为“true”，则输出列表中的相应项目将替换为“replaceWith”输入中指定的项目。在下例中，我们使用“Formula”节点并输入公式“x%2==0”，该公式查找给定项除以 2 之后的余数，然后检查该余数是否等于零。对于偶数，该公式将返回“true”。请注意，输入 x 保留为空。通过使用该公式作为“ReplaceByCondition”节点中的条件，将生成一个输出列表，其中每个偶数会替换为指定项(在本例中为整数 10)。
___
## 示例文件

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

