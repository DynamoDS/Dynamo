## In profondità
UVParameterAtPoint trova la posizione UV della superficie in corrispondenza del punto di input su una superficie. Se il punto di input non si trova sulla superficie, questo nodo troverà il punto sulla superficie più vicina al punto di input. Nell'esempio seguente, viene prima creata una superficie utilizzando BySweep2Rails. Viene quindi utilizzato un Code Block per specificare un punto in cui trovare il parametro UN. Il punto non si trova sulla superficie, pertanto il nodo utilizza il punto più vicino sulla superficie come posizione di cui trovare il parametro UV.
___
## File di esempio

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

