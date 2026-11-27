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
#include <SmApiIntersectors.h>
#include <SmApiPolygons.h>
#include <SmApiPrimitives.h>
#include <SmApiQueries.h>
#include <SmApiSurfaces.h>
#include <SmApiTrimmedSurfaces.h>
#include <SmAxis2Placement.h>
#include <SmBSplineCurve.h>
#include <SmBSplineSurface.h>
#include <SmBrep.h>
#include <SmCircle.h>
#include <SmCurve.h>
#include <SmEdge.h>
#include <SmEllipse.h>
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

struct OgGeomList
{
    std::vector<OgPoint> points;
    std::vector<std::unique_ptr<OgCurve>> curves;
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
    return 6;
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

/* =========================== Modeling Operations =========================== */

int32_t og_curve_extrude(
    const OgCurve* const* curves,
    uint32_t count,
    const OgPoint* direction,
    double distance,
    int32_t cap_ends,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curves || count == 0 || !finite(direction) || !finite(distance) || std::abs(distance) <= 1e-12)
        return OG_INVALID_ARGUMENT;

    const SmVector3d dir = to_vector(*direction);
    const double dir_len = dir.Length();
    if (!positive(dir_len))
        return OG_INVALID_ARGUMENT;
    const SmVector3d unit_dir = dir / dir_len;

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

        SmBrep* out = nullptr;
        const auto status = SmApiCreateLinearSweep(arr, unit_dir, distance, cap_ends ? TRUE : FALSE, out);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_solid_by_loft(
    const OgCurve* const* curves,
    const uint32_t* curves_per_profile,
    uint32_t profile_count,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curves || !curves_per_profile || profile_count < 2)
        return OG_INVALID_ARGUMENT;

    uint32_t total = 0;
    for (uint32_t i = 0; i < profile_count; ++i)
    {
        if (curves_per_profile[i] == 0)
            return OG_INVALID_ARGUMENT;
        total += curves_per_profile[i];
    }

