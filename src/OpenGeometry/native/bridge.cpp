#include "bridge.h"

#include <algorithm>
#include <cmath>
#include <limits>
#include <memory>
#include <mutex>
#include <unordered_map>
#include <vector>

#include <SmApiBrep.h>
#include <SmApiCurves.h>
#include <SmApiGeneral.h>
#include <SmApiPrimitives.h>
#include <SmApiQueries.h>
#include <SmApiSurfaces.h>
#include <SmApiTrimmedSurfaces.h>
#include <SmAxis2Placement.h>
#include <SmBSplineCurve.h>
#include <SmBrep.h>
#include <SmCircle.h>
#include <SmCurve.h>
#include <SmEdge.h>
#include <SmFace.h>
#include <SmLine.h>
#include <SmPoly.h>
#include <SmSurface.h>
#include <SmVertex.h>

struct OgSolid
{
    std::unique_ptr<SmBrep> brep;
};

struct OgCurve
{
    std::unique_ptr<SmCurve> curve;
    double t_min = 0.0;
    double t_max = 1.0;
    bool reversed = false;
};

struct OgMesh
{
    std::vector<OgVertex> vertices;
};

namespace
{
std::mutex kernel_mutex;

template <typename Operation>
int32_t with_kernel(Operation operation)
{
    try
    {
        std::lock_guard<std::mutex> lock(kernel_mutex);
        static const bool initialized = [] {
            SmApiCreateContext();
            return true;
        }();
        (void)initialized;
        return operation();
    }
    catch (...)
    {
        return OG_INTERNAL_ERROR;
    }
}

bool finite(double value)
{
    return std::isfinite(value);
}

bool finite(const OgPoint* point)
{
    return point && std::isfinite(point->x) && std::isfinite(point->y) && std::isfinite(point->z);
}

bool positive(double value)
{
    return std::isfinite(value) && value > 0;
}

bool non_negative(double value)
{
    return std::isfinite(value) && value >= 0;
}

SmVector3d to_vector(const OgPoint& point)
{
    return SmVector3d(point.x, point.y, point.z);
}

SmPoint3d to_sm_point(const OgPoint& point)
{
    return SmPoint3d(point.x, point.y, point.z);
}

OgPoint to_point(const SmVector3d& vector)
{
    return {vector.x, vector.y, vector.z};
}

int32_t publish_solid(SmApiStatus status, std::unique_ptr<SmBrep> solid, OgSolid** result)
{
    if (status != SM_SUCCESS || !solid)
        return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
    *result = new OgSolid{std::move(solid)};
    return OG_SUCCESS;
}

int32_t publish_curve(
    SmApiStatus status,
    std::unique_ptr<SmCurve> curve,
    double t_min,
    double t_max,
    bool reversed,
    OgCurve** result)
{
    if (status != SM_SUCCESS || !curve || !finite(t_min) || !finite(t_max) || t_max <= t_min)
        return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
    *result = new OgCurve{std::move(curve), t_min, t_max, reversed};
    return OG_SUCCESS;
}

int32_t publish_curve_natural(SmApiStatus status, std::unique_ptr<SmCurve> curve, OgCurve** result)
{
    if (status != SM_SUCCESS || !curve)
        return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
    const SmExtent1d interval = curve->GetNaturalInterval();
    return publish_curve(status, std::move(curve), interval.GetMin(), interval.GetMax(), false, result);
}

std::unique_ptr<SmBrep> copy_solid(const OgSolid& solid)
{
    SmBrep* copy = nullptr;
    const auto status = SmApiBrepCopy(solid.brep.get(), copy);
    std::unique_ptr<SmBrep> owner(copy);
    return status == SM_SUCCESS ? std::move(owner) : nullptr;
}

std::unique_ptr<SmCurve> copy_curve_raw(const SmCurve& curve, bool force_nurbs)
{
    SmContext* ctx = SmApiGetOrCreateContext();
    SmCurve* copied = nullptr;
    SmStatus status = SM_SUCCESS;
    if (force_nurbs && curve.IsAnalytic())
    {
        status = curve.CopyAnalyticAsNurb(*ctx, copied);
        if (status != SM_SUCCESS || !copied)
            status = curve.Copy(*ctx, copied);
    }
    else
    {
        status = curve.Copy(*ctx, copied);
    }
    std::unique_ptr<SmCurve> owner(copied);
    return status == SM_SUCCESS ? std::move(owner) : nullptr;
}

std::unique_ptr<SmBSplineCurve> materialize_bspline_curve(const OgCurve& source)
{
    SmContext* ctx = SmApiGetOrCreateContext();
    auto copied = copy_curve_raw(*source.curve, true);
    if (!copied)
        return nullptr;

    // Ensure we hold an SmBSplineCurve (most curves including SmLine/SmCircle inherit from SmBSplineCurve).
    if (!copied->IsKindOf(SmBSplineCurve_TYPE))
    {
        SmBSplineCurve* exact = nullptr;
        if (copied->MakeExactBSplineIfPossible(exact) == SM_SUCCESS && exact)
        {
            copied.reset(exact);
        }
        else
        {
            SmTArray<double> breaks;
            const SmExtent1d nat = copied->GetNaturalInterval();
            breaks.Add(nat.GetMin());
            breaks.Add(nat.GetMax());
            double achieved = 0.0;
            SmBSplineCurve* approx = nullptr;
            if (copied->ApproximateCurve(*ctx, breaks, 1e-6, achieved, approx, FALSE) != SM_SUCCESS || !approx)
                return nullptr;
            copied.reset(approx);
        }
    }

    const SmExtent1d natural = copied->GetNaturalInterval();
    const double span = natural.GetMax() - natural.GetMin();
    const double tol = std::max(1e-9, span * 1e-9);

    if (source.t_min > natural.GetMin() + tol)
    {
        SmCurve* upper = nullptr;
        if (copied->CreateBySplitCurve(*ctx, source.t_min, upper) != SM_SUCCESS || !upper)
            return nullptr;
        copied.reset(upper);
    }

    const SmExtent1d after_min = copied->GetNaturalInterval();
    if (source.t_max < after_min.GetMax() - tol)
    {
        SmCurve* tail = nullptr;
        if (copied->CreateBySplitCurve(*ctx, source.t_max, tail) != SM_SUCCESS)
            return nullptr;
        delete tail;
    }

    if (source.reversed)
    {
        const SmExtent1d trimmed_interval = copied->GetNaturalInterval();
        SmExtent1d reversed_interval;
        if (copied->ReverseParameterization(trimmed_interval, reversed_interval) != SM_SUCCESS)
            return nullptr;
    }

    if (!copied->IsKindOf(SmBSplineCurve_TYPE))
        return nullptr;
    return std::unique_ptr<SmBSplineCurve>(static_cast<SmBSplineCurve*>(copied.release()));
}

SmApiStatus align_z_axis(SmBrep* brep, const SmVector3d& origin, double dx, double dy, double dz)
{
    const double transverse = std::hypot(dx, dy);
    if (transverse == 0 && dz >= 0)
        return SM_SUCCESS;

    constexpr double degrees_per_radian = 57.29577951308232;
    const SmVector3d axis = transverse > 0
        ? SmVector3d(-dy / transverse, dx / transverse, 0)
        : SmVector3d(1, 0, 0);
    return SmApiRotate(brep, origin, axis, std::atan2(transverse, dz) * degrees_per_radian);
}

bool decompose_orthogonal_frame(
    const OgPoint* origin,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    const OgPoint* z_axis,
    SmPoint3d& out_origin,
    SmVector3d& out_ux,
    SmVector3d& out_uy,
    double& out_sx,
    double& out_sy,
    double& out_sz)
{
    if (!finite(origin) || !finite(x_axis) || !finite(y_axis) || !finite(z_axis))
        return false;

    const SmVector3d xv = to_vector(*x_axis);
    const SmVector3d yv = to_vector(*y_axis);
    const SmVector3d zv = to_vector(*z_axis);
    out_sx = xv.Length();
    out_sy = yv.Length();
    out_sz = zv.Length();
    if (!positive(out_sx) || !positive(out_sy) || !positive(out_sz))
        return false;

    out_ux = xv / out_sx;
    out_uy = yv / out_sy;
    const SmVector3d uz = zv / out_sz;

    constexpr double ortho_tol = 1e-6;
    if (std::abs(out_ux.Dot(out_uy)) > ortho_tol || std::abs(out_ux.Dot(uz)) > ortho_tol
        || std::abs(out_uy.Dot(uz)) > ortho_tol)
        return false;
    if ((out_ux * out_uy).Dot(uz) <= 0)
        return false;

    out_origin = to_sm_point(*origin);
    return true;
}

template <typename BooleanFunc>
int32_t run_boolean(const OgSolid* solid, const OgSolid* other, OgSolid** result, BooleanFunc op)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !other)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto first = copy_solid(*solid);
        auto second = copy_solid(*other);
        if (!first || !second)
            return OG_KERNEL_ERROR;

        SmBrep* out = nullptr;
        const auto status = op(first.get(), second.get(), out);
        if (status != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        // SMLib consumes B on success and returns A in place.
        second.release();
        if (out != first.get())
            return OG_KERNEL_ERROR;
        return publish_solid(status, std::move(first), result);
    });
}

