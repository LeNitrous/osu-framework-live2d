﻿// Copyright (c) Vignette Project.
// Licensed under MIT. Please see LICENSE for more details.
// This software implements Live2D. Copyright (c) Live2D Inc. All Rights Reserved.
// License for Live2D can be found here: http://live2d.com/eula/live2d-open-software-license-agreement_en.html

namespace Vignette.Application.Live2D.Json
{
    public class CubismUserDataSetting : ICubismJsonSetting
    {
        public int Version { get; set; }

        public Metadata Meta { get; set; }

        public Userdata UserData { get; set; }

        public class Metadata
        {
            public int UserDataCount { get; set; }

            public int TotalUserDataSize { get; set; }
        }

        public class Userdata
        {
            public string Target { get; set; }

            public string Id { get; set; }

            public string Value { get; set; }
        }
    }
}
