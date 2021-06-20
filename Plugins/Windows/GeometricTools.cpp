#include "pch.h"
#include "GeometricTools.h"

using namespace gte;

void ComputeMinimumVolumeBoxFromPoints(unsigned int numThreads,
	int numPoints, float const* points, unsigned int lgMaxSample,
	float center[3], float axis[9], float extent[3], float* volume)
{
	if (numPoints > 0 && points)
	{
		MinimumVolumeBox3<float, false> mvb(numThreads);
		auto const* vpoints = reinterpret_cast<Vector3<float> const*>(points);
		OrientedBox3<float> box;
		mvb(numPoints, vpoints, lgMaxSample, box, volume[0]);
		for (uint32_t i = 0; i < 3; ++i)
		{
			center[i] = box.center[i];
			extent[i] = box.extent[i];
			for (uint32_t j = 0; j < 3; ++j)
			{
				axis[3 * i + j] = box.axis[i][j];
			}
		}
	}
	else
	{
		for (uint32_t i = 0; i < 3; ++i)
		{
			center[i] = 0.0;
			extent[i] = 0.0;
			for (uint32_t j = 0; j < 3; ++j)
			{
				axis[3 * i + j] = 0.0;
			}
		}
		*volume = 0.0;
	}
}


int ComputeConvexHull3D(unsigned int numThreads, unsigned int numPoints, float const* points, unsigned int* dimensions,
						unsigned int primitivesArraySize, unsigned int* primitives, unsigned int* hullSize) {
	gte::ConvexHull3<float> ch;
	auto const* vertices = reinterpret_cast<gte::Vector3<float> const*>(points);
	ch(numPoints, vertices, numThreads);

	std::vector<size_t> const& hull = ch.GetHull();
	*hullSize = static_cast<unsigned int>(hull.size());

	if (*hullSize > primitivesArraySize) {
		return 0;
	}
	else {
		*dimensions = static_cast<unsigned int>(ch.GetDimension());
		for (size_t i = 0; i < *hullSize; ++i) {
			primitives[i] = static_cast<unsigned int>(hull[i]);
		}

		return 1;
	}
}
