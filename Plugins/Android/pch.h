#include <jni.h>
#include <errno.h>

#include <string.h>
#include <unistd.h>
#include <sys/resource.h>

#include <android/log.h>

// Download at https://github.com/davideberly/GeometricTools/tree/master/GTE
#include <Mathematics/MinimumVolumeBox3.h>
#include <Mathematics/ConvexHull3.h>
