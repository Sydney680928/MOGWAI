// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System.Reflection;

namespace MOGWAI_RUNTIME.Classes
{
    internal class Tools
    {
        public static void SuspendAutoPowerOff()
        {
#if ANDROID

            if (Platform.CurrentActivity is MainActivity activity)
            {
                activity.SetScreenOn();
            }

#elif IOS

            Platforms.iOS.IosTools.SuspendAutoPowerOff();

#endif
        }

        public static void ResumeAutoPowerOff()
        {
#if ANDROID

            if (Platform.CurrentActivity is MainActivity activity)
            {
                activity.LeaveScreenOff();
            }

#elif IOS

            Platforms.iOS.IosTools.ResumeAutoPowerOff();

#endif
        }

        public static string? GetStringFromResource(string resource)
        {
            try
            {
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"MOGWAI_RUNTIME.Resources.Raw.{resource}");

                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
