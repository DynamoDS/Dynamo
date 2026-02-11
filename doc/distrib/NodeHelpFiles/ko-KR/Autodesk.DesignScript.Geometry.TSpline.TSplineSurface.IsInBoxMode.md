## 상세
상자 모드 및 매끄럽게 모드는 T-Spline 표면을 보는 두 가지 방법입니다. 매끄럽게 모드는 T-Spline 표면의 실제 모양이고 모델의 심미성과 치수를 미리 보는 데 유용합니다. 반면 상자 모드는 표면 구조에 대한 인사이트를 제공하며 표면 구조에 대한 이해를 더 높여줄 뿐만 아니라 크거나 복잡한 형상을 더 빠르게 미리 볼 수 있게 해줍니다. 상자 모드 및 매끄럽게 모드는 `TSplineSurface.EnableSmoothMode`와 같은 노드를 포함하는 초기 T-Spline 표면을 작성하는 순간 또는 그 이후에 제어할 수 있습니다.

T-Spline이 부적합해지면 미리보기가 자동으로 상자 모드로 전환됩니다. `TSplineSurface.IsInBoxMode` 노드는 표면이 부적합해졌는지를 식별하는 또 다른 방법입니다.

아래 예에서는 T-Spline 평면이 True로 설정된 `smoothMode` 입력을 사용하여 작성됩니다. 해당 면 중 두 개가 삭제되어 표면이 부적합해집니다. 미리보기만으로 알 수는 없지만 표면 미리보기가 상자 모드로 전환됩니다. `TSplineSurface.IsInBoxMode` 노드는 표면이 상자 모드인지 확인하는 데 사용됩니다.
___
## 예제 파일

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
