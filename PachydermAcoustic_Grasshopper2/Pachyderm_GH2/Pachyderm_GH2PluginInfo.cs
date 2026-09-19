//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
//'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
//'it under the terms of the GNU General Public License as published 
//'by the Free Software Foundation; either version 3 of the License, or 
//'(at your option) any later version. 
//'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
//'but WITHOUT ANY WARRANTY; without even the implied warranty of 
//'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
//'GNU General Public License for more details. 
//' 
//'You should have received a copy of the GNU General Public 
//'License along with Pachyderm-Acoustic; if not, write to the Free Software 
//'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using System;
using System.Reflection;

namespace Pachyderm_GH
{
    public sealed class Pachyderm_GH2PluginInfo : Grasshopper2.Framework.Plugin
    {

        static T GetAttribute<T>() where T : Attribute => typeof(Pachyderm_GH2PluginInfo).Assembly.GetCustomAttribute<T>();

        public Pachyderm_GH2PluginInfo()
          : base(new Guid("5A5C5158-F1ED-4ECB-9CDA-B543E928BE81"), new Nomen(
                    GetAttribute<AssemblyTitleAttribute>()?.Title,
                    GetAttribute<AssemblyDescriptionAttribute>()?.Description),
                 typeof(Pachyderm_GH2PluginInfo).Assembly.GetName().Version)
        {
            Icon = AbstractIcon.FromResource("Pachyderm_GH2Plugin", typeof(Pachyderm_GH2PluginInfo));
        }

        public override string Author => GetAttribute<AssemblyCompanyAttribute>()?.Company;

        public override sealed IIcon Icon { get; }

        public override sealed string Copyright => GetAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? base.Copyright;

        // public override sealed string Website => "https://mywebsite.example.com";

        // public override sealed string Contact => "myemail@example.com";

        // public override sealed string LicenceAgreement => "license or URL";

    }
}