OgPoint vertex_normal(SmPolyVertex& vertex, SmFace* face, const SmVector3d& fallback)
{
    const auto& faces = vertex.GetFacesRef();
    const auto& normals = vertex.GetNormalsRef();
    for (ULONG i = 0; i < faces.GetSize() && i < normals.GetSize(); ++i)
    {
        if (faces[i] == static_cast<SmTopology*>(face))
        {
            const double length = normals[i].Length();
            if (positive(length))
                return to_point(normals[i] / length);
        }
    }
    return to_point(fallback);
}

double map_normalized_param(const OgCurve& curve, double u)
{
    const double eff_u = curve.reversed ? (1.0 - u) : u;
    return curve.t_min + eff_u * (curve.t_max - curve.t_min);
}

int32_t wrap_surface_as_brep(SmSurface* raw_surface, OgSolid** result)
{
    std::unique_ptr<SmSurface> surf_owner(raw_surface);
    if (!surf_owner)
        return OG_KERNEL_ERROR;

    std::unique_ptr<SmBrep> brep(new (*SmApiGetOrCreateContext()) SmBrep());
    SmFace* face = nullptr;
    const SmStatus stat = brep->CreateFaceFromSurface(
        surf_owner.get(), surf_owner->GetNaturalUVDomain(), face);
    if (stat != SM_SUCCESS || !face)
    {
        if (surf_owner->GetOwner() != nullptr)
            surf_owner.release();
        return OG_KERNEL_ERROR;
    }

    // Ownership of raw_surface has transferred to brep.
    surf_owner.release();
    return publish_solid(SM_SUCCESS, std::move(brep), result);
}

template <typename TopoPtr>
std::unordered_map<const TopoPtr*, uint32_t> build_index_map(const SmTArray<TopoPtr*>& items)
{
    std::unordered_map<const TopoPtr*, uint32_t> map;
    map.reserve(items.GetSize());
    for (ULONG i = 0; i < items.GetSize(); ++i)
        map[items[i]] = static_cast<uint32_t>(i);
    return map;
}

int32_t copy_index_array(
    const std::vector<uint32_t>& indices, uint32_t* out_buffer, uint32_t capacity, uint32_t* actual_count)
{
    if (!actual_count)
        return OG_INVALID_ARGUMENT;
    *actual_count = static_cast<uint32_t>(indices.size());
    if (out_buffer == nullptr && capacity == 0)
        return OG_SUCCESS;
    if (!out_buffer || capacity < indices.size())
        return OG_INVALID_ARGUMENT;
    if (!indices.empty())
        std::copy(indices.begin(), indices.end(), out_buffer);
    return OG_SUCCESS;
}
} // namespace

uint32_t og_abi_version()
{
    return 3;
}

int32_t og_create_box(const OgPoint* corner, double size_x, double size_y, double size_z, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(corner) || !positive(size_x) || !positive(size_y) || !positive(size_z))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBrep* box = nullptr;
        const auto status = SmApiCreateBox(to_vector(*corner), size_x, size_y, size_z, box);
        return publish_solid(status, std::unique_ptr<SmBrep>(box), result);
    });
}

int32_t og_create_cylinder(const OgPoint* start, const OgPoint* end, double radius, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(start) || !finite(end) || !positive(radius))
        return OG_INVALID_ARGUMENT;

    const SmVector3d origin = to_vector(*start);
    const double dx = end->x - start->x;
    const double dy = end->y - start->y;
    const double dz = end->z - start->z;
    const double height = std::hypot(std::hypot(dx, dy), dz);
    if (!positive(height))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBrep* cylinder = nullptr;
        auto status = SmApiCreateCylinder(origin, radius, height, cylinder);
        std::unique_ptr<SmBrep> owner(cylinder);
        if (status == SM_SUCCESS && owner)
            status = align_z_axis(cylinder, origin, dx, dy, dz);
        return publish_solid(status, std::move(owner), result);
    });
}

int32_t og_create_sphere(const OgPoint* center, double radius, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(center) || !positive(radius))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBrep* sphere = nullptr;
        const auto status = SmApiCreateSphere(to_vector(*center), radius, sphere);
        return publish_solid(status, std::unique_ptr<SmBrep>(sphere), result);
    });
}

int32_t og_create_cone(
    const OgPoint* start, const OgPoint* end, double start_radius, double end_radius, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(start) || !finite(end) || !non_negative(start_radius) || !non_negative(end_radius)
        || (start_radius == 0 && end_radius == 0))
        return OG_INVALID_ARGUMENT;

    const SmVector3d origin = to_vector(*start);
    const double dx = end->x - start->x;
    const double dy = end->y - start->y;
    const double dz = end->z - start->z;
    const double height = std::hypot(std::hypot(dx, dy), dz);
    if (!positive(height))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBrep* cone = nullptr;
        SmApiStatus status = SM_SUCCESS;
        if (start_radius == 0)
        {
            const SmVector3d top_origin = to_vector(*end);
            status = SmApiCreateCone(top_origin, end_radius, 0.0, height, cone);
            std::unique_ptr<SmBrep> owner(cone);
            if (status == SM_SUCCESS && owner)
                status = align_z_axis(cone, top_origin, -dx, -dy, -dz);
            return publish_solid(status, std::move(owner), result);
        }

        status = SmApiCreateCone(origin, start_radius, end_radius, height, cone);
        std::unique_ptr<SmBrep> owner(cone);
        if (status == SM_SUCCESS && owner)
            status = align_z_axis(cone, origin, dx, dy, dz);
        return publish_solid(status, std::move(owner), result);
    });
}

int32_t og_difference(const OgSolid* solid, const OgSolid* tool, OgSolid** result)
{
    return run_boolean(solid, tool, result, SmApiBooleanDifference);
}

