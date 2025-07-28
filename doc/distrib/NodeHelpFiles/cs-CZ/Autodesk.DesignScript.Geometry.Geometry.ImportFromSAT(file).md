## Podrobnosti
Uzel Geometry ImportFromSAT importuje geometrii do aplikace Dynamo ze souboru typu SAT. Tento uzel přijímá jako vstup hodnotu `file` a také přijímá řetězec s platnou cestou souboru. Například níže jsme dříve exportovali geometrii do souboru SAT (viz část ExportToSAT). Název souboru, který jsme vybrali, byl example.sat a byl exportován do složky na uživatelské ploše. V tomto příkladu ukážeme dva různé uzly používané k importu geometrie ze souboru SAT. Jeden má jako vstupní typ hodnotu `filePath` a druhý má jako vstupní typ hodnotu `file`. Hodnota `filePath` je vytvořena pomocí uzlu FilePath, který může vybrat soubor pomocí tlačítka Procházet. V druhém příkladu určíme soubor ručně pomocí prvku řetězce.
___
## Vzorový soubor

![ImportFromSAT (file)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(file)_img.jpg)

