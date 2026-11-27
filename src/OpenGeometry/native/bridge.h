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
typedef struct OgCurve OgCurve;
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

/* Solid and sheet BRep inputs are borrowed. On success, each output handle has one owner. */
OG_API int32_t og_create_box(
    const OgPoint* corner, double size_x, double size_y, double size_z, OgSolid** result);
OG_API int32_t og_create_cylinder(
    const OgPoint* start, const OgPoint* end, double radius, OgSolid** result);
OG_API int32_t og_create_sphere(
    const OgPoint* center, double radius, OgSolid** result);
OG_API int32_t og_create_cone(
    const OgPoint* start, const OgPoint* end, double start_radius, double end_radius, OgSolid** result);

OG_API int32_t og_difference(const OgSolid* solid, const OgSolid* tool, OgSolid** result);
OG_API int32_t og_union(const OgSolid* solid, const OgSolid* other, OgSolid** result);
OG_API int32_t og_intersect(const OgSolid* solid, const OgSolid* other, OgSolid** result);

OG_API int32_t og_translate(const OgSolid* solid, const OgPoint* delta, OgSolid** result);
OG_API int32_t og_rotate(
    const OgSolid* solid, const OgPoint* origin, const OgPoint* axis, double degrees, OgSolid** result);
OG_API int32_t og_scale(
    const OgSolid* solid, const OgPoint* origin, double sx, double sy, double sz, OgSolid** result);
OG_API int32_t og_transform(
    const OgSolid* solid,
    const OgPoint* origin,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    const OgPoint* z_axis,
    OgSolid** result);

OG_API int32_t og_volume(const OgSolid* solid, double* volume);
OG_API int32_t og_area(const OgSolid* solid, double* area);
OG_API int32_t og_centroid(const OgSolid* solid, OgPoint* centroid);
OG_API int32_t og_bounding_box(const OgSolid* solid, OgPoint* min_pt, OgPoint* max_pt);
OG_API int32_t og_closest_point(
    const OgSolid* solid, const OgPoint* query, OgPoint* closest, double* distance);
OG_API int32_t og_solid_distance(const OgSolid* a, const OgSolid* b, double* distance);

OG_API int32_t og_tessellate(const OgSolid* solid, double chord_tolerance, OgMesh** result);
OG_API void og_release_solid(OgSolid* solid);

/* Curves */
OG_API int32_t og_curve_create_line(const OgPoint* start, const OgPoint* end, OgCurve** result);
OG_API int32_t og_curve_create_circle(
    const OgPoint* center, const OgPoint* x_axis, const OgPoint* y_axis, double radius, OgCurve** result);
OG_API int32_t og_curve_create_arc(
    const OgPoint* center,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    double radius,
    double start_deg,
    double end_deg,
    OgCurve** result);
OG_API int32_t og_curve_create_interp_points(const OgPoint* points, uint32_t count, OgCurve** result);
OG_API int32_t og_curve_create_control_points(
    const OgPoint* points, uint32_t count, int32_t degree, OgCurve** result);
OG_API int32_t og_curve_length(const OgCurve* curve, double* length);
OG_API int32_t og_curve_is_closed(const OgCurve* curve, int32_t* is_closed);
OG_API int32_t og_curve_evaluate(
    const OgCurve* curve, double normalized_t, OgPoint* point, OgPoint* tangent, OgPoint* normal);
OG_API int32_t og_curve_parameter_at_length(
    const OgCurve* curve, double segment_length, double* normalized_t);
OG_API int32_t og_curve_bounding_box(const OgCurve* curve, OgPoint* min_pt, OgPoint* max_pt);
OG_API int32_t og_curve_closest_point(
    const OgCurve* curve, const OgPoint* query, OgPoint* closest, double* normalized_t, double* distance);
OG_API int32_t og_curve_reverse(const OgCurve* curve, OgCurve** result);
OG_API int32_t og_curve_trim(
    const OgCurve* curve, double start_normalized_t, double end_normalized_t, OgCurve** result);
