## In-Depth

V níže uvedeném příkladu se provede operace Zrušit svar u řady hran povrchu T-Spline. V důsledku toho jsou vrcholy vybraných hran nespojité. Na rozdíl od možnosti Zrušit vyostření, která vytváří ostrý přechod kolem hrany při zachování spojení, možnost Zrušit svar vytvoří nespojitost. Toto je možné prokázat porovnáním počtu vrcholů před a po provedení operace. Všechny následné operace na hranách nebo vrcholech, u kterých byly zrušeny svary, budou také znázorňovat, že povrch není propojen podél hrany beze svaru.

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
