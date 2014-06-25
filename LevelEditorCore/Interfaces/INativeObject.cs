//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace LevelEditorCore
{
    public interface INativeObject
    {
        /// <summary>
        /// Invoke native member function </summary>
        /// <param name="fn">function name</param>
        /// <param name="arg">arguments </param>
        /// <param name="retval">return value</param>
        void InvokeFunction(string fn, IntPtr arg, out IntPtr retval);

        /// <summary>
        /// Gets instance id</summary>
        ulong InstanceId { get; }
    }
}
