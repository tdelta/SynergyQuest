// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2021
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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