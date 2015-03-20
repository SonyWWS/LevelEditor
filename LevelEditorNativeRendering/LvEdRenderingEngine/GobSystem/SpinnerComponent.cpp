#include "SpinnerComponent.h"
#include "GameObject.h"
using namespace LvEdEngine;

void SpinnerComponent::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{
    if(updateType != UpdateType::GamePlay)
        return;

    float dt = fr.ElapsedTime;

    if(!GetActive()) return;
	m_rot.x += dt * m_rps.x * TwoPi;
    m_rot.y += dt * m_rps.y * TwoPi;
    m_rot.z += dt * m_rps.z * TwoPi;

	// wrap angles
	if (m_rot.x >= TwoPi) m_rot.x -= TwoPi;
	else if (m_rot.x < -TwoPi) m_rot.x += TwoPi;

	if (m_rot.y >= TwoPi) m_rot.y -= TwoPi;
	else if (m_rot.y <= -TwoPi) m_rot.y += TwoPi;

	if (m_rot.z >= TwoPi) m_rot.z -= TwoPi;
	else if (m_rot.z <= -TwoPi) m_rot.z += TwoPi;

	const Matrix& xform = GetOwner()->GetTransform();

	Matrix scaleMtrx; scaleMtrx.MakeIdentity();
	scaleMtrx.M11 = length(Vector3(&xform.M11));
	scaleMtrx.M22 = length(Vector3(&xform.M21));
	scaleMtrx.M33 = length(Vector3(&xform.M31));

	Matrix rotMtrx = Matrix::CreateRotationX(m_rot.x)
		* Matrix::CreateRotationY(m_rot.y)
		* Matrix::CreateRotationZ(m_rot.z);

	Matrix transMtrx; transMtrx.MakeIdentity();
	transMtrx.M41 = xform.M41;
	transMtrx.M42 = xform.M42;
	transMtrx.M43 = xform.M43;
	Matrix m = (scaleMtrx * rotMtrx) * transMtrx;
	GetOwner()->SetTransform(m);    
}

