## In Depth
In the example below, a simple T-Spline box surface is transformed into a mesh using a `TSplineSurface.ToMesh` node. The `minSegments` input defines the minimum number of segments for a face in each direction and is important for controlling mesh definition. The `tolerance` input corrects inaccuracies by adding more vertex positions to match the original surface within the given tolerance. The result is a mesh whose definition is previewed using a `Mesh.VertexPositions` node. 
The output mesh can contain both triangles and quads which is important to keep in mind if using MeshToolkit nodes. 
___
## Example File

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)