<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseVertices --->
<!--- ZLORG7PG4XWDBYXJHH7YVPDCIU4QYMZWAMABFPVWNAZ7VTQTX2YQ --->
## In-Depth
V níže uvedeném příkladu je vytvořen tvar s nevyostřenými hranami pomocí uzlu `TSplineSurface.BuildFromLines` a s nastavením vstupu `creaseOuterVertices` na hodnotu False. Vrchol s indexem 1 je poté vyostřen pomocí uzlu `TSplineSurface.CreaseVertices` a tvar je přesunut doprava, aby byl lépe vidět.

## Vzorový soubor

![Example](./ZLORG7PG4XWDBYXJHH7YVPDCIU4QYMZWAMABFPVWNAZ7VTQTX2YQ_img.jpg)
