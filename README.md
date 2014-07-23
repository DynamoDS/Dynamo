
Feature Comparison
-----

| Feature Name | Helix 3D | Bloodstone |
| ------------ |:--------:|:----------:|
| Zoom to fit | Yes | **No** |
| Zoom with mouse | Yes | **No** |
| Pan | Yes | **No** |
| Rotate | Yes | Yes |
| Off-centered rotation | No | Yes |
| Grid lines | Yes | No |
| UCS icon | Yes | No |
| Text support | Yes | No |
| Selection highlight | Deferred | Real-time |
| Phong shading | Yes | Yes |
| Phong shading light sources | 3 | 1 |
| Flat shading | No | Yes |
| Per-vertex color | No | Yes |
| Per-node color | No | Yes |

Prioritized Task List
-----
- [ ] Implement pan operation
- [ ] Implement zoom operation
- [ ] Implement geometry clearing when document is closed
- [ ] Implement geometry clearing when nodes are deleted

Screenshots
-----
Stadium designed by @elayabharath
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/eb-stadium-v0.png)

Another stadium designed by @elayabharath
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/eb-stadium-v1.png)

Color setting is now available on per-node basis
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/node-and-primitive-colors.png)

Render style settings on a per-node basis, with default set to "Phong Shading"
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/render-style-phong-shading.png)

Render style of "Primitive Color" allows color specified on triangles to show through without shading
![Image](https://raw.githubusercontent.com/DynamoDS/Dynamo/Bloodstone/doc/img/render-style-primitive-color.png)
