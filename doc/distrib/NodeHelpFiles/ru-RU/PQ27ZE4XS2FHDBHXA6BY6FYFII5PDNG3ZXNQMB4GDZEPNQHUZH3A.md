<!--- Autodesk.DesignScript.Geometry.Surface.BySweep(profile, path, cutEndOff) --->
<!--- PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A --->
## Подробности
`Surface.BySweep (profile, path, cutEndOff)` создает поверхность путем сдвига входной кривой вдоль заданной траектории. Входной параметр `cutEndOff` определяет, нужно ли обрезать конец сдвига и сделать его перпендикулярным траектории.

В примере ниже синусоида используется в направлении Y в качестве кривой профиля. Эта кривая поворачивается на -90 градусов вокруг мировой оси Z, чтобы использовать ее как криволинейную траекторию. Surface BySweep перемещает кривую профиля вдоль криволинейной траектории, создавая поверхность.


___
## Файл примера

![Surface.BySweep](./PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A_img.jpg)