OG_API int32_t og_curve_translate(const OgCurve* curve, const OgPoint* delta, OgCurve** result);
OG_API int32_t og_curve_rotate(
    const OgCurve* curve, const OgPoint* origin, const OgPoint* axis, double degrees, OgCurve** result);
OG_API int32_t og_curve_scale(
    const OgCurve* curve, const OgPoint* origin, double sx, double sy, double sz, OgCurve** result);
OG_API int32_t og_curve_transform(
    const OgCurve* curve,
    const OgPoint* origin,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    const OgPoint* z_axis,
    OgCurve** result);
OG_API int32_t og_curve_sample(
    const OgCurve* curve, uint32_t segments, OgPoint* points, uint32_t capacity, uint32_t* actual_count);
OG_API void og_release_curve(OgCurve* curve);

/* Surfaces and PolySurfaces (sheet BReps represented as OgSolid handles) */
OG_API int32_t og_surface_by_four_points(
    const OgPoint* p0, const OgPoint* p1, const OgPoint* p2, const OgPoint* p3, OgSolid** result);
OG_API int32_t og_surface_by_patch(const OgCurve* const* curves, uint32_t count, OgSolid** result);
OG_API int32_t og_surface_by_loft(const OgCurve* const* profiles, uint32_t count, OgSolid** result);
OG_API int32_t og_surface_evaluate(
    const OgSolid* surface_brep, double normalized_u, double normalized_v, OgPoint* point, OgPoint* normal);
OG_API int32_t og_brep_join_surfaces(
    const OgSolid* const* breps, uint32_t count, int32_t make_solid, OgSolid** result);

/* Topology queries */
OG_API int32_t og_brep_topology_counts(
    const OgSolid* brep, uint32_t* face_count, uint32_t* edge_count, uint32_t* vertex_count);
OG_API int32_t og_brep_face_info(
    const OgSolid* brep, uint32_t face_index, double* area, OgPoint* min_pt, OgPoint* max_pt);
OG_API int32_t og_brep_face_to_surface(const OgSolid* brep, uint32_t face_index, OgSolid** result);
OG_API int32_t og_brep_face_edges(
    const OgSolid* brep, uint32_t face_index, uint32_t* edge_indices, uint32_t capacity, uint32_t* actual_count);
OG_API int32_t og_brep_face_vertices(
    const OgSolid* brep, uint32_t face_index, uint32_t* vertex_indices, uint32_t capacity, uint32_t* actual_count);
OG_API int32_t og_brep_edge_info(
    const OgSolid* brep,
    uint32_t edge_index,
    uint32_t* start_vertex_index,
    uint32_t* end_vertex_index,
    double* length);
OG_API int32_t og_brep_edge_faces(
    const OgSolid* brep, uint32_t edge_index, uint32_t* face_indices, uint32_t capacity, uint32_t* actual_count);
OG_API int32_t og_brep_edge_curve(const OgSolid* brep, uint32_t edge_index, OgCurve** result);
OG_API int32_t og_brep_vertex_point(const OgSolid* brep, uint32_t vertex_index, OgPoint* point);
OG_API int32_t og_brep_vertex_edges(
    const OgSolid* brep, uint32_t vertex_index, uint32_t* edge_indices, uint32_t capacity, uint32_t* actual_count);
OG_API int32_t og_brep_vertex_faces(
    const OgSolid* brep, uint32_t vertex_index, uint32_t* face_indices, uint32_t capacity, uint32_t* actual_count);

/* Immutable snapshot. Each consecutive group of three vertices is a triangle. */
OG_API uint32_t og_mesh_vertex_count(const OgMesh* mesh);
OG_API int32_t og_copy_mesh_vertices(const OgMesh* mesh, OgVertex* vertices, uint32_t capacity);
OG_API void og_release_mesh(OgMesh* mesh);

#ifdef __cplusplus
}
#endif
