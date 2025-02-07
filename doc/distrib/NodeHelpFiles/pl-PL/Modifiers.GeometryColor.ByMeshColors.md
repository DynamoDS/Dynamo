## Informacje szczegółowe
Węzeł `GeometryColor.ByMeshColor` zwraca obiekt GeometryColor, który jest siatką w kolorach zgodnych z podaną listą kolorów. Istnieje kilka sposobów używania tego węzła:

- jeśli podany jest jeden kolor, cała siatka jest kolorowana tym kolorem;
- jeśli liczba kolorów odpowiada liczbie trójkątów, każdy trójkąt jest kolorowany odpowiednim kolorem z listy;
- jeśli liczba kolorów odpowiada liczbie unikatowych wierzchołków, kolor każdego trójkąta siatki jest interpolowany jako wartość przypadająca między wartościami kolorów poszczególnych wierzchołków;
- jeśli liczba kolorów jest równa liczbie nieunikatowych wierzchołków, kolor każdego trójkąta jest interpolowany jako wartość przypadająca między wartościami kolorów na powierzchni, ale bez mieszania między powierzchniami.

## Przykład

W poniższym przykładzie siatka zostaje oznaczona kolorami na podstawie rzędnych jej wierzchołków. Najpierw za pomocą węzła `Mesh.Vertices` zostają pobrane unikatowe wierzchołki siatki, które zostają następnie przeanalizowane, i za pomocą węzła `Point.Z` zostają pobrane rzędne poszczególnych punktów wierzchołków. Następnie węzeł `Map.RemapRange` odwzorowywuje te wartości na nowy zakres od 0 do 1 przez ich proporcjonalne przeskalowanie. Na koniec za pomocą węzła `Color Range` wygenerowana zostaje lista kolorów odpowiadających odwzorowanym wartościom. Użyj tej listy kolorów jako pozycji danych wejściowych `colors` węzła `GeometryColor.ByMeshColors`. Wynikiem jest siatka oznaczona kolorami, w której kolor każdego trójkąta jest interpolowany jako wartość przypadająca między kolorami wierzchołków, w wyniku czego powstaje gradient.

## Plik przykładowy

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
