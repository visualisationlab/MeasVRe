#pragma once

extern "C"
{
	void ComputeMinimumVolumeBoxFromPoints(unsigned int numThreads,
		int numPoints, float const* points, unsigned int lgMaxSample,
		float center[3], float axis[9], float extent[3], float volume[1]);

	int ComputeConvexHull3D(unsigned int numThreads, unsigned int numPoints, float const* points,
		unsigned int dimensions[1], unsigned int primitivesArraySize,
		unsigned int* primitives, unsigned int hullSize[1]);
}
