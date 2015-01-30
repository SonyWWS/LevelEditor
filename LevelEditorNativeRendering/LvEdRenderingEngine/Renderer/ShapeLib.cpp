//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ShapeLib.h"
#include "RenderBuffer.h"
#include "Model.h"
#include "RenderUtil.h"
#include "GpuResourceFactory.h"


namespace LvEdEngine
{

static Mesh* s_meshes[RenderShape::MAX];


// declarations

    static Mesh* CreateUnitSphere(ID3D11Device* device);
    static Mesh* CreateUnitCylinder(ID3D11Device* device);
    static Mesh* CreateUnitCone(ID3D11Device* device);
    static Mesh* CreateUnitCube(ID3D11Device* device);
    static Mesh* CreateUnitQuad(ID3D11Device* device);
    static Mesh* CreateUnitQuadLine(ID3D11Device* device);
    static Mesh* CreateStarUnitQuads(ID3D11Device* device);
    static Mesh* CreateUnitTorus(ID3D11Device* device);
    static void CreateSphere(float radius,
        uint32_t slices,
        uint32_t stacks,
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);

 

    static void CreateCone(float rad,
        float height,
        uint32_t slices,
        uint32_t stacks, 
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);

    static void CreateCylinder(float radius1,
        float radius2,
        float start,
        float height,
        uint32_t slices,
        uint32_t stacks,
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);

    static void CreateTorus(float innerRadius, 
        float outerRadius,
        uint32_t rings,
        uint32_t sides,
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);


    static void CreateCube(float width, 
        float height, 
        float depth,
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);
     
    static void CreateQuad(float width, 
        float height, 
        std::vector<float3>* pos,
        std::vector<float3>* nor,
        std::vector<float2>* tex,
        std::vector<uint32_t>* indices);

