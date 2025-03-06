## Подробности
`PolySurface.BySweep (rail, crossSection)` возвращает объект PolySurface путем сдвига списка соединенных непересекающихся линий вдоль направляющей. Входной элемент `crossSection` может получить список соединенных кривых, которые должны встречаться в начальной или конечной точке, или узел не вернет объект PolySurface. Этот узел аналогичен узлу `PolySurface.BySweep (rail, profile)`, единственное отличие в том, что входной параметр `crossSection` принимает список кривых, а параметр `profile` — только одну кривую.

В примере ниже объект PolySurface создается путем сдвига вдоль дуги.


___
## Файл примера

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