    return with_kernel([&]() -> int32_t {
        SmTArray<ULONG> counts;
        for (uint32_t i = 0; i < profile_count; ++i)
            counts.Add(static_cast<ULONG>(curves_per_profile[i]));

        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(total);
        SmTArray<const SmCurve*> arr;
        for (uint32_t i = 0; i < total; ++i)
        {
            if (!curves[i] || !curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        const ULONG degree = profile_count == 2 ? 1 : (profile_count == 3 ? 2 : 3);
        SmBrep* out = nullptr;
        const auto status = SmApiCreateSkinPrimitive(
            counts, arr, degree, TRUE, out, nullptr, nullptr, nullptr);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_solid_by_sweep(
    const OgCurve* const* profile_curves,
    uint32_t profile_count,
    const OgCurve* path_curve,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!profile_curves || profile_count == 0 || !path_curve || !path_curve->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(profile_count);
        SmTArray<SmCurve*> arr;
        for (uint32_t i = 0; i < profile_count; ++i)
        {
            if (!profile_curves[i] || !profile_curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*profile_curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        auto path_bsp = materialize_bspline_curve(*path_curve);
        if (!path_bsp)
            return OG_KERNEL_ERROR;

        SmSweepOptions options;
        options.bCapEndsArg = TRUE;
        SmBrep* out = nullptr;
        const auto status = SmApiCreateCurveSweep(
            arr, path_bsp.get(), nullptr, nullptr, &options, out, nullptr, nullptr, nullptr);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_solid_by_revolve(
    const OgCurve* const* profile_curves,
    uint32_t profile_count,
    const OgPoint* axis_origin,
    const OgPoint* axis_dir,
    double start_deg,
    double sweep_deg,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!profile_curves || profile_count == 0 || !finite(axis_origin) || !finite(axis_dir)
        || !finite(start_deg) || !finite(sweep_deg) || std::abs(sweep_deg) <= 1e-9 || std::abs(sweep_deg) > 360.0 + 1e-6)
        return OG_INVALID_ARGUMENT;

    const SmVector3d axis = to_vector(*axis_dir);
    if (!positive(axis.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(profile_count);
        SmTArray<SmCurve*> arr;
        const SmPoint3d origin = to_sm_point(*axis_origin);

        for (uint32_t i = 0; i < profile_count; ++i)
        {
            if (!profile_curves[i] || !profile_curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*profile_curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            if (std::abs(start_deg) > 1e-9)
            {
                if (SmApiRotate(mat.get(), origin, axis, start_deg) != SM_SUCCESS)
                    return OG_KERNEL_ERROR;
            }
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        SmBrep* out = nullptr;
        const auto status = SmApiCreateRotationalSweep(arr, origin, axis, sweep_deg, TRUE, out);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_surface_offset(const OgSolid* surface_brep, double distance, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!surface_brep || !surface_brep->brep || !finite(distance))
        return OG_INVALID_ARGUMENT;

    if (std::abs(distance) <= 1e-12)
    {
        return with_kernel([&]() -> int32_t {
            auto copy = copy_solid(*surface_brep);
            return publish_solid(copy ? SM_SUCCESS : SM_ERR, std::move(copy), result);
        });
    }

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*surface_brep);
        if (!copy)
            return OG_KERNEL_ERROR;
        SmBrep* out = nullptr;
        const auto status = SmApiOffsetBrepFull(copy.get(), distance, TRUE, TRUE, out);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_surface_thicken(
    const OgSolid* surface_brep, double thickness, int32_t both_sides, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!surface_brep || !surface_brep->brep || !finite(thickness) || std::abs(thickness) <= 1e-9)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<const SmFace*> empty_faces;
        if (both_sides)
        {
            // Shift the sheet by -thickness/2 along its normal, then shell by +thickness.
            auto base_copy = copy_solid(*surface_brep);
            if (!base_copy)
                return OG_KERNEL_ERROR;
            SmBrep* shifted_raw = nullptr;
            const auto off_stat = SmApiOffsetBrepFull(base_copy.get(), -0.5 * thickness, TRUE, TRUE, shifted_raw);
            std::unique_ptr<SmBrep> shifted(shifted_raw);
            if (off_stat != SM_SUCCESS || !shifted)
                return OG_KERNEL_ERROR;

            SmBrep* out = nullptr;
            const auto status = SmApiShellBrepFull(shifted.get(), thickness, TRUE, TRUE, TRUE, empty_faces, out);
            return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
        }

        SmBrep* out = nullptr;
        const auto status = SmApiShellBrepFull(
            surface_brep->brep.get(), thickness, TRUE, TRUE, TRUE, empty_faces, out);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_solid_fillet_edges(
    const OgSolid* solid,
    const uint32_t* edge_indices,
    uint32_t edge_count,
    double radius,
    int32_t chamfer,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !solid->brep || !edge_indices || edge_count == 0 || !positive(radius))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;

        SmTArray<SmEdge*> all_edges;
        copy->GetEdges(all_edges);
        SmTArray<SmEdge*> selected;
        for (uint32_t i = 0; i < edge_count; ++i)
        {
            if (edge_indices[i] >= all_edges.GetSize())
                return OG_INVALID_ARGUMENT;
            selected.Add(all_edges[edge_indices[i]]);
        }

        const ULONG xsect = chamfer ? 0 : 1; // 0 = linear chamfer setback, 1 = circular fillet
        const auto status = SmApiFilletEdges(copy.get(), selected, radius, xsect, 1, 1.0);
        return publish_solid(status, std::move(copy), result);
    });
}

int32_t og_solid_thin_shell(
    const OgSolid* solid, double internal_thickness, double external_thickness, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !solid->brep || !non_negative(internal_thickness) || !non_negative(external_thickness)
        || (internal_thickness <= 1e-9 && external_thickness <= 1e-9))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        SmTArray<const SmFace*> empty_faces;
        if (external_thickness <= 1e-9)
        {
            SmBrep* out = nullptr;
            const auto status = SmApiShellBrepFull(
                solid->brep.get(), -internal_thickness, TRUE, TRUE, TRUE, empty_faces, out);
            return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
        }
        if (internal_thickness <= 1e-9)
        {
            SmBrep* out = nullptr;
            const auto status = SmApiShellBrepFull(
                solid->brep.get(), external_thickness, TRUE, TRUE, TRUE, empty_faces, out);
            return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
        }

        // Both internal and external thickness > 0: outer offset minus inner offset.
        auto copy_for_outer = copy_solid(*solid);
        auto copy_for_inner = copy_solid(*solid);
        if (!copy_for_outer || !copy_for_inner)
            return OG_KERNEL_ERROR;

        SmBrep* outer_raw = nullptr;
        SmBrep* inner_raw = nullptr;
        const auto s1 = SmApiOffsetBrepFull(copy_for_outer.get(), external_thickness, TRUE, TRUE, outer_raw);
        std::unique_ptr<SmBrep> outer(outer_raw);
        if (s1 != SM_SUCCESS || !outer)
            return OG_KERNEL_ERROR;

        const auto s2 = SmApiOffsetBrepFull(copy_for_inner.get(), -internal_thickness, TRUE, TRUE, inner_raw);
        std::unique_ptr<SmBrep> inner(inner_raw);
        if (s2 != SM_SUCCESS || !inner)
            return OG_KERNEL_ERROR;

        SmBrep* diff_raw = nullptr;
        const auto sd = SmApiBooleanDifference(outer.get(), inner.get(), diff_raw);
        if (sd != SM_SUCCESS || diff_raw != outer.get())
            return OG_KERNEL_ERROR;
        inner.release();
        return publish_solid(sd, std::move(outer), result);
    });
}

int32_t og_solid_cut_plane(
    const OgSolid* solid, const OgPoint* plane_origin, const OgPoint* plane_normal, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !solid->brep || !finite(plane_origin) || !finite(plane_normal))
        return OG_INVALID_ARGUMENT;

    const SmVector3d normal = to_vector(*plane_normal);
    const double nlen = normal.Length();
    if (!positive(nlen))
        return OG_INVALID_ARGUMENT;
    const SmVector3d un = normal / nlen;

    return with_kernel([&]() -> int32_t {
        SmPoint3d mn, mx;
        if (SmApiBrepBoundingBox(solid->brep.get(), FALSE, mn, mx) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        const SmPoint3d center(0.5 * (mn.x + mx.x), 0.5 * (mn.y + mx.y), 0.5 * (mn.z + mx.z));
        const double diag = (mx - mn).Length();
        const double extent = std::max(diag, 1.0) * 2.0;

        const SmVector3d offset = center - to_sm_point(*plane_origin);
        const double signed_dist = offset.Dot(un);
        if (signed_dist <= -diag - 1e-6)
        {
            *result = nullptr;
            return OG_SUCCESS;
        }
        if (signed_dist >= diag + 1e-6)
        {
            auto copy = copy_solid(*solid);
            return publish_solid(copy ? SM_SUCCESS : SM_ERR, std::move(copy), result);
        }

        // Build an orthonormal frame on the cutting plane centered near the solid.
        const SmVector3d ref = std::abs(un.z) < 0.9 ? SmVector3d(0, 0, 1) : SmVector3d(0, 1, 0);
        SmVector3d ux = ref * un;
        ux.Unitize();
        SmVector3d uy = un * ux;
        uy.Unitize();
        const SmPoint3d plane_center = center - un * signed_dist;

        SmBrep* half_space_raw = nullptr;
        auto status = SmApiCreateBox(
            SmVector3d(-extent, -extent, 0.0), 2.0 * extent, 2.0 * extent, extent, half_space_raw);
        std::unique_ptr<SmBrep> half_space(half_space_raw);
        if (status != SM_SUCCESS || !half_space)
            return OG_KERNEL_ERROR;

        SmAxis2Placement placement(plane_center, ux, uy);
        status = SmApiTransform(half_space.get(), placement);
        if (status != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        auto copy = copy_solid(*solid);
        if (!copy)
            return OG_KERNEL_ERROR;

        SmBrep* out = nullptr;
        status = SmApiBooleanIntersection(copy.get(), half_space.get(), out);
        if (status != SM_SUCCESS || out != copy.get())
            return OG_KERNEL_ERROR;
        half_space.release();

        SmTArray<SmFace*> faces;
        copy->GetFaces(faces);
        if (faces.GetSize() == 0)
        {
            *result = nullptr;
            return OG_SUCCESS;
        }
        return publish_solid(SM_SUCCESS, std::move(copy), result);
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

/* =========================== Slice D: Curves, NURBS Surfaces, Trimming, Intersections =========================== */

namespace
{
bool build_unit_frame(
    const OgPoint* x_axis, const OgPoint* y_axis, SmVector3d& ux, SmVector3d& uy)
{
    if (!finite(x_axis) || !finite(y_axis))
        return false;
    const SmVector3d xv = to_vector(*x_axis);
    const SmVector3d yv = to_vector(*y_axis);
    const double xl = xv.Length();
    const double yl = yv.Length();
    if (!positive(xl) || !positive(yl))
        return false;
    ux = xv / xl;
    uy = yv / yl;
    return std::abs(ux.Dot(uy)) <= 1e-6;
}

SmBSplineSurface* get_first_bspline_surface(const OgSolid* surface_brep, std::unique_ptr<SmBSplineSurface>& temp_copy)
{
    if (!surface_brep || !surface_brep->brep)
        return nullptr;
    SmTArray<SmFace*> faces;
    surface_brep->brep->GetFaces(faces);
    if (faces.GetSize() == 0)
        return nullptr;
    SmSurface* srf = faces[0]->GetSurface();
    if (!srf)
        return nullptr;
    if (srf->IsKindOf(SmBSplineSurface_TYPE))
        return static_cast<SmBSplineSurface*>(srf);
    SmBSplineSurface* exact = nullptr;
    if (srf->MakeExactBSplineIfPossible(exact) == SM_SUCCESS && exact)
    {
        temp_copy.reset(exact);
        return temp_copy.get();
    }
    return nullptr;
}

std::unique_ptr<OgCurve> wrap_owned_curve(SmCurve* raw)
{
    if (!raw)
        return nullptr;
    const SmExtent1d ivl = raw->GetNaturalInterval();
    if (!finite(ivl.GetMin()) || !finite(ivl.GetMax()) || ivl.GetMax() <= ivl.GetMin())
    {
        delete raw;
        return nullptr;
    }
    return std::make_unique<OgCurve>(OgCurve{std::unique_ptr<SmCurve>(raw), ivl.GetMin(), ivl.GetMax(), false});
}
} // namespace

int32_t og_curve_create_ellipse(
    const OgPoint* center,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    double radius_x,
    double radius_y,
    OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    SmVector3d ux, uy;
    if (!finite(center) || !positive(radius_x) || !positive(radius_y) || !build_unit_frame(x_axis, y_axis, ux, uy))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmEllipse* ellipse = nullptr;
        auto status = SmApiCreateEllipse(SmPoint3d(0, 0, 0), radius_x, radius_y, ellipse);
        std::unique_ptr<SmCurve> owner(ellipse);
        if (status == SM_SUCCESS && owner)
        {
            SmAxis2Placement placement(to_sm_point(*center), ux, uy);
            status = SmApiTransform(owner.get(), placement);
        }
        return publish_curve_natural(status, std::move(owner), result);
    });
}

int32_t og_curve_create_ellipse_arc(
    const OgPoint* center,
    const OgPoint* x_axis,
    const OgPoint* y_axis,
    double radius_x,
    double radius_y,
    double start_deg,
    double sweep_deg,
    OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    SmVector3d ux, uy;
    if (!finite(center) || !positive(radius_x) || !positive(radius_y)
        || !finite(start_deg) || !finite(sweep_deg)
        || std::abs(sweep_deg) <= 1e-9 || std::abs(sweep_deg) > 360.0 + 1e-6
        || !build_unit_frame(x_axis, y_axis, ux, uy))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        const double end_deg = start_deg + sweep_deg;
        const bool reversed = sweep_deg < 0.0;
        const double a0 = reversed ? end_deg : start_deg;
        const double a1 = reversed ? start_deg : end_deg;

        SmBSplineCurve* arc = nullptr;
        auto status = SmApiCreateArc(SmPoint3d(0, 0, 0), 1.0, a0, a1, arc);
        std::unique_ptr<SmCurve> owner(arc);
        if (status == SM_SUCCESS && owner)
            status = SmApiScale(owner.get(), SmVector3d(radius_x, radius_y, 1.0));
        if (status == SM_SUCCESS && owner)
        {
            SmAxis2Placement placement(to_sm_point(*center), ux, uy);
            status = SmApiTransform(owner.get(), placement);
        }
        if (status != SM_SUCCESS || !owner)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        const SmExtent1d ivl = owner->GetNaturalInterval();
        return publish_curve(status, std::move(owner), ivl.GetMin(), ivl.GetMax(), reversed, result);
    });
}

int32_t og_curve_create_helix(
    const OgPoint* axis_point,
    const OgPoint* axis_dir,
    const OgPoint* start_point,
    double pitch,
    double angle_turns_deg,
    OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!finite(axis_point) || !finite(axis_dir) || !finite(start_point)
        || !finite(pitch) || std::abs(pitch) <= 1e-7
        || !finite(angle_turns_deg) || std::abs(angle_turns_deg) <= 1e-6)
        return OG_INVALID_ARGUMENT;

    SmVector3d uz = to_vector(*axis_dir);
    const double uz_len = uz.Length();
    if (!positive(uz_len))
        return OG_INVALID_ARGUMENT;
    uz /= uz_len;

    const SmPoint3d ap = to_sm_point(*axis_point);
    const SmPoint3d sp = to_sm_point(*start_point);
    const double proj = (sp - ap).Dot(uz);
    const SmPoint3d base = ap + uz * proj;
    const SmVector3d radial = sp - base;
    const double radius = radial.Length();
    if (radius <= 1e-7)
        return OG_INVALID_ARGUMENT;

    SmVector3d ux = radial / radius;
    const bool neg_pitch = pitch < 0.0;
    if (neg_pitch)
        uz *= -1.0;
    SmVector3d uy = uz * ux;
    uy.Unitize();

    const double num_turns = std::abs(angle_turns_deg) / 360.0;
    const double height = std::abs(pitch) * num_turns;
    const SmBoolean right_handed = ((angle_turns_deg >= 0.0) != neg_pitch) ? TRUE : FALSE;

    return with_kernel([&] {
        SmBSplineCurve* helix = nullptr;
        auto status = SmApiCreateHelix(
            SmPoint3d(0, 0, 0), height, radius, radius, num_turns, right_handed, 1e-4, helix);
        std::unique_ptr<SmCurve> owner(helix);
        if (status == SM_SUCCESS && owner)
        {
            SmAxis2Placement placement(base, ux, uy);
            status = SmApiTransform(owner.get(), placement);
        }
        return publish_curve_natural(status, std::move(owner), result);
    });
}

int32_t og_curve_offset(const OgCurve* curve, double distance, OgCurve** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !finite(distance))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        if (std::abs(distance) <= 1e-12)
        {
            auto copy = copy_curve_raw(*curve->curve, false);
            return publish_curve(
                copy ? SM_SUCCESS : SM_ERR, std::move(copy), curve->t_min, curve->t_max, curve->reversed, result);
        }

        auto bsp = materialize_bspline_curve(*curve);
        if (!bsp)
            return OG_KERNEL_ERROR;
        SmBSplineCurve* offset = nullptr;
        const auto status = SmApiOffsetCurve(bsp.get(), distance, offset);
        return publish_curve_natural(status, std::unique_ptr<SmCurve>(offset), result);
    });
}

int32_t og_curve_project_onto_brep(
    const OgCurve* curve, const OgSolid* brep, const OgPoint* direction, OgGeomList** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !brep || !brep->brep || !finite(direction))
        return OG_INVALID_ARGUMENT;

