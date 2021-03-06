// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
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

using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Data
{
    /**
     * <summary>
     * We keep a file <see cref="ResourcePathSettings.ExternalArtCredits"/> which contains
     * information (author, license etc.) about every art asset (music, graphics, sounds, ...) which was not
     * specifically made for this game but sourced from platforms like OpenGameArt.org etc.
     *
     * This class provides access to this information.
     * </summary>
     */
    public class ExternalArtCredits
    {
        /**
         * <summary>
         * Read <see cref="ResourcePathSettings.ExternalArtCredits"/> and list all contained information.
         * </summary>
         * <remarks>
         * The file must be in YAML format.
         * </remarks>
         */
        public static Dictionary<string, CreditsEntry> GetExternalArtCredits()
        {
            var externalArtCreditsRaw = ResourcePathSettings.Instance.ExternalArtCredits.text;
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize<
                Dictionary<string, CreditsEntry>
            >(externalArtCreditsRaw);
        }
        
        /**
         * <summary>
         * This class describes the format of every entry of the credits file
         * </summary>
         */
        public class CreditsEntry
        {
            public string author;
            public string description;
            public string license;
            public string modifications;
            public string link;
        }
    }
}