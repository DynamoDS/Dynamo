<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## En detalle:
`Surface.TrimWithEdgeLoops` recorta la superficie con una colección de una o más PolyCurves cerradas que deben encontrarse todas en la superficie dentro de la tolerancia especificada. Si es necesario recortar uno o más agujeros de la superficie de entrada, debe especificarse un bucle exterior para el contorno de la superficie y un bucle interior para cada agujero. Si es necesario recortar la región entre el contorno de superficie y los agujeros, solo debe especificarse el bucle para cada agujero. Para una superficie periódica sin bucle exterior, como una superficie esférica, la región recortada se puede controlar mediante la inversión de la dirección de la curva de bucle.

Se utiliza la tolerancia establecida al decidir si los extremos de la curva son coincidentes y si una curva y una superficie son coincidentes. La tolerancia proporcionada no puede ser menor que ninguna de las tolerancias utilizadas en la creación de las PolyCurves de entrada. El valor predeterminado 0.0 indica que se utilizará la máxima tolerancia empleada en la creación de las PolyCurves de entrada.

En el ejemplo siguiente, se recortan dos bucles de una superficie, lo que devuelve dos superficies nuevas resaltadas en azul. El control deslizante de número ajusta la forma de las nuevas superficies.

___
## Archivo de ejemplo

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
