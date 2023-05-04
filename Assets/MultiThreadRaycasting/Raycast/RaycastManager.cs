using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;


public class RaycastManager : MonoBehaviour
{
    private const int DefaultRaycastLayers = -5;

    private static RaycastManager _instance;

    private Dictionary<IBatchRaycaster, NativeArray<RaycastCommand>> _quenue = new(10);

    //private int gIterator;
    //private Dictionary<IBatchRaycaster, int> _queueCasters = new Dictionary<IBatchRaycaster, int>(10);
    //private NativeList<RaycastCommand> _queueCommands = new NativeList<RaycastCommand>(20, Allocator.Persistent);

    private static void Initialise()
    {
        if (_instance != null) return;
        var manager = new GameObject("RaycastManager");
        DontDestroyOnLoad(manager);
        _instance = manager.AddComponent<RaycastManager>();
    }

    private void FixedUpdate()
    {
        int cmdAndHitsCount = _quenue.Sum(e=> e.Value.Length);
        var hits = new NativeArray<RaycastHit>(cmdAndHitsCount,Allocator.TempJob);
        var commands = new NativeArray<RaycastCommand>(cmdAndHitsCount,Allocator.TempJob);

        {
            int index = 0;
            foreach (var (raycaster, cmd) in _quenue)
            {
                switch (raycaster)
                {
                    case IBatchRaycasterMultiple batchRaycasterMultiple:
                        for (int j = 0; j < batchRaycasterMultiple.totalPossibleRaycasts; j++)
                        {
                            commands[index+j] = cmd[j];
                        }
                        index += batchRaycasterMultiple.totalPossibleRaycasts;
                        break;
                    case IBatchRaycasterSingle batchRaycasterSingle:
                        commands[index] = cmd[0];
                        index++;
                        break;
                }

            }
        }


        RaycastCommand.ScheduleBatch(commands, hits, cmdAndHitsCount).Complete();

        int i = 0;
        foreach (var (raycaster, command) in _quenue.ToArray())
        {
            switch (raycaster)
            {
                case IBatchRaycasterMultiple batchRaycasterMultiple:
                {
                    bool anyValid = false;
                    for (int j = 0; j < batchRaycasterMultiple.totalPossibleRaycasts; j++)
                    {
                        if (commands[i+j].distance > 0)
                        {
                            anyValid = true;
                            break;
                        }
                    }
                    
                    if(!anyValid) continue;

                    var slice = hits.Slice(i, batchRaycasterMultiple.totalPossibleRaycasts);
                    batchRaycasterMultiple.OnRaycastResult(slice.ToArray());

                    var nativeKeyValueArrays = _quenue[raycaster];
                    for (int j = 0; j < batchRaycasterMultiple.totalPossibleRaycasts; j++)
                    {
                        nativeKeyValueArrays[j] = default;
                    }
                    _quenue[raycaster] = nativeKeyValueArrays;
                    
                    i += batchRaycasterMultiple.totalPossibleRaycasts;
                    break;
                } 
                case IBatchRaycasterSingle batchRaycasterSingle:
                {
                    if (commands[i].distance<0.00001f)
                    {
                        continue;
                    }
                    batchRaycasterSingle.OnRaycastResult(hits[i]);
                    
                    
                    var nativeKeyValueArrays = _quenue[raycaster];
                    nativeKeyValueArrays[0] = default;
                    _quenue[raycaster] = nativeKeyValueArrays;
                    i++;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(raycaster));
            }
        }

        commands.Dispose();
        hits.Dispose();
    }


    public static void SubscribeRaycastCalls(IBatchRaycasterMultiple raycaster)
    {
        Initialise();
        var array = new NativeArray<RaycastCommand>(raycaster.totalPossibleRaycasts, Allocator.Persistent);
        _instance._quenue.Add(raycaster,array);
    }
    
    public static void SubscribeRaycastCalls(IBatchRaycasterSingle raycaster)
    {
        Initialise();
        var array = new NativeArray<RaycastCommand>(1, Allocator.Persistent);
        _instance._quenue.Add(raycaster,array);
    }
    
    public static void UnSubscribeRaycastCalls(IBatchRaycaster raycaster)
    {
        if (_instance._quenue.TryGetValue(raycaster, out var array))
        {
            array.Dispose();
            _instance._quenue.Remove(raycaster);
        }
    }
    
    
    

    public static void Raycast(
        IBatchRaycaster requestor,int raycastId,
        Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        if (_instance._quenue.TryGetValue(requestor, out var array))
        {
            array[raycastId] = new RaycastCommand(origin, direction, maxDistance, layerMask);
        }

       
        //Physics.defaultPhysicsScene.Raycast(origin, direction, maxDistance, layerMask);
    }
    
    public static void Raycast(
        IBatchRaycaster requestor,int raycastId,
        Vector3 origin, Vector3 direction, RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        if (_instance._quenue.TryGetValue(requestor, out var array))
        {
            array[raycastId] = new RaycastCommand(origin, direction, maxDistance, layerMask);
        }
        //Physics.defaultPhysicsScene.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
    }
    
    public static void Raycast(
        IBatchRaycaster requestor,int raycastId,
        Ray ray, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        Raycast(requestor, raycastId, ray.origin, ray.direction, maxDistance, layerMask);
    }
    
    public static void Raycast(
        IBatchRaycaster requestor,int raycastId,
        Ray ray, RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        Raycast(requestor, raycastId,ray.origin,ray.direction, hitInfo, maxDistance, layerMask);
    }
    
    
    public static void Linecast(
        IBatchRaycaster requestor,int raycastId,
        Vector3 start,
        Vector3 end,
        RaycastHit hitInfo,
        int layerMask = DefaultRaycastLayers)
    {
        Vector3 direction = end - start;
        Raycast(requestor, raycastId,start, direction, hitInfo, direction.magnitude, layerMask);
    }

}
