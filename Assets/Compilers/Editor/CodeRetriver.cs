using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CodeRetriever
{
    // <summary>
    // Represents a class that allows retrieving the C# code of a property's body.
    // The class searches for the code in all .cs files within a specified folder and checks
    // if the class is the same as the owner of the PropertyInfo.
    // </summary>
    public class PropertyCodeRetriever
    {
        // <summary>
        // Retrieves the C# code of the get and set bodies of a specified PropertyInfo.
        //
        // Parameters:
        // - propertyInfo: The PropertyInfo object for which to retrieve the code.
        // - folderPath: The path to the folder containing the .cs files to search in.
        //
        // Returns:
        // - A tuple containing the getBody and setBody strings of the property's code.
        //
        // Exceptions:
        // - Throws a FileNotFoundException if the specified folder path does not exist.
        // - Throws an ArgumentException if the PropertyInfo does not belong to a class.
        // </summary>
        public static (string getBody, string setBody) RetrievePropertyCode(PropertyInfo propertyInfo,
            string folderPath)
        {
            // Check if the folder path exists.
            if (!Directory.Exists(folderPath))
            {
                throw new FileNotFoundException("The specified folder path does not exist.");
            }

            // Check if the PropertyInfo belongs to a class.
            if (propertyInfo.DeclaringType == null)
            {
                throw new ArgumentException("The PropertyInfo does not belong to a class.");
            }

            // Get the class name of the owner of the PropertyInfo.
            string className = propertyInfo.DeclaringType.Name;

            // Get all .cs files in the specified folder.
            string[] csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            // Iterate through each .cs file.
            foreach (string csFile in csFiles)
            {
                // Read the contents of the file.
                string fileContent = File.ReadAllText(csFile);

                // Check if the file contains the class name.
                if (fileContent.Contains(className))
                {
                    // Find the property in the file.
                    string propertyCode = FindPropertyCode(fileContent, propertyInfo.Name);

                    Debug.Log(propertyCode);
                    // If the property code is found, extract the get and set bodies.
                    if (!string.IsNullOrEmpty(propertyCode))
                    {
                        string getBody = ExtractPropertyBody(propertyCode, "get");
                        string setBody = ExtractPropertyBody(propertyCode, "set");

                        return (getBody, setBody);
                    }
                }
            }

            // If the property code is not found, return empty strings.
            return (string.Empty, string.Empty);
        }

        // <summary>
        // Finds the code block of a property with the specified name in a given file content.
        //
        // Parameters:
        // - fileContent: The content of the file to search in.
        // - propertyName: The name of the property to find.
        //
        // Returns:
        // - A string containing the code block of the property, or an empty string if not found.
        // </summary>
        private static string FindPropertyCode(string fileContent, string propertyName)
        {
            Debug.Log(fileContent);
            //bset stable 

            var patterns = new string[]
            {
                $@"(?s)(public\s+override\s+)?(public\s+|private\s+|protected\s+|internal\s+|protected\s+internal\s+)?\s*(static\s+|const\s+|readonly\s+)?([\w<>,?]+\s+)?{propertyName}\s*{{(.*?)(?=^}})",
                $@"(?s)(public\s+override\s+)?(public\s+|private\s+|protected\s+|internal\s+|protected\s+internal\s+)?\s*([\w<>,?]+\s+)?{propertyName}\s*=>(.*?)(?=;|$)",
                $@"(?m)^\s*(?:""(?:[^""\\]|\\.)*""|//.*|/\*.*?\*/|\[.*?\])*\s*(?:(?:public|private|protected|internal|protected internal)\s+)?(?:static\s+|const\s+|readonly\s+)?\w+[\w\s<>,?]*\??\s+{propertyName}\s*{{[^{{}}]*?(?<!;)}}",
            };


            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(fileContent, pattern,
                    RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                    return matches[0].Value;
            }

            return null;
        }

        // <summary>
        // Finds the index of the closing brace of a code block starting from a specified line and position.
        //
        // Parameters:
        // - lines: The array of lines to search in.
        // - startLineIndex: The index of the line to start searching from.
        // - startPosition: The position within the start line to start searching from.
        //
        // Returns:
        // - An integer representing the index of the closing brace, or -1 if not found.
        // </summary>
        private static int FindClosingBraceIndex(string[] lines, int startLineIndex, int startPosition)
        {
            int braceCount = 1;

            // Iterate through each line starting from the specified line index.
            for (int i = startLineIndex; i < lines.Length; i++)
            {
                // Iterate through each character starting from the specified position.
                for (int j = startPosition + 1; j < lines[i].Length; j++)
                {
                    // Check if the character is an opening brace.
                    if (lines[i][j] == '{')
                    {
                        braceCount++;
                    }
                    // Check if the character is a closing brace.
                    else if (lines[i][j] == '}')
                    {
                        braceCount--;

                        // If the closing brace is found, return its index.
                        if (braceCount == 0)
                        {
                            return j;
                        }
                    }
                }

                // Reset the start position for subsequent lines.
                startPosition = -1;
            }

            // If the closing brace is not found, return -1.
            return -1;
        }

        // <summary>
        // Extracts the body of a get or set accessor from a property code block.
        //
        // Parameters:
        // - propertyCode: The code block of the property.
        // - accessorType: The type of accessor to extract ("get" or "set").
        //
        // Returns:
        // - A string containing the body of the specified accessor, or an empty string if not found.
        // </summary>
        private static string ExtractPropertyBody(string propertyCode, string accessorType)
        {
            var patterns = new string[]
            {
                $@"(?<={accessorType}\s*=>\s*).*?(?=;)",
                $@"(?<={accessorType}\s*{{}}).*?(?=;)",

            };


            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(propertyCode, pattern,
                    RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                    return matches[0].Value;
            }


            propertyCode = propertyCode.Substring(propertyCode.IndexOf(accessorType));
            propertyCode = propertyCode.Substring(propertyCode.IndexOf('{') + 1,
                propertyCode.IndexOf('}') - propertyCode.IndexOf('{') - 1);
            return propertyCode;
        }
    }
}