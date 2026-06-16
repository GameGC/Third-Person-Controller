#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameGC.CommonEditorUtils.Editor
{
    public static class AllTypesContainer
    {
        public static readonly List<Type> AllTypes;
        static AllTypesContainer()
        {
            AllTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).ToList();
        }
    }
}
#endif
