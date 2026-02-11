<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## En detalle:
`Surface.Thicken (surface, thickness, both_sides)` crea un sólido mediante el desfase de una superficie según la entrada `thickness` y tapando los extremos para cerrar el sólido. Este nodo tiene una entrada adicional para especificar si se debe engrosar o no en ambos lados. La entrada `both_sides` utiliza un valor booleano, "True" (verdadero) para engrosar en ambos lados y "False" (falso) para engrosar en uno de ellos. Tenga en cuenta que el parámetro `thickness` determina el grosor total del sólido final, por lo que si `both_sides` se establece en "True" (verdadero), el resultado se desfasará desde la superficie original la mitad del grosor de entrada en ambos lados.

En el ejemplo siguiente, creamos primero una superficie mediante `Surface.BySweep2Rails`. A continuación, creamos un sólido mediante un control deslizante de número para determinar la entrada `thickness` del nodo `Surface.Thicken`. Un conmutador booleano controla si se engrosa en ambos lados o solo en uno.

___
## Archivo de ejemplo

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