    const SmVector3d dir = to_vector(*direction);
    if (!positive(dir.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto bsp = materialize_bspline_curve(*curve);
        auto brep_copy = copy_solid(*brep);
        if (!bsp || !brep_copy)
            return OG_KERNEL_ERROR;

        SmTArray<SmCurve*> projected;
        const auto status = SmApiProjectCurve(brep_copy.get(), bsp.get(), dir, projected);
        auto list = std::make_unique<OgGeomList>();
        if (status == SM_SUCCESS)
        {
            for (ULONG i = 0; i < projected.GetSize(); ++i)
            {
                if (auto wrapped = wrap_owned_curve(projected[i]))
                    list->curves.push_back(std::move(wrapped));
            }
        }
        else if (status != SM_ERR)
        {
            for (ULONG i = 0; i < projected.GetSize(); ++i)
                delete projected[i];
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }
        *result = list.release();
        return OG_SUCCESS;
    });
}

int32_t og_nurbs_surface_by_control_points(
    const OgPoint* points, uint32_t u_count, uint32_t v_count, int32_t u_degree, int32_t v_degree, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!points || u_count < 2 || v_count < 2 || u_degree < 1 || v_degree < 1
        || static_cast<uint32_t>(u_degree) >= u_count || static_cast<uint32_t>(v_degree) >= v_count)
        return OG_INVALID_ARGUMENT;

    const uint32_t total = u_count * v_count;
    for (uint32_t i = 0; i < total; ++i)
    {
        if (!finite(&points[i]))
            return OG_INVALID_ARGUMENT;
    }

    return with_kernel([&]() -> int32_t {
        SmTArray<SmPoint3d> ctrl_pts;
        for (uint32_t i = 0; i < total; ++i)
            ctrl_pts.Add(to_sm_point(points[i]));

        const ULONG deg_u = static_cast<ULONG>(u_degree);
        const ULONG deg_v = static_cast<ULONG>(v_degree);
        const ULONG n_knots_u = u_count - deg_u + 1;
        const ULONG n_knots_v = v_count - deg_v + 1;

        SmTArray<double> knots_u;
        SmTArray<ULONG> mult_u;
        for (ULONG i = 0; i < n_knots_u; ++i)
        {
            knots_u.Add(n_knots_u > 1 ? static_cast<double>(i) / static_cast<double>(n_knots_u - 1) : 0.0);
            mult_u.Add((i == 0 || i == n_knots_u - 1) ? (deg_u + 1) : 1);
        }

        SmTArray<double> knots_v;
        SmTArray<ULONG> mult_v;
        for (ULONG i = 0; i < n_knots_v; ++i)
        {
            knots_v.Add(n_knots_v > 1 ? static_cast<double>(i) / static_cast<double>(n_knots_v - 1) : 0.0);
            mult_v.Add((i == 0 || i == n_knots_v - 1) ? (deg_v + 1) : 1);
        }

        SmBSplineSurface* srf = nullptr;
        const auto status = SmBSplineSurface::CreateCanonical(
            *SmApiGetOrCreateContext(),
            deg_u,
            deg_v,
            ctrl_pts,
            SM_SF_UNSPECIFIED,
            mult_u,
            mult_v,
            knots_u,
            knots_v,
            SM_KT_UNSPECIFIED,
            nullptr,
            nullptr,
            srf);
        if (status != SM_SUCCESS || !srf)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_nurbs_surface_by_points(
    const OgPoint* points, uint32_t u_count, uint32_t v_count, int32_t u_degree, int32_t v_degree, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!points || u_count < 2 || v_count < 2 || u_degree < 1 || v_degree < 1
        || static_cast<uint32_t>(u_degree) >= u_count || static_cast<uint32_t>(v_degree) >= v_count)
        return OG_INVALID_ARGUMENT;

    const uint32_t total = u_count * v_count;
    for (uint32_t i = 0; i < total; ++i)
    {
        if (!finite(&points[i]))
            return OG_INVALID_ARGUMENT;
    }

    return with_kernel([&]() -> int32_t {
        SmTArray<SmPoint3d> pts;
        for (uint32_t i = 0; i < total; ++i)
            pts.Add(to_sm_point(points[i]));

        SmContext& ctx = *SmApiGetOrCreateContext();
        SmBSplineSurface* srf = nullptr;
        auto status = SmBSplineSurface::InterpolatePoints(
            ctx,
            pts,
            static_cast<ULONG>(u_count),
            static_cast<ULONG>(v_count),
            static_cast<ULONG>(u_degree),
            static_cast<ULONG>(v_degree),
            nullptr,
            nullptr,
            nullptr,
            SM_IT_CHORDLENGTH,
            srf);

        if (status != SM_SUCCESS || !srf)
        {
            delete srf;
            srf = nullptr;
            status = SmBSplineSurface::ApproximatePoints(
                ctx,
                pts,
                static_cast<ULONG>(u_count),
                static_cast<ULONG>(v_count),
                static_cast<ULONG>(u_degree),
                static_cast<ULONG>(v_degree),
                nullptr,
                srf,
                TRUE);
        }

        if (status != SM_SUCCESS || !srf)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_nurbs_surface_info(
    const OgSolid* surface_brep,
    int32_t* u_degree,
    int32_t* v_degree,
    uint32_t* u_count,
    uint32_t* v_count,
    int32_t* is_rational,
    int32_t* is_periodic_u,
    int32_t* is_periodic_v)
{
    if (!surface_brep || !u_degree || !v_degree || !u_count || !v_count
        || !is_rational || !is_periodic_u || !is_periodic_v)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::unique_ptr<SmBSplineSurface> temp;
        SmBSplineSurface* bsp = get_first_bspline_surface(surface_brep, temp);
        if (!bsp)
            return OG_KERNEL_ERROR;

        *u_degree = static_cast<int32_t>(bsp->GetDegree(SM_SP_U));
        *v_degree = static_cast<int32_t>(bsp->GetDegree(SM_SP_V));
        *u_count = static_cast<uint32_t>(bsp->GetNumberControlPoints(SM_SP_U));
        *v_count = static_cast<uint32_t>(bsp->GetNumberControlPoints(SM_SP_V));
        const SmExtent2d dom = bsp->GetNaturalUVDomain();
        *is_rational = bsp->IsRational() ? 1 : 0;
        *is_periodic_u = bsp->IsPeriodic(dom, SM_SP_U) ? 1 : 0;
        *is_periodic_v = bsp->IsPeriodic(dom, SM_SP_V) ? 1 : 0;
        return OG_SUCCESS;
    });
}

int32_t og_nurbs_surface_control_points(
    const OgSolid* surface_brep,
    OgPoint* points,
    double* weights,
    uint32_t capacity,
    uint32_t* actual_count)
{
    if (!actual_count)
        return OG_INVALID_ARGUMENT;
    *actual_count = 0;
    if (!surface_brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::unique_ptr<SmBSplineSurface> temp;
        SmBSplineSurface* bsp = get_first_bspline_surface(surface_brep, temp);
        if (!bsp)
            return OG_KERNEL_ERROR;

        ULONG uc = 0, vc = 0;
        SmTArray<SmPoint3d> cps;
        SmTArray<double> wts;
        if (bsp->GetControlPointNet(uc, vc, cps, wts) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        const uint32_t total = static_cast<uint32_t>(cps.GetSize());
        *actual_count = total;
        if (points == nullptr && capacity == 0)
            return OG_SUCCESS;
        if (!points || !weights || capacity < total)
            return OG_INVALID_ARGUMENT;

        const bool has_weights = wts.GetSize() == cps.GetSize();
        for (uint32_t i = 0; i < total; ++i)
        {
            points[i] = {cps[i].x, cps[i].y, cps[i].z};
            weights[i] = has_weights ? wts[i] : 1.0;
        }
        return OG_SUCCESS;
    });
}

int32_t og_nurbs_surface_knots(
    const OgSolid* surface_brep,
    int32_t v_direction,
    double* knots,
    uint32_t capacity,
    uint32_t* actual_count)
{
    if (!actual_count)
        return OG_INVALID_ARGUMENT;
    *actual_count = 0;
    if (!surface_brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::unique_ptr<SmBSplineSurface> temp;
        SmBSplineSurface* bsp = get_first_bspline_surface(surface_brep, temp);
        if (!bsp)
            return OG_KERNEL_ERROR;

        SmTArray<double> unique_knots;
        SmTArray<ULONG> mults;
        const SmSurfParamType dir = v_direction ? SM_SP_V : SM_SP_U;
        if (bsp->GetKnots(dir, unique_knots, &mults) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        std::vector<double> expanded;
        for (ULONG i = 0; i < unique_knots.GetSize(); ++i)
        {
            const ULONG m = i < mults.GetSize() ? mults[i] : 1;
            for (ULONG k = 0; k < m; ++k)
                expanded.push_back(unique_knots[i]);
        }

        const uint32_t total = static_cast<uint32_t>(expanded.size());
        *actual_count = total;
        if (knots == nullptr && capacity == 0)
            return OG_SUCCESS;
        if (!knots || capacity < total)
            return OG_INVALID_ARGUMENT;

        if (!expanded.empty())
            std::copy(expanded.begin(), expanded.end(), knots);
        return OG_SUCCESS;
    });
}

int32_t og_surface_by_sweep(const OgCurve* profile, const OgCurve* path, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!profile || !profile->curve || !path || !path->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto prof_bsp = materialize_bspline_curve(*profile);
        auto path_bsp = materialize_bspline_curve(*path);
        if (!prof_bsp || !path_bsp)
            return OG_KERNEL_ERROR;

        SmTArray<SmCurve*> arr;
        arr.Add(prof_bsp.get());
        SmSweepOptions options;
        options.bCapEndsArg = FALSE;
        SmBrep* out = nullptr;
        auto status = SmApiCreateCurveSweep(
            arr, path_bsp.get(), nullptr, nullptr, &options, out, nullptr, nullptr, nullptr);
        if (status == SM_SUCCESS && out)
            return publish_solid(status, std::unique_ptr<SmBrep>(out), result);

        delete out;
        SmSurface* srf = nullptr;
        status = SmApiCreateSweep(prof_bsp.get(), path_bsp.get(), srf);
        if (status != SM_SUCCESS || !srf)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_surface_by_sweep_2_rails(
    const OgCurve* path, const OgCurve* guide_rail, const OgCurve* profile, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!path || !path->curve || !guide_rail || !guide_rail->curve || !profile || !profile->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto r1 = materialize_bspline_curve(*path);
        auto r2 = materialize_bspline_curve(*guide_rail);
        auto prof = materialize_bspline_curve(*profile);
        if (!r1 || !r2 || !prof)
            return OG_KERNEL_ERROR;

        SmTArray<SmBSplineCurve*> profiles;
        profiles.Add(prof.get());
        SmTArray<SmBSplineCurve*> rails;
        rails.Add(r1.get());
        rails.Add(r2.get());

        SmBSplineSurface* srf = nullptr;
        auto status = SmBSplineSurface::CreateSweepSurface(
            *SmApiGetOrCreateContext(), profiles, rails, 1e-4, srf, 8, 2);
        if (status == SM_SUCCESS && srf)
            return wrap_surface_as_brep(srf, result);

        // Fallback: generate compatible cross-section profiles positioned and scaled between the two rails.
        delete srf;
        srf = nullptr;

        const SmExtent1d r1_ivl = r1->GetNaturalInterval();
        const SmExtent1d r2_ivl = r2->GetNaturalInterval();
        const SmExtent1d p_ivl = prof->GetNaturalInterval();
        SmPoint3d p0, p1;
        if (prof->EvaluatePoint(p_ivl.GetMin(), p0) != SM_SUCCESS
            || prof->EvaluatePoint(p_ivl.GetMax(), p1) != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        const SmVector3d chord0 = p1 - p0;
        const double chord0_len = chord0.Length();
        if (!positive(chord0_len))
            return OG_KERNEL_ERROR;
        const SmVector3d e0 = chord0 / chord0_len;

        // Build reference orthonormal frame from profile chord + profile midpoint deviation.
        SmPoint3d p_mid;
        prof->EvaluatePoint(0.5 * (p_ivl.GetMin() + p_ivl.GetMax()), p_mid);
        SmVector3d dev = (p_mid - p0) - e0 * (p_mid - p0).Dot(e0);
        if (dev.Length() <= 1e-8)
        {
            const SmVector3d ref = std::abs(e0.z) < 0.9 ? SmVector3d(0, 0, 1) : SmVector3d(0, 1, 0);
            dev = ref * e0;
        }
        dev.Unitize();
        SmVector3d b0 = e0 * dev;
        b0.Unitize();

        constexpr int kSections = 9;
        constexpr int kSamples = 17;
        SmTArray<SmPoint3d> grid;
        std::vector<SmPoint3d> local_pts;
        local_pts.reserve(kSamples);
        for (int j = 0; j < kSamples; ++j)
        {
            const double v = static_cast<double>(j) / static_cast<double>(kSamples - 1);
            SmPoint3d pt;
            if (prof->EvaluatePoint(p_ivl.GetMin() + v * (p_ivl.GetMax() - p_ivl.GetMin()), pt) != SM_SUCCESS)
                return OG_KERNEL_ERROR;
            const SmVector3d rel = pt - p0;
            local_pts.push_back(SmPoint3d(rel.Dot(e0) / chord0_len, rel.Dot(dev) / chord0_len, rel.Dot(b0) / chord0_len));
        }

        SmVector3d prev_n = dev;
        for (int i = 0; i < kSections; ++i)
        {
            const double u = static_cast<double>(i) / static_cast<double>(kSections - 1);
            SmPoint3d a, b;
            if (r1->EvaluatePoint(r1_ivl.GetMin() + u * (r1_ivl.GetMax() - r1_ivl.GetMin()), a) != SM_SUCCESS
                || r2->EvaluatePoint(r2_ivl.GetMin() + u * (r2_ivl.GetMax() - r2_ivl.GetMin()), b) != SM_SUCCESS)
                return OG_KERNEL_ERROR;

            const SmVector3d chord = b - a;
            const double len = chord.Length();
            if (!positive(len))
                return OG_KERNEL_ERROR;
            const SmVector3d ex = chord / len;
            SmVector3d ey = prev_n - ex * prev_n.Dot(ex);
            if (ey.Length() <= 1e-8)
            {
                const SmVector3d ref = std::abs(ex.z) < 0.9 ? SmVector3d(0, 0, 1) : SmVector3d(0, 1, 0);
                ey = ref * ex;
            }
            ey.Unitize();
            prev_n = ey;
            SmVector3d ez = ex * ey;
            ez.Unitize();

            for (int j = 0; j < kSamples; ++j)
            {
                const SmPoint3d& lp = local_pts[j];
                grid.Add(a + ex * (lp.x * len) + ey * (lp.y * len) + ez * (lp.z * len));
            }
        }

        status = SmBSplineSurface::InterpolatePoints(
            *SmApiGetOrCreateContext(), grid, kSections, kSamples, 3, 3, nullptr, nullptr, nullptr, SM_IT_CHORDLENGTH, srf);
        if (status != SM_SUCCESS || !srf)
            return OG_KERNEL_ERROR;
        return wrap_surface_as_brep(srf, result);
    });
}

int32_t og_surface_by_revolve(
    const OgCurve* const* profile_curves,
    uint32_t profile_count,
    const OgPoint* axis_origin,
    const OgPoint* axis_dir,
    double start_deg,
    double sweep_deg,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!profile_curves || profile_count == 0 || !finite(axis_origin) || !finite(axis_dir)
        || !finite(start_deg) || !finite(sweep_deg) || std::abs(sweep_deg) <= 1e-9 || std::abs(sweep_deg) > 360.0 + 1e-6)
        return OG_INVALID_ARGUMENT;

    const SmVector3d axis = to_vector(*axis_dir);
    if (!positive(axis.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        std::vector<std::unique_ptr<SmBSplineCurve>> owned;
        owned.reserve(profile_count);
        SmTArray<SmCurve*> arr;
        const SmPoint3d origin = to_sm_point(*axis_origin);

        for (uint32_t i = 0; i < profile_count; ++i)
        {
            if (!profile_curves[i] || !profile_curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*profile_curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            if (std::abs(start_deg) > 1e-9)
            {
                if (SmApiRotate(mat.get(), origin, axis, start_deg) != SM_SUCCESS)
                    return OG_KERNEL_ERROR;
            }
            arr.Add(mat.get());
            owned.push_back(std::move(mat));
        }

        SmBrep* out = nullptr;
        const auto status = SmApiCreateRotationalSweep(arr, origin, axis, sweep_deg, FALSE, out);
        return publish_solid(status, std::unique_ptr<SmBrep>(out), result);
    });
}

int32_t og_surface_trim_with_curves(
    const OgSolid* surface_brep,
    const OgCurve* const* curves,
    const uint32_t* curves_per_loop,
    uint32_t loop_count,
    OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!surface_brep || !surface_brep->brep || !curves || !curves_per_loop || loop_count == 0)
        return OG_INVALID_ARGUMENT;

    uint32_t total = 0;
    for (uint32_t i = 0; i < loop_count; ++i)
    {
        if (curves_per_loop[i] == 0)
            return OG_INVALID_ARGUMENT;
        total += curves_per_loop[i];
    }

    return with_kernel([&]() -> int32_t {
        SmTArray<SmFace*> faces;
        surface_brep->brep->GetFaces(faces);
        if (faces.GetSize() == 0)
            return OG_KERNEL_ERROR;
        SmSurface* base_srf = faces[0]->GetSurface();
        if (!base_srf)
            return OG_KERNEL_ERROR;

        SmSurface* copied_srf = nullptr;
        if (base_srf->Copy(*SmApiGetOrCreateContext(), copied_srf) != SM_SUCCESS || !copied_srf)
            return OG_KERNEL_ERROR;
        std::unique_ptr<SmSurface> srf_owner(copied_srf);

        SmTArray<ULONG> loop_counts;
        for (uint32_t i = 0; i < loop_count; ++i)
            loop_counts.Add(static_cast<ULONG>(curves_per_loop[i]));

        std::vector<std::unique_ptr<SmBSplineCurve>> owned_curves;
        owned_curves.reserve(total);
        SmTArray<SmCurve*> trim_curves;
        for (uint32_t i = 0; i < total; ++i)
        {
            if (!curves[i] || !curves[i]->curve)
                return OG_INVALID_ARGUMENT;
            auto mat = materialize_bspline_curve(*curves[i]);
            if (!mat)
                return OG_KERNEL_ERROR;
            trim_curves.Add(mat.get());
            owned_curves.push_back(std::move(mat));
        }

        SmBrep* trimmed = nullptr;
        const auto status = SmApiTrimSurfaceWith3dCurves(srf_owner.get(), loop_counts, trim_curves, trimmed);
        if (status != SM_SUCCESS || !trimmed)
        {
            // SmApiTrimSurfaceWith3dCurves may have attached objects to an internal partial Brep on failure.
            if (srf_owner->GetOwner() != nullptr)
                srf_owner.release();
            for (auto& c : owned_curves)
            {
                if (c->GetOwner() != nullptr)
                    c.release();
            }
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }

        srf_owner.release();
        for (auto& c : owned_curves)
            c.release();
        return publish_solid(SM_SUCCESS, std::unique_ptr<SmBrep>(trimmed), result);
    });
}

int32_t og_intersect_curves(const OgCurve* curve_a, const OgCurve* curve_b, OgGeomList** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve_a || !curve_a->curve || !curve_b || !curve_b->curve)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto a = materialize_bspline_curve(*curve_a);
        auto b = materialize_bspline_curve(*curve_b);
        if (!a || !b)
            return OG_KERNEL_ERROR;

        SmTArray<SmPoint3d> pts;
        const auto status = SmApiIntersectCurves(a.get(), b.get(), &pts, nullptr, nullptr);
        auto list = std::make_unique<OgGeomList>();
        if (status == SM_SUCCESS)
        {
            for (ULONG i = 0; i < pts.GetSize(); ++i)
                list->points.push_back({pts[i].x, pts[i].y, pts[i].z});
        }
        else if (status != SM_ERR)
        {
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }
        *result = list.release();
        return OG_SUCCESS;
    });
}

