cmake_minimum_required(VERSION 3.24)
project(OpenGeometry LANGUAGES CXX)

if(NOT WIN32 OR NOT MSVC OR NOT CMAKE_CXX_COMPILER_ARCHITECTURE_ID STREQUAL "x64")
    message(FATAL_ERROR "The proof of concept builds with MSVC for Windows x64.")
endif()

find_package(Git REQUIRED)
set(USD_BREP_SOURCE "" CACHE PATH "Path to the pinned usd-brep checkout")
set(USD_BREP_EXPECTED_REF "" CACHE STRING "Optional override commit SHA for independent usd-brep update rehearsal")
if(USD_BREP_EXPECTED_REF STREQUAL "")
    file(STRINGS "${CMAKE_CURRENT_SOURCE_DIR}/usd-brep.ref" usd_brep_ref)
else()
    set(usd_brep_ref "${USD_BREP_EXPECTED_REF}")
endif()
execute_process(
    COMMAND "${GIT_EXECUTABLE}" -C "${USD_BREP_SOURCE}" rev-parse HEAD
    RESULT_VARIABLE git_result
    OUTPUT_VARIABLE usd_brep_head
    OUTPUT_STRIP_TRAILING_WHITESPACE)
if(NOT git_result EQUAL 0 OR NOT usd_brep_head STREQUAL usd_brep_ref)
    message(FATAL_ERROR "USD_BREP_SOURCE must be a usd-brep checkout at ${usd_brep_ref}.")
endif()
execute_process(
    COMMAND "${GIT_EXECUTABLE}" -C "${USD_BREP_SOURCE}" status --porcelain --untracked-files=normal
        -- source/SMLib source/SM_API
    RESULT_VARIABLE git_result
    OUTPUT_VARIABLE usd_brep_changes
    OUTPUT_STRIP_TRAILING_WHITESPACE)
if(NOT git_result EQUAL 0 OR NOT usd_brep_changes STREQUAL "")
    message(FATAL_ERROR "The pinned SMLib and SM_API directories must be clean.")
endif()

# Only build the two modeling libraries. The parallel and TBB allocator
# paths are opt-in in the pinned sources and are left disabled here.
set(smlib "${USD_BREP_SOURCE}/source/SMLib")
set(smapi "${USD_BREP_SOURCE}/source/SM_API")
file(GLOB smlib_sources CONFIGURE_DEPENDS "${smlib}/src/*.cpp")
file(GLOB smapi_sources CONFIGURE_DEPENDS "${smapi}/src/*.cpp")

add_library(SMLib SHARED ${smlib_sources})
target_include_directories(SMLib PUBLIC "${smlib}/inc")
target_precompile_headers(SMLib PRIVATE "${smlib}/inc/StdAfx.h")
target_compile_definitions(SMLib PRIVATE NL_DEBUG_CODE)

add_library(SM_API SHARED ${smapi_sources})
target_include_directories(SM_API PUBLIC "${smapi}/inc")
target_precompile_headers(SM_API PRIVATE "${smapi}/inc/StdAfx.h")
target_compile_definitions(SM_API PRIVATE SM_API_EXPORTS)
target_link_libraries(SM_API PUBLIC SMLib)

add_library(OpenGeometryBridge SHARED bridge.cpp)
target_link_libraries(OpenGeometryBridge PRIVATE SM_API)
target_compile_definitions(OpenGeometryBridge PRIVATE OG_EXPORTS)

foreach(target IN ITEMS SMLib SM_API OpenGeometryBridge)
    target_compile_features(${target} PRIVATE cxx_std_17)
    target_compile_definitions(${target} PRIVATE NOMINMAX _SCL_SECURE_NO_WARNINGS)
    target_compile_options(${target} PRIVATE /utf-8 /bigobj /wd4244 /wd4267 /wd4305)
endforeach()
