using System;
using System.Reflection;
using FuzzySearch;
using ThirdPersonController.Core;
using TypeNamespaceTree;
using UnityEditor;
using UnityEngine;


public partial class FuzzyWindow : EditorWindow
{
    public event Action<Type> TypeClicked;
    
    static FuzzyWindow()
    {
        ShowAsDropDownFitToScreen = typeof(EditorWindow).GetMethod("ShowAsDropDownFitToScreen",
            BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private readonly float height = 320;
    private float _minOptionWidth;
    private Rect _activatorPosition;
    private bool _scrollToSelected;
    private float _initialY;
    private bool _initialYSet;

    private const float searchFieldHeight = 20;
    private const float headerHeight = 25;

    private const float initialSpace = 7;
    private static Event e => Event.current;

    private FuzzySearchPanel _searchPanel;
    private FuzzyHeaderWithBackButtonPanel _headerWithBackButtonPanel;
    private FuzzyListPanel _list;

    private IOptionTree[] hierarchyTree;
    private IOptionTree[] typeTree1d;

    private static Type _baseType;
    private void OnEnable()
    {
        _searchPanel = new FuzzySearchPanel();
        _searchPanel.QueryChanged += SearchPanelOnQueryChanged;

        _headerWithBackButtonPanel = new FuzzyHeaderWithBackButtonPanel();
        _headerWithBackButtonPanel.BackClicked += OnHeaderBackClicked;
        _list = new FuzzyListPanel();

        var types = GetNonAbstractTypesSubclassOf(_baseType);

        hierarchyTree = BuildTreeHierarchy(types,_baseType == typeof(BaseFeature)).Childs.ToArray();
        _list.SetData(hierarchyTree);
        _list.NextClicked += EnterChild;
        _list.TypeClicked += ListOnTypeClicked;
        

        typeTree1d = BuildTree1d(types);
    }

    private void ListOnTypeClicked(Type type)
    {
        TypeClicked?.Invoke(type);
        Close();
    }

    private void SearchPanelOnQueryChanged(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _list.SetData(hierarchyTree);
            _list.Repaint = true;
        }
        else
        {
            var queryChars = query.ToCharArray();

            Array.Sort(typeTree1d, Sort);
            _list.SetData(typeTree1d);

            int Sort(IOptionTree a, IOptionTree b)
            {
                int score0 = 0;
                int score1 = 0;

                string contentA = a.Content.text;
                string contentB = b.Content.text;
                
                if (contentA.Contains(query, StringComparison.InvariantCultureIgnoreCase)) score0 += contentA.Length;
                if (contentB.Contains(query, StringComparison.InvariantCultureIgnoreCase)) score1 += contentA.Length;
                
                for (int i = 0; i < queryChars.Length; i++)
                {
                    if (contentA.Contains(queryChars[i], StringComparison.InvariantCultureIgnoreCase)) score0++;
                    if (contentB.Contains(queryChars[i], StringComparison.InvariantCultureIgnoreCase)) score1++;
                }

                return score1.CompareTo(score0);
            }
        }

        animTarget = 1;
        lastRepaintTime = DateTime.Now;
    }

    private void OnHeaderBackClicked()
    {
        EnterParent();
        var newChilds = _list.Parent.Parent.Childs;
        _list.Parent = newChilds[0].Parent;
        _list.SetData(newChilds.ToArray());
        _list.Repaint = true;
    }

    private void Update()
    {
        if (_list.Repaint)
        {
            Repaint();
            _list.Repaint = false;
        }
    }

    private void OnLevelGUI(float anim, in bool isRepaint)
    {
        anim = Mathf.Floor(anim) + Mathf.SmoothStep(0, 1, Mathf.Repeat(anim, 1));

        var levelPosition = new Rect(position.width * (1 - anim) + 1, 30, position.width - 2, height - 31);
        GUILayout.BeginArea(levelPosition);

        if (_list.Parent.Parent != null)
        {
            var headerPosition = GUILayoutUtility.GetRect(16, headerHeight);

            _headerWithBackButtonPanel.OnGUI(_list.Parent, headerPosition,in isRepaint);
        }

        _list.OnGUI(in isRepaint);

        GUILayout.EndArea();
    }


    private static readonly MethodInfo ShowAsDropDownFitToScreen;

    private static FuzzyWindow instance;

    public static void Show(Rect activatorPosition,Vector2 size, Type type,Action<Type> onClicked)
    {
        // Makes sure control exits DelayedTextFields before opening the window
        //GUIUtility.keyboardControl = 0;

        _baseType = type;

        if (instance != null)
        {
            try
            {
                instance.Close();
            }
            catch (Exception exception)
            {
            }
        }

        {
            
            instance = CreateInstance<FuzzyWindow>();

            instance.CreateWindow(activatorPosition,size);
            instance.TypeClicked += onClicked;
        }
    }

    private float anim = 1;
    private int animTarget = 1;
    private float animationSpeed = 4;

    private DateTime lastRepaintTime;

    private float repaintDeltaTime => (float) (DateTime.Now - lastRepaintTime).TotalSeconds;

    private bool isAnimating => anim != animTarget;

    //Be sure do not call any layout change while the UI is repainting or force a repaint
    //before the layout is completed.
    //This will probably break the UI

    private void OnGUI()
    {
        GUILayout.Space(7);

        bool isRepaint = Event.current.type == EventType.Repaint;

        var searchFieldPosition = GUILayoutUtility.GetRect(10, searchFieldHeight);
        searchFieldPosition.x += 8;
        searchFieldPosition.width -= 16;

        _searchPanel.OnGUI(searchFieldPosition);

        OnLevelGUI(anim, in isRepaint);

        UpdateAnimation(in isRepaint);
        if (isRepaint) 
            Rescale();
    }

    private void Rescale()
    {
        var copy = position;
        copy.height = headerHeight + initialSpace + searchFieldHeight + Mathf.Max(_list.GetHeight(), 100);
        copy.width = Mathf.Max(_headerWithBackButtonPanel.GetWidth(), _searchPanel.GetWidth(), _list.Width) + 18f;
        position = copy;
    }

    private Vector2 startPostion;
    private Vector2 savedSize;
    private void CreateWindow(Rect activatorPosition,Vector2 size)
    {
        // Show and focus the window
        wantsMouseMove = true;
        var initialSize = new Vector2(size.x, headerHeight + initialSpace + searchFieldHeight + 100); 
        startPostion = new Vector2(activatorPosition.x,activatorPosition.y);
        ShowAsDropDown(EditorGUIUtility.GUIToScreenRect(activatorPosition),initialSize);
        savedSize = initialSize;
        Focus();
    }
}