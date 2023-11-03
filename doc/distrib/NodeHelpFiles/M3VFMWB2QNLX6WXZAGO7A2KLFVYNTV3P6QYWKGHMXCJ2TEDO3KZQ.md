## In-Depth
`TSplineSurface.BuildPipes` generates a T-Spline piped surface using a network of curves. Individual pipes are considered joined if their end points are within the maximum tolerance set by the `snappingTolerance` input. The result of this node can be fine-tuned with a set of inputs that allow to set values for all pipes or individually, if the input is a list equal in length to the number of pipes. The following inputs can be used this way: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` and `endPositions`.

In the example below, three curves joined at endpoints are provided as input for the `TSplineSurface.BuildPipes` node. The `defaultRadius` in this case is a single value for all three pipes, defining the radius of pipes by default unless start and end radii are provided.
Next, `segmentsCount` sets three different values for each individual pipe. The input is a list of three values, each corresponding to a pipe. 

More adjustments become available if `autoHandleStart` and `autoHandleEnd` are set to False. This allows control over the start and end rotations of each pipe (`startRotations` and `endRotations` inputs), as well as the radii at the end and start of each pipe, by specifying the `startRadii` and `endRadii`. Finally, the `startPositions` and `endPositions` allow to offset of the segments at the start or the end of each curve respectively. This input expects a value corresponding to the parameter of the curve where the segments start or end (values between 0 and 1).

## Example File
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)   