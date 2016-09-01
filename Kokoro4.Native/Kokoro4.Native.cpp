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
		fread(&hdr, sizeof(char), sizeof(hdr), f);

		//Read data straight into the buffers
		fseek(f, hdr.indexOffset, SEEK_SET);
		fread(targetLocs[(int)TargetIndices::Index], sizeof(char), hdr.vertexOffset - hdr.indexOffset, f);
		fread(targetLocs[(int)TargetIndices::Vertex], sizeof(char), hdr.uvOffset - hdr.vertexOffset, f);
		fread(targetLocs[(int)TargetIndices::UV], sizeof(char), hdr.normalOffset - hdr.uvOffset, f);
		fread(targetLocs[(int)TargetIndices::Normal], sizeof(char), hdr.normalLen, f);
		fclose(f);

		return 0;
	}
	else return -1;
}

__declspec(dllexport) int LoadDDS1Texture(const char *file, void *buffer)
{
	//Load DDS1 textures straight into the buffers
	return 0;
}