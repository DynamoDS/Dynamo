## 详细
“Solid.Repair”将尝试修复存在无效几何图形的实体，还可能会执行优化。“Solid.Repair”将返回一个新的实体对象。

如果在对输入的几何图形或转换的几何图形执行操作时遇到错误，则此节点非常有用。

在下面的示例中，“Solid.Repair”用于从 **.SAT** 修复几何图形。文件中的几何图形无法进行布尔运算或修剪，“Solid.Repair”将清理导致失败的任何*无效几何图形*。

通常，您不需要对在 Dynamo 中创建的几何图形使用此功能，而仅对来自外部源的几何图形使用此功能。如果您发现情况并非如此，请向 Dynamo 团队 GitHub 报告错误
___
## 示例文件

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
