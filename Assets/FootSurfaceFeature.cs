using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using ThirdPersonController.MovementStateMachine;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class StepDecalFeature : BaseFeature
{
    public float footStepPlaceInterval = 0.5f;
    
    protected IMoveStateMachineVariables moveVariables;
    
    private Transform _leftFoot;
    private Transform _rightFoot;

    private float _leftFootCounter;
    private float _rightFootCounter;

    private Transform transform;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        transform = resolver.GetComponent<Transform>();
        
        var animator = resolver.GetComponent<Animator>();
        _leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        moveVariables = variables as IMoveStateMachineVariables;
    }

    public override void OnEnterState() => ResetBothCounters();

    protected void PlaceFootDecals(SurfaceEffect effect)
    {
        if(effect.decalsVariant == null || effect.decalsVariant.Length < 1)return;
        var up = transform.up;

        GameObject prefab = null;
        if (_leftFootCounter < 0.01f)
        {
            _leftFootCounter = footStepPlaceInterval;

            prefab = GetRandomPrefab(effect);
            var instance = PlaceDecal(_leftFoot, effect, prefab, in up);

            if (effect.destroyTimer.HasValue) Object.Destroy(instance, effect.destroyTimer.Value);
        }

        if (_rightFootCounter < 0.01f)
        {
            _rightFootCounter = footStepPlaceInterval;

            prefab ??= GetRandomPrefab(effect);
            var instance = PlaceDecal(_rightFoot, effect, prefab, in up, true);

            if (effect.destroyTimer.HasValue) Object.Destroy(instance, effect.destroyTimer.Value);
        }

        _leftFootCounter -= Time.deltaTime;
        _rightFootCounter -= Time.deltaTime;
    }

    protected void SetBothCountersTo0()
    {
        if(_leftFootCounter > footStepPlaceInterval * 0.6666667F && _rightFootCounter > footStepPlaceInterval * 0.6666667F) return;
        _leftFootCounter = 0;
        _rightFootCounter = 0;
    }
    
    protected void ResetBothCounters()
    {
        _leftFootCounter = footStepPlaceInterval * 2;
        _rightFootCounter = footStepPlaceInterval;
    }
    

    private GameObject PlaceDecal(Transform foot, SurfaceEffect effect, GameObject prefab, in Vector3 up, bool invertScale = false)
    {
        var position = foot.position;
        position.y = moveVariables.GroundHit.point.y + effect.spawnDistance;
        var rotation = Quaternion.AngleAxis(foot.eulerAngles.y, up);

        var instance = Object.Instantiate(prefab, position, rotation).transform;

        var scale = instance.localScale;
        scale *= Random.Range(effect.minMaxRandomScale.x, effect.minMaxRandomScale.y);
        if (invertScale) scale.x *= -1;
        instance.localScale = scale;

        return instance.gameObject;
    }
    private static GameObject GetRandomPrefab(SurfaceEffect effect)
    {
        return effect.decalsVariant[Random.Range(0, effect.decalsVariant.Length)];
    }
}

[MovedFrom("SoundFeature")]
public class FootSurfaceFeature : StepDecalFeature
{
    public FootSurfaceFeature()
    {
        movementFadeMultiplier = 2;
        movementPitchMultiplier = 1.3f;
        jumpSoundDelay = 0.1f;
    }
    [Min(1)]
    public float movementFadeMultiplier;
    
    [Min(1)]
    public float movementPitchMultiplier;
   
    public SurfaceEffect defaultEffect;
    public SurfaceEffect defaultJump;
    
    public float jumpSoundDelay;
    

    private AudioSource _source;
    private IBaseInputReader _inputReader;
    private Animator _animator;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        base.CacheReferences(variables,resolver);
        _source = (moveVariables as MovementStateMachineVariables).gameObject.AddComponent<AudioSource>();
        
        _inputReader = resolver.GetComponent<IBaseInputReader>();
        _animator = resolver.GetComponent<Animator>();

    }

    public override void OnUpdateState()
    {
        if (moveVariables.IsGrounded && !moveVariables.IsSlopeBadForMove)
        {
            PlayGroundSounds();
        }
    }



    private void PlayGroundSounds()
    {
        if (_inputReader.isJump)
        {
            var hitReaction =
                SurfaceSystem.instance.GetSurfaceHitEffect(moveVariables.GroundHit, SurfaceHitType.Jump, defaultJump);
            
            SetBothCountersTo0();

            Object audio = hitReaction.Audio ? hitReaction.Audio : defaultJump.Audio;
            PlaySound(audio);
            PlaceFootDecals(hitReaction);
            ResetBothCounters();
        }
        else
        {
            if (_inputReader.moveInputMagnitude > 0)
            {
                var hitReaction =
                    SurfaceSystem.instance.GetSurfaceHitEffect(moveVariables.GroundHit, SurfaceHitType.Foot, defaultEffect);
                
                Object audio = hitReaction.Audio ? hitReaction.Audio : defaultEffect.Audio;
                PlaySound(audio, _inputReader.moveInputMagnitude * movementPitchMultiplier);
                if (_inputReader.moveInputMagnitude > 0)
                    PlaceFootDecals(hitReaction);
            }
            else
            {
                if(_source.pitch>1)
                    _source.pitch = Mathf.Clamp(_source.pitch - Time.deltaTime*movementFadeMultiplier,1,4);
               //else
               //{
               //    _source.Stop();
               //}
                
            }
        }
    }

    private void PlaySound(Object audioType,float pitch = 1)
    {
        switch (audioType)
        {
            case null: {_source.Stop(); return;}
            case AudioClip clip:
            {
                if (!_source.isPlaying || _source.clip != clip) 
                    PlayClip(clip,pitch);
                else
                    _source.pitch = pitch;

                return;
            }
            case IAudioType audio:
            {
                audio.Play(_source);
                _source.pitch = pitch;
                return;
            }
        }
        

    }
    
    private void PlayClip(AudioClip clip, float volume = 1f,float pitch = 1f)
    {
        Debug.Log(_source.isPlaying);
        if (_source.isPlaying) 
            _source.Stop();
        _source.clip = clip;
        _source.spatialBlend = 0f;
        _source.volume = volume;
        _source.pitch = pitch;
        _source.Play();
        Debug.Log(_source.isPlaying);

        _source.SetScheduledEndTime( AudioSettings.dspTime + 3.0F+clip.length);
    }
}
