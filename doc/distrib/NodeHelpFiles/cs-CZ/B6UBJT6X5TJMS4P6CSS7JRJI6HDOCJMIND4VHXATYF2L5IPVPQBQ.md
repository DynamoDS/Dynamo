<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
Uzel `TSplineSurface.RemoveReflections` odebere odrazy ze vstupu `tSplineSurface`. Odstraněním odrazů se tvar nezmění, ale přeruší se závislost mezi odráženými částmi geometrie, čili je možné je nezávisle upravovat.

V níže uvedeném příkladu se nejprve vytvoří povrch T-Spline použitím osových a radiálních odrazů. Povrch je poté předán do uzlu `TSplineSurface.RemoveReflections`, čímž se odeberou odrazy. Ke znázornění vlivu tohoto kroku na další úpravy je jeden z vrcholů přesunut pomocí uzlu `TSplineSurface.MoveVertex`. Kvůli odebrání odrazů z povrchu je upraven pouze jeden vrchol.

## Vzorový soubor

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
