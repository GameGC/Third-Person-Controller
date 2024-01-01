using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cinemachine;
using Cinemachine.Editor;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[AttributeUsage(AttributeTargets.Class)]
public class ToolBarDisplayGroup : PropertyAttribute
{
    public string groupName;

    public ToolBarDisplayGroup(string groupName)
    {
        this.groupName = groupName;
    }
}

public abstract class BaseWeaponWithExtensions : MonoBehaviour
{
    [FormerlySerializedAs("EDITOR_extensions")] [SerializeField, HideInInspector] internal BaseWeaponExtension[] extensions;
    
    protected virtual void OnValidate()
    {
        extensions = GetComponents<BaseWeaponExtension>();
    }

    protected void Execute_OnShootExtensions()
    {
        foreach (var baseWeaponExtension in extensions)
        {
            baseWeaponExtension.OnShoot();
        }
    }
    
    protected void Execute_BeginReloadExtensions()
    {
        foreach (var baseWeaponExtension in extensions)
        {
            baseWeaponExtension.OnBeginReload();
        }
    }
    
    protected void Execute_EndReloadExtensions()
    {
        foreach (var baseWeaponExtension in extensions)
        {
            baseWeaponExtension.OnEndReload();
        }
    }
    
    protected void Execute_BeginCooldownExtensions()
    {
        foreach (var baseWeaponExtension in extensions)
        {
            baseWeaponExtension.OnBeginCooldown();
        }
    }

    protected void Execute_EndCooldownExtensions()
    {
        foreach (var baseWeaponExtension in extensions)
        {
            baseWeaponExtension.OnEndCooldown();
        }
    }
}

public class ShootingWeapon : BaseWeaponWithExtensions,IWeaponInfo
{
    public DefaultRaycastBullet prefab;
    public Transform spawnPoint;
    
    public GameObject muzzle;
    public float muzzleTime = 1;

    public bool hasAutoReloadOnStart = true;
    public int totalAmmo = 1000; 
    public int ammoImMagazine = 5;
    
    public float cooldownTime = 3;
    [ClipToSeconds]
    public float reloadingTime = 10;

    public bool calcFormat;
    public int maxShootsPerMinute;
    

    private IFightingStateMachineVariables Variables;
    
    private CinemachineImpulseSource _impulseSource;

    public void CacheReferences(IFightingStateMachineVariables variables)
    {
        Variables = variables;
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        if (calcFormat)
        {
            float reloadsPerMinute = (float)maxShootsPerMinute / ammoImMagazine;
            reloadingTime = 60f / reloadsPerMinute;
        }
    }
    private void Start()
    {
        if (hasAutoReloadOnStart) 
            AutoReload();
    }

    private void AutoReload()
    {
        if (totalAmmo - ammoImMagazine <= 0) return;
        totalAmmo -= ammoImMagazine;
        remainingAmmo = ammoImMagazine;
    }

    public void Shoot()
    { 
        if(Variables.isCooldown)   throw new Exception("Wrong Cooldown execution, check conditions in executor class");
        if(Variables.isReloading)  throw new Exception("Wrong Reloading execution, check conditions in executor class");
        if(!Variables.couldAttack) throw new Exception("Wrong CouldAttack execution, check conditions in executor class");
        
        if (remainingAmmo > 0)
        {
            remainingAmmo--;
            Variables.couldAttack = remainingAmmo > 0;


            Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            
            Execute_OnShootExtensions();

            _impulseSource?.GenerateImpulse();
            
            
            if (muzzle)
            {
                muzzle.SetActive(true);
                Invoke(nameof(DisableMuzzle),muzzleTime);
            }
        }
        

        //reload
        if (remainingAmmo < 1)
        {
            if (totalAmmo - ammoImMagazine <= 0) return;
            Variables.isReloading = true;
            
            Execute_BeginReloadExtensions();
            Invoke(nameof(Reload),reloadingTime);
        }
        //cooldown
        else
        {
            Variables.isCooldown = true;
            
            Execute_BeginCooldownExtensions();
            Invoke(nameof(Cooldown),cooldownTime);
        }
    }

    public int remainingAmmo { get; private set; }
    public int maxAmmo => ammoImMagazine;

    private void Reload()
    {
        totalAmmo -= ammoImMagazine;
        remainingAmmo = ammoImMagazine;
        Variables.isReloading = false;
        Variables.couldAttack = true;
        Execute_EndReloadExtensions();
    }
    
    private void Cooldown()
    {
        Variables.isCooldown = false;
        Execute_EndCooldownExtensions();
    }

    private void DisableMuzzle() =>muzzle.SetActive(false);


    private void OnDrawGizmos() => Gizmos.DrawRay(spawnPoint.position,spawnPoint.forward*100);