int32_t og_union(const OgSolid* solid, const OgSolid* other, OgSolid** result)
{
    return run_boolean(solid, other, result, SmApiBooleanUnion);
}

int32_t og_intersect(const OgSolid* solid, const OgSolid* other, OgSolid** result)
{
    return run_boolean(solid, other, result, SmApiBooleanIntersection);
}

int32_t og_translate(const OgSolid* solid, const OgPoint* delta, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !finite(delta))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiTranslate(copy.get(), to_vector(*delta));
        return publish_solid(status, std::move(copy), result);
    });
}

int32_t og_rotate(
    const OgSolid* solid, const OgPoint* origin, const OgPoint* axis, double degrees, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !finite(origin) || !finite(axis) || !finite(degrees))
        return OG_INVALID_ARGUMENT;

    const SmVector3d axis_vec = to_vector(*axis);
    if (!positive(axis_vec.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiRotate(copy.get(), to_vector(*origin), axis_vec, degrees);
        return publish_solid(status, std::move(copy), result);
    });
}

int32_t og_scale(
    const OgSolid* solid, const OgPoint* origin, double sx, double sy, double sz, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !finite(origin) || !positive(sx) || !positive(sy) || !positive(sz))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiScaleByPt(copy.get(), to_vector(*origin), SmVector3d(sx, sy, sz));
        return publish_solid(status, std::move(copy), result);
    });
}

int32_t og_transform(
    const OgSolid* solid,
    const OgPoint* origin,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    const OgPoint* z_axis,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid)
        return OG_INVALID_ARGUMENT;

    SmPoint3d sm_origin;
    SmVector3d ux, uy;
    double sx = 1.0, sy = 1.0, sz = 1.0;
    if (!decompose_orthogonal_frame(origin, x_axis, y_axis, z_axis, sm_origin, ux, uy, sx, sy, sz))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;

        SmApiStatus status = SM_SUCCESS;
        if (std::abs(sx - 1.0) > 1e-12 || std::abs(sy - 1.0) > 1e-12 || std::abs(sz - 1.0) > 1e-12)
        {
            status = SmApiScale(copy.get(), SmVector3d(sx, sy, sz));
            if (status != SM_SUCCESS)
                return publish_solid(status, std::move(copy), result);
        }

        SmAxis2Placement placement(sm_origin, ux, uy);
        status = SmApiTransform(copy.get(), placement);
        return publish_solid(status, std::move(copy), result);
    });
}

int32_t og_volume(const OgSolid* solid, double* volume)
{
    if (!volume)
        return OG_INVALID_ARGUMENT;
    *volume = 0;
    if (!solid)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        constexpr double relative_accuracy = 1e-7;
        double measured = 0;
        if (SmApiBrepComputeVolume(solid->brep.get(), relative_accuracy, measured) != SM_SUCCESS
            || !std::isfinite(measured))
            return OG_KERNEL_ERROR;
        *volume = measured;
        return OG_SUCCESS;
    });
}

int32_t og_area(const OgSolid* solid, double* area)
{
    if (!area)
        return OG_INVALID_ARGUMENT;
    *area = 0;
    if (!solid)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        constexpr double relative_accuracy = 1e-7;
        double measured = 0;
        if (SmApiBrepComputeArea(solid->brep.get(), relative_accuracy, measured) != SM_SUCCESS
            || !std::isfinite(measured))
            return OG_KERNEL_ERROR;
        *area = measured;
        return OG_SUCCESS;
    });
}

int32_t og_centroid(const OgSolid* solid, OgPoint* centroid)
{
    if (!centroid)
        return OG_INVALID_ARGUMENT;
    *centroid = {0, 0, 0};
    if (!solid)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        constexpr double relative_accuracy = 1e-7;
        double area = 0, volume = 0, mass = 0;
        SmPoint3d center;
        SmVector3d moments, products;
        const auto status = SmApiBrepComputeMassProperties(
            solid->brep.get(), relative_accuracy, 1.0, SmPoint3d(0, 0, 0),
            area, volume, mass, center, moments, products);
        if (status != SM_SUCCESS || !std::isfinite(center.x) || !std::isfinite(center.y) || !std::isfinite(center.z))
            return OG_KERNEL_ERROR;
        *centroid = {center.x, center.y, center.z};
        return OG_SUCCESS;
    });
}

int32_t og_bounding_box(const OgSolid* solid, OgPoint* min_pt, OgPoint* max_pt)
{
    if (!min_pt || !max_pt)
        return OG_INVALID_ARGUMENT;
    *min_pt = {0, 0, 0};
    *max_pt = {0, 0, 0};
    if (!solid)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmPoint3d mn, mx;
        const auto status = SmApiBrepBoundingBox(solid->brep.get(), TRUE, mn, mx);
        if (status != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        *min_pt = {mn.x, mn.y, mn.z};
        *max_pt = {mx.x, mx.y, mx.z};
        return OG_SUCCESS;
    });
}

int32_t og_closest_point(
    const OgSolid* solid, const OgPoint* query, OgPoint* closest, double* distance)
{
    if (!closest || !distance)
        return OG_INVALID_ARGUMENT;
    *closest = {0, 0, 0};
    *distance = 0;
    if (!solid || !finite(query))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmPoint3d pt;
        double dist = 0;
        const auto status = SmApiBrepClosestPoint(
            solid->brep.get(), to_sm_point(*query), pt, dist);
        if (status != SM_SUCCESS || !std::isfinite(dist))
            return OG_KERNEL_ERROR;
        *closest = {pt.x, pt.y, pt.z};
        *distance = dist;
        return OG_SUCCESS;
    });
}

int32_t og_solid_distance(const OgSolid* a, const OgSolid* b, double* distance)
{
    if (!distance)
        return OG_INVALID_ARGUMENT;
    *distance = 0;
    if (!a || !b)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        double dist = 0;
        const auto status = SmApiBrepDistance(a->brep.get(), b->brep.get(), dist);
        if (status != SM_SUCCESS || !std::isfinite(dist))
            return OG_KERNEL_ERROR;
        *distance = dist;
        return OG_SUCCESS;
    });
}

int32_t og_tessellate(const OgSolid* solid, double chord_tolerance, OgMesh** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !std::isfinite(chord_tolerance) || chord_tolerance < 0)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTessellationParams params;
        params.dChordHeightTol = chord_tolerance;
        SmPolyBrep* poly = nullptr;
        const auto status = SmApiTessellate(solid->brep.get(), poly, params);
        std::unique_ptr<SmPolyBrep> owner(poly);
        if (status != SM_SUCCESS || !owner)
            return OG_KERNEL_ERROR;

        auto mesh = std::make_unique<OgMesh>();
        SmTArray<SmPolyFace*> faces;
        owner->GetPolyFaces(faces);
        if (faces.GetSize() > std::numeric_limits<uint32_t>::max() / 3)
            return OG_INTERNAL_ERROR;

        mesh->vertices.reserve(static_cast<size_t>(faces.GetSize()) * 3);
        for (ULONG i = 0; i < faces.GetSize(); ++i)
        {
            SmTArray<SmPolyLoop*> loops;
            faces[i]->GetPolyLoops(loops);
            if (loops.GetSize() != 1)
                return OG_KERNEL_ERROR;

            SmTArray<SmPolyEdge*> edges;
            loops[0]->GetPolyEdges(edges);
            if (edges.GetSize() != 3)
                return OG_KERNEL_ERROR;

            const auto normal = faces[i]->GetNormal(TRUE, FALSE);
            for (ULONG corner = 0; corner < 3; ++corner)
            {
                auto& vertex = *edges[corner]->GetStartPolyVertex();
                mesh->vertices.push_back({
                    to_point(vertex.GetPoint()), vertex_normal(vertex, faces[i]->GetOriginalFace(), normal)
                });
            }
        }
        *result = mesh.release();
        return OG_SUCCESS;
    });
}

