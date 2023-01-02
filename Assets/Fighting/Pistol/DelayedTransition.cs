using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ThirdPersonController.Core;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[Serializable]
public class DelayedTransition : BaseStateTransition
{
    [SerializeField,ClipToSecondsAttribute] private float delayTime;

    private bool _wasStarted;
    private bool _wasTimeFinished;

    private Timer _tempTimer;
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _tempTimer = new Timer(delayTime * 1000) {AutoReset = false };
        _tempTimer.Elapsed += TimerOnElapsed;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _wasTimeFinished = true;
        _tempTimer.Stop();
        _tempTimer.Enabled = false;
    }
    
    public override bool couldHaveTransition
    {
        get
        {
            if (!_wasStarted)
            {
                _tempTimer.Enabled = true;
                _tempTimer.Start();
                _wasStarted = true;
            }

            if (_wasTimeFinished)
            {
                _wasTimeFinished = false;
                _wasStarted = false;
                return true;
            }

            return false;
        }
    }
    
    
}

[Serializable]
public class MultipleConditionTransition : BaseStateTransition
{
    [Header("When call conditions true")]
    
    
    [SerializeReference,SerializeReferenceAddButton(typeof(BaseStateTransition))]
    public BaseStateTransition[] Transitions = new BaseStateTransition[0];

    public void OnValidate()
    {
        for (var i = 0; i < Transitions.Length; i++)
        {
            Transitions[i].path = $"{path}.Transitions.Array.data[{i}]";
        }

    }
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        foreach (var transition in Transitions)
        {
            transition.Initialise(variables,resolver);
        }
    }

    public override bool couldHaveTransition 
    {
        get { return Transitions.All(t => t.couldHaveTransition); }
    }
}



[Serializable]
public class BaseRigFeature : BaseFeature
{
    [SerializeField] private string layerName;
    [SerializeField] private bool returnPreviousValueOnExit = true;
    
    protected RigLayer _targetLayer;
    private float _previousValue;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _targetLayer = resolver.GetComponent<RigBuilder>().layers.
            FirstOrDefault(l => l.name == layerName);
    }

    public override void OnEnterState()
    {
        _previousValue = _targetLayer.rig.weight;
    }

    public override void OnExitState()
    {
        if (returnPreviousValueOnExit)
            _targetLayer.rig.weight = _previousValue;
    }
}
public class RigFadeFeature : BaseRigFeature
{
     [SerializeField] private float fadeMax = 1;
     [SerializeField] private float deltaMultiplayer = 1;

    private float tempLerp;

    public override void OnEnterState()
    {
        tempLerp = 0;
        base.OnEnterState();
    }

    public override void OnUpdateState()
    {
        tempLerp += deltaMultiplayer * Time.deltaTime;
        _targetLayer.rig.weight = Mathf.Lerp(0, fadeMax, tempLerp);
    }
}

public class RigSetFeature : BaseRigFeature
{
    [SerializeField] private float weight;

    public override void OnEnterState()
    {
        base.OnEnterState();
        _targetLayer.rig.weight = weight;
    }
}










[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ClipToSecondsAttribute : PropertyAttribute{ }


[CustomPropertyDrawer(typeof(ClipToSecondsAttribute))]
public class ClipToSecondsDrawer : PropertyDrawer
{
    private static Dictionary<string, AnimationClip> tempBuffer = new Dictionary<string, AnimationClip>(10);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 36;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var path = property.propertyPath;
        tempBuffer.TryGetValue(path, out var clip);


        position.height = 18;
        
        EditorGUI.BeginChangeCheck();
        clip= EditorGUI.ObjectField(position,"Clip", clip,typeof(AnimationClip),false) as AnimationClip;
        if (EditorGUI.EndChangeCheck())
        {
            if (!tempBuffer.ContainsKey(path))
                tempBuffer.Add(path, clip);
            else tempBuffer[path] = clip;

            property.floatValue = clip.length;
            property.serializedObject.ApplyModifiedProperties();
        }

        position.y += position.height;

        EditorGUI.PropertyField(position, property, label);

    }
}