    private void OnDisable() => CancelInvoke();
}

[CustomEditor(typeof(ShootingWeapon))]
public class WeaponShootingInfoEditor : Editor
{
    private class Tab
    {
        public string name;
        public List<Editor> editors = new List<Editor>(capacity:3);

        public Tab(string name) => this.name = name;
    }
    List<MonoBehaviour> list;
    private GUIContent[] _editorNames;
    private List<Tab> _tabs = new List<Tab>();
    
    private int _selectedTab;
    private bool useMinified;

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            _selectedTab = value;
            EditorPrefs.SetInt("weaponTabSelected", value);
        }
    }

    private void OnEnable()
    {
        var target = this.target as ShootingWeapon;
        useMinified = EditorPrefs.GetBool("useWeaponMinifiedEditor", true);
        
        list = new List<MonoBehaviour>(target.extensions);
        for (var i = 0; i < list.Count; i++)
        {
            var type = list[i].GetType();
            var attr = type.GetCustomAttribute<ToolBarDisplayGroup>();

            var editor = CreateEditor(list[i]);
            if (attr == null)
            {
                var tab = new Tab(type.Name);
                tab.editors.Add(editor);
                _tabs.Add(tab);
            }
            else
            {
                int index = _tabs.FindIndex(t => t.name == attr.groupName);
                if(index > -1) _tabs[index].editors.Add(editor);
                else
                {
                    var tab = new Tab(attr.groupName);
                    tab.editors.Add(editor);
                    _tabs.Add(tab);
                }
            }
        }


        _editorNames = EditorGUIUtility.TrTempContent(_tabs.Select(l => l.name).ToArray());
        ArrayUtility.Insert(ref _editorNames,0,new GUIContent("Shooting"));
        
        _selectedTab = Mathf.Clamp(EditorPrefs.GetInt("weaponTabSelected"),0,_tabs.Count);
        
        if(useMinified) Minify();
        else UnMinify();
    }

    public override void OnInspectorGUI()
    {
        GUI.backgroundColor = new Color(1f, 0.58f, 0f, 0.21f);
        using (new GUILayout.HorizontalScope())
        {
            var content = new GUIContent("View Type: ");
            GUILayout.Label(content,GUILayout.Width(EditorStyles.label.CalcSize(content).x));
            EditorGUI.BeginChangeCheck();
            

            var viewType = GUILayout.Toolbar(useMinified ? 0 : 1, new[] {"Designer", "Developer"},GUI.skin.button,GUI.ToolbarButtonSize.FitToContents);
            if (EditorGUI.EndChangeCheck())
            {
                useMinified = viewType == 0;
                if(useMinified) Minify();
                else UnMinify();
                InspectorUtility.RepaintGameView();
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.Space(9f);


        if (!useMinified)
        {
            base.OnInspectorGUI();
            return;
        }

        for (int i = 0; i < _editorNames.Length; i+=3)
        {
            var filtered = _editorNames.Where((_, ind) => ind >= i && ind < i + 3).ToArray();
            int selectIndex = _selectedTab >= i && _selectedTab < i+3? _selectedTab - i:-1;
            EditorGUI.BeginChangeCheck();
            selectIndex = GUILayout.Toolbar(selectIndex, filtered,GUI.skin.button,GUI.ToolbarButtonSize.Fixed,GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                SelectedTab = i + selectIndex;
            }
        }
        //SelectedTab = GUILayout.Toolbar(_selectedTab, 
        //    _editorNames,GUI.skin.button,GUI.ToolbarButtonSize.FitToContents,GUILayout.ExpandHeight(true));
        
        if (_selectedTab == 0)
        {
            DrawPropertiesExcluding(serializedObject,"m_Script");
        }
        else
        {
            foreach (var editor in _tabs[_selectedTab-1].editors)
            {
                editor.OnInspectorGUI();
                GUILayout.Space(18f);
            }
        }
    }

    private void Minify()
    {
        EditorPrefs.SetBool("useWeaponMinifiedEditor", true);

        foreach (var monoBehaviour in list)
            monoBehaviour.hideFlags = HideFlags.HideInInspector;
        
    }
    
    
    private void UnMinify()
    {
        EditorPrefs.SetBool("useWeaponMinifiedEditor", false);

        foreach (var monoBehaviour in list)
            monoBehaviour.hideFlags = HideFlags.None;
    }
}

public interface IWeaponInfo
{
    public void CacheReferences(IFightingStateMachineVariables variables);

    public void Shoot();
    
    public int remainingAmmo { get; }
    public int maxAmmo { get; }

}

public abstract class GunModule : MonoBehaviour
{
    protected IFightingStateMachineVariables Variables;

    public virtual void OnAfterShoot(){}
}