void og_release_solid(OgSolid* solid)
{
    std::lock_guard<std::mutex> lock(kernel_mutex);
    delete solid;
}

/* =========================== Curves =========================== */

int32_t og_curve_create_line(const OgPoint* start, const OgPoint* end, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(start) || !finite(end))
        return OG_INVALID_ARGUMENT;

    const double len = std::hypot(std::hypot(end->x - start->x, end->y - start->y), end->z - start->z);
    if (!positive(len))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmLine* line = nullptr;
        const auto status = SmApiCreateLineSegment(to_sm_point(*start), to_sm_point(*end), line);
        return publish_curve_natural(status, std::unique_ptr<SmCurve>(line), result);
    });
}

int32_t og_curve_create_circle(
    const OgPoint* center, const OgPoint* x_axis, const OgPoint* y_axis, double radius, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(center) || !finite(x_axis) || !finite(y_axis) || !positive(radius))
        return OG_INVALID_ARGUMENT;

    const SmVector3d xv = to_vector(*x_axis);
    const SmVector3d yv = to_vector(*y_axis);
    const double xl = xv.Length();
    const double yl = yv.Length();
    if (!positive(xl) || !positive(yl))
        return OG_INVALID_ARGUMENT;

    const SmVector3d ux = xv / xl;
    const SmVector3d uy = yv / yl;
    if (std::abs(ux.Dot(uy)) > 1e-6)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmCircle* circle = nullptr;
        auto status = SmApiCreateCircle(SmPoint3d(0, 0, 0), radius, circle);
        std::unique_ptr<SmCurve> owner(circle);
        if (status == SM_SUCCESS && owner)
        {
            SmAxis2Placement placement(to_sm_point(*center), ux, uy);
            status = SmApiTransform(owner.get(), placement);
        }
        return publish_curve_natural(status, std::move(owner), result);
    });
}

int32_t og_curve_create_arc(
    const OgPoint* center,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    double radius,
    double start_deg,
    double end_deg,
    OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(center) || !finite(x_axis) || !finite(y_axis) || !positive(radius)
        || !finite(start_deg) || !finite(end_deg) || std::abs(end_deg - start_deg) <= 1e-9)
        return OG_INVALID_ARGUMENT;

    const SmVector3d xv = to_vector(*x_axis);
    const SmVector3d yv = to_vector(*y_axis);
    const double xl = xv.Length();
    const double yl = yv.Length();
    if (!positive(xl) || !positive(yl))
        return OG_INVALID_ARGUMENT;

    const SmVector3d ux = xv / xl;
    const SmVector3d uy = yv / yl;
    if (std::abs(ux.Dot(uy)) > 1e-6)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBSplineCurve* arc = nullptr;
        auto status = SmApiCreateArc(SmPoint3d(0, 0, 0), radius, start_deg, end_deg, arc);
        std::unique_ptr<SmCurve> owner(arc);
        if (status == SM_SUCCESS && owner)
        {
            SmAxis2Placement placement(to_sm_point(*center), ux, uy);
            status = SmApiTransform(owner.get(), placement);
        }
        return publish_curve_natural(status, std::move(owner), result);
    });
}

int32_t og_curve_create_interp_points(const OgPoint* points, uint32_t count, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!points || count < 2)
        return OG_INVALID_ARGUMENT;
    for (uint32_t i = 0; i < count; ++i)
    {
        if (!finite(&points[i]))
            return OG_INVALID_ARGUMENT;
    }

    if (count == 2)
        return og_curve_create_line(&points[0], &points[1], result);

    return with_kernel([&]() -> int32_t {
        if (count == 3)
        {
            // Quadratic Bezier interpolating P0 at t=0, P1 at t=0.5, P2 at t=1:
            // control point C1 = 2*P1 - 0.5*(P0 + P2).
            SmTArray<SmPoint3d> cps;
            cps.Add(to_sm_point(points[0]));
            cps.Add(SmPoint3d(
                2.0 * points[1].x - 0.5 * (points[0].x + points[2].x),
                2.0 * points[1].y - 0.5 * (points[0].y + points[2].y),
                2.0 * points[1].z - 0.5 * (points[0].z + points[2].z)));
            cps.Add(to_sm_point(points[2]));
            SmTArray<double> knots;
            knots.Add(0.0);
            knots.Add(1.0);
            SmTArray<ULONG> mults;
            mults.Add(3);
            mults.Add(3);
            SmBSplineCurve* crv = nullptr;
            const auto status = SmApiCreateCanonicalCurve(
                cps, knots, mults, nullptr, 2, SM_CF_UNSPECIFIED, crv);
            return publish_curve_natural(status, std::unique_ptr<SmCurve>(crv), result);
        }

        SmTArray<SmPoint3d> arr;
        for (uint32_t i = 0; i < count; ++i)
            arr.Add(to_sm_point(points[i]));
        SmCurve* crv = nullptr;
        const auto status = SmApiCreateCurveInterpPoints(arr, crv, SM_CP_CHORDLENGTH);
        return publish_curve_natural(status, std::unique_ptr<SmCurve>(crv), result);
    });
}

int32_t og_curve_create_control_points(
    const OgPoint* points, uint32_t count, int32_t degree, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!points || count < 2 || degree < 1 || static_cast<uint32_t>(degree) >= count)
        return OG_INVALID_ARGUMENT;
    for (uint32_t i = 0; i < count; ++i)
    {
        if (!finite(&points[i]))
            return OG_INVALID_ARGUMENT;
    }

    return with_kernel([&]() -> int32_t {
        SmTArray<SmPoint3d> cps;
        for (uint32_t i = 0; i < count; ++i)
            cps.Add(to_sm_point(points[i]));

        const ULONG deg = static_cast<ULONG>(degree);
        const ULONG unique_knots = count - deg + 1;
        SmTArray<double> knots;
        SmTArray<ULONG> mults;
        for (ULONG i = 0; i < unique_knots; ++i)
        {
            const double kv = unique_knots > 1
                ? static_cast<double>(i) / static_cast<double>(unique_knots - 1)
                : 0.0;
            knots.Add(kv);
            mults.Add((i == 0 || i == unique_knots - 1) ? (deg + 1) : 1);
        }

        SmBSplineCurve* crv = nullptr;
        const auto status = SmApiCreateCanonicalCurve(
            cps, knots, mults, nullptr, deg, SM_CF_UNSPECIFIED, crv);
        return publish_curve_natural(status, std::unique_ptr<SmCurve>(crv), result);
    });
}

int32_t og_curve_length(const OgCurve* curve, double* length)
{
    if (!length)
        return OG_INVALID_ARGUMENT;
    *length = 0;
    if (!curve || !curve->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        double measured = 0.0;
        const SmExtent1d interval(curve->t_min, curve->t_max);
        if (curve->curve->Length(interval, 1e-7, measured) != SM_SUCCESS || !std::isfinite(measured))
            return OG_KERNEL_ERROR;
        *length = measured;
        return OG_SUCCESS;
    });
}

