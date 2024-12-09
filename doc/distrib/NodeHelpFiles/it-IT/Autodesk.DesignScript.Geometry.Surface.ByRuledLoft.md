## In profondità
Surface.ByRuledLoft utilizza un elenco ordinato di curve come input ed esegue il loft di una superficie rigata con linee rette tra le curve. Rispetto a ByLoft, ByRuledLoft può essere leggermente più veloce, ma la superficie risultante è meno levigata. Nell'esempio seguente, si inizia con una linea lungo l'asse X. Si trasla questa linea in una serie di linee che seguono una curva seno nella direzione Y. L'utilizzo di questo elenco risultante di linee come input per Surface.ByRuledLoft produce una superficie con segmenti di linea retta tra le curve di input.
___
## File di esempio

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

