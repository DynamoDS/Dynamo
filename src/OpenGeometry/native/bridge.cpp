#include "bridge.h"

#include <algorithm>
#include <cmath>
#include <limits>
#include <memory>
#include <mutex>
#include <vector>

#include <SmApiBrep.h>
#include <SmApiGeneral.h>
#include <SmApiPrimitives.h>
#include <SmApiQueries.h>
#include <SmBrep.h>
#include <SmPoly.h>

struct OgSolid
{
    std::unique_ptr<SmBrep> brep;
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

bool finite(const OgPoint* point)
{
    return point && std::isfinite(point->x) && std::isfinite(point->y) && std::isfinite(point->z);
}

bool positive(double value)
{
    return std::isfinite(value) && value > 0;
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

std::unique_ptr<SmBrep> copy_solid(const OgSolid& solid)
{
    SmBrep* copy = nullptr;
    const auto status = SmApiBrepCopy(solid.brep.get(), copy);
    std::unique_ptr<SmBrep> owner(copy);
    return status == SM_SUCCESS ? std::move(owner) : nullptr;
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
}

uint32_t og_abi_version()
{
    return 1;
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
        const auto status = SmApiCreateBox(
            SmVector3d(corner->x, corner->y, corner->z), size_x, size_y, size_z, box);
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

    const SmVector3d origin(start->x, start->y, start->z);
    const double dx = end->x - start->x;
    const double dy = end->y - start->y;
    const double dz = end->z - start->z;
    const double transverse = std::hypot(dx, dy);
    const double height = std::hypot(transverse, dz);
    if (!positive(height))
        return OG_INVALID_ARGUMENT;

    return with_kernel([&] {
        SmBrep* cylinder = nullptr;
        auto status = SmApiCreateCylinder(origin, radius, height, cylinder);
        std::unique_ptr<SmBrep> owner(cylinder);
        if (status == SM_SUCCESS && owner && (transverse > 0 || dz < 0))
        {
            constexpr double degrees_per_radian = 57.29577951308232;
            const SmVector3d axis = transverse > 0
                ? SmVector3d(-dy / transverse, dx / transverse, 0)
                : SmVector3d(1, 0, 0);
            status = SmApiRotate(cylinder, origin, axis, std::atan2(transverse, dz) * degrees_per_radian);
        }
        return publish_solid(status, std::move(owner), result);
    });
}

int32_t og_difference(const OgSolid* solid, const OgSolid* tool, OgSolid** result)
{
    if (!result)
        return OG_INVALID_ARGUMENT;
    *result = nullptr;
    if (!solid || !tool)
        return OG_INVALID_ARGUMENT;

    return with_kernel([&]() -> int32_t {
        auto first = copy_solid(*solid);
        auto second = copy_solid(*tool);
        if (!first || !second)
            return OG_KERNEL_ERROR;

        SmBrep* cut = nullptr;
        const auto status = SmApiBooleanDifference(first.get(), second.get(), cut);
        if (status != SM_SUCCESS)
            return OG_KERNEL_ERROR;

        // SMLib consumes B on success and returns A in place. The copies
        // keep the input handles usable on other branches of the graph.
        second.release();
        if (cut != first.get())
            return OG_KERNEL_ERROR;
        return publish_solid(status, std::move(first), result);
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
