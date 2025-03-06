<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` remove as reflexões da entrada `tSplineSurface`. A remoção de reflexões não modifica a forma, mas quebra a dependência entre as partes refletidas da geometria, permitindo que você as edite de forma independente.

No exemplo abaixo, é criada uma superfície da T-Spline primeiro aplicando reflexões axiais e radiais. Em seguida, a superfície é passada para o nó `TSplineSurface.RemoveReflections`, removendo assim as reflexões. Para ilustrar como isso afeta as alterações posteriores, um dos vértices é movido usando um nó `TSplineSurface.MoveVertex`. Devido às reflexões que estão sendo removidas da superfície, somente um vértice é modificado.

## Arquivo de exemplo

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
