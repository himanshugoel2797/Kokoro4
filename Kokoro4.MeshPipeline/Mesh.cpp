#include "stdafx.h"
#include "Mesh.h"

#include <fbxsdk.h>

int Mesh_ProcessMeshNode(FbxNode *node) {
	printf("Mesh\r\n");
	return 0;
}

int Mesh_ProcessSkeletonNode(FbxNode *node) {
	printf("Skeleton\r\n");
	return 0;
}

int Mesh_ProcessLightNode(FbxNode *node) {
	printf("Light\r\n");
	return 0;
}

int Mesh_ProcessCameraNode(FbxNode *node) {
	printf("Camera\r\n");
	return 0;
}

int Mesh_ProcessNode(FbxNode *node) {

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
			Mesh_ProcessMeshNode(child);
			break;
		case FbxNodeAttribute::EType::eSkeleton:
			Mesh_ProcessSkeletonNode(child);
			break;
		case FbxNodeAttribute::EType::eLight:
			Mesh_ProcessLightNode(child);
			break;
		case FbxNodeAttribute::EType::eCamera:
			Mesh_ProcessCameraNode(child);
			break;
		}

		Mesh_ProcessNode(child);
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

	Mesh_ProcessNode(scene->GetRootNode());

	return 0;
}