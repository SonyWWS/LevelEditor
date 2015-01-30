//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "MeshUtil.h"
#include <memory>
#include <vector>

using namespace std;

namespace MeshUtil
{

//void ComputeNormals(
//    int dimX,
//    int dimZ, 
//    const float3* inPos, 
//    float3*  outNormals)
//{
//     // A--------B    
//    // |      / |
//    // |   /    |
//    // | /      |
//    // C--------D
//    // each cell has two tri  ABC and CBD
//
//
//    //  triangles surrounding a vertex.
//    //  the vertex normal is the average of all the triangles normal surrounding it.
//    //  *--------*--------*
//    //  |      / |      / |
//    //  |   /    |t2  /   |
//    //  | /   t1 | / t3   |
//    //  *--------*--------*
//    //  |  t6  / | t4  /  |
//    //  |   / t5 |    /   |
//    //  | /      | /      |
//    //  *--------*--------*
//    //
//
//    //normals.resize(pos.size());    
//
//    int numCellX = (dimX - 1);
//    int numCellZ = (dimZ - 1);
//    int numFaces = numCellX * numCellZ * 2;
//    std::vector<float3> faceNormals;
//    faceNormals.resize(numFaces);
//
//    // step 1: compute face normal for each tri.    
//    int k = 0;            
//    for (int zc = 0; zc < numCellZ; zc++)
//    {
//        for (int xc = 0; xc < numCellX; xc++)
//        {                   
//            // get four vertices for the current cell
//            float3 A = inPos[zc * dimX + xc];
//            float3 B = inPos[zc * dimX + xc + 1];
//            float3 C = inPos[(zc + 1) * dimX + xc];
//            float3 D = inPos[(zc + 1) * dimX + xc + 1];
//            // compute face normal for triangle ABC
//            float3 BA = B - A;
//            float3 CA = C - A;
//            float3 n = cross(CA,BA);            
//            faceNormals[k] = n;
//
//            // compute face normal for triangle BCD
//            float3 BD = B - D;
//            float3 CD = C - D;
//            n = cross(BD, CD);
//            faceNormals[k+1] = n;
//            k += 2;            
//        }
//    }
//
//
//    // for each vertex average all the normals for all faces that
//    // rounds the vertex
//    k = 0;
//    int triPerRow = numCellX * 2;
//    for (int z = 0; z < dimZ; z++)
//    {
//        for (int x = 0; x < dimX; x++)
//        {
//            if (x == 0 || z == 0 || x == numCellX || z == numCellZ)
//            {
//                outNormals[k] = float3(0, 1, 0);                        
//            }
//            else
//            {                                
//                // vertex normal is average normal for all the surrounding face,
//                // index of the 
//                // the upper three tris
//                int t1 = (z - 1) * triPerRow + (x - 1) * 2 + 1;
//                int t2 = t1 + 1;  
//                int t3 = t2 + 1;
//
//                // the lower tris
//                int t4 = z * triPerRow + x * 2;
//                int t5 = t4 - 1;
//                int t6 = t5 - 1;                
//                float3 n = faceNormals[t1] + faceNormals[t2] + faceNormals[t3] + faceNormals[t4] + faceNormals[t5] +
//                    faceNormals[t6];                        
//                n = normalize(n);                
//                outNormals[k] = n;                
//            }
//            k++;
//        }
//    }
//}
//
//void ApplyGaussianBlur(float* data, int w, int h)
//{
//    // todo imple
//    size_t bufsize = w * h;
//    //float* tmp = new float[bufsize];
//    //memcpy_s(tmp,bufsize,data,bufsize);          
//    vector<float> tmp(data, data+bufsize);
//    for(int z = 1; z < (h-1); z++)
//    {
//        for(int x = 1; x < (w-1); x++)
//        {
//            // apply Gaussian kernel 
//            // | 1  2  1 |
//            // | 2  4  2 |
//            // | 1  2  1 |
//
//            int   k =  (z - 1) * w + x - 1;
//            float h1 = tmp[k];     // topLeft
//            float h2 = tmp[k + 1]; // topCenter
//            float h3 = tmp[k + 2]; // topRight;
//
//            k = z * w + x - 1;
//            float h4 = tmp[k];      // MidLeft
//            float h5 = tmp[k + 1];  // MidCenter
//            float h6 = tmp[k + 2];  // MidRight
//
//            k = (z + 1) * w + x - 1;
//            float h7 = tmp[k];     // ButtonLeft
//            float h8 = tmp[k + 1]; // ButtonCenter
//            float h9 = tmp[k + 2]; // ButtonRight
//
//            k = z * w + x;
//            data[k] =  (h1 + 2*h2 + h3 + 2*h4 + 4*h5 + 2*h6 + h7 + 2*h8 + h9) / 16.0f;
//
//        }
//    }
//
//}
}