int32_t og_curve_is_closed(const OgCurve* curve, int32_t* is_closed)
{
    if (!is_closed)
        return OG_INVALID_ARGUMENT;
    *is_closed = 0;
    if (!curve || !curve->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmPoint3d p0, p1;
        if (curve->curve->EvaluatePoint(curve->t_min, p0) != SM_SUCCESS
            || curve->curve->EvaluatePoint(curve->t_max, p1) != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        const double dist = (p1 - p0).Length();
        *is_closed = dist <= 1e-6 ? 1 : 0;
        return OG_SUCCESS;
    });
}

int32_t og_curve_evaluate(
    const OgCurve* curve, double normalized_t, OgPoint* point, OgPoint* tangent, OgPoint* normal)
{
    if (!curve || !curve->curve || !finite(normalized_t) || normalized_t < -1e-9 || normalized_t > 1.0 + 1e-9)
        return OG_INVALID_ARGUMENT;
    const double u = std::clamp(normalized_t, 0.0, 1.0);

    return with_kernel([&]() -> int32_t {
        const double t = map_normalized_param(*curve, u);
        SmVector3d pt_val, d1_val, d2_val;
        SmVector3d* p_pt = &pt_val;
        SmVector3d* p_d1 = (tangent || normal) ? &d1_val : nullptr;
        SmVector3d* p_d2 = normal ? &d2_val : nullptr;

        if (SmApiEvaluateCurve(curve->curve.get(), t, p_pt, p_d1, p_d2) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        if (point)
            *point = to_point(pt_val);

        if (tangent || normal)
        {
            if (curve->reversed)
                d1_val *= -1.0;
            const double d1_len = d1_val.Length();
            if (!positive(d1_len))
                return OG_KERNEL_ERROR;
            const SmVector3d unit_t = d1_val / d1_len;
            if (tangent)
                *tangent = to_point(unit_t);

            if (normal)
            {
                // Principal normal = component of d2 orthogonal to tangent.
                SmVector3d perp = d2_val - unit_t * d2_val.Dot(unit_t);
                double perp_len = perp.Length();
                if (perp_len > 1e-9)
                {
                    *normal = to_point(perp / perp_len);
                }
                else
                {
                    // Straight curve fallback: deterministic unit vector perpendicular to tangent.
                    const SmVector3d ref = std::abs(unit_t.z) < 0.9 ? SmVector3d(0, 0, 1) : SmVector3d(0, 1, 0);
                    SmVector3d n = ref * unit_t;
                    const double nl = n.Length();
                    *normal = positive(nl) ? to_point(n / nl) : OgPoint{0, 0, 1};
                }
            }
        }
        return OG_SUCCESS;
    });
}

int32_t og_curve_parameter_at_length(const OgCurve* curve, double segment_length, double* normalized_t)
{
    if (!normalized_t)
        return OG_INVALID_ARGUMENT;
    *normalized_t = 0.0;
    if (!curve || !curve->curve || !finite(segment_length) || segment_length < -1e-9)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        double total = 0.0;
        const SmExtent1d full_ivl(curve->t_min, curve->t_max);
        if (curve->curve->Length(full_ivl, 1e-7, total) != SM_SUCCESS || !positive(total))
            return OG_KERNEL_ERROR;
        if (segment_length > total + 1e-6)
            return OG_INVALID_ARGUMENT;

        const double target = std::clamp(segment_length, 0.0, total);
        if (target <= 1e-12)
        {
            *normalized_t = 0.0;
            return OG_SUCCESS;
        }
        if (target >= total - 1e-12)
        {
            *normalized_t = 1.0;
            return OG_SUCCESS;
        }

        double lo = 0.0;
        double hi = 1.0;
        for (int iter = 0; iter < 60; ++iter)
        {
            const double mid = 0.5 * (lo + hi);
            const double t_mid = map_normalized_param(*curve, mid);
            const SmExtent1d sub_ivl = curve->reversed
                ? SmExtent1d(t_mid, curve->t_max)
                : SmExtent1d(curve->t_min, t_mid);
            double sub_len = 0.0;
            if (curve->curve->Length(sub_ivl, 1e-8, sub_len) != SM_SUCCESS)
                return OG_KERNEL_ERROR;

            if (std::abs(sub_len - target) <= 1e-8)
            {
                lo = hi = mid;
                break;
            }
            if (sub_len < target)
                lo = mid;
            else
                hi = mid;
        }
        *normalized_t = 0.5 * (lo + hi);
        return OG_SUCCESS;
    });
}

int32_t og_curve_bounding_box(const OgCurve* curve, OgPoint* min_pt, OgPoint* max_pt)
{
    if (!min_pt || !max_pt)
        return OG_INVALID_ARGUMENT;
    *min_pt = {0, 0, 0};
    *max_pt = {0, 0, 0};
    if (!curve || !curve->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmExtent3d box;
        const SmExtent1d interval(curve->t_min, curve->t_max);
        if (curve->curve->CalculateTightBoundingBox(interval, &box) != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        const SmPoint3d mn = box.GetMin();
        const SmPoint3d mx = box.GetMax();
        *min_pt = {mn.x, mn.y, mn.z};
        *max_pt = {mx.x, mx.y, mx.z};
        return OG_SUCCESS;
    });
}

int32_t og_curve_closest_point(
    const OgCurve* curve, const OgPoint* query, OgPoint* closest, double* normalized_t, double* distance)
{
    if (!closest || !normalized_t || !distance)
        return OG_INVALID_ARGUMENT;
    *closest = {0, 0, 0};
    *normalized_t = 0.0;
    *distance = 0.0;
    if (!curve || !curve->curve || !finite(query))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        const SmPoint3d q = to_sm_point(*query);
        SmPoint3d best_pt;
        double best_t = curve->t_min;
        double best_dist = std::numeric_limits<double>::infinity();

        // Check curve endpoints first so trimmed intervals always clamp cleanly.
        SmPoint3d p0, p1;
        if (curve->curve->EvaluatePoint(curve->t_min, p0) != SM_SUCCESS
            || curve->curve->EvaluatePoint(curve->t_max, p1) != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        best_pt = p0;
        best_t = curve->t_min;
        best_dist = (p0 - q).Length();
        const double d1 = (p1 - q).Length();
        if (d1 < best_dist)
        {
            best_pt = p1;
            best_t = curve->t_max;
            best_dist = d1;
        }

        SmPoint3d solved_pt;
        double solved_t = 0.0;
        double solved_dist = 0.0;
        if (SmApiCurveClosestPoint(curve->curve.get(), q, solved_pt, solved_t, solved_dist) == SM_SUCCESS
            && solved_t >= curve->t_min - 1e-9 && solved_t <= curve->t_max + 1e-9
            && solved_dist < best_dist)
        {
            best_pt = solved_pt;
            best_t = std::clamp(solved_t, curve->t_min, curve->t_max);
            best_dist = solved_dist;
        }

        double u = (best_t - curve->t_min) / (curve->t_max - curve->t_min);
        u = std::clamp(u, 0.0, 1.0);
        if (curve->reversed)
            u = 1.0 - u;

        *closest = {best_pt.x, best_pt.y, best_pt.z};
        *normalized_t = u;
        *distance = best_dist;
        return OG_SUCCESS;
    });
}

int32_t og_curve_reverse(const OgCurve* curve, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;
        return publish_curve(SM_SUCCESS, std::move(copy), curve->t_min, curve->t_max, !curve->reversed, result);
    });
}

