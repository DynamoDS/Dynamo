<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Подробности
В приведенном ниже примере Т-сплайновая поверхность создается с помощью узла `TSplineSurface.ByBoxLengths`.
Для выбора грани используется узел `TSplineTopology.FaceByIndex`, а для ее подразделения — узел `TSplineSurface.SubdivideFaces`.
Этот узел разделяет указанные грани на грани меньшего размера: четыре для обычных граней и три, пять или больше для граней NGon.
Если логический входной параметр `exact` имеет значение True, результатом является поверхность, которая при добавлении разделения пытается сохранить исходную форму. Для сохранения формы можно добавить изолинии. Если для параметра задано значение False, узел делит только выбранную грань, что часто приводит к образованию поверхности, отличающейся от исходной.
Узлы `TSplineFace.UVNFrame` и `TSplineUVNFrame.Position` используются для выделения центра разделяемой грани.
___
## Файл примера

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
