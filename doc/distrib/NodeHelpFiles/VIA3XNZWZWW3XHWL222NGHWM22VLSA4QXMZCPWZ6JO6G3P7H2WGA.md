## In Depth
In the example below, a planar TSpline surface with extruded, subdivided and pulled vertices and faces is inspected with the `TSplineTopology.DecomposedFaces` node, which returns a list of the following types of faces contained in the TSpline surface:

- `all`: list of all faces
- `regular`: list of regular faces
- `nGons`: list of Ngon faces
- `border`: list of border faces
- `inner`: list of inner faces

The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the different types of faces in the surface.
___
## Example File

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)