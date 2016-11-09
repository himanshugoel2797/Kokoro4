#pragma once
#include <cstdint>

enum class TargetIndices {
	Index = 0,
	Vertex = 1,
	UV = 2,
	Normal = 3
};

#define MESH_MAGIC_NUM ('M' | 'E' << 8 | 'S' << 16 | 'H' << 24)

typedef struct MeshHeader {
public:
	uint32_t magic_num;
	uint32_t indexOffset;
	uint32_t vertexOffset;
	uint32_t uvOffset;
	uint32_t normalOffset;
	uint32_t normalLen;
} MeshHeader;

extern "C" __declspec(dllexport) int LoadMesh(const char *file, void** targetLocs);

extern "C" __declspec(dllexport) int LoadBakedDraws(const char *file, void *target);