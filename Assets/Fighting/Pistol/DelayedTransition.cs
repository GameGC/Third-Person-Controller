using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Fighting.Pushing;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

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

public class EndPlayTransition : BaseStateTransition
{
    public string WaitForAnimationEnd;
    
    private AnimationLayer _layer;
    private bool _wasStarted;

    private Task waitTask;
    
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _layer = (variables as FightingStateMachineVariables).GetComponent<AnimationLayer>();
    }

    
    public override bool couldHaveTransition
    {
        get
        {
            if (!_wasStarted)
            {
                waitTask = _layer.WaitForAnimationFinish(WaitForAnimationEnd);
                _wasStarted = true;
            }

            if (waitTask.IsCompleted)
            {
                waitTask = null;
                _wasStarted = false;
                return true;
            }
            return false;
        }
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
        return 18;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = position.width;
        position.width = width * 0.6666667F;
        EditorGUI.PropertyField(position, property, label);
        position.x +=  position.width;
        position.width = width * 0.33333334F;
        
        var path = property.propertyPath;
        tempBuffer.TryGetValue(path, out var clip);


      
        
        EditorGUI.BeginChangeCheck();
        clip= EditorGUI.ObjectField(position, clip,typeof(AnimationClip),false) as AnimationClip;
        if (EditorGUI.EndChangeCheck())
        {
            if (!tempBuffer.ContainsKey(path))
                tempBuffer.Add(path, clip);
            else tempBuffer[path] = clip;

            property.floatValue = clip.length;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
