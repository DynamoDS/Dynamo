<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## 상세
상자 모드 및 매끄럽게 모드는 T-Spline 표면을 보는 두 가지 방법입니다. 매끄럽게 모드는 T-Spline 표면의 실제 모양이고 모델의 심미성과 치수를 미리 보는 데 유용합니다. 반면 상자 모드는 표면 구조에 대한 인사이트를 제공하며 표면 구조에 대한 이해를 더 높여줄 뿐만 아니라 크거나 복잡한 형상을 더 빠르게 미리 볼 수 있게 해줍니다. `TSplineSurface.EnableSmoothMode` 노드를 통해 형상 개발의 여러 단계에서 이 두 가지 미리보기 상태 사이를 전환할 수 있습니다.

아래 예에서는 베벨 작업이 T-Spline 상자 표면에서 수행됩니다. 먼저 그 결과가 상자 모드(상자 표면의 `SmoothMode` 입력이 false로 설정됨)로 시각화되어 모양의 구조를 더 잘 이해할 수 있습니다. 그런 다음 매끄럽게 모드가 `TSplineSurface.EnableSmoothMode` 노드를 통해 활성화되고 두 모드를 동시에 미리 볼 수 있도록 결과가 오른쪽으로 이동됩니다.
___
## 예제 파일

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