int32_t og_curve_trim(
    const OgCurve* curve, double start_normalized_t, double end_normalized_t, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !finite(start_normalized_t) || !finite(end_normalized_t)
        || start_normalized_t < -1e-9 || end_normalized_t > 1.0 + 1e-9
        || end_normalized_t - start_normalized_t <= 1e-9)
        return OG_INVALID_ARGUMENT;

    const double u0 = std::clamp(start_normalized_t, 0.0, 1.0);
    const double u1 = std::clamp(end_normalized_t, 0.0, 1.0);

    return with_kernel([&]() -> int32_t {
        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;

        const double t0 = map_normalized_param(*curve, u0);
        const double t1 = map_normalized_param(*curve, u1);
        const double lo = std::min(t0, t1);
        const double hi = std::max(t0, t1);
        return publish_curve(SM_SUCCESS, std::move(copy), lo, hi, curve->reversed, result);
    });
}

int32_t og_curve_translate(const OgCurve* curve, const OgPoint* delta, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !finite(delta))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiTranslate(copy.get(), to_vector(*delta));
        return publish_curve(status, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
    });
}

int32_t og_curve_rotate(
    const OgCurve* curve, const OgPoint* origin, const OgPoint* axis, double degrees, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !finite(origin) || !finite(axis) || !finite(degrees))
        return OG_INVALID_ARGUMENT;

    const SmVector3d axis_vec = to_vector(*axis);
    if (!positive(axis_vec.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiRotate(copy.get(), to_vector(*origin), axis_vec, degrees);
        return publish_curve(status, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
    });
}

int32_t og_curve_scale(
    const OgCurve* curve, const OgPoint* origin, double sx, double sy, double sz, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !finite(origin) || !positive(sx) || !positive(sy) || !positive(sz))
        return OG_INVALID_ARGUMENT;

    const bool non_uniform = std::abs(sx - sy) > 1e-12 || std::abs(sx - sz) > 1e-12;
    return with_kernel([&]() -> int32_t {
        // Non-uniform scaling of analytic curves (e.g. circles) requires NURBS representation.
        if (non_uniform && curve->curve->IsAnalytic())
        {
            auto bsp = materialize_bspline_curve(*curve);
            if (!bsp)
                return OG_KERNEL_ERROR;
            const auto status = SmApiScaleByPt(bsp.get(), to_vector(*origin), SmVector3d(sx, sy, sz));
            return publish_curve_natural(status, std::unique_ptr<SmCurve>(bsp.release()), result);
        }

        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;
        const auto status = SmApiScaleByPt(copy.get(), to_vector(*origin), SmVector3d(sx, sy, sz));
        return publish_curve(status, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
    });
}

int32_t og_curve_transform(
    const OgCurve* curve,
    const OgPoint* origin,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    const OgPoint* z_axis,
    OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve)
        return OG_INVALID_ARGUMENT;

    SmPoint3d sm_origin;
    SmVector3d ux, uy;
    double sx = 1.0, sy = 1.0, sz = 1.0;
    if (!decompose_orthogonal_frame(origin, x_axis, y_axis, z_axis, sm_origin, ux, uy, sx, sy, sz))
        return OG_INVALID_ARGUMENT;

    const bool has_scale = std::abs(sx - 1.0) > 1e-12 || std::abs(sy - 1.0) > 1e-12 || std::abs(sz - 1.0) > 1e-12;
    const bool non_uniform = std::abs(sx - sy) > 1e-12 || std::abs(sx - sz) > 1e-12;

    return with_kernel([&]() -> int32_t {
        if (non_uniform && curve->curve->IsAnalytic())
        {
            auto bsp = materialize_bspline_curve(*curve);
            if (!bsp)
                return OG_KERNEL_ERROR;
            auto status = SmApiScale(bsp.get(), SmVector3d(sx, sy, sz));
            if (status == SM_SUCCESS)
            {
                SmAxis2Placement placement(sm_origin, ux, uy);
                status = SmApiTransform(bsp.get(), placement);
            }
            return publish_curve_natural(status, std::unique_ptr<SmCurve>(bsp.release()), result);
        }

        auto copy = copy_curve_raw(*curve->curve, false);
        if (!copy)
            return OG_KERNEL_ERROR;
        SmApiStatus status = SM_SUCCESS;
        if (has_scale)
        {
            status = SmApiScale(copy.get(), SmVector3d(sx, sy, sz));
            if (status != SM_SUCCESS)
                return publish_curve(status, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
        }
        SmAxis2Placement placement(sm_origin, ux, uy);
        status = SmApiTransform(copy.get(), placement);
        return publish_curve(status, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
    });
}

int32_t og_curve_sample(
    const OgCurve* curve, uint32_t segments, OgPoint* points, uint32_t capacity, uint32_t* actual_count)
{
    if (!actual_count)
        return OG_INVALID_ARGUMENT;
    *actual_count = 0;
    if (!curve || !curve->curve || segments < 1)
        return OG_INVALID_ARGUMENT;

    const uint32_t point_count = segments + 1;
    *actual_count = point_count;
    if (points == nullptr && capacity == 0)
        return OG_SUCCESS;
    if (!points || capacity < point_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        for (uint32_t i = 0; i < point_count; ++i)
        {
            const double u = static_cast<double>(i) / static_cast<double>(segments);
            const double t = map_normalized_param(*curve, u);
            SmPoint3d pt;
            if (curve->curve->EvaluatePoint(t, pt) != SM_SUCCESS)
                return OG_KERNEL_ERROR;
            points[i] = {pt.x, pt.y, pt.z};
        }
        return OG_SUCCESS;
    });
}

void og_release_curve(OgCurve* curve)
{
    std::lock_guard<std::mutex> lock(kernel_mutex);
    delete curve;
}

/* =========================== Surfaces & PolySurfaces =========================== */

int32_t og_surface_by_four_points(
    const OgPoint* p0, const OgPoint* p1, const OgPoint* p2, const OgPoint* p3, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(p0) || !finite(p1) || !finite(p2) || !finite(p3))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmSurface* srf = nullptr;
        // Dynamo perimeter order: p0=(0,0), p1=(1,0), p2=(1,1), p3=(0,1).
        // SmApiCreateSurfaceFromCornerPoints order: (umin,vmin), (umax,vmin), (umin,vmax), (umax,vmax).
        const auto status = SmApiCreateSurfaceFromCornerPoints(
            to_sm_point(*p0), to_sm_point(*p1), to_sm_point(*p3), to_sm_point(*p2), srf);
        if (status != SM_SUCCESS || !srf)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_surface_by_patch(const OgCurve* const* curves, uint32_t count, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curves || count == 0)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(count);
        SmTArray<SmCurve*> arr;
        for (uint32_t i = 0; i < count; ++i)
        {
            if (!curves[i] || !curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        SmBrep* brep = nullptr;
        const auto status = SmApiCreatePlanarFaces(arr, brep);
        if (status != SM_SUCCESS || !brep)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;

        // SmApiCreatePlanarFaces consumes all input curves on success.
        for (auto& item : owned)
            item.release();

        return publish_solid(status, std::unique_ptr<SmBrep>(brep), result);
    });
}

