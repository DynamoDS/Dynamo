<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## Подробности
В приведенном ниже примере плоская Т-сплайновая поверхность с выдавленными, разделенными и вытянутыми вершинами и гранями проверяется с помощью узла `TSplineTopology.DecomposedFaces`, который возвращает список следующих типов граней, содержащихся в T-сплайновой поверхности:

— `all`: список всех граней;
— `regular`: список обычных граней;
— `nGons`: список граней NGon;
— `border`: список граней границ;
— `inner`: список внутренних граней.

Узлы `TSplineFace.UVNFrame` и `TSplineUVNFrame.Position` используются для выделения различных типов граней поверхности.
___
## Файл примера

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
