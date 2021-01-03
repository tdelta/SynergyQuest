using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Editor.Validation
{
    public interface Check
    {
        [CanBeNull] Issue PerformCheck();
    }
    
    public interface Issue
    {
        string Description { get; }
        
        bool CanAutofix { get; }

        void Autofix();
    }

    public static class CheckSharedMethods
    {
        public static Lazy<List<(string, Func<Check>)>> CheckFactories = new Lazy<List<(string, Func<Check>)>>(() =>
        {
            var type = typeof(Check);

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface)
                .Select<Type, (string, Func<Check>)>(p =>
                {
                    var constructorInfo = p.GetConstructor(new Type[0]);
                    if (constructorInfo == null)
                    {
                        throw new ApplicationException($"All implementors of {nameof(Check)} must also implement a public constructor with no parameters. {p.Name} does not comply to this.");
                    }
                    
                    return (
                            p.Name,
                            () => (Check) constructorInfo.Invoke(new object[0])
                    );
                })
                .ToList();
        });
    }

    public static class IssueSharedMethods
    {
        public static string GetLogText(this Issue issue)
        {
            var autofixableString = issue.CanAutofix ? " (autofixable)" : "";
            return $"Detected issue{autofixableString}: {issue.Description}";
        }
        
        public static void Log(this Issue issue)
        {
            Debug.LogError(issue.GetLogText());
        }
    }
}