    static void CreateQuadLine(float width, float height, std::vector<float3>* pos);
           
    
//-------------------------------------------------------------------------------------------------
// create the shapes.
void ShapeLibStartup(ID3D11Device* device)
{
    s_meshes[RenderShape::QuadLineStrip]    = CreateUnitQuadLine(device);
    s_meshes[RenderShape::Quad]             = CreateUnitQuad(device);
    s_meshes[RenderShape::Sphere]           = CreateUnitSphere(device);
    s_meshes[RenderShape::Cylinder]         = CreateUnitCylinder(device);
    s_meshes[RenderShape::Torus]            = CreateUnitTorus(device);
    s_meshes[RenderShape::Cube]             = CreateUnitCube(device);
    s_meshes[RenderShape::Cone]             = CreateUnitCone(device);
    s_meshes[RenderShape::AsteriskQuads]    = CreateStarUnitQuads(device);

}
    
//-------------------------------------------------------------------------------------------------
// destroy the shapes
void ShapeLibShutdown()
{
    for(unsigned int i = RenderShape::MIN; i < RenderShape::MAX; ++i)
    {        
        SAFE_DELETE(s_meshes[i]);     
    }
}



//-------------------------------------------------------------------------------------------------
Mesh* ShapeLibGetMesh(RenderShapeEnum shape)
{
    return s_meshes[shape];
}


// ----------------------------------------------------------------------------------------------
// UTILITY FUNCTIONS
// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitSphere(ID3D11Device* device) 
{
    Mesh* mesh = new Mesh();
    mesh->name = "UnitSphere";    
    
    CreateSphere( 0.5f, 16, 16, &mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);

    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();

    return mesh;
}

// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitCylinder(ID3D11Device* device)
{
    Mesh* mesh = new Mesh();        
    mesh->name = "UnitCylinder";

    CreateCylinder( 0.5f, 0.5f, -.5f, 1.0f, 16, 6, &mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);
    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();

    return mesh;    
}

static Mesh* CreateUnitTorus(ID3D11Device* device)
{
    Mesh* mesh = new Mesh();        
    mesh->name = "UnitTorus";

    CreateTorus( 0.3f, 0.5f, 32, 8,&mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);
    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();

    return mesh;    

}

// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitCone(ID3D11Device* device)
{
    Mesh* mesh = new Mesh(); 
    mesh->name = "UnitCone";
    CreateCylinder( 0.5f, 0.0f, -0.5f, 1.0f, 16, 6, &mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);
    mesh->ComputeTangents();
    mesh->Construct(device);  
    mesh->ComputeBound();
    return mesh;
}

// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitCube(ID3D11Device* device)
{
    Mesh* mesh = new Mesh(); 
    mesh->name = "UnitCube";
    
    CreateCube(0.5f, 0.5f, 0.5f, &mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);
    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();
    return mesh;
}


// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitQuad(ID3D11Device* device)
{
    Mesh* mesh = new Mesh(); 
    mesh->name = "UnitQuad";
    
    CreateQuad(0.5f, 0.5f, &mesh->pos, &mesh->nor, &mesh->tex, &mesh->indices);
    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();
    return mesh;
}


static Mesh* CreateStarUnitQuads(ID3D11Device* device)
{
    Mesh* mesh = new Mesh(); 
    mesh->name = "StarQuads";

    mesh->pos.resize(12);
    mesh->nor.resize(12);
    mesh->tex.resize(12);
    mesh->indices.resize(18);

    std::vector<float3> pos;
    std::vector<float2> tex;
    std::vector<float3> norm;
    std::vector<uint32_t> indices;
    CreateQuad(0.5f, 0.5f, &pos, &norm, &tex, &indices);
    
    Matrix rotY60  = Matrix::CreateRotationY(ToRadian(60));
    Matrix rotY120 = Matrix::CreateRotationY(ToRadian(120));
      
    for(int i = 0; i < 4; i++)
    {
        pos[i].y += 0.5f;
        mesh->pos[i] = pos[i];
        mesh->nor[i] = norm[i];
        mesh->tex[i] = tex[i];

        mesh->pos[i+4]  = float3::TransformNormal(pos[i],rotY60);
        mesh->nor[i+4]  = float3::TransformNormal(norm[i],rotY60);       
        mesh->tex[i+4]  = tex[i];

        mesh->pos[i+8] = float3::TransformNormal(pos[i],rotY120);
        mesh->nor[i+8] = float3::TransformNormal(norm[i],rotY120);       
        mesh->tex[i+8] = tex[i];
    }

    for(int i = 0; i < 6; i++)
    {
        mesh->indices[i]    = indices[i];
        mesh->indices[i+6]  = indices[i] + 4;
        mesh->indices[i+12] = indices[i] + 8;        
    }

    mesh->ComputeTangents();
    mesh->Construct(device);
    mesh->ComputeBound();
    return mesh;
}

// ----------------------------------------------------------------------------------------------
static Mesh* CreateUnitQuadLine(ID3D11Device* device)
{
    Mesh* mesh = new Mesh();
    mesh->primitiveType = PrimitiveType::LineStrip;
    CreateQuadLine(0.5f, 0.5f, &mesh->pos);
    //mesh->vertexBuffer = CreateVertexBuffer(device, VertexFormat::VF_P, (void*)&mesh->pos[0], (uint32_t)mesh->pos.size());    
    mesh->Construct(device);
    
#if defined(DEBUG) || defined(_DEBUG)
    mesh->vertexBuffer->SetDebugName("UnitQuadLine");    
#endif

    mesh->ComputeBound();
    return mesh;
}

//---------------------------------------------------------------------------------------------
static void CreateSphere(float radius, uint32_t slices, uint32_t stacks, std::vector<float3>* pos, std::vector<float3>* nor, std::vector<float2>* tex, std::vector<uint32_t>* indices)
{
    assert(radius>0);
    assert(slices>=2);
    assert(stacks>=2);
    assert(pos);
    assert(indices);
            
    // caches sin cos.
    float* cosPhi = new float[stacks];
    float* sinPhi = new float[stacks];
    float phiStep = LvEdEngine::Pi / stacks;

    float* cosTheta = new float[slices];
    float* sinTheta = new float[slices];
    float thetaStep = LvEdEngine::TwoPi / slices;

    float phi = 0;
    for (unsigned s = 0; s < stacks; s++, phi += phiStep)
    {
        sinPhi[s] = (float)sin(phi);
        cosPhi[s] = (float)cos(phi);
    }

    float theta = 0;
    for (unsigned s = 0; s < slices; s++, theta += thetaStep)
    {
        sinTheta[s] = (float)sin(theta);
        cosTheta[s] = (float)cos(theta);
    }
            
    float3 northPole(0, radius, 0);
    pos->push_back(northPole); // north pole.            
    if(nor) nor->push_back(normalize(northPole)); // north pole.
    if(tex) tex->push_back(float2(0,0));
            
    for (unsigned s = 1; s < stacks; s++)
    {
        float y = radius * cosPhi[s];
        float r = radius * sinPhi[s];
        for (unsigned l = 0; l < slices; l++)
        {
            float3 p;
            p.y = y;
            p.z = r * cosTheta[l];
            p.x = r * sinTheta[l];
            float u = l/(float) slices;
            float v = 1.0f - (float) s/stacks;
            pos->push_back(p);                    
            if(nor) nor->push_back(normalize(p));
            if(tex) tex->push_back(float2(u,v));
        }
    }
    float3 southPole(0, -radius, 0);
    pos->push_back(southPole);
    if(nor) nor->push_back(normalize(southPole));
    if(tex) tex->push_back(float2(1,1));


    // 2l + (s-2) * 2l
    // 2l * ( 1 + s-2)
    // 2l * ( s- 1)

            

    // create index of north pole cap.
    for (uint32_t l = 1; l < slices; l++)
    {
        indices->push_back(l + 1);
        indices->push_back(0);
        indices->push_back(l);
    }
    indices->push_back(1);
    indices->push_back(0);
    indices->push_back(slices);

    for (uint32_t s = 0; s < (stacks - 2); s++)
    {
        uint32_t l = 1;
        for (; l < slices; l++)
        {
            indices->push_back( (s + 1) * slices + l + 1); // bottom right.
            indices->push_back( s * slices + l + 1); // top right.
            indices->push_back(s * slices + l); // top left.

            indices->push_back( (s + 1) * slices + l); // bottom left.
            indices->push_back( (s + 1) * slices + l + 1); // bottom right.
            indices->push_back( s * slices + l); // top left.

        }

        indices->push_back( (s + 1) * slices + 1); // bottom right.
        indices->push_back( s * slices + 1); // top right.
        indices->push_back( s * slices + slices); // top left.


        indices->push_back( (s + 1) * slices + slices); // bottom left.
        indices->push_back( (s + 1) * slices + 1); // bottom right.
        indices->push_back( s * slices + slices); // top left.


    }

    // create index for south pole cap.
    uint32_t baseIndex = slices * (stacks - 2);
    uint32_t lastIndex = (uint32_t)pos->size() - 1;
    for (uint32_t l = 1; l < slices; l++)
    {
        indices->push_back( baseIndex + l);
        indices->push_back( lastIndex);
        indices->push_back( baseIndex + l + 1);
    }
    indices->push_back(baseIndex + slices);
    indices->push_back(lastIndex);
    indices->push_back(baseIndex + 1);

    // release memory
    delete[] cosPhi;
    delete[] sinPhi;
    delete[] cosTheta;
    delete[] sinTheta;
}
//
////---------------------------------------------------------------------------------------------
//static void CreateDome(float radius, uint32_t dim, std::vector<float3>* pos, std::vector<float3>* nor, std::vector<float2>* tex, std::vector<uint32_t>* indices)
//{
//    UNREFERENCED_VARIABLE(nor);
//    assert(radius>0);
//    assert(dim > 2);        
//    assert(pos);
//    assert(nor == NULL); // normal generation not currently supported.
//    assert(indices);
//        
//	float cosTheta = (100.0f * (Pi/180.0f));
//		
//	for (uint32_t i = 0; i < dim; i++)
//	{
//		float rxz = cosTheta * (float)i / (float)(dim - 1);
//		for (uint32_t j = 0; j < dim; j++)
//		{
//                float theta = TwoPi * (float)j / (float)(dim - 1);
//                    
//                float x = sin(rxz) * cos(theta);
//                float y = cos(rxz);
//                float z = sin(rxz) * sin(theta);
//				float u = j / (dim - 1.0f);
//				float v = i / (dim - 1.0f);
//                VertexPT vertex(x * radius,y * radius,z * radius,u,v);
//				pos->push_back(vertex.Position);
//                if(tex)
//                {
//                    tex->push_back(vertex.Tex);
//                }
//		}
//	}
//		
//	for (uint32_t i = 0; i < dim - 1; i++)
//    {
//		for (uint32_t j = 0; j < dim - 1; j++)
//		{
//			indices->push_back(i * dim + j);				
//			indices->push_back((i + 1) * dim + j);
//			indices->push_back((i + 1) * dim + j + 1);
//				
//			indices->push_back((i + 1) * dim + j + 1);				
//			indices->push_back( i * dim + j + 1);
//			indices->push_back( i * dim + j);
//		}
//	}
//
//}	



static void CreateTorus(float innerRadius, 
    float outerRadius,
    uint32_t rings,
    uint32_t sides,
    std::vector<float3>* pos,
    std::vector<float3>* nor,
    std::vector<float2>* tex,
    std::vector<uint32_t>* indices)
{
   
    uint32_t ringStride = rings + 1;
    uint32_t sideStride = sides + 1;

    // radiusC: distance to center of the ring
    float radiusC = (innerRadius + outerRadius) * 0.5f;

    //radiusR: the radius of the ring
    float radiusR = (outerRadius - radiusC);
    
    for (uint32_t i = 0; i <= rings; i++)
    {
        float u = (float)i / rings;
                       
        float outerAngle = i * TwoPi / rings;

        // xform from ring space to torus space.
        Matrix transform = Matrix::CreateTranslation(radiusC, 0, 0) * Matrix::CreateRotationY(outerAngle);
        
        // create vertices for each ring.
        for (uint32_t j = 0; j <= sides; j++)
        {
            float v = (float)j / sides;
            
            float innerAngle = j * TwoPi / sides + Pi;
            float dx = cos(innerAngle);
            float dy = sin(innerAngle);

            // normal, position ,and texture coordinates
            float3 n(dx, dy, 0);
            float3 p = n * radiusR;
            float2 t(u, v);

            p.Transform(transform);
            n.TransformNormal(transform);
            
            pos->push_back(p);
            nor->push_back(n);
            tex->push_back(t);

            // And create indices for two triangles.
            uint32_t nextI = (i + 1) % ringStride;
            uint32_t nextJ = (j + 1) % sideStride;

            indices->push_back(nextI * sideStride + j);            
            indices->push_back(i * sideStride + nextJ);
            indices->push_back(i * sideStride + j);
            
            indices->push_back(nextI * sideStride + j);            
            indices->push_back(nextI * sideStride + nextJ);
            indices->push_back(i * sideStride + nextJ);            
        }
    }
}


static void CreateCylinder(float radius1, float radius2, float start, float height, uint32_t slices, uint32_t stacks, 
    std::vector<float3>* pos, std::vector<float3>* nor, std::vector<float2>* tex, std::vector<uint32_t>* indices)
{
    float stackHeight = height / stacks;

    // Amount to increment radius as we move up each stack level from bottom to top.
    float radiusStep = (radius2 - radius1) / stacks;

    uint32_t numRings = stacks+1;

    // Compute vertices for each stack ring.
    for (uint32_t i = 0; i < numRings; ++i)
    {
        float y = start + i*stackHeight;
        float r = radius1 + i*radiusStep;

        // Height and radius of next ring up.
        float y_next =  start + (i + 1)*stackHeight;
        float r_next = radius1 + (i + 1)*radiusStep;

        // vertices of ring
        float dTheta = 2.0f*LvEdEngine::Pi/slices;
        for (uint32_t j = 0; j <= slices; ++j)
        {
            float c = (float) cos(j*dTheta);
            float s = (float) sin(j*dTheta);

            // tex coord if needed.
            float u = j/(float) slices;
            float v = 1.0f - (float) i/stacks;

            // Partial derivative in theta direction to get tangent vector (this is a unit vector).
            float3 T(-s, 0.0f, c);

            // Compute tangent vector down the slope of the cone (if the top/bottom 
            // radii differ then we get a cone and not a true cylinder).
            float3 P(r*c, y, r*s);
            float3 P_next(r_next*c, y_next, r_next*s);
            float3 B = normalize(P - P_next);
            float2 UV(u,v);


            float3 N = normalize(cross(T, B));

            // flip to right hand
            P.z *= -1;
            N.z *= -1;

            pos->push_back(P);
            if(nor) nor->push_back(N);
            if(tex) tex->push_back(UV);
                        
        }
    }

    uint32_t numRingVertices = slices+1;

    // Compute indices for each stack.
    for(uint32_t i = 0; i < stacks; ++i)
    {
        for (uint32_t j = 0; j < slices; ++j)
        {
            indices->push_back(i*numRingVertices + j);                    
            indices->push_back((i + 1)*numRingVertices + j + 1);
            indices->push_back((i + 1) * numRingVertices + j);

            indices->push_back(i*numRingVertices + j);                    
            indices->push_back(i*numRingVertices + j + 1);
            indices->push_back((i + 1) * numRingVertices + j + 1);
        }
    }

    // build bottom cap.
    if(radius1 > 0)
    {
        uint32_t baseIndex = (uint32_t) pos->size();
        // Duplicate cap vertices because the texture coordinates and normals differ.
        float y = start;

        // vertices of ring
        float dTheta = 2.0f * LvEdEngine::Pi / slices;
        for (uint32_t i = 0; i <= slices; ++i)
        {
            float x = radius1 * (float)cos(i * dTheta);
            float z = radius1 * (float)sin(i * dTheta);

            // Map [-1,1]-->[0,1] for planar texture coordinates.
            //float u = +0.5f * x / radius1 + 0.5f;
            //float v = -0.5f * z / radius1 + 0.5f;
 
            pos->push_back(float3(x,y,-z));
            if(nor) nor->push_back(float3(0.0f, -1.0f, 0.0f));
            if(tex) tex->push_back(float2(x,y));
        }


        // cap center vertex
        pos->push_back(float3(0.0f, y, 0.0f));
        if(nor) nor->push_back(float3(0.0f, -1.0f, 0.0f));
        if(tex) tex->push_back(float2(0.5f, 0.5f));


        // index of center vertex
        uint32_t centerIndex = (uint32_t)pos->size() - 1;
        for (uint32_t i = 0; i < slices; ++i)
        {
            indices->push_back(centerIndex);
            indices->push_back(baseIndex + i + 1);
            indices->push_back(baseIndex + i);
        }
    }

    // build top cap.
    if(radius2 > 0)
    {
        uint32_t baseIndex = (uint32_t) pos->size();

        // Duplicate cap vertices because the texture coordinates and normals differ.
        float y =  start + height;

        // vertices of ring
        float dTheta = 2.0f * LvEdEngine::Pi / slices;
        for (uint32_t i = 0; i <= slices; ++i)
        {
            float x = radius2 * (float)cos(i * dTheta);
            float z = radius2 * (float)sin(i * dTheta);

            pos->push_back(float3(x, y, -z));
            if(nor) nor->push_back(float3(0.0f, 1.0f, 0.0f) );

            // Map [-1,1]-->[0,1] for planar texture coordinates.
            //float u = +0.5f * x / radius2 + 0.5f;
            //float v = -0.5f * z / radius2 + 0.5f;
            if(tex) tex->push_back(float2(x,y));
        }

        // pos, norm, tex1 for cap center vertex
        pos->push_back(float3(0.0f, y, 0.0f));
        if(nor) nor->push_back(float3(0.0f, 1.0f, 0.0f));
        if(tex) tex->push_back(float2(0.5f, 0.5f));
                     
               
        // index of center vertex
        uint32_t centerIndex = (uint32_t)pos->size() - 1;
        for (uint32_t i = 0; i < slices; ++i)
        {
            indices->push_back(centerIndex);                    
            indices->push_back(baseIndex + i);
            indices->push_back(baseIndex + i + 1);
        }
    }
}

//---------------------------------------------------------------------------------------------
static void CreateCube(float width, float height, float depth, 
    std::vector<float3>* pos, std::vector<float3>* nor, std::vector<float2>* tex, std::vector<uint32_t>* indices)
{
    pos->reserve(24);
    nor->reserve(24);
    tex->reserve(24);
    indices->reserve(36);

    VertexPNT v[24];

    // Fill in the front face vertex data.
    v[ 0] = VertexPNT(-width, -height, -depth, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f);
    v[ 1] = VertexPNT(-width,  height, -depth, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
    v[ 2] = VertexPNT( width,  height, -depth, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f);
    v[ 3] = VertexPNT( width, -height, -depth, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f);

    // Fill in the back face vertex data.
    v[ 4] = VertexPNT(-width, -height, depth, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f);
    v[ 5] = VertexPNT( width, -height, depth, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f);
    v[ 6] = VertexPNT( width,  height, depth, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
    v[ 7] = VertexPNT(-width,  height, depth, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);

    // Fill in the top face vertex data.
    v[ 8] = VertexPNT(-width, height, -depth, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f);
    v[ 9] = VertexPNT(-width, height,  depth, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f);
    v[10] = VertexPNT( width, height,  depth, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f);
    v[11] = VertexPNT( width, height, -depth, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f);

    // Fill in the bottom face vertex data.
    v[12] = VertexPNT(-width, -height, -depth, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f);
    v[13] = VertexPNT( width, -height, -depth, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f);
    v[14] = VertexPNT( width, -height,  depth, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f);
    v[15] = VertexPNT(-width, -height,  depth, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f);

    // Fill in the left face vertex data.
    v[16] = VertexPNT(-width, -height,  depth, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
    v[17] = VertexPNT(-width,  height,  depth, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
    v[18] = VertexPNT(-width,  height, -depth, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
    v[19] = VertexPNT(-width, -height, -depth, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

    // Fill in the right face vertex data.
    v[20] = VertexPNT( width, -height, -depth, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
    v[21] = VertexPNT( width,  height, -depth, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
    v[22] = VertexPNT( width,  height,  depth, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
    v[23] = VertexPNT( width, -height,  depth, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

    for(int i = 0; i < 24; ++i)
    {
        v[i].Position.z *= -1;
        v[i].Normal.z *= -1;
        pos->push_back(v[i].Position);
        if(nor) nor->push_back(v[i].Normal);
        if(tex) tex->push_back(v[i].Tex);
    }


    uint32_t index[36];

	
    // Fill in the front face index data
    index[0] = 0; index[1] = 1; index[2] = 2;
    index[3] = 0; index[4] = 2; index[5] = 3;

    // Fill in the back face index data
    index[6] = 4; index[7] = 5; index[8] = 6;
    index[9] = 4; index[10] = 6; index[11] = 7;

    // Fill in the top face index data
    index[12] = 8; index[13] = 9; index[14] = 10;
    index[15] = 8; index[16] = 10; index[17] = 11;

    // Fill in the bottom face index data
    index[18] = 12; index[19] = 13; index[20] = 14;
    index[21] = 12; index[22] = 14; index[23] = 15;

    // Fill in the left face index data
    index[24] = 16; index[25] = 17; index[26] = 18;
    index[27] = 16; index[28] = 18; index[29] = 19;

    // Fill in the right face index data
    index[30] = 20; index[31] = 21; index[32] = 22;
    index[33] = 20; index[34] = 22; index[35] = 23;

    // reverse windings
    for (int i = 0; i < 34; i += 3)
    {
        indices->push_back(index[i + 2]);
        indices->push_back(index[i + 1]);
        indices->push_back(index[i]);
    }
}

//---------------------------------------------------------------------------------------------
static void CreateQuad(float width, float height, std::vector<float3>* pos, std::vector<float3>* nor, std::vector<float2>* tex, std::vector<uint32_t>* indices)
{   
    VertexPNT v[4];
    float maxt = 1.0f;
    // Fill in the front face vertex data.
    //                  position                normal           texcoord
    v[0] = VertexPNT( -width, -height, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, maxt);
    v[1] = VertexPNT( -width,  height, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f, 0.0f);
    v[2] = VertexPNT(  width,  height, 0.0f,  0.0f, 0.0f, 1.0f,  maxt, 0.0f);
    v[3] = VertexPNT(  width, -height, 0.0f,  0.0f, 0.0f, 1.0f,  maxt, maxt);

    // copy data to arrays
    for(int i = 0; i < 4; ++i)
    {
        pos->push_back(v[i].Position);
        if(nor) nor->push_back(v[i].Normal);
        if(tex) tex->push_back(v[i].Tex);
    }

    indices->push_back(2);
    indices->push_back(1);
    indices->push_back(0);

    indices->push_back(3);
    indices->push_back(2);
    indices->push_back(0);
}

//---------------------------------------------------------------------------------------------
static void CreateQuadLine(float width, float height, std::vector<float3>* pos)
{    
    // quad line vertices.
    // note the fith vertex is same as first that to complete quad line
    // when using primitive type line-strip
    pos->push_back(float3( -width, -height, 0.0f));
    pos->push_back(float3( -width,  height, 0.0f));
    pos->push_back(float3(  width,  height, 0.0f));
    pos->push_back(float3(  width, -height, 0.0f));
    pos->push_back(float3( -width, -height, 0.0f));
}
    
}; // namespace
