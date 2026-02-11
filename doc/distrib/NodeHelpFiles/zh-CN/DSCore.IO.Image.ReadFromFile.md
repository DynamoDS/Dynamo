## 详细
“Read From File”会将文件用作输入。该文件应为包含以逗号分隔的数据的文本文件。文件中的不同行对应于外部列表，而每行中的各个值对应于内部列表。在下例中，我们先使用“File Path”节点和“File.FromPath”节点，来创建一个指向文本文件的文件对象。然后，我们使用“ReadFromFile”节点，以基于 CSV 文件创建列表。
___
## 示例文件

![ReadFromFile](./DSCore.IO.Image.ReadFromFile_img.jpg)