int32_t og_surface_by_loft(const OgCurve* const* profiles, uint32_t count, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!profiles || count < 2)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(count);
        SmTArray<SmBSplineCurve*> arr;
        for (uint32_t i = 0; i < count; ++i)
        {
            if (!profiles[i] || !profiles[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*profiles[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        if (SmApiMakeCurvesCompatible(arr) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        SmSurface* srf = nullptr;
        SmApiStatus status = SM_SUCCESS;
        if (count == 2)
            status = SmApiCreateRuledSurface(arr[0], arr[1], srf);
        else
            status = SmApiCreateSkin(arr, srf);

        if (status != SM_SUCCESS || !srf)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;

        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_surface_evaluate(
    const OgSolid* surface_brep, double normalized_u, double normalized_v, OgPoint* point, OgPoint* normal)
{
    if (!surface_brep || !surface_brep->brep || !finite(normalized_u) || !finite(normalized_v)
        || normalized_u < -1e-9 || normalized_u > 1.0 + 1e-9
        || normalized_v < -1e-9 || normalized_v > 1.0 + 1e-9)
        return OG_INVALID_ARGUMENT;

    const double u_clamped = std::clamp(normalized_u, 0.0, 1.0);
    const double v_clamped = std::clamp(normalized_v, 0.0, 1.0);

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        surface_brep->brep->GetFaces(faces);
        if (faces.GetSize() == 0)
            return OG_KERNEL_ERROR;

        SmFace* face = faces[0];
        SmSurface* srf = face->GetSurface();
        if (!srf)
            return OG_KERNEL_ERROR;

        const SmExtent2d dom = face->GetUVDomain();
        const double u = dom.GetMin().x + u_clamped * (dom.GetMax().x - dom.GetMin().x);
        const double v = dom.GetMin().y + v_clamped * (dom.GetMax().y - dom.GetMin().y);
        const SmVector2d uv(u, v);

        if (point)
        {
            SmVector3d pt;
            if (SmApiEvaluateSurfacePoint(srf, uv, pt) != SM_SUCCESS)
                return OG_KERNEL_ERROR;
            *point = to_point(pt);
        }

        if (normal)
        {
            SmVector3d n;
            if (SmApiEvaluateSurfaceNormal(srf, uv, n) != SM_SUCCESS)
                return OG_KERNEL_ERROR;

            SmFaceuse* upward = face->GetUpwardFaceuse();
            if (upward && upward->GetShell()
                && surface_brep->brep->GetInfiniteRegion() != upward->GetShell()->GetRegion())
            {
                n *= -1.0;
            }
            const double len = n.Length();
            if (!positive(len))
                return OG_KERNEL_ERROR;
            *normal = to_point(n / len);
        }
        return OG_SUCCESS;
    });
}

int32_t og_brep_join_surfaces(
    const OgSolid* const* breps, uint32_t count, int32_t make_solid, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!breps || count == 0)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::unique_ptr<SmBrep> combined(new (*SmApiGetOrCreateContext()) SmBrep());
        for (uint32_t i = 0; i < count; ++i)
        {
            if (!breps[i] || !breps[i]->brep)
                return OG_INVALID_ARGUMENT;
            SmTArray<SmFace*> faces;
            breps[i]->brep->GetFaces(faces);
            if (faces.GetSize() == 0)
                return OG_INVALID_ARGUMENT;
            if (breps[i]->brep->CopyFaces(faces, combined.get(), nullptr, FALSE) != SM_SUCCESS)
                return OG_KERNEL_ERROR;
        }

        if (make_solid)
        {
            SmBoolean is_solid = FALSE;
            ULONG stitched = 0;
            double max_vgap = 0.0, max_egap = 0.0;
            const auto status = SmApiStitchIntoSolid(combined.get(), is_solid, stitched, max_vgap, max_egap);
            if (status != SM_SUCCESS || !is_solid)
                return OG_KERNEL_ERROR;
        }
        else if (count > 1)
        {
            const auto status = SmApiStitch(combined.get());
            if (status != SM_SUCCESS)
                return OG_KERNEL_ERROR;
        }

        return publish_solid(SM_SUCCESS, std::move(combined), result);
    });
}

/* =========================== Topology Queries =========================== */

int32_t og_brep_topology_counts(
    const OgSolid* brep, uint32_t* face_count, uint32_t* edge_count, uint32_t* vertex_count)
{
    if (!brep || !brep->brep || !face_count || !edge_count || !vertex_count)
        return OG_INVALID_ARGUMENT;
    *face_count = 0;
    *edge_count = 0;
    *vertex_count = 0;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        SmTArray<SmEdge*> edges;
        SmTArray<SmVertex*> vertices;
        brep->brep->GetFaces(faces);
        brep->brep->GetEdges(edges);
        brep->brep->GetVertices(vertices);
        *face_count = static_cast<uint32_t>(faces.GetSize());
        *edge_count = static_cast<uint32_t>(edges.GetSize());
        *vertex_count = static_cast<uint32_t>(vertices.GetSize());
        return OG_SUCCESS;
    });
}

int32_t og_brep_face_info(
    const OgSolid* brep, uint32_t face_index, double* area, OgPoint* min_pt, OgPoint* max_pt)
{
    if (!brep || !brep->brep || !area || !min_pt || !max_pt)
        return OG_INVALID_ARGUMENT;
    *area = 0.0;
    *min_pt = {0, 0, 0};
    *max_pt = {0, 0, 0};

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        brep->brep->GetFaces(faces);
        if (face_index >= faces.GetSize())
            return OG_INVALID_ARGUMENT;
        SmFace* face = faces[face_index];

        double measured = 0.0;
        if (SmApiFaceComputeArea(face, 1e-6, measured) != SM_SUCCESS || !std::isfinite(measured))
            return OG_KERNEL_ERROR;
        SmPoint3d mn, mx;
        if (SmApiFaceBoundingBox(face, TRUE, mn, mx) != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        *area = measured;
        *min_pt = {mn.x, mn.y, mn.z};
        *max_pt = {mx.x, mx.y, mx.z};
        return OG_SUCCESS;
    });
}

int32_t og_brep_face_to_surface(const OgSolid* brep, uint32_t face_index, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!brep || !brep->brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        brep->brep->GetFaces(faces);
        if (face_index >= faces.GetSize())
            return OG_INVALID_ARGUMENT;

        SmTArray<SmFace*> single;
        single.Add(faces[face_index]);
        std::unique_ptr<SmBrep> extracted(new (*SmApiGetOrCreateContext()) SmBrep());
        const auto status = brep->brep->CopyFaces(single, extracted.get(), nullptr, FALSE);
        return publish_solid(status, std::move(extracted), result);
    });
}

int32_t og_brep_face_edges(
    const OgSolid* brep, uint32_t face_index, uint32_t* edge_indices, uint32_t capacity, uint32_t* actual_count)
{
    if (!brep || !brep->brep || !actual_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        SmTArray<SmEdge*> all_edges;
        brep->brep->GetFaces(faces);
        brep->brep->GetEdges(all_edges);
        if (face_index >= faces.GetSize())
            return OG_INVALID_ARGUMENT;

        const auto edge_map = build_index_map(all_edges);
        SmTArray<SmEdge*> face_edges;
        faces[face_index]->GetEdges(face_edges);

        std::vector<uint32_t> indices;
        indices.reserve(face_edges.GetSize());
        for (ULONG i = 0; i < face_edges.GetSize(); ++i)
        {
            const auto it = edge_map.find(face_edges[i]);
            if (it != edge_map.end())
                indices.push_back(it->second);
        }
        return copy_index_array(indices, edge_indices, capacity, actual_count);
    });
}

