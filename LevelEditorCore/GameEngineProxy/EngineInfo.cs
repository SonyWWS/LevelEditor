//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace LevelEditorCore
{
    /// <summary>
    /// Provide access to game engine information.</summary>
    public class EngineInfo
    {
        /// <summary>
        /// Construct new instance of EngineInfo 
        /// by parsing engineInfoStr.</summary>
        /// <param name="engineInfoStr"></param>
        public EngineInfo(string engineInfoStr)
        {

            if (string.IsNullOrWhiteSpace(engineInfoStr))
                throw new ArgumentNullException("engineInfoStr");
            ResourceInfos = new ResourcesInfos(engineInfoStr);
        }

        /// <summary>
        /// Information about all the supported assets types</summary>
        public readonly ResourcesInfos ResourceInfos;
       
    }
}
