// Kokoro4.MeshPipeline.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include "Mesh.h"

#include <string>

using namespace std;

const char* pipeline_types[] = {
	"mesh",
	NULL,
};

int(*pipeline_hdlrs[])(int, char **, const char*) = {
	Mesh_Process,
	NULL,
};

int main(int argc, char *argv[])
{
	if (argc < 2) {
		printf("Available Content Processors:\r\n");

		int idx = 0;
		while (pipeline_types[idx] != NULL) {
			printf("\t%s\r\n", pipeline_types[idx]);
			idx++;
		}

		return 0;
	}

	//Determine pipeline content type
	int pipeline_idx = 0;
	while (pipeline_types[pipeline_idx] != NULL) {
		if (strncmp(pipeline_types[pipeline_idx], argv[1], strnlen(pipeline_types[pipeline_idx], 10)) == 0)
			return pipeline_hdlrs[pipeline_idx](argc, argv, pipeline_types[pipeline_idx]);
		pipeline_idx++;
	}

	printf("Unknown content processor type.");

    return 0;
}

