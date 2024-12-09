<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## 상세
아래 예에서는 T-Spline 표면의 2개의 절반이 `TSplineSurface.ByCombinedTSplineSurfaces` 노드를 사용하여 하나로 결합됩니다. 대칭 기준면을 따라 있는 정점이 겹치는데, 이는 `TSplineSurface.MoveVertices` 노드를 사용하여 정점 중 하나를 이동하면 표시됩니다. 이를 복구하기 위해 `TSplineSurface.WeldCoincidentVertices` 노드를 사용하여 용접이 수행됩니다. 이제 정점 이동의 결과는 달라지며, 더 나은 미리 보기를 위해 측면으로 이동됩니다.
___
## 예제 파일

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