int32_t og_intersect_curve_brep(const OgCurve* curve, const OgSolid* brep, OgGeomList** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!curve || !curve->curve || !brep || !brep->brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto crv = materialize_bspline_curve(*curve);
        auto copy = copy_solid(*brep);
        if (!crv || !copy)
            return OG_KERNEL_ERROR;

        SmTArray<SmCurve*> curves;
        SmTArray<SmPoint3d> pts;
        const auto status = SmApiIntersectCurveBrep(crv.get(), copy.get(), curves, pts);
        auto list = std::make_unique<OgGeomList>();
        if (status == SM_SUCCESS)
        {
            for (ULONG i = 0; i < pts.GetSize(); ++i)
                list->points.push_back({pts[i].x, pts[i].y, pts[i].z});
            for (ULONG i = 0; i < curves.GetSize(); ++i)
            {
                if (auto wrapped = wrap_owned_curve(curves[i]))
                    list->curves.push_back(std::move(wrapped));
            }
        }
        else if (status != SM_ERR)
        {
            for (ULONG i = 0; i < curves.GetSize(); ++i)
                delete curves[i];
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }
        *result = list.release();
        return OG_SUCCESS;
    });
}

int32_t og_intersect_breps(const OgSolid* brep_a, const OgSolid* brep_b, OgGeomList** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!brep_a || !brep_a->brep || !brep_b || !brep_b->brep)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy_a = copy_solid(*brep_a);
        auto copy_b = copy_solid(*brep_b);
        if (!copy_a || !copy_b)
            return OG_KERNEL_ERROR;

        SmTArray<SmCurve*> curves;
        SmTArray<SmPoint3d> pts;
        const auto status = SmApiIntersectBreps(copy_a.get(), copy_b.get(), curves, &pts);
        auto list = std::make_unique<OgGeomList>();
        if (status == SM_SUCCESS)
        {
            for (ULONG i = 0; i < pts.GetSize(); ++i)
                list->points.push_back({pts[i].x, pts[i].y, pts[i].z});
            for (ULONG i = 0; i < curves.GetSize(); ++i)
            {
                if (auto wrapped = wrap_owned_curve(curves[i]))
                    list->curves.push_back(std::move(wrapped));
            }
        }
        else if (status != SM_ERR)
        {
            for (ULONG i = 0; i < curves.GetSize(); ++i)
                delete curves[i];
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }
        *result = list.release();
        return OG_SUCCESS;
    });
}

