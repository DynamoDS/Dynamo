<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` entfernt die Reflexionen aus der Eingabe `tSplineSurface`. Durch das Entfernen von Reflexionen wird die Form nicht geändert, sondern die Abhängigkeit zwischen den reflektierten Teilen der Geometrie gelöst, sodass Sie sie unabhängig bearbeiten können.

Im folgenden Beispiel wird zuerst eine T-Spline-Oberfläche erstellt, indem axiale und radiale Reflexionen angewendet werden. Die Oberfläche wird dann an den Block `TSplineSurface.RemoveReflections` übergeben, wodurch die Reflexionen entfernt werden. Um zu veranschaulichen, wie sich dies auf spätere Änderungen auswirkt, wird einer der Scheitelpunkte mithilfe eines `TSplineSurface.MoveVertex`-Blocks verschoben. Infolge der Entfernung der Reflexionen von der Oberfläche wird nur ein Scheitelpunkt geändert.

## Beispieldatei

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
