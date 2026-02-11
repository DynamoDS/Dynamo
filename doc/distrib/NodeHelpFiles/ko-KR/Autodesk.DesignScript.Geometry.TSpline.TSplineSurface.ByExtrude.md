## In-Depth
아래 예에서는 T-Spline 표면이 지정된 프로파일 `curve'의 돌출로 작성됩니다. 곡선은 열리거나 닫힐 수 있습니다. 돌출은 제공된 `direction`으로 수행되며, 양방향일 수 있고 `frontDistance` 및 `backDistance` 입력으로 제어됩니다. 스팬은 지정된 `frontSpans` 및 `backSpans`를 사용하여 돌출의 두 방향에 대해 개별적으로 설정할 수 있습니다. 곡선을 따라 표면의 정의를 설정하기 위해 `profileSpan`은 면 수를 제어하고 `uniform`은 이를 균일한 방식으로 분산하거나 곡률을 고려합니다. 마지막으로 'InSmoothMode`는 표면이 매끄럽게 모드 또는 상자 모드로 표시되는지를 제어합니다.

## 예제 파일
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
