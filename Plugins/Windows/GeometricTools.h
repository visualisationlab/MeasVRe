#pragma once

#define DLL_EXPORT __declspec(dllexport) 

extern "C"
{
	DLL_EXPORT void ComputeMinimumVolumeBoxFromPoints(unsigned int numThreads,
		int numPoints, float const* points, unsigned int lgMaxSample,
		float center[3], float axis[9], float extent[3], float* volume);

	DLL_EXPORT int ComputeConvexHull3D(unsigned int numThreads, unsigned int numPoints, float const* points,
		unsigned int* dimensions, unsigned int primitivesArraySize,
		unsigned int* primitives, unsigned int* hullSize);
}

