/// <summary>
/// David Eberly, Geometric Tools, Redmond WA 98052
/// Copyright (c) 1998-2021
/// Distributed under the Boost Software License, Version 1.0.
/// https://www.boost.org/LICENSE_1_0.txt
/// https://www.geometrictools.com/License/Boost/LICENSE_1_0.txt
/// Version: 4.0.2021.04.22
/// </summary>

using System.Runtime.InteropServices;

/// <summary>
/// C# interface to the Geometric Tools native plugin that contains methods from
/// the Geomtric Tools Engine: https://github.com/davideberly/GeometricTools/tree/master/GTE.
/// </summary>
public class GTE
{
    /// <summary>
    /// Compute the minimum volume box from an array of 3D points using
    /// MinimimumVolumeBox3D.h from GTE:
    ///     https://www.geometrictools.com/Documentation/MinimumVolumeBox.pdf
    /// </summary>
    /// <param name="numThreads"></param>
    /// To execute in the main thread, set numThreads to 0. To run multithreaded on the CPU,
    /// set numThreads to a positive number.
    /// <param name="numPoints"></param>
    /// Number of input points.
    /// <param name="points"></param>
    /// Array of 3D input points.
    /// <param name="lgMaxSample">
    /// Maximum amount of samples to use to search for the minimum volume box.
    /// The logarithm base 2 of maximum sample index must satisfy the condition
    /// lgMaxSample >= 2, so there are at least 4 samples. Do not choose it to be too large
    /// when using rational computation because the computational costs are excessive.</param>
    /// <param name="center">
    /// Array to write the 3D coordinate of the centre of the resulting box to.
    /// </param>
    /// <param name="axis">
    /// Array to write the three vectors that form the axes of the resulting box to.
    /// </param>
    /// <param name="extent">
    /// Array to write the halved extents of the resulting box along the three axes to.
    /// </param>
    /// <param name="volume">
    /// Variable to write the volume of the resulting box to.
    /// </param>
    [DllImport("GeometricTools", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ComputeMinimumVolumeBoxFromPoints(uint numThreads, int numPoints, float[] points,
                                                                uint lgMaxSample, float[] center, float[] axis,
                                                                float[] extent, out float volume);

    /// <summary>
    /// Compute the minimum volume box from an array of 3D points using
    /// ConvexHull3.h from GTE:
    /// https://github.com/davideberly/GeometricTools/blob/master/GTE/Mathematics/ConvexHull3.h
    /// </summary>
    /// <param name="numThreads">
    /// Runs single-threaded when numThreads = 0. It runs multithreaded when numThreads > 0,
    /// where the number of threads is 2^{numThreads} > 1.
    /// </param>
    /// <param name="numPoints">
    /// Number of input points.
    /// </param>
    /// <param name="points">
    /// Array of 3D input points.
    /// </param>
    /// <param name="dimensions">
    /// Variable to write the dimension of the resulting hull to.
    /// </param>
    /// <param name="hullSizeIn">
    /// The size of the hull array to write the results to.
    /// </param>
    /// <param name="hull">
    /// The returned hull array is organized according to the hull dimension:
    ///     <list type="bullet">
    ///     <item><term> 0 </term>
    ///         <description>
    ///         The hull contains a single point. The returned array has size 1 with
    ///         index into the input points array corresponding to that point.
    ///         </description>
    ///     </item><item><term> 1 </term>
    ///         <description>
    ///         The hull is a line segment. The returned array has size 2 with
    ///         indices into the input points array corresponding to the segment endpoints.
    ///         </description>
    ///     </item><item><term> 2 </term>
    ///         <description>
    ///         The hull is a convex polygon in 3D. The returned array has size N with
    ///         indices into the input points array corresponding to the polygon vertices.
    ///         The vertices are ordered.
    ///         </description>
    ///     </item> <item><term> 3 </term>
    ///         <description>
    ///         The hull is a convex polyhedron. The returned array has T triples of indices,
    ///         each triple corresponding to a triangle face of the hull. The face vertices
    ///         are counterclockwise when viewed by an observer outside the polyhedron.
    ///         It is possible that some triangle faces are coplanar.
    ///         </description>
    ///     </item>
    ///     </list>
    /// </param>
    /// <param name="hullSize">
    /// The amount of indices required in the hull array to write the result.
    /// </param>
    /// <returns>
    /// True if the hull buffer was larger enough to write the result to, False otherwise.
    /// </returns>
    [DllImport("GeometricTools", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ComputeConvexHull3D(uint numThreads, uint numPoints, float[] points, out uint dimensions,
                                                  uint hullSizeIn, uint[] hull, out uint hullSize);
}