int32_t og_brep_face_vertices(
    const OgSolid* brep, uint32_t face_index, uint32_t* vertex_indices, uint32_t capacity, uint32_t* actual_count)
{
    if (!brep || !brep->brep || !actual_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        SmTArray<SmVertex*> all_vertices;
        brep->brep->GetFaces(faces);
        brep->brep->GetVertices(all_vertices);
        if (face_index >= faces.GetSize())
            return OG_INVALID_ARGUMENT;

        const auto vert_map = build_index_map(all_vertices);
        SmTArray<SmVertex*> face_verts;
        faces[face_index]->GetVertices(face_verts);

        std::vector<uint32_t> indices;
        indices.reserve(face_verts.GetSize());
        for (ULONG i = 0; i < face_verts.GetSize(); ++i)
        {
            const auto it = vert_map.find(face_verts[i]);
            if (it != vert_map.end())
                indices.push_back(it->second);
        }
        return copy_index_array(indices, vertex_indices, capacity, actual_count);
    });
}

int32_t og_brep_edge_info(
    const OgSolid* brep,
    uint32_t edge_index,
    uint32_t* start_vertex_index,
    uint32_t* end_vertex_index,
    double* length)
{
    if (!brep || !brep->brep || !start_vertex_index || !end_vertex_index || !length)
        return OG_INVALID_ARGUMENT;
    *start_vertex_index = UINT32_MAX;
    *end_vertex_index = UINT32_MAX;
    *length = 0.0;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmEdge*> edges;
        SmTArray<SmVertex*> vertices;
        brep->brep->GetEdges(edges);
        brep->brep->GetVertices(vertices);
        if (edge_index >= edges.GetSize())
            return OG_INVALID_ARGUMENT;

        SmEdge* edge = edges[edge_index];
        double measured = 0.0;
        if (SmApiEdgeComputeLength(edge, 1e-6, measured) != SM_SUCCESS || !std::isfinite(measured))
            return OG_KERNEL_ERROR;

        const auto vert_map = build_index_map(vertices);
        SmVertex* vs = nullptr;
        SmVertex* ve = nullptr;
        edge->GetVertices(vs, ve);
        if (vs)
        {
            const auto it = vert_map.find(vs);
            if (it != vert_map.end())
                *start_vertex_index = it->second;
        }
        if (ve)
        {
            const auto it = vert_map.find(ve);
            if (it != vert_map.end())
                *end_vertex_index = it->second;
        }
        *length = measured;
        return OG_SUCCESS;
    });
}

int32_t og_brep_edge_faces(
    const OgSolid* brep, uint32_t edge_index, uint32_t* face_indices, uint32_t capacity, uint32_t* actual_count)
{
    if (!brep || !brep->brep || !actual_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmEdge*> edges;
        SmTArray<SmFace*> all_faces;
        brep->brep->GetEdges(edges);
        brep->brep->GetFaces(all_faces);
        if (edge_index >= edges.GetSize())
            return OG_INVALID_ARGUMENT;

        const auto face_map = build_index_map(all_faces);
        SmTArray<SmFace*> edge_faces;
        edges[edge_index]->GetFaces(edge_faces);

        std::vector<uint32_t> indices;
        indices.reserve(edge_faces.GetSize());
        for (ULONG i = 0; i < edge_faces.GetSize(); ++i)
        {
            const auto it = face_map.find(edge_faces[i]);
            if (it != face_map.end())
                indices.push_back(it->second);
        }
        return copy_index_array(indices, face_indices, capacity, actual_count);
    });
}

int32_t og_brep_edge_curve(const OgSolid* brep, uint32_t edge_index, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!brep || !brep->brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmEdge*> edges;
        brep->brep->GetEdges(edges);
        if (edge_index >= edges.GetSize())
            return OG_INVALID_ARGUMENT;

        SmEdge* edge = edges[edge_index];
        SmCurve* raw = edge->GetCurve();
        if (!raw)
            return OG_KERNEL_ERROR;

        double t_min = 0.0, t_max = 0.0;
        if (SmApiEdgeParameterRange(edge, t_min, t_max) != SM_SUCCESS || t_max <= t_min)
            return OG_KERNEL_ERROR;

        auto copy = copy_curve_raw(*raw, false);
        if (!copy)
            return OG_KERNEL_ERROR;

        return publish_curve(SM_SUCCESS, std::move(copy), t_min, t_max, false, result);
    });
}

int32_t og_brep_vertex_point(const OgSolid* brep, uint32_t vertex_index, OgPoint* point)
{
    if (!brep || !brep->brep || !point)
        return OG_INVALID_ARGUMENT;
    *point = {0, 0, 0};

    return with_kernel([&]() -> int32_t {
        SmTArray<SmVertex*> vertices;
        brep->brep->GetVertices(vertices);
        if (vertex_index >= vertices.GetSize())
            return OG_INVALID_ARGUMENT;

        SmPoint3d pt;
        if (SmApiVertexGetPoint(vertices[vertex_index], pt) != SM_SUCCESS)
            return OG_KERNEL_ERROR;
        *point = {pt.x, pt.y, pt.z};
        return OG_SUCCESS;
    });
}

int32_t og_brep_vertex_edges(
    const OgSolid* brep, uint32_t vertex_index, uint32_t* edge_indices, uint32_t capacity, uint32_t* actual_count)
{
    if (!brep || !brep->brep || !actual_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmVertex*> vertices;
        SmTArray<SmEdge*> all_edges;
        brep->brep->GetVertices(vertices);
        brep->brep->GetEdges(all_edges);
        if (vertex_index >= vertices.GetSize())
            return OG_INVALID_ARGUMENT;

        const auto edge_map = build_index_map(all_edges);
        SmTArray<SmEdge*> vert_edges;
        vertices[vertex_index]->GetEdges(vert_edges);

        std::vector<uint32_t> indices;
        indices.reserve(vert_edges.GetSize());
        for (ULONG i = 0; i < vert_edges.GetSize(); ++i)
        {
            const auto it = edge_map.find(vert_edges[i]);
            if (it != edge_map.end())
                indices.push_back(it->second);
        }
        return copy_index_array(indices, edge_indices, capacity, actual_count);
    });
}

int32_t og_brep_vertex_faces(
    const OgSolid* brep, uint32_t vertex_index, uint32_t* face_indices, uint32_t capacity, uint32_t* actual_count)
{
    if (!brep || !brep->brep || !actual_count)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<SmVertex*> vertices;
        SmTArray<SmFace*> all_faces;
        brep->brep->GetVertices(vertices);
        brep->brep->GetFaces(all_faces);
        if (vertex_index >= vertices.GetSize())
            return OG_INVALID_ARGUMENT;

        const auto face_map = build_index_map(all_faces);
        SmTArray<SmFace*> vert_faces;
        vertices[vertex_index]->GetFaces(vert_faces);

        std::vector<uint32_t> indices;
        indices.reserve(vert_faces.GetSize());
        for (ULONG i = 0; i < vert_faces.GetSize(); ++i)
        {
            const auto it = face_map.find(vert_faces[i]);
            if (it != face_map.end())
                indices.push_back(it->second);
        }
        return copy_index_array(indices, face_indices, capacity, actual_count);
    });
}

/* =========================== Mesh =========================== */

uint32_t og_mesh_vertex_count(const OgMesh* mesh)
{
    return mesh ? static_cast<uint32_t>(mesh->vertices.size()) : 0;
}

int32_t og_copy_mesh_vertices(const OgMesh* mesh, OgVertex* vertices, uint32_t capacity)
{
    if (!mesh || mesh->vertices.size() > capacity || (!vertices && capacity != 0))
        return OG_INVALID_ARGUMENT;
    if (!mesh->vertices.empty())
        std::copy(mesh->vertices.begin(), mesh->vertices.end(), vertices);
    return OG_SUCCESS;
}

void og_release_mesh(OgMesh* mesh)
{
    delete mesh;
}
