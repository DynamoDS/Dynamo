## 상세
폭과 높이가 지정된 From Pixels는 색상의 입력 단순 리스트에서 이미지를 작성합니다. 여기서 각 색상은 1픽셀이 됩니다. 폭에 높이를 곱한 값은 총 색상 수와 같아야 합니다. 아래 예에서는 먼저 ByARGB 노드를 사용하여 색상 리스트를 작성합니다. 코드 블록은 0~255의 값 범위를 작성하는데, r 및 g 입력에 연결되면 검은색에서 노란색까지 일련의 색상이 생성됩니다. 여기서는 폭이 8인 이미지를 작성합니다. Count 노드 및 Division 노드를 사용하여 이미지의 높이를 결정합니다. Watch Image 노드를 사용하면 작성된 이미지를 미리 볼 수 있습니다.
___
## 예제 파일

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

