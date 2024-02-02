# VirtualBrain
*VirtualBrain* is a C# library that can be integrated in Unity applications to explore the internal structure of the brain. A virtual model is created from *MRI* data, including both the internal details of the brain and the surface of the cortex.

## Code modules

- `brain.cs` - State machine of the main execution loop of the application.
- `PointCloud.cs` - Point cloud data as read from a *NIfTI* file.
- `BrainMesh.cs` - Inner brain mesh as read from point cloud data.
- `DFSmesh.cs` - Cortex mesh as read from *BrainSuit* file.

## Data samples
- `brain.txt` - *NIfTI* scan of an anonymous patient.
- `cortex.bytes` - Corresponding *BrainSuit* file.

## Support files
- `IntVector.cs` - Auxiliary extension of `Vector3Int` with additional *ad hoc* operations.
- `LoaderSlider.cs` - Auxiliary class to display progress bars.
- `myUnlit.shader` - Inner brain mesh material shader.
- `cortexShader.shader` - Cortex mesh material shader.
- `ring[RGB].png` - Graphic components to display cursors.

## References
This work has been published as *Teaching the Virtual Brain,* J. Hernández-Aceituno, R. Arnay, G. Hernández, L. Ezama, N. Janssen - *Journal of Digital Imaging* 35, 1599–1610 (2022) - [DOI: 10.1007/s10278-022-00652-5](https://link.springer.com/article/10.1007/s10278-022-00652-5)
