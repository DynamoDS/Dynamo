## In Depth
Creates a NurbsSurface with specified control vertices, knots, weights, and U V degrees. There are several restrictions on the data which, if broken, will cause the function to fail and will throw an exception. Degree: Both u- and v- degree should be >= 1 (piecewise-linear spline) and less than 26 (the maximum B-spline basis degree supported by ASM). Weights: All weight values (if supplied) should be strictly positive. Weights smaller than 1e-11 will be rejected and the function will fail. Knots: Both knot vectors should be non-decreasing sequences. Interior knot multiplicity should be no larger than degree + 1 at the start/end knot and degree at an internal knot (this allows surfaces with G1 discontinuities to be represented). Note that non-clamped knot vectors are supported, but will be converted to clamped ones, with the corresponding changes applied to the control point/weight data.
___
## Example File



