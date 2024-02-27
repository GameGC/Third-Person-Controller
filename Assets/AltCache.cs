#if UNITY_EDITOR
using System.Reflection;
#endif

using UnityEngine;

namespace Unity.VisualScripting
{
    /// <summary>
    /// Caches the input so that all nodes connected to the output
    /// retrieve the value only once.
    /// </summary>
    [UnitCategory("Control")]
    [UnitOrder(15)]
    public sealed class AltCache : Unit
    {
        /// <summary>
        /// The moment at which to cache the value.
        /// The output value will only get updated when this gets triggered.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The value to cache when the node is entered.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }

        /// <summary>
        /// The cached value, as it was the last time this node was entered.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Cached")]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        /// <summary>
        /// The action to execute once the value has been cached.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Store);
            input = ValueInput<object>(nameof(input));
            output = ValueOutput(nameof(output),GetValue);
            exit = ControlOutput(nameof(exit));

            Requirement(input, enter);
            Assignment(enter, output);
            Succession(enter, exit);
        }


#if UNITY_EDITOR
        private static readonly FieldInfo collectionField =
            typeof(VariableDeclarations).GetField("collection", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
        
       
        private ControlOutput Store(Flow flow)
        {
#if UNITY_EDITOR
            Debug.Log(Application.isPlaying);
            if (Application.isPlaying)
            {
                graph.variables.Set(guid.ToString(), flow.GetValue(input));
                EditorApplicationUtility.onExitPlayMode += OnExitPlayMode;
            }

            void OnExitPlayMode()
            {
                EditorApplicationUtility.onExitPlayMode -= OnExitPlayMode;
                (collectionField.GetValue(graph.variables) as VariableDeclarationCollection).Remove(
                    this.guid.ToString());
            }
#else
             graph.variables.Set(guid.ToString(), flow.GetValue(input));
#endif
            return exit;
        }

       

        private object GetValue(Flow flow)
        {
            return graph.variables.Get(guid.ToString());
        }

        
    }
}