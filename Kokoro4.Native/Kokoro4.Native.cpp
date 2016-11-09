// Kokoro4.Native.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "Kokoro4.Native.h"
#include <stdio.h>

__declspec(dllexport) int LoadMesh(const char *file, void** targetLocs) 
{
	FILE *f = fopen(file, "rb");
	if (f) {
		MeshHeader hdr;
		fread(&hdr, sizeof(char), sizeof(MeshHeader), f);

		if (hdr.magic_num != MESH_MAGIC_NUM)
			return 0;

		//Read data straight into the buffers
		fseek(f, hdr.indexOffset, SEEK_SET);
		fread(targetLocs[(int)TargetIndices::Index], sizeof(char), hdr.vertexOffset - hdr.indexOffset, f);
		fread(targetLocs[(int)TargetIndices::Vertex], sizeof(char), hdr.uvOffset - hdr.vertexOffset, f);
		fread(targetLocs[(int)TargetIndices::UV], sizeof(char), hdr.normalOffset - hdr.uvOffset, f);
		fread(targetLocs[(int)TargetIndices::Normal], sizeof(char), hdr.normalLen, f);
		fclose(f);

		return (hdr.vertexOffset - hdr.indexOffset)/2;
	}
	else return 0;
}

__declspec(dllexport) int LoadBakedDraws(const char *file, void *target)
{
	FILE *f = fopen(file, "rb");
	if (f) {

		fseek(f, 0, SEEK_END);
		long len = ftell(f);

		fread(target, 1, len, f);
		fclose(f);

		return 1;
	}
	return 0;
}