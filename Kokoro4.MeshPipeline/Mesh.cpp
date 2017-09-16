#include "stdafx.h"
#include "Mesh.h"

#include <fbxsdk.h>

typedef struct {
	FbxScene *scene;
	const char *inputFile;
} MeshProcessorState;

static char* props[] = {
	NULL,
};

int Mesh_ProcessMeshNode(MeshProcessorState *state, FbxNode *node) {
	printf("Mesh\r\n");

	FbxMesh *m = node->GetMesh();

	int VertexCount = m->GetControlPointsCount();
	int TexCoordLayer = -1, NormalLayer = -1;

	int LayerCount = m->GetLayerCount();
	for (int idx = 0; idx < LayerCount; idx++) {
		auto Layer = m->GetLayer(idx);
		if (TexCoordLayer == -1 && Layer->GetUVs() != NULL)
			TexCoordLayer = idx;

		if (NormalLayer == -1 && Layer->GetNormals() != NULL)
			NormalLayer = idx;

		if (TexCoordLayer != -1 && NormalLayer != -1)
			break;
	}
	if (TexCoordLayer == -1 | NormalLayer == -1) {
		printf("Error: Need TexCoords and Normals\r\n");
		return 0;
	}

	auto Vertices = m->GetControlPoints();
	auto TexCoords = m->GetLayer(TexCoordLayer)->GetUVs();
	auto Normals = m->GetLayer(NormalLayer)->GetNormals();


	//Obtain any custom properties, and associated data
	auto PropertyIter = m->GetFirstProperty();
	while (PropertyIter.IsValid()) {

		const char *PropName = PropertyIter.GetNameAsCStr();
		int PropIdx = -1;


		//Get the property index
		while (props[++PropIdx] != NULL)
			if (strncmp(PropName, props[PropIdx], strnlen(props[PropIdx], 4096)) == 0)
				break;

		if (props[PropIdx] != NULL) {
			//Process the properties
			
		}
		else
			printf("Unknown Property Found: %s\r\n", PropName);

		PropertyIter = m->GetNextProperty(PropertyIter);
	}

	//Process the above data and write it to disk

	//Store the mesh
	//Store all UV layers
	//Store material data and selected shaders
	//Store all particle emitter information
	//Store the skeleton
	//Track skeleton mount points
	//Store physics bodies
	//Store the animation tracks

	//TODO: Later
	//Accoustic modelling
	//Quadric simplification based LoD generation and transition distance computation
	//Damage model generation
	//Fracture simulation


	return 0;
}

int Mesh_ProcessSkeletonNode(MeshProcessorState *state, FbxNode *node) {
	printf("Skeleton\r\n");
	return 0;
}

int Mesh_ProcessLightNode(MeshProcessorState *state, FbxNode *node) {
	printf("Light\r\n");
	return 0;
}

int Mesh_ProcessCameraNode(MeshProcessorState *state, FbxNode *node) {
	printf("Camera\r\n");
	return 0;
}

int Mesh_ProcessNode(MeshProcessorState *state, FbxNode *node) {

	if (node == NULL)
		return 0;

	printf("Process Child\r\n");

	//TODO: handle relative transforms

	int child_cnt = node->GetChildCount();
	for (int idx = 0; idx < child_cnt; idx++) {
		FbxNode *child = node->GetChild(idx);
		FbxNodeAttribute *nodeAttr = child->GetNodeAttribute();

		switch (nodeAttr->GetAttributeType()) {
		case FbxNodeAttribute::EType::eMesh:
			Mesh_ProcessMeshNode(state, child);
			break;
		case FbxNodeAttribute::EType::eSkeleton:
			Mesh_ProcessSkeletonNode(state, child);
			break;
		case FbxNodeAttribute::EType::eLight:
			Mesh_ProcessLightNode(state, child);
			break;
		case FbxNodeAttribute::EType::eCamera:
			Mesh_ProcessCameraNode(state, child);
			break;
		case FbxNodeAttribute::EType::eNull:
			continue;
		}

		Mesh_ProcessNode(state, child);
	}

	return 0;
}

int Mesh_Process(int argc, char *argv[], const char *pipeline_name) {

	if (argc < 3) {
		printf("Content Processor Usage:\r\n");
		printf("%s %s [input file]\r\n", argv[0], pipeline_name);
		return 0;
	}

	const char *src_fname = argv[2];

	FbxManager* fbxSdkMgr = FbxManager::Create();
	FbxIOSettings* ioSettings = FbxIOSettings::Create(fbxSdkMgr, IOSROOT);

	ioSettings->SetBoolProp(IMP_FBX_MATERIAL, true);
	ioSettings->SetBoolProp(IMP_FBX_TEXTURE, true);
	ioSettings->SetBoolProp(IMP_FBX_ANIMATION, true);
	ioSettings->SetBoolProp(IMP_FBX_MODEL, true);
	ioSettings->SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);

	fbxSdkMgr->SetIOSettings(ioSettings);

	FbxImporter* fbxImporter = FbxImporter::Create(fbxSdkMgr, "");
	if (!fbxImporter->Initialize(src_fname, -1, ioSettings)) {	//Load the file
		printf("Failed to open file. Exiting.\r\n");
		return -1;
	}

	FbxScene* scene = FbxScene::Create(fbxSdkMgr, "scene");
	fbxImporter->Import(scene);	//Import the file contents into the scene
	fbxImporter->Destroy(); //Done with the importer

	MeshProcessorState state;
	state.scene = scene;
	state.inputFile = src_fname;

	Mesh_ProcessNode(&state, scene->GetRootNode());

	return 0;
}