int32_t og_intersect_brep_plane(
    const OgSolid* brep, const OgPoint* plane_origin, const OgPoint* plane_normal, OgGeomList** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!brep || !brep->brep || !finite(plane_origin) || !finite(plane_normal))
        return OG_INVALID_ARGUMENT;

    const SmVector3d n = to_vector(*plane_normal);
    if (!positive(n.Length()))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto copy = copy_solid(*brep);
        if (!copy)
            return OG_KERNEL_ERROR;

        SmTArray<SmCurve*> curves;
        const auto status = SmApiIntersectBrepWithPlane(copy.get(), to_vector(*plane_origin), n, curves);
        auto list = std::make_unique<OgGeomList>();
        if (status == SM_SUCCESS)
        {
            for (ULONG i = 0; i < curves.GetSize(); ++i)
            {
                if (auto wrapped = wrap_owned_curve(curves[i]))
                    list->curves.push_back(std::move(wrapped));
            }
        }
        else if (status != SM_ERR)
        {
            for (ULONG i = 0; i < curves.GetSize(); ++i)
                delete curves[i];
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;
        }
        *result = list.release();
        return OG_SUCCESS;
    });
}

uint32_t og_geom_list_point_count(const OgGeomList* list)
{
    return list ? static_cast<uint32_t>(list->points.size()) : 0;
}

