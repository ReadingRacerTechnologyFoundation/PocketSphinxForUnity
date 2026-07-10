/*
 * Copyright (c) 2025 Reading Racer Technology Foundation
 *
 * This file is part of ReadingRacerTechnologyFoundation/PocketSphinxForUnity
 *
 * SPDX-License-Identifier: LGPL-3.0-or-later
 *
 * This software is distributed without any warranty.
 * See the LICENSE file in the project root for full terms.
 *
 * This source may contain or make use of third-party components,
 * including PocketSphinx and SphinxBase, which are licensed separately.
 * See THIRD_PARTY_LICENSES.txt for details.
 */
using System.Text;

namespace Rrtf
{
    /// <summary>
    /// Helper extentions for string builder
    /// </summary>
    public static class StringBuilderExtentions
    {
        /// <summary>
        /// helper function that words like appendline but will always use \n
        /// WARNING WE NEED TO USE .Append(\n) because the c++ code expencts \n not \r\n like it does for windows when you use AppendLine
        /// </summary>
        /// <param name="sb">the string builder we are using</param>
        /// <param name="s">the string to append</param>
        /// <returns>the stringbuilder that was passed in</returns>
        public static StringBuilder AppendNewline(this StringBuilder sb, string s)
        {
            return sb.Append(s).Append("\n");
        }

        /// <summary>
        /// helper function that appends a TRANSITION state when building an FSG
        /// </summary>
        /// <param name="sb">the string builder being used</param>
        /// <param name="from">from state</param>
        /// <param name="to">to state</param>
        /// <param name="prob">probability</param>
        /// <param name="word">word for the state</param>
        /// <returns>The stringbuilder that was passed in</returns>
        public static StringBuilder AddFSGTransition(this StringBuilder sb, int from, int to, double prob, string word)
        {
            return sb.Append($"TRANSITION {from} {to} {prob} {word}\n");
        }
    }
}