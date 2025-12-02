using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace NBShaderEditor
{
    public class ItemDrawerBase
    {
        // private ShaderGUIHelperNew _shaderGUIHelperNew;
        static Stack<(string,string)> _scopeContextStack = new Stack<(string,string)>();
        private bool _hasMixedValue;
        private Shader _shader;

        public (string, string) NameTuple;
        public ItemDrawerBase Parent;

    }
}