int32_t og_geom_list_get_point(const OgGeomList* list, uint32_t index, OgPoint* point)
{
    if (!list || !point || index >= list->points.size())
        return OG_INVALID_ARGUMENT;
    *point = list->points[index];
    return OG_SUCCESS;
}

uint32_t og_geom_list_curve_count(const OgGeomList* list)
{
    return list ? static_cast<uint32_t>(list->curves.size()) : 0;
}

int32_t og_geom_list_take_curve(OgGeomList* list, uint32_t index, OgCurve** curve)
{
    if (!curve)
        return OG_INVALID_ARGUMENT;
    *curve = nullptr;
    if (!list || index >= list->curves.size() || !list->curves[index])
        return OG_INVALID_ARGUMENT;
    *curve = list->curves[index].release();
    return OG_SUCCESS;
}

void og_release_geom_list(OgGeomList* list)
{
    std::lock_guard<std::mutex> lock(kernel_mutex);
    delete list;
}

/* =========================== Slice E: Polygon Mesh Booleans =========================== */

namespace
{
struct QuantizedPointKey
{
    int64_t x, y, z;
    bool operator==(const QuantizedPointKey& o) const { return x == o.x && y == o.y && z == o.z; }
};

struct QuantizedPointHash
{
    size_t operator()(const QuantizedPointKey& k) const
    {
        size_t h = static_cast<size_t>(k.x) * 73856093ULL;
        h ^= static_cast<size_t>(k.y) * 19349663ULL;
        h ^= static_cast<size_t>(k.z) * 83492791ULL;
        return h;
    }
};

std::unique_ptr<SmPolyBrep> build_poly_brep(
    const OgPoint* verts, uint32_t vert_count, const uint32_t* tris, uint32_t tri_count)
{
    SmContext* ctx = SmApiGetOrCreateContext();
    const SmZoneTol3d tol(1e-6);
    std::unique_ptr<SmPolyBrep> poly(new (*ctx) SmPolyBrep(tol, std::max<ULONG>(32, tri_count * 4)));
    SmPolyRegion* region = new (poly.get()) SmPolyRegion(poly.get());
    SmPolyShell* shell = new (poly.get()) SmPolyShell(region);

    std::unordered_map<QuantizedPointKey, SmPolyVertex*, QuantizedPointHash> unique_verts;
    unique_verts.reserve(vert_count);
    std::vector<SmPolyVertex*> mapped(vert_count, nullptr);
    for (uint32_t i = 0; i < vert_count; ++i)
    {
        if (!finite(&verts[i]))
            return nullptr;
        const QuantizedPointKey key{
            static_cast<int64_t>(std::llround(verts[i].x * 1e6)),
            static_cast<int64_t>(std::llround(verts[i].y * 1e6)),
            static_cast<int64_t>(std::llround(verts[i].z * 1e6))};
        auto it = unique_verts.find(key);
        if (it != unique_verts.end())
        {
            mapped[i] = it->second;
        }
        else
        {
            SmPolyVertex* pv = new (poly.get()) SmPolyVertex(to_sm_point(verts[i]), tol);
            unique_verts[key] = pv;
            mapped[i] = pv;
        }
    }

    for (uint32_t t = 0; t < tri_count; ++t)
    {
        const uint32_t i0 = tris[t * 3];
        const uint32_t i1 = tris[t * 3 + 1];
        const uint32_t i2 = tris[t * 3 + 2];
        if (i0 >= vert_count || i1 >= vert_count || i2 >= vert_count)
            return nullptr;
        SmPolyVertex* v0 = mapped[i0];
        SmPolyVertex* v1 = mapped[i1];
        SmPolyVertex* v2 = mapped[i2];
        if (v0 == v1 || v1 == v2 || v0 == v2)
            continue;
        SmPolyFace* face = nullptr;
        if (poly->CreatePolyTriangle(region, shell, v0, v1, v2, face) != SM_SUCCESS || !face)
            return nullptr;
    }

    ULONG stitched = 0, lamina = 0;
    if (poly->Stitch(SmZoneTol3d(1e-5), TRUE, FALSE, TRUE, stitched, lamina, TRUE) != SM_SUCCESS)
        return nullptr;

    return poly;
}

std::unique_ptr<OgMesh> extract_poly_mesh(SmPolyBrep& poly)
{
    auto mesh = std::make_unique<OgMesh>();
    SmTArray<SmPolyFace*> poly_faces;
    poly.GetPolyFaces(poly_faces);
    for (ULONG i = 0; i < poly_faces.GetSize(); ++i)
    {
        SmPolyFace* pf = poly_faces[i];
        SmPolyLoop* loop = pf ? pf->GetOuterPolyLoop() : nullptr;
        if (!loop)
            continue;
        SmTArray<SmPolyEdge*> edges;
        loop->GetPolyEdges(edges);
        const ULONG n = edges.GetSize();
        if (n < 3)
            continue;

        SmVector3d fn = pf->GetNormal(TRUE, FALSE);
        const double fn_len = fn.Length();
        const OgPoint face_norm = positive(fn_len) ? to_point(fn / fn_len) : OgPoint{0, 0, 1};

        SmPolyVertex* v0 = edges[0]->GetStartPolyVertex();
        if (!v0)
            continue;
        const OgPoint p0 = to_point(v0->GetPoint());
        for (ULONG j = 1; j + 1 < n; ++j)
        {
            SmPolyVertex* v1 = edges[j]->GetStartPolyVertex();
            SmPolyVertex* v2 = edges[j + 1]->GetStartPolyVertex();
            if (!v1 || !v2)
                continue;
            mesh->vertices.push_back({p0, face_norm});
            mesh->vertices.push_back({to_point(v1->GetPoint()), face_norm});
            mesh->vertices.push_back({to_point(v2->GetPoint()), face_norm});
        }
    }
    return mesh;
}
} // namespace

