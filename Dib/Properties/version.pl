﻿#
# version.pl
# DisplaySetup.Editor
# Copyright (c) 2011, S. Grottel
# Copyright (c) 2011, VISUS (Universitaet Stuttgart)
# All rights reserved.
#
# See: LICENCE.TXT or
# https://svn.vis.uni-stuttgart.de/utilities/rev2ver/LICENCE.TXT
#
my $path = shift;

push @INC, $path . "\\rev2ver";
require "rev2ver.inc";

my $verInfo = getRevisionInfo($path);

my $majorVer = 1;
my $minorVer = 3;
my $buildNum = $verInfo->rev;
my $revNum = 0;

my $content = qq[/// DO NOT EDIT!
/// This file contains code automatically generated by "version.pl"
namespace Dib.Properties {

    /// <summary>
    /// static class holding build version information about the application
    /// </summary>
    internal static partial class VersionInfo {

        /// <summary>
        /// The version string of the application
        /// </summary>
        internal const string VersionString = "] . $majorVer . "." . $minorVer . "." . $buildNum . "." . $revNum . qq[";

    };

}
];

writeFileChanges($path . "\\Properties\\VersionInfo.gen.cs", $content);
