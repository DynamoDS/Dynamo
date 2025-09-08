## In Depth
WriteText will write a string to a specified file. If the file does not exists, this node will create the file. To create newlines in the output file, we can use the escape character '\r\n'. In the example below, we have a list of strings that we want to write as three separate lines in a text file. We join the list into a single string using '\r\n' as the separator. We then use a WriteText node to write this to a text file.
___
## Example File

![WriteText](./DSCore.IO.FileSystem.WriteText_img.jpg)

