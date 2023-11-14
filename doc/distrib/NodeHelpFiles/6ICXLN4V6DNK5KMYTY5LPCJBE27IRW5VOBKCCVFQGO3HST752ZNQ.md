## In Depth

In the example below, a T-Spline surface is matched with a NURBS curve using
`TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)` node. The minimum input required for the
node is the base `tSplineSurface`, a set of edges of the surface, provided in `tsEdges` input, and a curve or
list of curves.
The following inputs control the parameters of the match:
- `continuity` allows to set the continuity type for the match. The input expects values 0, 1, or 2, corresponding to G0 Positional, G1 Tangent and G2 Curvature continuity. However, for matching a surface with a curve, only the GO (input value 0) is available.
- `useArcLength` controls the alignment type options. If set to True, the alignment type used is Arc
Length. This alignment minimizes the physical distance between each point of the T-Spline surface and
the corresponding point on the curve. When False input is provided, the alignment type is Parametric -
each point on the T-Spline surface is matched to a point of comparable parametric distance along the
match target curve.
- `useRefinement` when set to True, adds control points to the surface is attempt to match the target
within a given `refinementTolerance`
- `numRefinementSteps` is the maximum number or times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity. 
- `flipSourceTargetAlignment` reverses the alignment direction. 


## Example File

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
