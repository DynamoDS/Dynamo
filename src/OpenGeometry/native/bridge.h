#pragma once

#include <stdint.h>

#if defined(OG_EXPORTS)
#define OG_API __declspec(dllexport)
#else
#define OG_API __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef struct OgSolid OgSolid;
typedef struct OgMesh OgMesh;

typedef struct OgPoint
{
    double x, y, z;
} OgPoint;

typedef struct OgVertex
{
    OgPoint position;
    OgPoint normal;
} OgVertex;

enum OgStatus
{
    OG_SUCCESS = 0,
    OG_INVALID_ARGUMENT = 1,
    OG_KERNEL_ERROR = 2,
    OG_INTERNAL_ERROR = 3
};

OG_API uint32_t og_abi_version(void);

/* Solid inputs are borrowed. On success, each output handle has one owner. */
OG_API int32_t og_create_box(
    const OgPoint* corner, double size_x, double size_y, double size_z, OgSolid** result);
OG_API int32_t og_create_cylinder(
    const OgPoint* start, const OgPoint* end, double radius, OgSolid** result);
OG_API int32_t og_difference(const OgSolid* solid, const OgSolid* tool, OgSolid** result);
OG_API int32_t og_volume(const OgSolid* solid, double* volume);
OG_API int32_t og_tessellate(const OgSolid* solid, double chord_tolerance, OgMesh** result);
OG_API void og_release_solid(OgSolid* solid);

/* Immutable snapshot. Each consecutive group of three vertices is a triangle. */
OG_API uint32_t og_mesh_vertex_count(const OgMesh* mesh);
OG_API int32_t og_copy_mesh_vertices(const OgMesh* mesh, OgVertex* vertices, uint32_t capacity);
OG_API void og_release_mesh(OgMesh* mesh);

#ifdef __cplusplus
}
#endif