int32_t og_poly_boolean(
    const OgPoint* verts_a,
    uint32_t vert_count_a,
    const uint32_t* tris_a,
    uint32_t tri_count_a,
    const OgPoint* verts_b,
    uint32_t vert_count_b,
    const uint32_t* tris_b,
    uint32_t tri_count_b,
    int32_t operation,
    OgMesh** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!verts_a || vert_count_a < 4 || !tris_a || tri_count_a < 4
        || !verts_b || vert_count_b < 4 || !tris_b || tri_count_b < 4
        || operation < 0 || operation > 2)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto poly_a = build_poly_brep(verts_a, vert_count_a, tris_a, tri_count_a);
        auto poly_b = build_poly_brep(verts_b, vert_count_b, tris_b, tri_count_b);
        if (!poly_a || !poly_b)
            return OG_INVALID_ARGUMENT;

        SmPolyBrep* out = nullptr;
        SmApiStatus status = SM_ERR;
        if (operation == 0)
            status = SmApiPolyBooleanUnion(poly_a.get(), poly_b.get(), out);
        else if (operation == 1)
            status = SmApiPolyBooleanIntersection(poly_a.get(), poly_b.get(), out);
        else
            status = SmApiPolyBooleanDifference(poly_a.get(), poly_b.get(), out);

        if (status != SM_SUCCESS || !out)
            return status == SM_ERR_INVALID_INPUT ? OG_INVALID_ARGUMENT : OG_KERNEL_ERROR;

        // SmApiPolyBoolean* consumes poly_b on success and returns poly_a in place.
        poly_b.release();
        std::unique_ptr<SmPolyBrep> out_owner;
        SmPolyBrep* target = poly_a.get();
        if (out != poly_a.get())
        {
            out_owner.reset(out);
            target = out_owner.get();
        }

        auto mesh = extract_poly_mesh(*target);
        if (!mesh || mesh->vertices.empty())
            return OG_KERNEL_ERROR;
        *result = mesh.release();
        return OG_SUCCESS;
    });
}
