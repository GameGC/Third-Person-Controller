using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AimTransition : BaseStateTransition
{
    [SerializeField] private bool isAim;

    private IBaseInputReader _input;
    public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _input = resolver.GetComponent<IBaseInputReader>();
    }

    public override bool couldHaveTransition => false;

    public bool couldHaveTransition2
    {
        get
        {
            return false;
        }
        set
        {
            Debug.Log("f");
        }
    }
}

public class MultiTransitionCompiler //: IPreprocessBuildWithReport
{
    //public int callbackOrder { get; }
    //public void OnPreprocessBuild(BuildReport report)
    //{
    //}

    [MenuItem("Tools/CodeGen")]
    public static void Generate()
    {
        var generator = new ClassCodeGenerator(new ClassCodeGenerator.ClassDefine("SomeAimTransition"));
        generator.AddField(new []{typeof(SerializeField)},ClassCodeGenerator.FieldDefine.Modificators.private_,typeof(bool),"isAim");
        generator.AddField(null,ClassCodeGenerator.FieldDefine.Modificators.private_ ,typeof(IBaseInputReader),"_input");
        File.WriteAllText(Application.dataPath+"/Compilers/Generated/Test.cs",generator.GenerateClassCode());
    }
    
    [MenuItem("Tools/Retrived")]
    public static void Retrive()
    {
        var result =CodeRetriever.PropertyCodeRetriever.RetrievePropertyCode(
            typeof(AimTransition).GetProperty("couldHaveTransition2", BindingFlags.Public | BindingFlags.Instance),Application.dataPath+"/Compilers/");
        Debug.Log(result.getBody);
        Debug.Log(result.setBody);
    }
    
    
    
}

public class ClassCodeGenerator
{
    public struct ClassDefine
    {
        public ClassDefine(string className) : this()
        {
            this.className = className;
        }

        public ClassDefine(string className, Type[] attributes)
        {
            this.className = className;
            this.attributes = attributes;
        }
        
        public string className;
        public Type[] attributes;
    }
    
    public struct FieldDefine
    {
        [Flags] 
        public enum Modificators
        {
            private_ = 0x0,
            protected_ = 0x1,
            public_ = 0x2,
            const_ = 0x4,
            virtual_ = 0x8,
            override_ = 0x10,
        }
        public string FieldName;
        public Type FieldType;
        public Modificators FieldModificators;

        public FieldDefine(string fieldName, Type fieldType, Modificators fieldModificators)
        {
            FieldName = fieldName;
            FieldType = fieldType;
            FieldModificators = fieldModificators;
        }
    }

    private ClassDefine classDefine;


    private List<string> fields;
    private List<string> properties;
    private List<string> methods;
    private List<string> attributes;

    // <summary>
    // Constructs a new instance of the ClassCodeGenerator class with the specified class name.
    //
    // Parameters:
    // - classDefine: The name of the class to be generated.
    // </summary>
    public ClassCodeGenerator(ClassDefine classDefine)
    {
        this.classDefine = classDefine;
        fields = new List<string>();
        properties = new List<string>();
        methods = new List<string>();
        attributes = new List<string>();
    }

    // <summary>
    // Adds a field to the class.
    //
    // Parameters:
    // - fieldType: The type of the field.
    // - fieldName: The name of the field.
    // </summary>
    public void AddField(Type[] attributes,FieldDefine.Modificators modificators,Type fieldType, string fieldName)
    {
        string attributesCode = string.Empty;
        if (attributes != null && attributes.Length > 0)
        {
            attributesCode = attributes.Aggregate("", (current, attribute) => current + $"{attribute.Name},");
            attributesCode = $"[{attributesCode.Remove(attributesCode.Length - 1)}]";
        }

        var flags = GetFlags(modificators);
        string modString = flags.Aggregate("", (current, flag) => current + $"{flag.ToString().Replace("_", "")} ");

        string field = $"{attributesCode} {modString} {fieldType.Name} {fieldName};";
        fields.Add(field);
    }
    
    private static IEnumerable<Enum> GetFlags(Enum input)
    {
        foreach (Enum value in Enum.GetValues(input.GetType()))
        {
            if (input.HasFlag(value))
            {
                yield return value;
            }
        }
    }

    // <summary>
    // Adds a property to the class.
    //
    // Parameters:
    // - propertyType: The type of the property.
    // - propertyName: The name of the property.
    // </summary>

    public void AddProperty(Type propertyType, string propertyName, bool hasGet, bool hasSet)
    {
        string getText = hasGet ? "get;" : string.Empty;
        string setText = hasSet ? "set;" : string.Empty;

        string property = $"public {propertyType.Name} {propertyName} {{ {getText} {setText} }}";
        properties.Add(property);
    }

    public void AddProperty(Type propertyType, string propertyName, bool hasGet, bool hasSet, string getBody = null,
        string setBody = null)
    {
        string getText = hasGet ? string.IsNullOrEmpty(getBody) ? "get;" : $"get=>{{{getBody}}}" : string.Empty;
        string setText = hasSet ? string.IsNullOrEmpty(setBody) ? "set;" : $"set=>{{{setBody}}}" : string.Empty;

        string property = $"public {propertyType.Name} {propertyName} {{ {getText} {setText} }}";
        properties.Add(property);
    }

    // <summary>
    // Adds a method to the class.
    //
    // Parameters:
    // - returnType: The return type of the method.
    // - methodName: The name of the method.
    // - methodBody: The body of the method.
    // </summary>
    public void AddMethod(Type returnType, string methodName, string methodBody)
    {
        string method = $"{returnType.Name} {methodName}() {{ {methodBody} }}";
        methods.Add(method);
    }

    // <summary>
    // Adds an attribute to the class.
    //
    // Parameters:
    // - attributeName: The name of the attribute.
    // </summary>
    public void AddAttribute(string attributeName)
    {
        string attribute = $"[{attributeName}]";
        attributes.Add(attribute);
    }

    // <summary>
    // Generates the C# class code as a string.
    //
    // Returns:
    // - A string representing the generated C# class code.
    // </summary>
    public string GenerateClassCode()
    {
        StringBuilder codeBuilder = new StringBuilder();

        // Add attributes.
        foreach (string attribute in attributes)
        {
            codeBuilder.AppendLine(attribute);
        }

        // Add class declaration.
        if (classDefine.attributes != null && classDefine.attributes.Length >0)
        {
            var attributesCode = classDefine.attributes.Aggregate("", (current, attribute) => current + $"{attribute.Name},");
            attributesCode = attributesCode.Remove(attributesCode.Length - 1);
            codeBuilder.AppendLine($"[{attributesCode}]");
        }

        codeBuilder.AppendLine($"public class {classDefine.className}");
        codeBuilder.AppendLine("{");

        // Add fields.
        foreach (string field in fields)
        {
            codeBuilder.AppendLine($"    {field}");
        }

        // Add properties.
        foreach (string property in properties)
        {
            codeBuilder.AppendLine($"    {property}");
        }

        // Add methods.
        foreach (string method in methods)
        {
            codeBuilder.AppendLine($"    {method}");
        }

        codeBuilder.AppendLine("}");

        return codeBuilder.ToString();
    }
}