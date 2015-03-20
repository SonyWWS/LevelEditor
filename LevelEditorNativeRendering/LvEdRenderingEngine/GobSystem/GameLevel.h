//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObjectGroup.h"
#include "../Renderer/RenderUtil.h"

namespace LvEdEngine
{
    class SkyDome;
    class TerrainGob;
    // this is the root top level containter for game objects.    
	class GameLevel : public GameObjectGroup
	{
	public:   

        GameLevel(): m_activeskyeDome(NULL) {}
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "GameLevel";}
               
        std::vector<TerrainGob*> Terrains;
        SkyDome* m_activeskyeDome;

        void SetFogEnabled(bool enabled) { m_fog.enabled = enabled;}
        void SetFogColor(int color ) { ConvertColor(color, &m_fog.Color);}
        void SetFogRange(float range) { m_fog.range = range;}
        void SetFogDensity(float density) { m_fog.density = density;  }       

        const ExpFog& GetFog() const {return m_fog;}
    private:
        ExpFog m_fog;     
    private:
        typedef GameObjectGroup super;

	};
}
