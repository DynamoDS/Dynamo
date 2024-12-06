## In Depth
Geometry ImportFromSAT imports Geometry to Dynamo from a SAT file type. This node takes a filePath as input, and also accepts a string with a valid file path. For the example below, we previously exported geometry to a SAT file (see ExportToSAT). The file name we chose was example.sat and it was exported to a folder on the users desktop. In the example, we show two different nodes used to import geometry from a SAT file. One has a filePath as the input type, and the other has a 'file' as the input type. The filePath is created using a FilePath node, which can select a file by clicking the Browse button. In the second example, we specify the file path manually by using a string element.
___
## Example File

![ImportFromSAT (filePath)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(filePath)_img.jpg)

